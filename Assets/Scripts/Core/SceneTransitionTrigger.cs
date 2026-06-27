using System.Collections;
using UnityEngine;

/// <summary>
/// 场景切换触发区 —— 挂在场景边缘的触发区域上
/// 玩家进入区域时显示切换按钮，离开时隐藏
/// 触发区位置会在运行时根据相机可见范围自动贴边
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class SceneTransitionTrigger : MonoBehaviour
{
    /// <summary>
    /// 触发区贴边方式
    /// </summary>
    public enum Edge
    {
        None,       // 使用固定位置，不自动贴边
        Left,       // 贴左边
        Right,      // 贴右边
        Top,        // 贴上边
        Bottom,     // 贴下边
        CenterTop,  // 上方居中（如大门）
        BottomLeft  // 左下角
    }

    [Header("目标场景设置")]
    [Tooltip("要切换到的场景名称")]
    public string targetSceneName;

    [Tooltip("目标场景中的出生点ID")]
    public string spawnPointId;

    [Tooltip("按钮上显示的文字")]
    public string buttonText = "进入";

    [Header("触发区位置")]
    [Tooltip("贴边方式：自动定位到画面边缘")]
    public Edge edge = Edge.None;

    [Tooltip("触发区大小")]
    public Vector2 triggerSize = new Vector2(2f, 4f);

    private bool playerInTrigger = false;
    private Camera cam;
    private float lastAspect = -1f;
    private float lastOrthoSize = -1f;

    void Start()
    {
        // 用协程延迟一帧，等 BackgroundAutoFit 调整完相机
        StartCoroutine(DelayedSnap());
    }

    IEnumerator DelayedSnap()
    {
        yield return null; // 等一帧
        yield return null; // 再等一帧，确保 BackgroundAutoFit.Update 已执行
        cam = Camera.main;
        if (cam == null) cam = FindObjectOfType<Camera>();
        if (edge != Edge.None)
            SnapToEdge();
    }

    void Update()
    {
        if (edge == Edge.None || cam == null) return;

        float aspect = (float)Screen.width / Screen.height;
        float ortho = cam.orthographicSize;

        // 屏幕尺寸或相机视野变化时重新定位（用较大容差避免浮点抖动导致每帧重算）
        if (Mathf.Abs(aspect - lastAspect) > 0.01f ||
            Mathf.Abs(ortho - lastOrthoSize) > 0.01f)
        {
            SnapToEdge();
            lastAspect = aspect;
            lastOrthoSize = ortho;
        }
    }

    /// <summary>
    /// 根据相机可见范围自动定位触发区到指定边缘
    /// </summary>
    void SnapToEdge()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) cam = FindObjectOfType<Camera>();
        }
        if (cam == null) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * ((float)Screen.width / Screen.height);

        // 触发区放在画面边缘内侧
        float inset = 1.0f;

        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;

        switch (edge)
        {
            case Edge.Left:
                transform.position = new Vector2(-halfW + inset, 0f);
                col.size = new Vector2(triggerSize.x, halfH * 2f);
                break;

            case Edge.Right:
                transform.position = new Vector2(halfW - inset, 0f);
                col.size = new Vector2(triggerSize.x, halfH * 2f);
                break;

            case Edge.Top:
                transform.position = new Vector2(0f, halfH - inset);
                col.size = new Vector2(halfW * 2f, triggerSize.y);
                break;

            case Edge.Bottom:
                transform.position = new Vector2(0f, -halfH + inset);
                col.size = new Vector2(halfW * 2f, triggerSize.y);
                break;

            case Edge.CenterTop:
                // 上方居中（如大门）—— 触发区顶部贴近画面顶边，向下延伸 triggerSize.y 的高度
                transform.position = new Vector2(0f, halfH - inset - triggerSize.y * 0.5f);
                col.size = new Vector2(triggerSize.x, triggerSize.y);
                break;

            case Edge.BottomLeft:
                // 左下角
                transform.position = new Vector2(-halfW + inset + 1.5f, -halfH + inset + 1.5f);
                col.size = new Vector2(triggerSize.x, triggerSize.y);
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[SceneTransitionTrigger] OnTriggerEnter2D: {other.gameObject.name}, tag={other.tag}");
        if (!other.CompareTag("Player") && other.gameObject.name != "Player") return;
        if (playerInTrigger) return;
        playerInTrigger = true;

        // 确保 SceneTransitionManager 存在
        SceneTransitionManager.EnsureExists();

        SceneTransitionManager.Instance?.ShowTransitionButton(buttonText, () =>
        {
            SceneTransitionManager.Instance?.TransitionToScene(targetSceneName, spawnPointId);
        });
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && other.gameObject.name != "Player") return;
        if (!playerInTrigger) return;
        playerInTrigger = false;

        SceneTransitionManager.Instance?.HideTransitionButton();
    }
}
