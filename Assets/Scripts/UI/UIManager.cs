using UnityEngine;
using TMPro;

/// <summary>
/// UI 管理器 —— 监听 GameManager 的数值变化事件，刷新 HUD 文本
/// 显示四维数值：繁荣、生态、觉醒、守护
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI prosperityText;    // 繁荣度文本
    public TextMeshProUGUI ecologyText;       // 生态值文本
    public TextMeshProUGUI awakeningText;    // 觉醒值文本
    public TextMeshProUGUI guardianshipText; // 守护值文本

    private GameManager gm;

    /// <summary>
    /// Start：获取 GameManager 引用并订阅数值变化事件
    /// </summary>
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("未找到 GameManager！");
            return;
        }

        // 订阅数值变化事件
        gm.OnValuesChanged += RefreshUI;
        RefreshUI();   // 初始刷新
    }

    /// <summary>
    /// 刷新 HUD 文本：更新四维数值显示
    /// </summary>
    private void RefreshUI()
    {
        prosperityText.text = $"繁荣: {gm.prosperity}";
        ecologyText.text = $"生态: {gm.ecology}";
        awakeningText.text = $"觉醒: {gm.awakening}";
        guardianshipText.text = $"守护: {gm.guardianship}";
    }

    /// <summary>
    /// OnDestroy：取消订阅事件，防止内存泄漏
    /// </summary>
    private void OnDestroy()
    {
        if (gm != null)
            gm.OnValuesChanged -= RefreshUI;
    }
}
