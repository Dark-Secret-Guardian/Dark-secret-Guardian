using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 浮动提示 —— 画面正中下方显示获得物品/数值变化，3秒后淡出
/// 单例，DontDestroyOnLoad
/// </summary>
public class FloatingNotification : MonoBehaviour
{
    public static FloatingNotification Instance { get; private set; }

    private const int CANVAS_SORTING_ORDER = 700;

    private Canvas notifCanvas;
    private TextMeshProUGUI notifText;
    private Image notifBg;

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
            GameObject go = new GameObject("[FloatingNotification]");
            go.AddComponent<FloatingNotification>();
        }
    }

    private void SetupUI()
    {
        GameObject canvasObj = new GameObject("[NotifCanvas]");
        canvasObj.transform.SetParent(transform, false);

        notifCanvas = canvasObj.AddComponent<Canvas>();
        notifCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        notifCanvas.sortingOrder = CANVAS_SORTING_ORDER;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.SetActive(false);

        // 背景
        GameObject bgObj = new GameObject("NotifBg");
        bgObj.transform.SetParent(canvasObj.transform, false);
        notifBg = bgObj.AddComponent<Image>();
        notifBg.color = new Color(0.12f, 0.10f, 0.08f, 0.85f);
        RectTransform bgRt = notifBg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.5f, 0);
        bgRt.anchorMax = new Vector2(0.5f, 0);
        bgRt.pivot = new Vector2(0.5f, 0);
        bgRt.anchoredPosition = new Vector2(0, 60);
        bgRt.sizeDelta = new Vector2(600, 50);

        // 文字
        GameObject textObj = new GameObject("NotifText");
        textObj.transform.SetParent(bgObj.transform, false);
        notifText = textObj.AddComponent<TextMeshProUGUI>();
        notifText.font = TMPFontHelper.GetFont();
        notifText.fontSize = 26;
        notifText.color = new Color(0.98f, 0.92f, 0.76f);
        notifText.alignment = TextAlignmentOptions.Center;
        notifText.enableWordWrapping = false;
        notifText.overflowMode = TextOverflowModes.Overflow;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(20, 5);
        textRt.offsetMax = new Vector2(-20, -5);
    }

    /// <summary>
    /// 显示提示文字，3秒后淡出
    /// </summary>
    public void Show(string message)
    {
        EnsureExists();
        if (Instance != this) { Instance.Show(message); return; }

        notifText.text = message;
        notifCanvas.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ShowAndFade());
    }

    /// <summary>
    /// 显示多条提示（合并为一行）
    /// </summary>
    public void Show(List<string> messages)
    {
        if (messages == null || messages.Count == 0) return;
        Show(string.Join("    ", messages));
    }

    private IEnumerator ShowAndFade()
    {
        // 淡入
        float fadeTime = 0.3f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float a = elapsed / fadeTime;
            notifBg.color = new Color(0.12f, 0.10f, 0.08f, 0.85f * a);
            notifText.color = new Color(0.98f, 0.92f, 0.76f, a);
            yield return null;
        }
        notifBg.color = new Color(0.12f, 0.10f, 0.08f, 0.85f);
        notifText.color = new Color(0.98f, 0.92f, 0.76f, 1f);

        // 持续显示 3 秒
        yield return new WaitForSeconds(3f);

        // 淡出
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float a = 1f - elapsed / fadeTime;
            notifBg.color = new Color(0.12f, 0.10f, 0.08f, 0.85f * a);
            notifText.color = new Color(0.98f, 0.92f, 0.76f, a);
            yield return null;
        }

        notifCanvas.gameObject.SetActive(false);
    }
}
