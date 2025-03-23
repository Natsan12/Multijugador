using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class BallPickup : NetworkBehaviour
{
    public NetworkVariable<bool> isTaken = new NetworkVariable<bool>(false);
    private ulong carriedByClientId;
    private Transform followTarget;
    private NetworkTransform netTransform;

    private void Awake()
    {
        netTransform = GetComponent<NetworkTransform>();
    }

    public void TryPickUp(ulong clientId)
    {
        if (isTaken.Value) return;
        isTaken.Value = true;
        carriedByClientId = clientId;
        netTransform.enabled = false;

        Debug.Log($"🎒 Balón recogido por jugador {clientId}");
    }

    public void Deliver()
    {
        isTaken.Value = false;
        carriedByClientId = 0;
        followTarget = null;

        Debug.Log("✅ Balón entregado");
    }

    [ClientRpc]
    public void SetFollowTargetClientRpc(ulong playerId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(playerId))
        {
            GameObject player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.gameObject;
            followTarget = player.GetComponent<PlayerController>().holdPoint;
        }
    }

    [ClientRpc]
    public void ClearFollowTargetClientRpc()
    {
        Debug.Log("🧹 followTarget limpiado en cliente");
        followTarget = null;
    }

    [ClientRpc]
    public void SetBallPositionClientRpc(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    [ClientRpc]
    public void ForceVisualResetClientRpc()
    {
        followTarget = null;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.detectCollisions = true;
    }

    private void LateUpdate()
    {
        // ❌ Antes: if (!IsOwner) return;
        // ✅ Solución: permitir que todos los clientes vean el movimiento si hay un followTarget
        if (followTarget != null)
        {
            transform.position = followTarget.position;
            transform.rotation = followTarget.rotation;
        }
    }

    public bool IsCarriedBy(ulong clientId) => carriedByClientId == clientId;
    public ulong GetCarrier() => carriedByClientId;
}
