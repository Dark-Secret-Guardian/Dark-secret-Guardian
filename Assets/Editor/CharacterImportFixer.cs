#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 角色图导入设置修复 —— Tools > 修复角色图导入设置
/// 将 Characters 目录下所有 PNG 设为 Sprite 类型、无压缩、无 Mipmap
/// </summary>
public static class CharacterImportFixer
{
    [MenuItem("Tools/修复角色图导入设置")]
    public static void FixAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Characters" });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }
            if (importer.mipmapEnabled != false)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }
            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }
            if (importer.alphaIsTransparency != true)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }
            if (importer.spritePixelsPerUnit != 32)
            {
                importer.spritePixelsPerUnit = 32;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
                count++;
                Debug.Log("[CharacterImportFixer] 已修复: " + path);
            }
        }

        Debug.Log($"[CharacterImportFixer] 完成，共修复 {count} 张图片");
    }
}
#endif
