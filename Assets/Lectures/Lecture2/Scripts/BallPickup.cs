using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class BallPickup : NetworkBehaviour
{
    public NetworkVariable<bool> isTaken = new NetworkVariable<bool>(false);
    private Rigidbody rb;
    private bool isBeingHeld = false;
    private Transform currentHoldPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isBeingHeld && currentHoldPoint != null)
        {
            transform.position = currentHoldPoint.position;
            transform.rotation = currentHoldPoint.rotation;
        }
    }

    public void TryPickUp(ulong playerId)
    {
        if (IsServer && !isTaken.Value)
        {
            Debug.Log($"[Servidor] Balón recogido por el jugador {playerId}");
            isTaken.Value = true;
            PickupClientRpc(playerId);
        }
    }

    [ClientRpc]
    void PickupClientRpc(ulong playerId)
    {
        if (NetworkManager.Singleton.LocalClientId == playerId)
        {
            var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            Transform holdPoint = player.GetComponentInChildren<HoldPoint>()?.transform;

            if (holdPoint != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.detectCollisions = false;

                currentHoldPoint = holdPoint;
                isBeingHeld = true;

                Debug.Log("[Cliente] Balón unido al HoldPoint");

                var animator = player.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.SetBool("IsPicking", true);
                    player.GetComponent<MonoBehaviour>().StartCoroutine(ResetPickAnimation(animator));
                }
            }
        }
    }

    private IEnumerator ResetPickAnimation(Animator animator)
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("IsPicking", false);
    }

    public void Deliver()
    {
        if (IsServer)
        {
            Debug.Log("[Servidor] Balón entregado al contenedor");
            isTaken.Value = false;
            DeliverClientRpc();
        }
    }

    [ClientRpc]
    void DeliverClientRpc()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.detectCollisions = true;

        currentHoldPoint = null;
        isBeingHeld = false;

        Debug.Log("[Cliente] Balón soltado (enviado lejos o reposicionado)");

        
    }

    public void PrepararComoEntregado()
    {
        Debug.Log("[Cliente] Balón marcado como entregado en el lugar");
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.detectCollisions = false;
        isBeingHeld = false;
        currentHoldPoint = null;
    }

    public bool IsBeingHeld()
    {
        return isBeingHeld;
    }
}
