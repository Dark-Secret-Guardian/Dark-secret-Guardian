using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 任务追踪 UI —— 右上角显示当前任务提示
/// 单例，DontDestroyOnLoad
/// </summary>
public class QuestTrackerUI : MonoBehaviour
{
    public static QuestTrackerUI Instance { get; private set; }

    private const int CANVAS_SORTING_ORDER = 500;

    private Canvas trackerCanvas;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descText;
    private Image panelBg;

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
            GameObject go = new GameObject("[QuestTrackerUI]");
            go.AddComponent<QuestTrackerUI>();
        }
    }

    private void SetupUI()
    {
        GameObject canvasObj = new GameObject("[QuestTrackerCanvas]");
        canvasObj.transform.SetParent(transform, false);

        trackerCanvas = canvasObj.AddComponent<Canvas>();
        trackerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        trackerCanvas.sortingOrder = CANVAS_SORTING_ORDER;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.SetActive(false);

        // 面板背景（半透明深色，古风边框色调）
        GameObject panelObj = new GameObject("TrackerPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.12f, 0.10f, 0.08f, 0.85f);
        RectTransform panelRt = panelBg.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(1, 1);
        panelRt.anchorMax = new Vector2(1, 1);
        panelRt.pivot = new Vector2(1, 1);
        panelRt.anchoredPosition = new Vector2(-20, -20);
        panelRt.sizeDelta = new Vector2(380, 100);

        // 标题
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.font = TMPFontHelper.GetFont();
        titleText.fontSize = 22;
        titleText.color = new Color(0.95f, 0.82f, 0.55f);
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.TopLeft;
        titleText.enableWordWrapping = true;
        titleText.overflowMode = TextOverflowModes.Ellipsis;
        RectTransform titleRt = titleText.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0, 1);
        titleRt.offsetMin = new Vector2(15, -35);
        titleRt.offsetMax = new Vector2(-15, -8);

        // 描述
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(panelObj.transform, false);
        descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.font = TMPFontHelper.GetFont();
        descText.fontSize = 18;
        descText.color = new Color(0.88f, 0.85f, 0.78f);
        descText.alignment = TextAlignmentOptions.TopLeft;
        descText.enableWordWrapping = true;
        descText.overflowMode = TextOverflowModes.Overflow;
        RectTransform descRt = descText.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0, 0);
        descRt.anchorMax = new Vector2(1, 1);
        descRt.pivot = new Vector2(0, 0);
        descRt.offsetMin = new Vector2(15, 10);
        descRt.offsetMax = new Vector2(-15, -40);
    }

    /// <summary>
    /// 显示任务追踪
    /// </summary>
    public void ShowQuest(string title, string description)
    {
        EnsureExists();
        if (Instance != this) { Instance.ShowQuest(title, description); return; }

        titleText.text = "▲ " + title;
        descText.text = description;
        trackerCanvas.gameObject.SetActive(true);
        Debug.Log($"[QuestTracker] 显示任务: {title}");
    }

    /// <summary>
    /// 隐藏任务追踪
    /// </summary>
    public void HideQuest()
    {
        if (Instance != this) { Instance?.HideQuest(); return; }
        trackerCanvas.gameObject.SetActive(false);
        Debug.Log("[QuestTracker] 任务已隐藏");
    }
}
