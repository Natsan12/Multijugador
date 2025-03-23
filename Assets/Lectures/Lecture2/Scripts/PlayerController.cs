using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    public Animator animator;
    public float moveSpeed = 1.5f;
    public float jumpForce = 5f;
    public Transform holdPoint;

    private Rigidbody rb;
    private Vector3 inputMovement;
    private bool isGrounded = true;
    private BallPickup carriedBall;

    private NetworkVariable<float> syncedSpeed = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    // 🆕 Para detectar si está dentro del contenedor
    private bool isInEntregaZone = false;
    private EntregaZone entregaZone;

    // 🆕 Para guardar posición inicial del balón al recogerlo
    private Vector3 lastBallPosition;
    private Quaternion lastBallRotation;

    private void Awake() => rb = GetComponent<Rigidbody>();

    private void Start()
    {
        if (!IsOwner)
        {
            rb.isKinematic = true;
            return;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleMovementInput();
        HandleJumpInput();
        HandlePickupDropInput();

        syncedSpeed.Value = inputMovement.magnitude;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        MovePlayer();
    }

    private void HandleMovementInput()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 inputDir = new Vector3(moveX, 0, moveZ).normalized;

        if (inputDir.magnitude < 0.1f)
        {
            inputMovement = Vector3.zero;
            return;
        }

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        inputMovement = (cameraForward * moveZ + cameraRight * moveX).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(inputMovement);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    }

    private void MovePlayer()
    {
        Vector3 movePosition = rb.position + inputMovement * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(movePosition);
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            animator.SetInteger("JumpPhase", 1);
        }

        if (!isGrounded)
        {
            float verticalVelocity = rb.linearVelocity.y;
            if (verticalVelocity > 0.1f) animator.SetInteger("JumpPhase", 1);
            else if (Mathf.Abs(verticalVelocity) <= 0.1f) animator.SetInteger("JumpPhase", 2);
            else if (verticalVelocity < -0.1f) animator.SetInteger("JumpPhase", 3);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetInteger("JumpPhase", 0);
        }
    }

    private void HandlePickupDropInput()
    {
        if (Input.GetMouseButtonDown(0)) TryPickup();

        if (Input.GetMouseButtonDown(1) && carriedBall != null)
        {
            if (isInEntregaZone && entregaZone != null)
            {
                Debug.Log("📤 Soltando balón dentro del contenedor");
                DeliverServerRpc(carriedBall.NetworkObject);
            }
            else
            {
                Debug.Log("🚫 No estás en la zona de entrega, reiniciando posición del balón");
                RestoreBallPositionClientRpc(carriedBall.NetworkObject, lastBallPosition, lastBallRotation);
            }

            carriedBall = null;
        }
    }

    void TryPickup()
    {
        if (carriedBall != null) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out BallPickup ball) && !ball.isTaken.Value)
            {
                // 🧠 Guardamos la posición original
                lastBallPosition = ball.transform.position;
                lastBallRotation = ball.transform.rotation;

                Debug.Log("🤲 Intentando recoger balón");
                PickupServerRpc(ball.NetworkObject);
                break;
            }
        }
    }

    [ServerRpc]
    void PickupServerRpc(NetworkObjectReference ballRef, ServerRpcParams rpcParams = default)
    {
        if (ballRef.TryGet(out NetworkObject ballNet))
        {
            var ball = ballNet.GetComponent<BallPickup>();
            if (!ball.isTaken.Value)
            {
                ball.TryPickUp(rpcParams.Receive.SenderClientId);
                SetBallClientRpc(ballRef, rpcParams.Receive.SenderClientId);
            }
        }
    }

    [ClientRpc]
    void SetBallClientRpc(NetworkObjectReference ballRef, ulong playerId)
    {
        if (ballRef.TryGet(out NetworkObject ballNet))
        {
            BallPickup ball = ballNet.GetComponent<BallPickup>();

            if (OwnerClientId == playerId) carriedBall = ball;

            Rigidbody rb = ball.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.detectCollisions = false;

            RequestAttachBallServerRpc(ballRef, playerId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestAttachBallServerRpc(NetworkObjectReference ballRef, ulong playerId)
    {
        if (ballRef.TryGet(out NetworkObject ballNet))
        {
            BallPickup ball = ballNet.GetComponent<BallPickup>();
            GameObject player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.gameObject;
            Transform holdPoint = player.GetComponent<PlayerController>().holdPoint;

            ball.transform.position = holdPoint.position;
            ball.transform.rotation = holdPoint.rotation;
            ball.SetFollowTargetClientRpc(playerId);
        }
    }

    [ServerRpc]
    void DeliverServerRpc(NetworkObjectReference ballRef)
    {
        if (ballRef.TryGet(out NetworkObject ballNet))
        {
            var ball = ballNet.GetComponent<BallPickup>();
            ball.Deliver();

            ball.transform.SetParent(null);

            Rigidbody rb = ball.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.detectCollisions = false;

            ball.ClearFollowTargetClientRpc();
            ClearCarriedBallClientRpc(OwnerClientId);

            // 🧠 Entrega visual desde zona
            if (entregaZone != null)
            {
                entregaZone.EntregarBalon(ball);
            }
        }
    }

    [ClientRpc]
    void ClearCarriedBallClientRpc(ulong playerId)
    {
        if (OwnerClientId == playerId)
        {
            carriedBall = null;
        }
    }

    // 🆕 Restaurar posición original si no se entrega correctamente
    [ClientRpc]
    void RestoreBallPositionClientRpc(NetworkObjectReference ballRef, Vector3 position, Quaternion rotation)
    {
        if (ballRef.TryGet(out NetworkObject ballNet))
        {
            ballNet.transform.position = position;
            ballNet.transform.rotation = rotation;
            Rigidbody rb = ballNet.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
    }

    private void LateUpdate()
    {
        animator.SetFloat("Speed", syncedSpeed.Value);
    }

    public void SetCarriedBall(BallPickup ball)
    {
        carriedBall = ball;
        animator.SetTrigger("Pick");
    }

    public bool HasBall() => carriedBall != null;
    public BallPickup GetCarriedBall() => carriedBall;
    public void ClearBall() => carriedBall = null;

    // 🆕 Detectar entrada/salida a la zona de entrega
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EntregaZone"))
        {
            isInEntregaZone = true;
            entregaZone = other.GetComponent<EntregaZone>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EntregaZone"))
        {
            isInEntregaZone = false;
            entregaZone = null;
        }
    }
}
