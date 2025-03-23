using UnityEngine;
using Unity.Netcode;

public class ThirdPersonCameraController : NetworkBehaviour
{
    private Camera mainCamera;

    [Header("C�mara")]
    public Vector3 offset = new Vector3(0f, 2f, -6f);
    public float followSpeed = 10f;

    [Header("Rotaci�n")]
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
            Debug.LogError("No se encontr� la c�mara principal.");
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

        // 1. Leer movimiento del mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minY, maxY);

        // 2. Calcular rotaci�n
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        // 3. Calcular posici�n deseada
        Vector3 desiredPosition = target.position + rotation * offset;

        // 4. Aplicar posici�n suavizada
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // 5. Hacer que mire hacia el jugador (ligeramente elevado)
        mainCamera.transform.LookAt(target.position + Vector3.up * 1.5f);
    }

}
