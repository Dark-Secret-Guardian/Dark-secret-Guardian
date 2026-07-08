using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Aesheria.Utils;

namespace Aesheria.UI
{
    /// <summary>
    /// 主菜单 —— 游戏启动入口
    /// 提供开始游戏和退出按钮
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class MainMenu : MonoBehaviour
    {
        public Button startButton;   // 开始游戏按钮
        public Button quitButton;    // 退出按钮

        void Start()
        {
            // 确保中文字体已初始化
            FontInitializer.EnsureFont();
            FontInitializer fi = GetComponent<FontInitializer>();
            if (fi != null) fi.ApplyToAllTexts();

            startButton.onClick.AddListener(StartGame);
            quitButton.onClick.AddListener(QuitGame);
        }

        /// <summary>
        /// 开始游戏：加载 SampleScene（build index 1）
        /// </summary>
        void StartGame()
        {
            SceneManager.LoadScene(1);
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            Application.ExternalEval("window.location.reload();");
#else
            Application.Quit();
#endif
        }
    }
}
