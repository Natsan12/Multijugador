// EntregaZone.cs
using UnityEngine;
using Unity.Netcode;

public class EntregaZone : NetworkBehaviour
{
    public Transform puntoDeColocacion;
    private int puntos = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        Debug.Log($"[EntregaZone] Trigger detectado con: {other.name}");

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.HasBall())
        {
            BallPickup balon = player.GetCarriedBall();

            // Validación adicional
            if (balon != null && !balon.IsBeingHeld())
            {
                Debug.Log("[EntregaZone] ✅ El jugador ya soltó el balón.");

                puntos++;
                Debug.Log($"🎯 Balón entregado correctamente. Puntos: {puntos}");

                balon.transform.SetParent(null);
                balon.transform.position = puntoDeColocacion.position;
                balon.transform.rotation = puntoDeColocacion.rotation;
                balon.PrepararComoEntregado();

                player.ClearBall();
            }
            else
            {
                Debug.Log("[EntregaZone] ⚠️ El jugador todavía tiene el balón en la mano. No se entrega.");
            }

        }
    }
}


