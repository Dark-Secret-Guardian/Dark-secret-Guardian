using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 结局 UI —— 显示结局标题和描述，提供重新开始和退出按钮
/// 由 GameManager.TriggerEnding 调用 ShowEnding 激活
/// 显示时暂停游戏（Time.timeScale = 0）
/// </summary>
public class EndingUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;       // 结局标题文本
    public TextMeshProUGUI descriptionText;  // 结局描述文本
    public Button restartButton;            // 重新开始按钮
    public Button quitButton;               // 退出按钮
    public GameObject endingPanel;           // 结局面板（整体显隐控制）

    /// <summary>
    /// Start：绑定按钮事件，默认隐藏结局面板
    /// </summary>
    void Start()
    {
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
        endingPanel.SetActive(false);    // 默认隐藏
    }

    /// <summary>
    /// 显示结局画面
    /// 设置标题和描述文本，暂停游戏
    /// </summary>
    /// <param name="title">结局标题</param>
    /// <param name="description">结局描述</param>
    public void ShowEnding(string title, string description)
    {
        titleText.text = title;
        descriptionText.text = description;
        endingPanel.SetActive(true);
        Time.timeScale = 0;    // 暂停游戏
    }

    /// <summary>
    /// 重新开始游戏
    /// 恢复时间缩放，清除存档数据，重新加载当前场景
    /// </summary>
    void RestartGame()
    {
        Time.timeScale = 1;    // 恢复时间缩放
        PlayerPrefs.DeleteKey("SaveData");   // 清除存档
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);   // 重新加载场景
    }

    /// <summary>
    /// 返回主菜单
    /// 恢复时间缩放，加载主菜单场景（build index 0）
    /// </summary>
    void QuitGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
