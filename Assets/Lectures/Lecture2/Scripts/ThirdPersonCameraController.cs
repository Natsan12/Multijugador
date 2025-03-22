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
    public float minY = -20f;
    public float maxY = 60f;

    private float currentYaw = 0f;
    private float currentPitch = 10f;
    private Transform target;

    private void Start()
    {
        if (!IsOwner) return;

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

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minY, maxY);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, desiredPosition, followSpeed * Time.deltaTime);
        mainCamera.transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
