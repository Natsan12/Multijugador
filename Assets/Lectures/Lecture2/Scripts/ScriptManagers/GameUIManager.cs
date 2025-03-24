using UnityEngine;
using TMPro;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    public GameObject startButtonObj;
    public TMP_Text messageText;
    public TMP_Text timerText; // ⏱️ NUEVO: campo para el cronómetro

    private void Awake()
    {
        Instance = this;
        messageText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart += MostrarInicio;
            GameManager.Instance.OnGameEnd += MostrarFinal;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart -= MostrarInicio;
            GameManager.Instance.OnGameEnd -= MostrarFinal;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.EstaPartidaActiva())
        {
            float tiempo = GameManager.Instance.GetTiempoRestante();
            ActualizarTimer(tiempo);
        }
    }

    private void ActualizarTimer(float segundos)
    {
        if (timerText == null) return;

        int minutos = Mathf.FloorToInt(segundos / 60f);
        int segundosRestantes = Mathf.FloorToInt(segundos % 60f);

        timerText.text = $"{minutos:00}:{segundosRestantes:00}";

        // Cambiar color a rojo si quedan 10 segundos o menos
        if (segundos <= 10f)
        {
            timerText.color = Color.red;
        }
        else
        {
            timerText.color = Color.white;
        }
    }

    private void MostrarInicio()
    {
        if (startButtonObj != null)
            startButtonObj.SetActive(false);

        StartCoroutine(MostrarMensajeTemporal("🎬 ¡La partida ha comenzado!", 3f, Color.cyan));
    }

    private void MostrarFinal()
    {
        StartCoroutine(MostrarMensajeTemporal("🏁 ¡La partida ha terminado!", 5f, Color.red));
    }

    public void MostrarGanador(string texto, Color color)
    {
        StartCoroutine(MostrarMensajeTemporal($"🏆 {texto}", 4f, color));
    }

    private IEnumerator MostrarMensajeTemporal(string mensaje, float duracion, Color color)
    {
        messageText.color = color;
        messageText.text = mensaje;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duracion);
        messageText.gameObject.SetActive(false);
    }
}
