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
        Debug.Log("🧠 GameManager cargado");
        if (Instance == null) Instance = this;
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
}
