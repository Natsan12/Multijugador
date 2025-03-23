using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;

    private NetworkVariable<int> player1Score = new NetworkVariable<int>();
    private NetworkVariable<int> player2Score = new NetworkVariable<int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void AddPointToPlayer(ulong clientId)
    {
        if (clientId == 0)
            player1Score.Value++;
        else if (clientId == 1)
            player2Score.Value++;

        Debug.Log($"🎯 Puntos actualizados: P1={player1Score.Value}, P2={player2Score.Value}");
    }

    public int GetScore(ulong clientId)
    {
        return clientId == 0 ? player1Score.Value : player2Score.Value;
    }

    public int GetPlayer1Score() => player1Score.Value;
    public int GetPlayer2Score() => player2Score.Value;
}
