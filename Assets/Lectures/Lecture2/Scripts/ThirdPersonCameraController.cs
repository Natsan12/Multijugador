using UnityEngine;
using Unity.Netcode;

public class ThirdPersonCameraController : NetworkBehaviour
{
    private Camera mainCamera;

    [Header("Cámara")]
    public Vector3 offset = new Vector3(0f, 2f, -6f);
    public float followSpeed = 10f;

    [Header("Rotación")]
    public float mouseSensitivity = 2f;
    public float minY = 5f;
    public float maxY = 60f;

    private float currentYaw = 0f;
    private float currentPitch = 10f;
    private Transform target;

    private void Start()
    {
        if (!IsOwner) return;
        if (target == null)
            
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No se encontró la cámara principal.");
            return;
        }

        mainCamera.enabled = true;
        mainCamera.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        target = transform;
    }

    private void LateUpdate()
    {
        if (!IsOwner || mainCamera == null || target == null) return;

        // Movimiento del mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minY, maxY);

        // Rotación solo en pitch para la cámara (no afecta offset invertido)
        Quaternion pitchRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        Quaternion yawRotation = Quaternion.Euler(0f, currentYaw, 0f);

        // Posición deseada: solo la Y rota verticalmente
        Vector3 rotatedOffset = pitchRotation * offset;
        Vector3 desiredPosition = target.position + yawRotation * rotatedOffset;

        // Aplicar posición suavizada
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // La cámara mira al personaje
        mainCamera.transform.LookAt(target.position + Vector3.up * 1.5f);
    }


}
