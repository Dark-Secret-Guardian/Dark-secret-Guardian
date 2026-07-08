using UnityEngine;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace Aesheria.Utils
{
    /// <summary>
    /// 运行时初始化中文字体
    /// 在 Awake 中创建 SimHei SDF 动态字体并分配给所有 TMP 文本
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class FontInitializer : MonoBehaviour
    {
        private static TMP_FontAsset _cachedFont;

        void Awake()
        {
            EnsureFont();
        }

        public static void EnsureFont()
        {
            if (_cachedFont != null) return;

            Font simhei = Resources.Load<Font>("Fonts & Materials/simhei");
            if (simhei == null)
            {
                Debug.LogError("[FontInitializer] simhei.ttf 未找到，请确保位于 Resources/Fonts & Materials/ 下");
                return;
            }

            _cachedFont = TMP_FontAsset.CreateFontAsset(simhei);
            _cachedFont.name = "SimHei SDF Runtime";
            _cachedFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;

            // 添加为默认字体的 fallback
            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null && !defaultFont.fallbackFontAssetTable.Contains(_cachedFont))
            {
                defaultFont.fallbackFontAssetTable.Add(_cachedFont);
            }

            Debug.Log($"[FontInitializer] 中文字体已创建: {_cachedFont.name}");
        }

        void Start()
        {
            EnsureFont();
            ApplyToAllTexts();
        }

        public void ApplyToAllTexts()
        {
            if (_cachedFont == null) return;
            var texts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                if (t.font == null || t.font.name == "LiberationSans SDF")
                    t.font = _cachedFont;
            }
        }
    }
}
