#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// 编辑器工具 —— 自动导入酒馆背景图
/// 在 Unity 打开时自动检查并从源路径复制背景图到 Resources/Backgrounds/
/// 同时配置导入设置为 Sprite 类型，确保 BackgroundManager 能正确加载
/// 也可通过菜单栏 Tools > 导入酒馆背景图 手动触发
/// </summary>
[InitializeOnLoad]
public static class BackgroundImporter
{
    // 源图片路径（微信临时文件，可能失效）
    private const string SOURCE_PATH = @"C:\Users\zsy13\Documents\xwechat_files\wxid_veef7t0qtjja22_3c4f\temp\RWTemp\2026-06\9e20f478899dc29eb19741386f9343c8\a37c49fcf50a875fcf306f6a5272467d.jpg";

    // 项目内目标路径（相对路径，用于 AssetDatabase）
    private const string DEST_ASSET_PATH = "Assets/Resources/Backgrounds/tavern.jpg";

    static BackgroundImporter()
    {
        // 延迟执行，确保 AssetDatabase 已就绪
        EditorApplication.delayCall += ImportIfMissing;
    }

    /// <summary>
    /// 如果目标图片不存在且源图片存在，则自动复制
    /// </summary>
    private static void ImportIfMissing()
    {
        string destFullPath = Path.Combine(Application.dataPath, "Resources/Backgrounds/tavern.jpg");

        // 已存在则只检查导入设置
        if (File.Exists(destFullPath))
        {
            ConfigureSpriteImportSettings();
            return;
        }

        // 尝试从源路径复制
        if (File.Exists(SOURCE_PATH))
        {
            // 确保目录存在
            string destDir = Path.GetDirectoryName(destFullPath);
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(SOURCE_PATH, destFullPath);
            Debug.Log("[BackgroundImporter] 酒馆背景图已自动复制到: " + DEST_ASSET_PATH);

            // 刷新 AssetDatabase 并配置导入设置
            AssetDatabase.Refresh();
            ConfigureSpriteImportSettings();
        }
    }

    /// <summary>
    /// 配置图片的导入设置为 Sprite 类型
    /// 这样 BackgroundManager 可以直接通过 Resources.Load&lt;Sprite&gt; 加载
    /// </summary>
    private static void ConfigureSpriteImportSettings()
    {
        TextureImporter importer = AssetImporter.GetAtPath(DEST_ASSET_PATH) as TextureImporter;
        if (importer == null) return;

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
            Debug.Log("[BackgroundImporter] 已将酒馆背景图导入设置配置为 Sprite 类型");
        }
    }

    /// <summary>
    /// 菜单项：手动导入背景图
    /// </summary>
    [MenuItem("Tools/导入酒馆背景图")]
    private static void ManualImport()
    {
        ImportIfMissing();
        Debug.Log("[BackgroundImporter] 导入完成");
    }
}
#endif
