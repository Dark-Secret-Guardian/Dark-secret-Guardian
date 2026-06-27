#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 角色图透明化处理 v2 —— Tools > 处理角色图透明背景
/// 用 RGBA32 格式重新创建纹理，确保 alpha 通道正确保存
/// </summary>
public static class CharacterTransparentProcessor
{
    [MenuItem("Tools/处理角色图透明背景")]
    public static void ProcessAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Characters" });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            // 临时设为可读
            importer.isReadable = true;
            importer.SaveAndReimport();

            Texture2D src = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (src == null) continue;

            int w = src.width;
            int h = src.height;

            // 用 RGBA32 格式新建纹理，确保有 alpha 通道
            Texture2D dst = new Texture2D(w, h, TextureFormat.RGBA32, false);
            dst.filterMode = FilterMode.Point;

            Color[] pixels = src.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                float r = c.r, g = c.g, b = c.b;

                // 检测棋盘格颜色：
                // 浅格：接近白色 (r≈g≈b≈0.9-1.0)
                // 深格：浅灰 (r≈g≈b≈0.7-0.85)
                bool isLight = r > 0.88f && g > 0.88f && b > 0.88f &&
                               Mathf.Abs(r - g) < 0.03f && Mathf.Abs(g - b) < 0.03f;
                bool isDark = r > 0.65f && r < 0.88f &&
                              Mathf.Abs(r - g) < 0.03f && Mathf.Abs(g - b) < 0.03f;

                // 检测纯白
                bool isWhite = r > 0.95f && g > 0.95f && b > 0.95f;

                // 检测纯黑（可能是上次处理残留）
                bool isBlack = r < 0.05f && g < 0.05f && b < 0.05f;

                if (isLight || isDark || isWhite || isBlack)
                {
                    // 设为透明，但保留 RGB 为白色（避免黑边）
                    pixels[i] = new Color(1f, 1f, 1f, 0f);
                }
            }

            dst.SetPixels(pixels);
            dst.Apply();

            // 保存为 PNG（RGBA32 确保带 alpha）
            byte[] pngData = dst.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngData);

            Object.DestroyImmediate(dst);
            count++;
            Debug.Log("[TransparentProcessor] 已处理: " + path);

            // 恢复不可读并设置正确的导入参数
            importer.isReadable = false;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Point;
            importer.alphaIsTransparency = true;
            importer.spritePixelsPerUnit = 32;
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();
        Debug.Log($"[TransparentProcessor] 完成，共处理 {count} 张图片");
    }
}
#endif
