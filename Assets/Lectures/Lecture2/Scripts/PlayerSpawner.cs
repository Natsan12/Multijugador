using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer || IsHost)
        {
            SpawnPlayerServerRpc(OwnerClientId);
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnPlayerServerRpc(ulong clientId)
    {
        GameObject playerInstance = Instantiate(playerPrefab);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
