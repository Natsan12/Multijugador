using Unity.Netcode;
using UnityEngine;

namespace Lectures.Lecture2.Scripts
{
    public class SampleNetworkTransform : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        private void Update()
        {
            if (IsOwner)
            {
                Position.Value = transform.position;
            }
            else
            {
                transform.position = Position.Value;
            }
        }
    }
}
