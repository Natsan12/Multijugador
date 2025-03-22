using UnityEngine;

public class DropZone : MonoBehaviour
{
    private int score = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickable") && other.transform.parent == null)
        {
            score++;
            Debug.Log("¡Objeto entregado! Puntos: " + score);
            Destroy(other.gameObject);
        }
    }
}
