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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

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
            animator.SetFloat("Speed", 0f);
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

        animator.SetFloat("Speed", 1.5f);
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

            if (verticalVelocity > 0.1f)
                animator.SetInteger("JumpPhase", 1);
            else if (Mathf.Abs(verticalVelocity) <= 0.1f)
                animator.SetInteger("JumpPhase", 2);
            else if (verticalVelocity < -0.1f)
                animator.SetInteger("JumpPhase", 3);
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
        if (Input.GetMouseButtonDown(0))
        {
            TryPickup();
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (carriedBall != null)
            {
                Debug.Log("📤 Entregando balón al soltar");
                DeliverServerRpc(carriedBall.NetworkObject);
                carriedBall = null;
            }
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
                Debug.Log("🤲 Intentando recoger balón");
                PickupServerRpc(ball.NetworkObject);
                carriedBall = ball;
                break;
            }
        }
    }

    [ServerRpc]
    void PickupServerRpc(NetworkObjectReference ballRef)
    {
        if (ballRef.TryGet(out NetworkObject ballNet))
        {
            var ball = ballNet.GetComponent<BallPickup>();
            if (!ball.isTaken.Value)
            {
                ball.TryPickUp(OwnerClientId);
            }
        }
    }

    [ServerRpc]
    void DeliverServerRpc(NetworkObjectReference ballRef)
    {
        if (ballRef.TryGet(out NetworkObject ballNet))
        {
            var ball = ballNet.GetComponent<BallPickup>();
            ball.Deliver();
        }
    }

    public void SetCarriedBall(BallPickup ball)
    {
        carriedBall = ball;
        animator.SetTrigger("Pick");
    }

    public bool HasBall()
    {
        return carriedBall != null;
    }

    public BallPickup GetCarriedBall()
    {
        return carriedBall;
    }

    public void ClearBall()
    {
        carriedBall = null;
    }
}
