using Unity.Netcode;
using UnityEngine;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public event Action OnGameStart;
    public event Action OnGameEnd;

    [Header("Configuración de Partida")]
    public int puntosParaGanar = 10;
    public float tiempoLimite = 60f;

    private float tiempoRestante;
    private bool partidaActiva = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        Debug.Log("🧠 GameManager cargado");
    }

    private void Update()
    {
        if (!IsServer || !partidaActiva) return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0)
        {
            FinalizarPartida();
        }
    }

    public void IniciarPartida()
    {
        if (!IsServer) return;

        Debug.Log("🎬 La partida ha comenzado");

        tiempoRestante = tiempoLimite;
        partidaActiva = true;

        OnGameStart?.Invoke();
    }

    public void FinalizarPartida()
    {
        if (!IsServer || !partidaActiva) return;

        Debug.Log("🏁 ¡La partida ha finalizado!");

        partidaActiva = false;
        OnGameEnd?.Invoke();

        Invoke(nameof(MostrarGanador), 4.5f); // Mostrar después del mensaje final
    }

    private void MostrarGanador()
    {
        string mensajeGanador = DeterminarGanador();
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.MostrarGanador(mensajeGanador, Color.green);
        }
    }

    public bool EstaPartidaActiva()
    {
        return partidaActiva;
    }

    public void RevisarPuntos(int score)
    {
        if (score >= puntosParaGanar)
        {
            FinalizarPartida();
        }
    }

    private string DeterminarGanador()
    {
        int score1 = ScoreManager.Instance.GetScore(0);
        int score2 = ScoreManager.Instance.GetScore(1);

        if (score1 > score2) return "Ganó el Jugador 1";
        if (score2 > score1) return "Ganó el Jugador 2";
        return "¡Empate!";
    }
}
