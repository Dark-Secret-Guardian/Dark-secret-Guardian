using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Aesheria.UI
{
    /// <summary>
    /// AI 旁白测试按钮 —— 点击后请求 AI 生成一段旁白文本并显示
    /// 显示一段时间后自动清除文本
    /// </summary>
    public class AITestButton : MonoBehaviour
    {
        public TextMeshProUGUI displayText;   // 旁白显示文本
        public float clearDelay = 3f;          // 文本自动清除延迟（秒）

        private GameManager gm;
        private float clearTimer = 0f;         // 清除计时器

        /// <summary>
        /// Start：获取 GameManager 引用，绑定按钮点击事件
        /// </summary>
        void Start()
        {
            gm = FindObjectOfType<GameManager>();
            Button btn = GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnClick);
        }

        /// <summary>
        /// 按钮点击回调
        /// 显示加载提示文本，然后请求 AI 旁白
        /// </summary>
        void OnClick()
        {
            if (displayText == null) return;
            displayText.text = "聆听灵脉低语...";  // 加载提示

            // 请求 AI 旁白，成功后显示结果并启动自动清除计时器
            gm.RequestAIDescription((result) =>
            {
                displayText.text = result;
                clearTimer = clearDelay;
            });
        }

        /// <summary>
        /// Update：处理自动清除计时器
        /// 文本显示 clearDelay 秒后自动清除
        /// </summary>
        void Update()
        {
            if (clearTimer > 0)
            {
                clearTimer -= Time.deltaTime;
                if (clearTimer <= 0 && displayText != null && displayText.text != "聆听灵脉低语...")
                {
                    displayText.text = "";   // 清除文本
                }
            }
        }
    }
}
