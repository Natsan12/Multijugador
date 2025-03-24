using Unity.Netcode;
using UnityEngine;
using TMPro;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;

    public TMP_Text player1Text;
    public TMP_Text player2Text;
    public Transform player1Transform;
    public Transform player2Transform;

    private NetworkVariable<int> player1Score = new NetworkVariable<int>(0);
    private NetworkVariable<int> player2Score = new NetworkVariable<int>(0);

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {
        // Mostrar puntajes en pantalla
        if (player1Text != null)
            player1Text.text = $"Jugador 1: {player1Score.Value} puntos";

        if (player2Text != null)
            player2Text.text = $"Jugador 2: {player2Score.Value} puntos";
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(ulong playerId)
    {
        if (GameManager.Instance == null || !GameManager.Instance.EstaPartidaActiva())
        {
            Debug.Log("⛔ No se puede sumar puntos: la partida no ha comenzado.");
            return;
        }

        if (playerId == 0)
            player1Score.Value++;
        else
            player2Score.Value++;

        int puntosActuales = playerId == 0 ? player1Score.Value : player2Score.Value;
        GameManager.Instance.RevisarPuntos(puntosActuales);
    }

    // ✅ Método público para obtener el puntaje actual
    public int GetScore(ulong playerId)
    {
        return playerId == 0 ? player1Score.Value : player2Score.Value;
    }
}
