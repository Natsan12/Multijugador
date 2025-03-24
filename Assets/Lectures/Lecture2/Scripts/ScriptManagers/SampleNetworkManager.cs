using System;
using Unity.Netcode;

using UnityEngine;

namespace Lectures.Lecture2.Scripts
{
    [Serializable]

    public enum ConnectionType
    {
        Host,
        Client,
        Server,
        Shutdown,
    }

    [RequireComponent(typeof(NetworkManager))]
    public class SampleNetworkManager : MonoBehaviour
    {
        public bool IsServer;
        private NetworkManager _networkManager;
        [SerializeField] private SampleNetworkTransform networkTransform;


        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
        }
        void Start()
        {
            if (IsServer)
            {
                StartHost(); // 👈 Esto activa host = servidor + cliente
                Debug.Log("✅ Este host está corriendo");
            }
        }

        #region Connection Methods

        public void CreateConnection(String connectionString)
        {
            ConnectionType connectionType = Enum.Parse<ConnectionType>(connectionString);


            if (_networkManager.IsClient || _networkManager.IsServer || _networkManager.IsHost)
            {
                connectionType = ConnectionType.Shutdown;
            }

            switch (connectionType)
            {
                case ConnectionType.Host:
                    StartHost();
                    break;

                case ConnectionType.Client:
                    StartClient();
                    break;
                case ConnectionType.Server:
                    StartServer();
                    break;
                case ConnectionType.Shutdown:
                    _networkManager.Shutdown();
                    break;

            }
        }
        private void StartHost()
        {
            _networkManager.StartHost();
        }

        private void StartClient()
        {
            _networkManager.StartClient();
        }

        private void StartServer()
        {
            Instantiate(networkTransform);
            _networkManager.StartServer();
        }
        #endregion
    }
}