using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerPickup : MonoBehaviour
{
    public Transform holdPoint;
    public float pickupRange = 2f;
    private GameObject heldObject;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && heldObject == null)
        {
            TryPickup();
        }

        if (Input.GetMouseButtonDown(1) && heldObject != null)
        {
            DropObject();
        }
    }

    void TryPickup()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Pickable"))
            {
                heldObject = col.gameObject;

                // Preparar objeto
                Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }

                // Poner en la mano
                heldObject.transform.position = holdPoint.position;
                heldObject.transform.SetParent(holdPoint);

                // Cambiar Layer
                heldObject.layer = LayerMask.NameToLayer("HeldObject");

                // Ignorar colisión con el jugador
                Collider objectCollider = heldObject.GetComponent<Collider>();
                Collider[] playerColliders = GetComponentsInChildren<Collider>();
                foreach (var playerCol in playerColliders)
                {
                    if (objectCollider != null)
                        Physics.IgnoreCollision(objectCollider, playerCol, true);
                }

                // Reproducir animación de recoger
                animator.SetBool("IsPicking", true);
                StartCoroutine(ResetIsPicking());

                return;
            }
        }
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;

                // 🔥 ESTO EVITA QUE EMPUJE O GIRE AL JUGADOR
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.mass = 0.1f;
                rb.linearDamping = 1f;
                rb.angularDamping = 0.5f;
            }

            StartCoroutine(ReenableCollision(heldObject));

            heldObject.transform.SetParent(null);
            heldObject.transform.rotation = Quaternion.identity;
            heldObject.layer = LayerMask.NameToLayer("Default");

            heldObject = null;
        }
    }

    private IEnumerator ReenableCollision(GameObject obj)
    {
        Collider objectCollider = obj.GetComponent<Collider>();
        Collider[] playerColliders = GetComponentsInChildren<Collider>();

        foreach (var col in playerColliders)
        {
            if (objectCollider != null)
                Physics.IgnoreCollision(objectCollider, col, true);
        }

        yield return new WaitForSeconds(0.5f);

        foreach (var col in playerColliders)
        {
            if (objectCollider != null)
                Physics.IgnoreCollision(objectCollider, col, false);
        }
    }

    private IEnumerator ResetIsPicking()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("IsPicking", false);
    }
}
