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
        player1Text.text = $"Jugador 1: {player1Score.Value} puntos";
        player2Text.text = $"Jugador 2: {player2Score.Value} puntos";
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(ulong playerId)
    {
        if (playerId == 0)
            player1Score.Value++;
        else
            player2Score.Value++;
    }
}
