using TMPro;
using UnityEngine;

/// <summary>
/// TMP 字体工具 —— 加载预创建的含中文 TMP 字体资产
/// 需先执行 Tools > 场景管理 > 生成中文字体
/// </summary>
public static class TMPFontHelper
{
    private static TMP_FontAsset _font;

    public static TMP_FontAsset GetFont()
    {
        if (_font != null) return _font;

        _font = Resources.Load<TMP_FontAsset>("Fonts/MSYH SDF");

        if (_font == null)
        {
            Debug.LogError("[TMPFontHelper] 未找到字体资产 Fonts/MSYH SDF！请先执行 Tools > 场景管理 > 生成中文字体");
            return TMP_Settings.defaultFontAsset;
        }

        // 确保动态模式（运行时按需加载字形到 atlas）
        _font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        _font.isMultiAtlasTexturesEnabled = true;

        Debug.Log($"[TMPFontHelper] 字体已加载: {_font.name}, atlas={_font.atlasTexture.width}x{_font.atlasTexture.height}, mode={_font.atlasPopulationMode}");
        return _font;
    }
}
