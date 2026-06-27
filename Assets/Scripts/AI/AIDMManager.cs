using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Aesheria.AI;

/// <summary>
/// AI DM 管理器 —— 控制开局旁白和中途随机触发
/// 单例，DontDestroyOnLoad
/// </summary>
public class AIDMManager : MonoBehaviour
{
    public static AIDMManager Instance { get; private set; }

    private const int CANVAS_SORTING_ORDER = 600;
    private const float MID_GAME_CHANCE = 0.25f;        // 中途触发概率
    private const float MID_GAME_COOLDOWN = 60f;         // 最短间隔（秒）

    private Canvas dmCanvas;
    private Image dmBg;
    private TextMeshProUGUI dmText;
    private AIDMClient aiClient;

    private bool hasShownOpening = false;
    private float lastMidGameTime = 0f;
    private bool isShowing = false;

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

        aiClient = GetComponent<AIDMClient>();
        if (aiClient == null)
            aiClient = gameObject.AddComponent<AIDMClient>();
    }

    void Start()
    {
        // 游戏启动后延迟触发开场旁白
        if (!hasShownOpening)
        {
            StartCoroutine(DelayedOpening());
        }
    }

    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("[AIDMManager]");
            go.AddComponent<AIDMClient>();
            go.AddComponent<AIDMManager>();
        }
    }

    private void SetupUI()
    {
        GameObject canvasObj = new GameObject("[AIDMCanvas]");
        canvasObj.transform.SetParent(transform, false);

        dmCanvas = canvasObj.AddComponent<Canvas>();
        dmCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dmCanvas.sortingOrder = CANVAS_SORTING_ORDER;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.SetActive(false);

        // 半透明全屏遮罩
        GameObject bgObj = new GameObject("DMBg");
        bgObj.transform.SetParent(canvasObj.transform, false);
        dmBg = bgObj.AddComponent<Image>();
        dmBg.color = new Color(0, 0, 0, 0.5f);
        dmBg.raycastTarget = false;
        RectTransform bgRt = dmBg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // 旁白文字（屏幕中上方）
        GameObject textObj = new GameObject("DMText");
        textObj.transform.SetParent(canvasObj.transform, false);
        dmText = textObj.AddComponent<TextMeshProUGUI>();
        dmText.font = TMPFontHelper.GetFont();
        dmText.fontSize = 30;
        dmText.color = new Color(0.95f, 0.92f, 0.82f, 0.95f);
        dmText.alignment = TextAlignmentOptions.Center;
        dmText.enableWordWrapping = true;
        dmText.overflowMode = TextOverflowModes.Overflow;
        dmText.raycastTarget = false;
        RectTransform textRt = dmText.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.1f, 0.55f);
        textRt.anchorMax = new Vector2(0.9f, 0.8f);
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 延迟触发开场旁白
    /// </summary>
    private IEnumerator DelayedOpening()
    {
        yield return new WaitForSeconds(2f);
        ShowOpening();
    }

    /// <summary>
    /// 显示开局旁白
    /// </summary>
    public void ShowOpening()
    {
        if (hasShownOpening) return;
        hasShownOpening = true;

        dmText.text = "灵脉的低语在远方回荡……";
        dmCanvas.gameObject.SetActive(true);

        aiClient.RequestOpeningNarration(
            (result) => { StartCoroutine(ShowText(result, 5f)); },
            (error) =>
            {
                Debug.LogWarning($"[AIDM] 开局旁白请求失败: {error}");
                StartCoroutine(ShowText("灵脉低语，繁华与枯寂同源。你的脚步将决定这片土地的命运。", 5f));
            }
        );
    }

    /// <summary>
    /// 尝试触发中途旁白（场景切换或对话结束时调用）
    /// </summary>
    public void TryMidGameCommentary()
    {
        if (!hasShownOpening) return;
        if (isShowing) return;
        if (Time.time - lastMidGameTime < MID_GAME_COOLDOWN) return;
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive()) return;

        if (Random.value > MID_GAME_CHANCE) return;

        lastMidGameTime = Time.time;

        aiClient.RequestMidGameCommentary(
            (result) => { StartCoroutine(ShowText(result, 3.5f)); },
            (error) =>
            {
                Debug.LogWarning($"[AIDM] 中途旁白请求失败: {error}");
            }
        );
    }

    /// <summary>
    /// 显示文字并淡入淡出
    /// </summary>
    private IEnumerator ShowText(string text, float holdTime)
    {
        isShowing = true;
        dmText.text = text;
        dmCanvas.gameObject.SetActive(true);

        // 淡入
        float fadeTime = 0.6f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float a = elapsed / fadeTime;
            dmBg.color = new Color(0, 0, 0, 0.5f * a);
            dmText.color = new Color(0.95f, 0.92f, 0.82f, 0.95f * a);
            yield return null;
        }
        dmBg.color = new Color(0, 0, 0, 0.5f);
        dmText.color = new Color(0.95f, 0.92f, 0.82f, 0.95f);

        // 持续
        yield return new WaitForSeconds(holdTime);

        // 淡出
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float a = 1f - elapsed / fadeTime;
            dmBg.color = new Color(0, 0, 0, 0.5f * a);
            dmText.color = new Color(0.95f, 0.92f, 0.82f, 0.95f * a);
            yield return null;
        }

        dmCanvas.gameObject.SetActive(false);
        isShowing = false;
    }
}
