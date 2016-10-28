using UnityEngine;
using System.Collections.Generic;

public class MainUI : MonoBehaviour {

    [SerializeField]
    private GameObject m_MenuCanvas;
    [SerializeField]
    private GameObject m_InstructionCanvas;
    [SerializeField]
    private GameObject m_CreditsCanvas;
    [SerializeField]
    private GameObject m_GameCanvas;

    private List<GameObject> m_Canvases;
    private GameObject m_PreviousCanvas;

    void Start() {
        InitializeCanvas();
        SwitchCanvas(m_MenuCanvas);
    }

    private void InitializeCanvas() {
        m_Canvases = new List<GameObject>();
        m_Canvases.Add(m_MenuCanvas);
        m_Canvases.Add(m_InstructionCanvas);
        m_Canvases.Add(m_CreditsCanvas);
        m_Canvases.Add(m_GameCanvas);
    }

    private void ResetCanvasAnchors(GameObject CurrentCanvas) {
        RectTransform canvasRect = CurrentCanvas.GetComponent<RectTransform>();
        canvasRect.localPosition = Vector3.zero;
        canvasRect.offsetMax = Vector2.zero;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
    }

    public void SwitchCanvas(GameObject NextCanvas) {
        foreach (GameObject Canvas in m_Canvases) {
            if (Canvas != NextCanvas) {
                Canvas.SetActive(false);
            } else {
                Canvas.SetActive(true);
                ResetCanvasAnchors(Canvas);
            }
        }
    }

    public void SetPreviousCanvas(GameObject PreviousCanvas) {
        m_PreviousCanvas = PreviousCanvas;
    }

    public void QuitApplication() {
        Application.Quit();
    }
}
