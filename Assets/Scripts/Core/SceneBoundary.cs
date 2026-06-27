using UnityEngine;

/// <summary>
/// 场景边界 —— 运行时根据相机实际可见范围动态调整四周边界墙位置
/// 确保玩家无法走出可见画面
/// </summary>
public class SceneBoundary : MonoBehaviour
{
    private Camera cam;
    private float lastScreenAspect = -1f;
    private float lastOrthoSize = -1f;

    private Transform wallLeft, wallRight, wallTop, wallBottom;
    private const float WALL_THICKNESS = 1f;
    private const float WALL_OFFSET = 0.3f; // 墙在画面边缘外，留空间给触发区

    void Start()
    {
        cam = Camera.main;
        if (cam == null) cam = FindObjectOfType<Camera>();

        wallLeft = transform.Find("Wall_Left");
        wallRight = transform.Find("Wall_Right");
        wallTop = transform.Find("Wall_Top");
        wallBottom = transform.Find("Wall_Bottom");

        // 延迟一帧执行，确保 BackgroundAutoFit 已完成相机调整
        Invoke("UpdateWalls", 0f);
    }

    void Update()
    {
        if (cam == null) return;

        float aspect = (float)Screen.width / Screen.height;
        if (Mathf.Abs(aspect - lastScreenAspect) > 0.01f ||
            Mathf.Abs(cam.orthographicSize - lastOrthoSize) > 0.01f)
        {
            UpdateWalls();
            lastScreenAspect = aspect;
            lastOrthoSize = cam.orthographicSize;
        }
    }

    void UpdateWalls()
    {
        if (cam == null) return;

        float halfH = cam.orthographicSize + WALL_OFFSET;
        float halfW = halfH * ((float)Screen.width / Screen.height);

        if (wallLeft != null)
        {
            wallLeft.position = new Vector2(-halfW - WALL_THICKNESS * 0.5f, 0f);
            var col = wallLeft.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(WALL_THICKNESS, halfH * 2f);
        }

        if (wallRight != null)
        {
            wallRight.position = new Vector2(halfW + WALL_THICKNESS * 0.5f, 0f);
            var col = wallRight.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(WALL_THICKNESS, halfH * 2f);
        }

        if (wallTop != null)
        {
            wallTop.position = new Vector2(0f, halfH + WALL_THICKNESS * 0.5f);
            var col = wallTop.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(halfW * 2f, WALL_THICKNESS);
        }

        if (wallBottom != null)
        {
            wallBottom.position = new Vector2(0f, -halfH - WALL_THICKNESS * 0.5f);
            var col = wallBottom.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(halfW * 2f, WALL_THICKNESS);
        }
    }
}
