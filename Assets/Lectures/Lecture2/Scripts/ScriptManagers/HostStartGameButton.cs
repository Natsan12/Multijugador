using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class HostStartGameButton : MonoBehaviour
{
    public Button startButton;

    private IEnumerator Start()
    {
        // Espera a que NetworkManager esté inicializado y GameManager esté listo
        yield return new WaitUntil(() => NetworkManager.Singleton != null && GameManager.Instance != null);

        // Espera a que la red esté en modo activo (Host/Server/Client)
        yield return new WaitUntil(() => NetworkManager.Singleton.IsListening);

        Debug.Log($"[DEBUG] FINAL: IsHost: {NetworkManager.Singleton.IsHost}, IsServer: {NetworkManager.Singleton.IsServer}");

        if (!NetworkManager.Singleton.IsServer)
        {
            startButton.gameObject.SetActive(false);
        }
        else
        {
            startButton.gameObject.SetActive(true);
            startButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null && NetworkManager.Singleton.IsServer)
                {
                    Debug.Log("🎬 ¡Botón presionado! Iniciando partida...");
                    GameManager.Instance.IniciarPartida();
                    startButton.gameObject.SetActive(false);
                }
            });
        }
    }
}
