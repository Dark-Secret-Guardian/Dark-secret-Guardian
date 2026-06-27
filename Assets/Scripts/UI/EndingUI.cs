using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 结局 UI —— 纯代码创建，单例，DontDestroyOnLoad
/// 显示结局标题、描述、色调背景，提供重新开始和退出按钮
/// </summary>
public class EndingUI : MonoBehaviour
{
    public static EndingUI Instance { get; private set; }

    private const int CANVAS_SORTING_ORDER = 2000;

    private Canvas endingCanvas;
    private Image bgImage;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descText;
    private Button restartButton;
    private Button quitButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupUI();
    }

    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("[EndingUI]");
            go.AddComponent<EndingUI>();
        }
    }

    private void SetupUI()
    {
        GameObject canvasObj = new GameObject("[EndingCanvas]");
        canvasObj.transform.SetParent(transform, false);

        endingCanvas = canvasObj.AddComponent<Canvas>();
        endingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        endingCanvas.sortingOrder = CANVAS_SORTING_ORDER;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.SetActive(false);

        // 全屏背景（结局色调）
        bgImage = canvasObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        RectTransform bgRt = bgImage.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // 标题
        GameObject titleObj = new GameObject("EndingTitle");
        titleObj.transform.SetParent(canvasObj.transform, false);
        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.font = TMPFontHelper.GetFont();
        titleText.fontSize = 56;
        titleText.color = new Color(0.95f, 0.85f, 0.6f);
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.enableWordWrapping = false;
        RectTransform titleRt = titleText.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.1f, 0.65f);
        titleRt.anchorMax = new Vector2(0.9f, 0.8f);
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;

        // 描述
        GameObject descObj = new GameObject("EndingDesc");
        descObj.transform.SetParent(canvasObj.transform, false);
        descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.font = TMPFontHelper.GetFont();
        descText.fontSize = 28;
        descText.color = new Color(0.88f, 0.85f, 0.78f);
        descText.alignment = TextAlignmentOptions.Center;
        descText.enableWordWrapping = true;
        descText.overflowMode = TextOverflowModes.Overflow;
        descText.lineSpacing = 1.6f;
        RectTransform descRt = descText.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.15f, 0.25f);
        descRt.anchorMax = new Vector2(0.85f, 0.6f);
        descRt.offsetMin = Vector2.zero;
        descRt.offsetMax = Vector2.zero;

        // 重新开始按钮
        GameObject restartObj = new GameObject("RestartBtn");
        restartObj.transform.SetParent(canvasObj.transform, false);
        restartButton = restartObj.AddComponent<Button>();
        Image restartBg = restartObj.AddComponent<Image>();
        restartBg.color = new Color(0.3f, 0.45f, 0.3f, 0.9f);
        RectTransform restartRt = restartBg.GetComponent<RectTransform>();
        restartRt.anchorMin = new Vector2(0.35f, 0.1f);
        restartRt.anchorMax = new Vector2(0.48f, 0.18f);
        restartRt.offsetMin = Vector2.zero;
        restartRt.offsetMax = Vector2.zero;

        GameObject restartTextObj = new GameObject("Text");
        restartTextObj.transform.SetParent(restartObj.transform, false);
        TextMeshProUGUI restartText = restartTextObj.AddComponent<TextMeshProUGUI>();
        restartText.font = TMPFontHelper.GetFont();
        restartText.fontSize = 26;
        restartText.color = Color.white;
        restartText.alignment = TextAlignmentOptions.Center;
        restartText.enableWordWrapping = false;
        restartText.text = "重新开始";
        RectTransform restartTextRt = restartText.GetComponent<RectTransform>();
        restartTextRt.anchorMin = Vector2.zero;
        restartTextRt.anchorMax = Vector2.one;
        restartTextRt.offsetMin = Vector2.zero;
        restartTextRt.offsetMax = Vector2.zero;

        var restartColors = restartButton.colors;
        restartColors.highlightedColor = new Color(0.35f, 0.5f, 0.35f, 0.9f);
        restartColors.pressedColor = new Color(0.2f, 0.3f, 0.2f, 0.9f);
        restartButton.colors = restartColors;
        restartButton.onClick.AddListener(RestartGame);

        // 退出按钮
        GameObject quitObj = new GameObject("QuitBtn");
        quitObj.transform.SetParent(canvasObj.transform, false);
        quitButton = quitObj.AddComponent<Button>();
        Image quitBg = quitObj.AddComponent<Image>();
        quitBg.color = new Color(0.5f, 0.3f, 0.3f, 0.9f);
        RectTransform quitRt = quitBg.GetComponent<RectTransform>();
        quitRt.anchorMin = new Vector2(0.52f, 0.1f);
        quitRt.anchorMax = new Vector2(0.65f, 0.18f);
        quitRt.offsetMin = Vector2.zero;
        quitRt.offsetMax = Vector2.zero;

        GameObject quitTextObj = new GameObject("Text");
        quitTextObj.transform.SetParent(quitObj.transform, false);
        TextMeshProUGUI quitText = quitTextObj.AddComponent<TextMeshProUGUI>();
        quitText.font = TMPFontHelper.GetFont();
        quitText.fontSize = 26;
        quitText.color = Color.white;
        quitText.alignment = TextAlignmentOptions.Center;
        quitText.enableWordWrapping = false;
        quitText.text = "退出游戏";
        RectTransform quitTextRt = quitText.GetComponent<RectTransform>();
        quitTextRt.anchorMin = Vector2.zero;
        quitTextRt.anchorMax = Vector2.one;
        quitTextRt.offsetMin = Vector2.zero;
        quitTextRt.offsetMax = Vector2.zero;

        var quitColors = quitButton.colors;
        quitColors.highlightedColor = new Color(0.55f, 0.35f, 0.35f, 0.9f);
        quitColors.pressedColor = new Color(0.35f, 0.2f, 0.2f, 0.9f);
        quitButton.colors = quitColors;
        quitButton.onClick.AddListener(QuitGame);
    }

    public void ShowEnding(string title, string description)
    {
        ShowEnding(title, description, new Color(0.1f, 0.1f, 0.12f, 0.95f));
    }

    public void ShowEnding(string title, string description, Color bgColor)
    {
        EnsureExists();
        if (Instance != this) { Instance.ShowEnding(title, description, bgColor); return; }

        titleText.text = title;
        descText.text = description;
        bgImage.color = bgColor;
        endingCanvas.gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    void RestartGame()
    {
        Time.timeScale = 1;
        QuestState.ClearAll();
        SceneManager.LoadScene(0);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
