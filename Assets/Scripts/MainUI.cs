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
    [SerializeField]
    private GameObject m_Game;

    private List<GameObject> m_Canvases;
    private Stack<GameObject> m_PreviousCanvas;
    private bool b_GameToggle;

    void Start() {
        InitializeCanvas();
        SwitchCanvas(m_MenuCanvas);
        b_GameToggle = false;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SwitchCanvas(m_MenuCanvas);
            ToggleGame();
        }
    }

    public void ToggleGame() {
        b_GameToggle = !b_GameToggle;
        m_Game.SetActive(b_GameToggle);
    }

    private void InitializeCanvas() {
        m_Canvases = new List<GameObject>();
        m_Canvases.Add(m_MenuCanvas);
        m_Canvases.Add(m_InstructionCanvas);
        m_Canvases.Add(m_CreditsCanvas);
        m_Canvases.Add(m_GameCanvas);

        m_PreviousCanvas = new Stack<GameObject>();
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

    public void ReturnToPrevious() {
        SwitchCanvas(m_PreviousCanvas.Peek());
        m_PreviousCanvas.Pop();
    }

    public void SetPreviousCanvas(GameObject PreviousCanvas) {
        m_PreviousCanvas.Push(PreviousCanvas);
    }

    public void QuitApplication() {
        Application.Quit();
    }
}
