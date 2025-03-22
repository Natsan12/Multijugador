using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    public Animator animator;
    public float moveSpeed = 2f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    private Vector3 inputMovement;
    private bool isGrounded = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleMovementInput();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        MovePlayer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetInteger("JumpPhase", 0);
        }
    }

    public void TriggerVictory()
    {
        animator.SetBool("IsVictory", true);
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

        // Movimiento relativo a c�mara
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        inputMovement = (cameraForward * moveZ + cameraRight * moveX).normalized;

        // Rotar al personaje en direcci�n de movimiento
        Quaternion targetRotation = Quaternion.LookRotation(inputMovement);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

        animator.SetFloat("Speed", 1.5f);
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

    private void MovePlayer()
    {
        Vector3 movePosition = rb.position + inputMovement * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(movePosition);
    }
}
