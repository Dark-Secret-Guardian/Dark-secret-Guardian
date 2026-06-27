using UnityEngine;

/// <summary>
/// 背景自适应 —— 同时调整相机视野和图片缩放，在保证不露边的前提下显示尽可能多的画面
/// 图片 16:9（2560x1440），在 16:9 屏幕上完美无裁切
/// 非 16:9 屏幕上只裁切最少的部分
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundAutoFit : MonoBehaviour
{
    private SpriteRenderer sr;
    private Camera cam;
    private float lastScreenAspect = -1f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        FitToScreen();
    }

    void Update()
    {
        float aspect = (float)Screen.width / Screen.height;
        if (!Mathf.Approximately(aspect, lastScreenAspect))
        {
            FitToScreen();
            lastScreenAspect = aspect;
        }
    }

    /// <summary>
    /// 智能适配：先调整相机 orthographicSize 适配图片宽高比，再缩放图片 cover 剩余部分
    /// 这样在 16:9 屏幕上完全不裁切，非 16:9 只裁最少内容
    /// </summary>
    void FitToScreen()
    {
        if (sr == null || sr.sprite == null || cam == null) return;

        float spriteW = sr.sprite.bounds.size.x;  // 图片世界宽度
        float spriteH = sr.sprite.bounds.size.y;  // 图片世界高度
        float imageAspect = spriteW / spriteH;
        float screenAspect = (float)Screen.width / Screen.height;

        // 第一步：调整相机视野，让相机看到的区域尽可能贴合图片比例
        if (screenAspect >= imageAspect)
        {
            // 屏幕更宽：以图片宽度为基准设定相机，上下会裁掉一点
            cam.orthographicSize = (spriteW / screenAspect) * 0.5f;
        }
        else
        {
            // 屏幕更高：以图片高度为基准设定相机，左右会裁掉一点
            cam.orthographicSize = spriteH * 0.5f;
        }

        // 第二步：缩放图片 cover 相机视野（补上剩余的裁切）
        float camH = cam.orthographicSize * 2f;
        float camW = camH * screenAspect;
        float scaleX = camW / spriteW;
        float scaleY = camH / spriteH;
        float scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
