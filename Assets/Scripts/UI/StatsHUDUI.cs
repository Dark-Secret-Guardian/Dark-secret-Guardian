using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 四维数值 HUD —— 左上角常驻显示繁荣/生态/觉醒/守护进度条
/// 单例，DontDestroyOnLoad，订阅 GameManager.OnValuesChanged 自动刷新
/// </summary>
public class StatsHUDUI : MonoBehaviour
{
    public static StatsHUDUI Instance { get; private set; }

    private const int CANVAS_SORTING_ORDER = 400;

    // 配色
    private static readonly Color PROSPERITY_COLOR = new Color(0.95f, 0.78f, 0.35f); // 金黄
    private static readonly Color ECOLOGY_COLOR = new Color(0.35f, 0.80f, 0.45f);    // 翠绿
    private static readonly Color AWAKENING_COLOR = new Color(0.45f, 0.65f, 0.95f);  // 天蓝
    private static readonly Color GUARDIANSHIP_COLOR = new Color(0.85f, 0.45f, 0.65f); // 玫红
    private static readonly Color BAR_BG = new Color(0.08f, 0.07f, 0.06f, 0.8f);
    private static readonly Color PANEL_BG = new Color(0.1f, 0.09f, 0.08f, 0.7f);
    private static readonly Color TEXT_COLOR = new Color(0.92f, 0.88f, 0.80f);

    private Canvas hudCanvas;
    private Image[] barFills = new Image[4];
    private TextMeshProUGUI[] valueTexts = new TextMeshProUGUI[4];

    // 用于数值跳变动画
    private int[] displayValues = { -1, -1, -1, -1 };
    private Coroutine[] animCoroutines = new Coroutine[4];
    private TextMeshProUGUI crystalText;
    private int displayCrystals = -1;

    private string[] statNames = { "繁荣", "生态", "觉醒", "守护" };
    private Color[] statColors = { PROSPERITY_COLOR, ECOLOGY_COLOR, AWAKENING_COLOR, GUARDIANSHIP_COLOR };

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

    void Start()
    {
        // 订阅 GameManager 数值变化事件
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.OnValuesChanged += Refresh;
            Debug.Log("[StatsHUD] 已订阅 GameManager.OnValuesChanged");
        }
        Refresh();
    }

    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("[StatsHUD]");
            go.AddComponent<StatsHUDUI>();
        }
    }

    private void SetupUI()
    {
        GameObject canvasObj = new GameObject("[StatsHUDCanvas]");
        canvasObj.transform.SetParent(transform, false);

        hudCanvas = canvasObj.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = CANVAS_SORTING_ORDER;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 面板背景
        GameObject panelObj = new GameObject("HUDPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = PANEL_BG;
        RectTransform panelRt = panelBg.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0, 1);
        panelRt.anchorMax = new Vector2(0, 1);
        panelRt.pivot = new Vector2(0, 1);
        panelRt.anchoredPosition = new Vector2(15, -15);
        panelRt.sizeDelta = new Vector2(280, 190);

        // 4 条数值条
        float barHeight = 28f;
        float barSpacing = 8f;
        float barWidth = 240f;
        float startX = 20f;
        float startY = -18f;

        for (int i = 0; i < 4; i++)
        {
            float y = startY - i * (barHeight + barSpacing);

            // 名称
            GameObject nameObj = new GameObject("Label_" + i);
            nameObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.font = TMPFontHelper.GetFont();
            nameText.fontSize = 18;
            nameText.color = statColors[i];
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.enableWordWrapping = false;
            nameText.text = statNames[i];
            RectTransform nameRt = nameText.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 1);
            nameRt.anchorMax = new Vector2(0, 1);
            nameRt.pivot = new Vector2(0, 1);
            nameRt.anchoredPosition = new Vector2(startX, y);
            nameRt.sizeDelta = new Vector2(50, barHeight);

            // 数值文字
            GameObject valObj = new GameObject("Value_" + i);
            valObj.transform.SetParent(panelObj.transform, false);
            valueTexts[i] = valObj.AddComponent<TextMeshProUGUI>();
            valueTexts[i].font = TMPFontHelper.GetFont();
            valueTexts[i].fontSize = 18;
            valueTexts[i].color = TEXT_COLOR;
            valueTexts[i].alignment = TextAlignmentOptions.Right;
            valueTexts[i].enableWordWrapping = false;
            valueTexts[i].text = "0";
            RectTransform valRt = valueTexts[i].GetComponent<RectTransform>();
            valRt.anchorMin = new Vector2(1, 1);
            valRt.anchorMax = new Vector2(1, 1);
            valRt.pivot = new Vector2(1, 1);
            valRt.anchoredPosition = new Vector2(-20, y);
            valRt.sizeDelta = new Vector2(60, barHeight);

            // 进度条背景
            GameObject barBgObj = new GameObject("BarBg_" + i);
            barBgObj.transform.SetParent(panelObj.transform, false);
            Image barBgImg = barBgObj.AddComponent<Image>();
            barBgImg.color = BAR_BG;
            RectTransform barBgRt = barBgImg.GetComponent<RectTransform>();
            barBgRt.anchorMin = new Vector2(0, 1);
            barBgRt.anchorMax = new Vector2(0, 1);
            barBgRt.pivot = new Vector2(0, 1);
            barBgRt.anchoredPosition = new Vector2(startX + 55, y);
            barBgRt.sizeDelta = new Vector2(barWidth - 55, barHeight - 6);

            // 进度条填充
            GameObject barFillObj = new GameObject("BarFill_" + i);
            barFillObj.transform.SetParent(barBgObj.transform, false);
            barFills[i] = barFillObj.AddComponent<Image>();
            barFills[i].color = statColors[i];
            barFills[i].raycastTarget = false;
            RectTransform barFillRt = barFills[i].GetComponent<RectTransform>();
            barFillRt.anchorMin = Vector2.zero;
            barFillRt.anchorMax = Vector2.one;
            barFillRt.pivot = new Vector2(0, 0.5f);
            barFillRt.offsetMin = Vector2.zero;
            barFillRt.offsetMax = Vector2.zero;
            barFillRt.localScale = new Vector3(0f, 1f, 1f); // 初始 0%
        }

        // 灵晶数量显示（面板底部）
        GameObject crystalObj = new GameObject("CrystalLabel");
        crystalObj.transform.SetParent(panelObj.transform, false);
        crystalText = crystalObj.AddComponent<TextMeshProUGUI>();
        crystalText.font = TMPFontHelper.GetFont();
        crystalText.fontSize = 22;
        crystalText.color = new Color(0.6f, 0.9f, 1f);
        crystalText.fontStyle = FontStyles.Bold;
        crystalText.alignment = TextAlignmentOptions.Center;
        crystalText.enableWordWrapping = false;
        crystalText.text = "◆ 灵晶: 0";
        RectTransform crystalRt = crystalText.GetComponent<RectTransform>();
        crystalRt.anchorMin = new Vector2(0, 0);
        crystalRt.anchorMax = new Vector2(1, 0);
        crystalRt.pivot = new Vector2(0.5f, 0);
        crystalRt.anchoredPosition = new Vector2(0, 8);
        crystalRt.sizeDelta = new Vector2(0, 30);
    }

    /// <summary>
    /// 刷新 HUD 显示（由 GameManager.OnValuesChanged 触发）
    /// </summary>
    private void Refresh()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        int[] values = { gm.prosperity, gm.ecology, gm.awakening, gm.guardianship };

        for (int i = 0; i < 4; i++)
        {
            if (displayValues[i] != values[i])
            {
                // 取消旧动画
                if (animCoroutines[i] != null)
                    StopCoroutine(animCoroutines[i]);
                animCoroutines[i] = StartCoroutine(AnimateBar(i, displayValues[i] < 0 ? 0 : displayValues[i], values[i]));
                displayValues[i] = values[i];
            }
        }

        // 更新灵晶显示
        if (crystalText != null && displayCrystals != gm.spiritCrystals)
        {
            crystalText.text = $"◆ 灵晶: {gm.spiritCrystals}";
            displayCrystals = gm.spiritCrystals;
        }
    }

    /// <summary>
    /// 进度条跳动动画
    /// </summary>
    private IEnumerator AnimateBar(int index, int fromVal, int toVal)
    {
        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // ease-out
            t = 1f - (1f - t) * (1f - t);

            int currentVal = Mathf.RoundToInt(Mathf.Lerp(fromVal, toVal, t));
            float fillAmount = currentVal / 100f;

            if (barFills[index] != null)
                barFills[index].transform.localScale = new Vector3(fillAmount, 1f, 1f);
            if (valueTexts[index] != null)
                valueTexts[index].text = currentVal.ToString();

            yield return null;
        }

        // 确保最终值精确
        if (barFills[index] != null)
            barFills[index].transform.localScale = new Vector3(toVal / 100f, 1f, 1f);
        if (valueTexts[index] != null)
            valueTexts[index].text = toVal.ToString();
    }
}
