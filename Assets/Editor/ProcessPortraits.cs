#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 立绘处理工具 —— 去除棋盘格背景
/// 菜单：Tools > 场景管理 > 处理立绘图片
/// 策略：只去除图片边缘区域（边框几圈像素）的棋盘格，
/// 再用 flood fill 从边缘向内扩展透明区域，不会误删角色衣服
/// </summary>
public static class ProcessPortraits
{
    [MenuItem("Tools/场景管理/处理立绘图片")]
    public static void Process()
    {
        string[] portraits = {
            "Assets/Resources/Characters/Portraits/tavern.png",
            "Assets/Resources/Characters/Portraits/exchange.png",
            "Assets/Resources/Characters/Portraits/wildness.png",
        };

        int totalChanged = 0;

        foreach (string path in portraits)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[ProcessPortraits] 文件不存在: {path}");
                continue;
            }

            EnsureReadable(path);
            int changed = ProcessOne(path);
            totalChanged += changed;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("完成",
            $"立绘处理完成！共去除 {totalChanged} 个背景像素。\n\n" +
            "使用 flood fill 算法，只去除背景，不碰角色。", "知道了");
    }

    private static void EnsureReadable(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        bool dirty = false;
        if (!importer.isReadable) { importer.isReadable = true; dirty = true; }
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; dirty = true; }
        if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; dirty = true; }
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; dirty = true; }
        if (importer.alphaIsTransparency) { importer.alphaIsTransparency = false; dirty = true; }

        if (dirty)
        {
            importer.SaveAndReimport();
            Debug.Log($"[ProcessPortraits] 已修复导入设置: {assetPath}");
        }
    }

    /// <summary>
    /// 判断像素是否是背景（棋盘格：纯白 #FFFFFF 或浅灰 #E0E0E0 左右）
    /// 只匹配非常接近纯白或纯灰的像素，不碰角色的白色衣服
    /// </summary>
    private static bool IsBackground(Color32 c)
    {
        // 纯白棋盘格块
        if (c.r > 245 && c.g > 245 && c.b > 245 && c.a > 200)
            return true;
        // 浅灰棋盘格块（棋盘格的灰色部分通常很均匀）
        if (c.r > 220 && c.r < 240 && c.g > 220 && c.g < 240 && c.b > 220 && c.b < 240 &&
            Mathf.Abs(c.r - c.g) < 3 && Mathf.Abs(c.g - c.b) < 3 && c.a > 200)
            return true;
        return false;
    }

    private static int ProcessOne(string assetPath)
    {
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (tex == null)
        {
            Debug.LogError($"[ProcessPortraits] 无法加载纹理: {assetPath}");
            return 0;
        }

        int w = tex.width;
        int h = tex.height;
        Color32[] pixels = tex.GetPixels32();

        // Flood fill 从图片四条边开始，只移除背景连通区域
        // 不会穿过角色身体去删除角色身上的白色
        bool[] visited = new bool[w * h];
        var stack = new System.Collections.Generic.Stack<int>();

        // 从四条边的像素开始
        for (int x = 0; x < w; x++)
        {
            int top = (h - 1) * w + x;
            int bot = x;
            if (IsBackground(pixels[top])) { stack.Push(top); visited[top] = true; }
            if (IsBackground(pixels[bot])) { stack.Push(bot); visited[bot] = true; }
        }
        for (int y = 0; y < h; y++)
        {
            int left = y * w;
            int right = y * w + w - 1;
            if (IsBackground(pixels[left])) { stack.Push(left); visited[left] = true; }
            if (IsBackground(pixels[right])) { stack.Push(right); visited[right] = true; }
        }

        int changed = 0;
        while (stack.Count > 0)
        {
            int idx = stack.Pop();
            int x = idx % w;
            int y = idx / w;

            pixels[idx] = new Color32(0, 0, 0, 0);
            changed++;

            // 检查 4 邻居
            if (x > 0) { int n = idx - 1; if (!visited[n] && IsBackground(pixels[n])) { visited[n] = true; stack.Push(n); } }
            if (x < w - 1) { int n = idx + 1; if (!visited[n] && IsBackground(pixels[n])) { visited[n] = true; stack.Push(n); } }
            if (y > 0) { int n = idx - w; if (!visited[n] && IsBackground(pixels[n])) { visited[n] = true; stack.Push(n); } }
            if (y < h - 1) { int n = idx + w; if (!visited[n] && IsBackground(pixels[n])) { visited[n] = true; stack.Push(n); } }
        }

        if (changed > 0)
        {
            string fullPath = Path.GetFullPath(assetPath);
            Texture2D cleanTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            cleanTex.SetPixels32(pixels);
            cleanTex.Apply();
            File.WriteAllBytes(fullPath, cleanTex.EncodeToPNG());

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.alphaIsTransparency = true;
                importer.isReadable = false;
                importer.SaveAndReimport();
            }

            Debug.Log($"[ProcessPortraits] {assetPath}: flood fill 去除 {changed} 个背景像素");
        }
        else
        {
            // 已经处理过，恢复不可读
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && importer.isReadable)
            {
                importer.alphaIsTransparency = true;
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
            Debug.Log($"[ProcessPortraits] {assetPath}: 无需处理");
        }

        return changed;
    }
}
#endif
