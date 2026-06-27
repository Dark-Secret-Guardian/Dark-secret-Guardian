using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 场景切换管理器 —— 单例，负责黑屏淡入淡出、场景加载、玩家持久化
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    private const float FADE_DURATION = 0.4f;
    private const int CANVAS_SORTING_ORDER = 1000;

    private Canvas fadeCanvas;
    private Image fadeImage;
    private Canvas buttonCanvas;
    private Button transitionButton;
    private TextMeshProUGUI buttonText;

    private bool isTransitioning = false;
    private GameObject persistentPlayer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupFadeCanvas();
        SetupButtonCanvas();
        EnsureEventSystem();
        Debug.Log("[SceneTransition] SceneTransitionManager 已初始化");
    }

    /// <summary>
    /// 如果 Instance 为空，自动创建一个
    /// </summary>
    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("SceneTransitionManager");
            go.AddComponent<SceneTransitionManager>();
        }
    }

    /// <summary>
    /// 确保场景中有 EventSystem，否则 UI 按钮无法响应点击
    /// </summary>
    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;

        GameObject esObj = new GameObject("[EventSystem]");
        esObj.transform.SetParent(transform, false);
        DontDestroyOnLoad(esObj);
        esObj.AddComponent<EventSystem>();
        esObj.AddComponent<StandaloneInputModule>();
        Debug.Log("[SceneTransition] EventSystem 已创建");
    }

    /// <summary>
    /// 创建黑屏淡入淡出 Canvas
    /// </summary>
    private void SetupFadeCanvas()
    {
        GameObject canvasObj = new GameObject("[FadeCanvas]");
        canvasObj.transform.SetParent(transform, false);
        DontDestroyOnLoad(canvasObj);

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = CANVAS_SORTING_ORDER;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);

        RectTransform rt = fadeImage.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        fadeImage.raycastTarget = false;
        canvasObj.SetActive(false);
    }

    /// <summary>
    /// 创建切换按钮 Canvas
    /// </summary>
    private void SetupButtonCanvas()
    {
        GameObject canvasObj = new GameObject("[TransitionButtonCanvas]");
        canvasObj.transform.SetParent(transform, false);
        DontDestroyOnLoad(canvasObj);

        buttonCanvas = canvasObj.AddComponent<Canvas>();
        buttonCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        buttonCanvas.sortingOrder = CANVAS_SORTING_ORDER - 1;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 按钮背景
        GameObject btnObj = new GameObject("TransitionButton");
        btnObj.transform.SetParent(canvasObj.transform, false);
        transitionButton = btnObj.AddComponent<Button>();
        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0);
        btnRt.anchorMax = new Vector2(0.5f, 0);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = new Vector2(0, 80);
        btnRt.sizeDelta = new Vector2(320, 60);

        // 按钮文字
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.font = TMPFontHelper.GetFont();
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.enableWordWrapping = false;
        buttonText.overflowMode = TextOverflowModes.Overflow;
        buttonText.text = "进入";

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        // 按钮悬停效果
        var colors = transitionButton.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.85f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        transitionButton.colors = colors;

        canvasObj.SetActive(false);
    }

    /// <summary>
    /// 显示切换按钮
    /// </summary>
    public void ShowTransitionButton(string text, UnityEngine.Events.UnityAction onClick)
    {
        if (isTransitioning) return;
        Debug.Log($"[SceneTransition] 显示按钮: {text}");
        buttonText.text = text;
        transitionButton.onClick.RemoveAllListeners();
        transitionButton.onClick.AddListener(onClick);
        buttonCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// 隐藏切换按钮
    /// </summary>
    public void HideTransitionButton()
    {
        buttonCanvas.gameObject.SetActive(false);
        transitionButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 切换场景：黑屏淡出 → 加载场景 → 移动玩家 → 淡入
    /// </summary>
    public void TransitionToScene(string sceneName, string spawnPointId)
    {
        if (isTransitioning) return;
        Debug.Log($"[SceneTransition] 开始切换到: {sceneName}, 出生点: {spawnPointId}");
        StartCoroutine(TransitionRoutine(sceneName, spawnPointId));
    }

    private IEnumerator TransitionRoutine(string sceneName, string spawnPointId)
    {
        isTransitioning = true;
        HideTransitionButton();

        // 禁用玩家输入
        SetPlayerInputEnabled(false);
        Debug.Log("[SceneTransition] 1. 玩家输入已禁用");

        // 持久化玩家 —— 必须先脱离父级，否则 DontDestroyOnLoad 会带走整个场景树
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.SetParent(null);
            persistentPlayer = player;
            DontDestroyOnLoad(player);
            Debug.Log("[SceneTransition] 2. 玩家已持久化");
        }
        else
        {
            Debug.LogWarning("[SceneTransition] 2. 未找到 Player！");
        }

        // 淡出（黑屏 alpha 0→1）
        fadeCanvas.gameObject.SetActive(true);
        yield return Fade(0f, 1f, FADE_DURATION);
        Debug.Log("[SceneTransition] 3. 黑屏淡出完成");

        // 加载新场景
        Debug.Log($"[SceneTransition] 4. 开始加载场景: {sceneName}");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        if (asyncLoad == null)
        {
            Debug.LogError($"[SceneTransition] 场景加载失败！场景名: {sceneName}，请检查 Build Settings");
            yield return Fade(1f, 0f, FADE_DURATION);
            fadeCanvas.gameObject.SetActive(false);
            isTransitioning = false;
            yield break;
        }
        while (!asyncLoad.isDone)
            yield return null;
        Debug.Log("[SceneTransition] 5. 场景加载完成");

        // 等待两帧确保场景物体就绪
        yield return null;
        yield return null;
        Debug.Log("[SceneTransition] 6. 等待帧完成");

        // 处理玩家去重：删除新场景中自带的 Player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            if (p != persistentPlayer)
            {
                Debug.Log("[SceneTransition] 删除重复Player: " + p.name);
                Destroy(p);
            }
        }

        // 如果没有持久化玩家，查找场景中的 Player
        if (persistentPlayer == null)
        {
            persistentPlayer = GameObject.Find("Player");
            if (persistentPlayer != null)
            {
                persistentPlayer.transform.SetParent(null);
                DontDestroyOnLoad(persistentPlayer);
            }
        }

        Debug.Log($"[SceneTransition] 7. persistentPlayer = {(persistentPlayer != null ? persistentPlayer.name : "null")}");

        // 移动玩家到出生点
        if (persistentPlayer != null)
        {
            MovePlayerToSpawnPoint(spawnPointId);
            SetPlayerInputEnabled(true);
            Debug.Log("[SceneTransition] 8. 玩家已移动并启用输入");
        }

        // 淡入（黑屏 alpha 1→0）
        yield return Fade(1f, 0f, FADE_DURATION);
        fadeCanvas.gameObject.SetActive(false);
        Debug.Log("[SceneTransition] 9. 淡入完成");

        isTransitioning = false;
        Debug.Log("[SceneTransition] 场景切换完成");

        // 场景切换后尝试触发 AI DM 中途旁白
        AIDMManager.Instance?.TryMidGameCommentary();
    }

    /// <summary>
    /// 将玩家移动到指定出生点，并确保在相机可见范围内
    /// </summary>
    private void MovePlayerToSpawnPoint(string spawnPointId)
    {
        if (string.IsNullOrEmpty(spawnPointId)) return;

        SceneSpawnPoint[] spawnPoints = Object.FindObjectsOfType<SceneSpawnPoint>();
        foreach (var sp in spawnPoints)
        {
            if (sp.spawnPointId == spawnPointId)
            {
                Vector3 pos = sp.transform.position;
                pos.z = persistentPlayer.transform.position.z;

                // 安全钳制：确保玩家在相机可见范围内（1.0 单位边距）
                Camera cam = Camera.main;
                if (cam != null)
                {
                    float margin = 1.0f;
                    float halfH = cam.orthographicSize - margin;
                    float halfW = halfH * ((float)Screen.width / Screen.height);
                    pos.x = Mathf.Clamp(pos.x, -halfW, halfW);
                    pos.y = Mathf.Clamp(pos.y, -halfH, halfH);
                }

                persistentPlayer.transform.position = pos;

                // 重置刚体速度
                var rb = persistentPlayer.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;

                Debug.Log($"[SceneTransition] 玩家已移动到出生点: {spawnPointId} @ {pos}");
                return;
            }
        }
        Debug.LogWarning($"[SceneTransition] 未找到出生点: {spawnPointId}");
    }

    /// <summary>
    /// 启用/禁用玩家输入
    /// </summary>
    private void SetPlayerInputEnabled(bool enabled)
    {
        if (persistentPlayer == null)
            persistentPlayer = GameObject.Find("Player");
        if (persistentPlayer == null) return;

        var movement = persistentPlayer.GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = enabled;
    }

    /// <summary>
    /// 黑屏淡入淡出协程
    /// </summary>
    private IEnumerator Fade(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = toAlpha;
        fadeImage.color = c;
    }
}
