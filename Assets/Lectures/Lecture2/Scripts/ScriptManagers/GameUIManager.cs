using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    public GameObject startButtonObj;
    public TMP_Text messageText;

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

    private void MostrarInicio()
    {
        if (startButtonObj != null)
            startButtonObj.SetActive(false);

        StartCoroutine(MostrarMensajeTemporal("🎬 ¡La partida ha comenzado!", 3f));
    }

    private void MostrarFinal()
    {
        StartCoroutine(MostrarMensajeTemporal("🏁 ¡La partida ha terminado!", 5f));
    }

    private IEnumerator MostrarMensajeTemporal(string mensaje, float duracion)
    {
        messageText.text = mensaje;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duracion);
        messageText.gameObject.SetActive(false);
    }
}
