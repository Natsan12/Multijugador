using Unity.Netcode;
using UnityEngine;

public class EntregaZone : NetworkBehaviour
{
    public Transform[] placementSpots; // Posiciones donde colocar los balones entregados
    private int currentSpotIndex = 0;

    // 🚫 No se usa OnTriggerEnter, entrega manual

    public void EntregarBalon(BallPickup ball)
    {
        if (!IsServer) return;

        ball.Deliver();

        // Posicionar balón en el contenedor visualmente
        Vector3 targetPos = placementSpots.Length > 0 && currentSpotIndex < placementSpots.Length
            ? placementSpots[currentSpotIndex].position
            : transform.position + Vector3.up * 0.5f;

        Quaternion targetRot = placementSpots.Length > 0 && currentSpotIndex < placementSpots.Length
            ? placementSpots[currentSpotIndex].rotation
            : Quaternion.identity;

        // 🧩 Opcional: "pegar" el balón al punto (parentarlo si lo necesitas totalmente fijo)
        ball.transform.SetParent(null); // o: ball.transform.SetParent(placementSpots[currentSpotIndex]);

        // 🔄 Sincronizar la posición con todos los clientes
        ball.SetBallPositionClientRpc(targetPos, targetRot);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.detectCollisions = false;

        currentSpotIndex++;

        Debug.Log("📦 Balón entregado manualmente");
    }
}
