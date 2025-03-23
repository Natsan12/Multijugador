using UnityEngine;
using Unity.Netcode;

public class BallSpawner : NetworkBehaviour
{
    void Start()
    {
        if (!IsServer) return;

        Debug.Log("🧠 [Host] Spawneando manualmente todos los balones de la escena");

        BallPickup[] balones = Object.FindObjectsByType<BallPickup>(FindObjectsSortMode.None);

        foreach (var balon in balones)
        {
            var netObj = balon.GetComponent<NetworkObject>();

            if (!netObj.IsSpawned)
            {
                Debug.Log($"🟢 Spawn forzado de: {balon.gameObject.name}");
                netObj.Spawn(true);
            }
            else
            {
                Debug.Log($"🟡 Ya estaba spawneado: {balon.gameObject.name}");
            }
        }
    }
}
