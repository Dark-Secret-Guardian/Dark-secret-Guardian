#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// 编辑器工具：创建含中文的 TMP 字体资产
/// 菜单：Tools > 场景管理 > 生成中文字体
/// 会自动复制系统字体、创建 TMP SDF 字体、保存到 Resources 目录
/// </summary>
public static class CreateChineseFontAsset
{
    [MenuItem("Tools/场景管理/生成中文字体")]
    public static void Create()
    {
        // 1. 查找系统中文字体文件
        string[] candidates = {
            @"C:\Windows\Fonts\msyh.ttc",
            @"C:\Windows\Fonts\msyh.ttf",
            @"C:\Windows\Fonts\simhei.ttf",
            @"C:\Windows\Fonts\simsun.ttc",
        };

        string srcPath = null;
        foreach (var p in candidates)
        {
            if (File.Exists(p)) { srcPath = p; break; }
        }

        if (srcPath == null)
        {
            EditorUtility.DisplayDialog("失败", "未找到系统中文字体文件", "知道了");
            return;
        }

        // 2. 复制到项目
        string fontsDir = "Assets/Fonts";
        if (!AssetDatabase.IsValidFolder(fontsDir))
            AssetDatabase.CreateFolder("Assets", "Fonts");

        string fileName = Path.GetFileName(srcPath);
        string destPath = fontsDir + "/" + fileName;

        if (!File.Exists(destPath))
        {
            File.Copy(srcPath, destPath);
            AssetDatabase.ImportAsset(destPath);
            Debug.Log($"[CreateChineseFont] 已复制字体: {destPath}");
        }

        // 3. 加载为 Font
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(destPath);
        if (sourceFont == null)
        {
            EditorUtility.DisplayDialog("失败", $"无法加载字体: {destPath}", "知道了");
            return;
        }

        // 4. 创建 TMP 动态字体资产
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,                             // samplingPointSize
            9,                              // atlasPadding
            GlyphRenderMode.SDFAA,          // renderMode
            1024,                           // atlasWidth
            1024,                           // atlasHeight
            AtlasPopulationMode.Dynamic     // 动态模式
        );

        if (fontAsset == null)
        {
            EditorUtility.DisplayDialog("失败", "CreateFontAsset 返回 null", "知道了");
            return;
        }

        fontAsset.name = "MSYH SDF";
        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        fontAsset.atlasTexture.filterMode = FilterMode.Bilinear;
        fontAsset.atlasTexture.name = "MSYH SDF Atlas";

        // 5. 保存到 Resources 目录
        string resDir = "Assets/Resources/Fonts";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(resDir))
            AssetDatabase.CreateFolder("Assets/Resources", "Fonts");

        string assetPath = resDir + "/MSYH SDF.asset";

        // 如果已存在先删除
        if (File.Exists(assetPath))
        {
            AssetDatabase.DeleteAsset(assetPath);
        }

        // 创建主资产
        AssetDatabase.CreateAsset(fontAsset, assetPath);

        // 把 atlas texture 作为子资产保存到同一 .asset 文件
        AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);

        // 把 material 也作为子资产保存
        if (fontAsset.material != null)
        {
            fontAsset.material.name = "MSYH SDF Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CreateChineseFont] 字体资产已保存: {assetPath}");
        EditorUtility.DisplayDialog("完成",
            $"中文字体已创建并保存到:\n{assetPath}\n\n" +
            "运行时会自动加载此字体。", "知道了");
    }
}
#endif
