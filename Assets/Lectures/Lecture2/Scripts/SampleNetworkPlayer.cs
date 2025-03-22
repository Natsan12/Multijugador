using Unity.Netcode;

using UnityEngine;

namespace Lectures.Lecture2.Scripts
{
    public class SampleNetworkPlayer : NetworkBehaviour
    {
        private readonly NetworkVariable<Vector3> _position = new(
            writePerm: NetworkVariableWritePermission.Server
        );

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Move();
            }

            // Sincronizar posici�n correctamente en todos los clientes
            _position.OnValueChanged += (oldValue, newValue) => transform.position = newValue;

            // Si el cliente no es el due�o del objeto, forzar la posici�n correcta al conectarse
            if (!IsOwner)
            {
                transform.position = _position.Value;
            }

            // Ajustar Rigidbody seg�n si es el due�o o no
            if (IsServer)
            {
                rb.isKinematic = false; // Permitir f�sicas en el servidor
                rb.useGravity = true;   // Aplicar gravedad en el servidor
            }
            else
            {
                rb.isKinematic = true;  // Clientes desactivan f�sicas para evitar errores
                rb.useGravity = false;  // Desactivar gravedad en clientes
            }
        }

        public void Move()
        {
            if (IsOwner) // Solo el due�o del objeto puede solicitar movimiento
            {
                SubmitPositionRequestRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void SubmitPositionRequestRpc(RpcParams rpcParams = default)
        {
            if (!IsServer) return; // Solo el servidor debe manejar la l�gica de movimiento

            Vector3 randomPosition = GetRandomPositionOnPlane();
            _position.Value = randomPosition; // Se sincroniza autom�ticamente en todos los clientes
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            float groundY = 0.1f; // Ajusta esto seg�n la altura exacta del suelo
            return new Vector3(Random.Range(-3f, 3f), groundY, Random.Range(-3f, 3f));
        }
    }
}
