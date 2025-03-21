using Unity.Netcode;
using UnityEngine;

namespace Lectures.Lecture2.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class SampleNetworkPlayer : NetworkBehaviour
    {
        private readonly NetworkVariable<Vector3> _position = new(
            writePerm: NetworkVariableWritePermission.Server);

        private Rigidbody _rigidbody;

        public override void OnNetworkSpawn()
        {
            _rigidbody = GetComponent<Rigidbody>();

            if (_rigidbody == null)
            {
                Debug.LogError("Rigidbody no encontrado en " + gameObject.name);
                return;
            }

            if (IsServer)
            {
                // Asignar una posición aleatoria inmediatamente
                SetRandomStartPositionServer();

                // Cuando un cliente se conecta, actualizar su posición
                NetworkManager.OnClientConnectedCallback += OnClientConnected;
            }
        }

        private void SetRandomStartPositionServer()
        {
            Vector3 randomPosition = GetRandomPositionOnPlane();
            _position.Value = randomPosition;
            _rigidbody.position = randomPosition; // Aplica en el servidor
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsServer)
            {
                // Reasignar posición a cada nuevo cliente para asegurarse de que reciba los datos
                SetRandomStartPositionServer();
            }
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-5f, 5f));
        }

        private void FixedUpdate()
        {
            if (_rigidbody == null) return;

            // Todos aplican la posición sincronizada desde el servidor
            _rigidbody.MovePosition(_position.Value);
        }
    }
}

/* private static Vector3 GetRandomPositionOnPlane()
 {
     return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
 }

 private void Update()
 {
     transform.position = _position.Value;
 }*/


