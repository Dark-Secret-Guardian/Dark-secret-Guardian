using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 背景管理器 —— 自动创建全屏背景图
/// 通过 RuntimeInitializeOnLoadMethod 自动初始化，无需手动挂载到场景
/// 若场景中无 Canvas，则自动创建一个专用 Canvas 用于背景渲染
/// 背景图从 Resources/Backgrounds/ 目录加载
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    private const string BG_CANVAS_NAME = "[BackgroundCanvas]";  // 专用背景 Canvas 名称
    private const string BG_OBJECT_NAME = "BackgroundImage";     // 背景图物体名称
    private const string DEFAULT_BACKGROUND = "tavern";           // 默认背景图名称（对应 Resources/Backgrounds/tavern）

    private Image backgroundImage;  // 背景图 Image 组件引用
    private RectTransform bgRectTransform;  // 背景图 RectTransform 引用

    /// <summary>
    /// 运行时自动初始化：场景加载后创建 BackgroundManager 单例
    /// </summary>
    // 已禁用：改用 SpriteRenderer 方案渲染背景
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        // 已存在则跳过
        if (FindObjectOfType<BackgroundManager>() != null) return;

        GameObject go = new GameObject("[BackgroundManager]");
        DontDestroyOnLoad(go);
        go.AddComponent<BackgroundManager>();
    }

    /// <summary>
    /// Awake：订阅场景加载事件，用于跨场景时重新挂载背景
    /// </summary>
    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Start：用协程等待一帧后初始化背景
    /// </summary>
    IEnumerator Start()
    {
        yield return null;  // 等待一帧，确保场景中所有物体已完成 Awake
        SetupBackground();
    }

    /// <summary>
    /// 场景加载回调：新场景加载后重新创建背景
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedSetup());
    }

    /// <summary>
    /// 协程：等待一帧后执行背景创建
    /// </summary>
    private IEnumerator DelayedSetup()
    {
        yield return null;
        SetupBackground();
    }

    /// <summary>
    /// 创建或更新背景图
    /// 1. 查找场景中已有的 Canvas，优先复用
    /// 2. 若无 Canvas，自动创建一个专用背景 Canvas
    /// 3. 在 Canvas 底层创建全屏 Image
    /// 4. 加载背景纹理
    /// </summary>
    private void SetupBackground()
    {
        // 检查是否已有专用背景 Canvas（跨场景复用）
        Canvas bgCanvas = FindObjectOfType<Canvas>();

        // 若场景中有 Canvas，直接在其下创建背景
        // 若无 Canvas，创建专用背景 Canvas
        if (bgCanvas == null)
        {
            // 创建专用背景 Canvas
            GameObject canvasObj = new GameObject(BG_CANVAS_NAME, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(canvasObj);

            bgCanvas = canvasObj.GetComponent<Canvas>();
            bgCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            bgCanvas.sortingOrder = -100;  // 置于所有其他 UI 之下

            // 配置 CanvasScaler 与项目设置一致（1920x1080）
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Debug.Log("[BackgroundManager] 场景中无 Canvas，已自动创建背景专用 Canvas");
        }

        // 检查 Canvas 下是否已存在背景图物体
        Transform existing = bgCanvas.transform.Find(BG_OBJECT_NAME);
        if (existing != null)
        {
            backgroundImage = existing.GetComponent<Image>();
            if (backgroundImage != null)
            {
                LoadBackground(DEFAULT_BACKGROUND);
                return;
            }
        }

        // 创建背景图 GameObject + Image 组件
        GameObject bgObj = new GameObject(BG_OBJECT_NAME, typeof(Image));
        bgObj.transform.SetParent(bgCanvas.transform, false);

        // 置于 Canvas 最底层（第一个子物体渲染在最下方）
        bgObj.transform.SetAsFirstSibling();

        // 拉伸铺满整个屏幕
        RectTransform rt = bgObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        backgroundImage = bgObj.GetComponent<Image>();
        backgroundImage.color = Color.white;
        backgroundImage.raycastTarget = false;  // 背景不拦截 UI 事件

        bgRectTransform = bgObj.GetComponent<RectTransform>();

        // 加载背景纹理
        LoadBackground(DEFAULT_BACKGROUND);
    }

    /// <summary>
    /// 加载背景图
    /// 优先加载 Sprite（图片导入设置中 Texture Type 为 Sprite 时）
    /// 降级加载 Texture2D 并运行时创建 Sprite（兼容任意导入设置）
    /// </summary>
    /// <param name="bgName">背景图名称（不含扩展名，对应 Resources/Backgrounds/ 下的文件）</param>
    public void LoadBackground(string bgName)
    {
        if (backgroundImage == null) return;

        // 优先尝试直接加载 Sprite
        Sprite sprite = Resources.Load<Sprite>($"Backgrounds/{bgName}");

        if (sprite == null)
        {
            // 降级：加载 Texture2D 并运行时创建 Sprite
            Texture2D tex = Resources.Load<Texture2D>($"Backgrounds/{bgName}");
            if (tex != null)
            {
                sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f)
                );
                Debug.Log($"[BackgroundManager] 通过 Texture2D 创建 Sprite: {tex.width}x{tex.height}");
            }
        }

        if (sprite != null)
        {
            backgroundImage.sprite = sprite;
            backgroundImage.type = Image.Type.Simple;
            backgroundImage.preserveAspect = false;  // 不用 Unity 内置的等比缩放，手动计算 cover 模式
            UpdateCoverSize();
            Debug.Log($"[BackgroundManager] 背景图加载成功: {bgName}");
        }
        else
        {
            Debug.LogWarning($"[BackgroundManager] 未找到背景图: Resources/Backgrounds/{bgName}");
        }
    }

    /// <summary>
    /// 当父级 RectTransform 尺寸变化时（如窗口缩放、分辨率切换），重新计算背景填充
    /// </summary>
    void OnRectTransformDimensionsChange()
    {
        UpdateCoverSize();
    }

    /// <summary>
    /// Cover 模式：图片填满整个屏幕，保持比例不变形，多出的部分裁掉
    /// 当屏幕宽高比 ≠ 图片宽高比时，图片会沿某一轴溢出并裁切
    /// </summary>
    private void UpdateCoverSize()
    {
        if (backgroundImage == null || backgroundImage.sprite == null || bgRectTransform == null) return;

        Canvas canvas = backgroundImage.canvas;
        if (canvas == null) return;

        RectTransform canvasRt = canvas.transform as RectTransform;
        if (canvasRt == null) return;

        float canvasWidth = canvasRt.rect.width;
        float canvasHeight = canvasRt.rect.height;
        if (canvasWidth <= 0 || canvasHeight <= 0) return;

        float canvasAspect = canvasWidth / canvasHeight;

        float spriteW = backgroundImage.sprite.rect.width;
        float spriteH = backgroundImage.sprite.rect.height;
        float spriteAspect = spriteW / spriteH;

        float targetWidth, targetHeight;

        if (canvasAspect > spriteAspect)
        {
            // 屏幕更宽：以宽度为准，高度溢出裁切
            targetWidth = canvasWidth;
            targetHeight = canvasWidth / spriteAspect;
        }
        else
        {
            // 屏幕更高：以高度为准，宽度溢出裁切
            targetHeight = canvasHeight;
            targetWidth = canvasHeight * spriteAspect;
        }

        bgRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        bgRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
    }

    /// <summary>
    /// OnDestroy：取消订阅事件，防止内存泄漏
    /// </summary>
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
