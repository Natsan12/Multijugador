using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ScoreUIInitializer : MonoBehaviour
{
    public ScoreManager scoreManager;

    private void Start()
    {
        // Canvas
        GameObject canvasGO = new GameObject("ScoreCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel contenedor en esquina superior derecha
        GameObject panelGO = new GameObject("ScorePanel");
        panelGO.transform.SetParent(canvasGO.transform);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(1, 1);
        panelRT.anchorMax = new Vector2(1, 1);
        panelRT.pivot = new Vector2(1, 1);
        panelRT.anchoredPosition = new Vector2(-20, -20);
        panelRT.sizeDelta = new Vector2(300, 100);

        var image = panelGO.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.35f);

        // Crear texto Jugador 1
        GameObject text1GO = new GameObject("Player1Text");
        text1GO.transform.SetParent(panelGO.transform);
        var text1 = text1GO.AddComponent<TextMeshProUGUI>();
        text1.fontSize = 24;
        text1.color = new Color32(100, 180, 255, 255); // Azul claro
        text1.alignment = TextAlignmentOptions.TopRight;
        text1.text = "Jugador 1: 0 puntos";
        var rt1 = text1.GetComponent<RectTransform>();
        rt1.anchorMin = new Vector2(0, 1);
        rt1.anchorMax = new Vector2(1, 1);
        rt1.pivot = new Vector2(1, 1);
        rt1.anchoredPosition = new Vector2(-10, -10);
        rt1.sizeDelta = new Vector2(280, 40);

        // Crear texto Jugador 2
        GameObject text2GO = new GameObject("Player2Text");
        text2GO.transform.SetParent(panelGO.transform);
        var text2 = text2GO.AddComponent<TextMeshProUGUI>();
        text2.fontSize = 24;
        text2.color = new Color32(100, 255, 100, 255); // Verde claro
        text2.alignment = TextAlignmentOptions.TopRight;
        text2.text = "Jugador 2: 0 puntos";
        var rt2 = text2.GetComponent<RectTransform>();
        rt2.anchorMin = new Vector2(0, 1);
        rt2.anchorMax = new Vector2(1, 1);
        rt2.pivot = new Vector2(1, 1);
        rt2.anchoredPosition = new Vector2(-10, -50);
        rt2.sizeDelta = new Vector2(280, 40);

        // Asignar al ScoreManager
        if (scoreManager != null)
        {
            scoreManager.player1Text = text1;
            scoreManager.player2Text = text2;

            // Asignar referencias para animación
            scoreManager.player1Transform = text1.transform;
            scoreManager.player2Transform = text2.transform;
        }
    }
}
