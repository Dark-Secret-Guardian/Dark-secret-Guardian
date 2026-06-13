using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 任务 UI —— 监听 QuestManager 的节点变化事件，动态刷新描述文本和选项按钮
/// 选项按钮通过预制体动态生成，选择后调用 QuestManager.ChooseOption
/// </summary>
public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI descriptionText;   // 任务描述文本
    public Transform optionsContainer;          // 选项按钮容器
    public GameObject optionButtonPrefab;       // 选项按钮预制体

    private QuestManager qm;                    // QuestManager 引用
    private List<Button> currentButtons = new List<Button>(); // 当前显示的选项按钮列表

    /// <summary>
    /// Start：获取 QuestManager 引用并订阅节点变化事件
    /// </summary>
    void Start()
    {
        qm = FindObjectOfType<QuestManager>();
        if (qm != null)
        {
            qm.OnQuestChanged += RefreshUI;
            RefreshUI();   // 初始刷新
        }
    }

    /// <summary>
    /// 刷新 UI
    /// 1. 更新任务描述文本
    /// 2. 销毁旧的选项按钮
    /// 3. 根据当前节点的选项列表动态生成新按钮
    /// </summary>
    void RefreshUI()
    {
        if (qm == null || qm.currentNode == null) return;

        // 更新描述文本
        descriptionText.text = qm.currentNode.description;

        // 销毁旧按钮
        foreach (var btn in currentButtons)
            Destroy(btn.gameObject);
        currentButtons.Clear();

        // 动态生成新选项按钮
        foreach (var opt in qm.GetAvailableOptions())
        {
            // 实例化按钮预制体
            GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = opt.optionText;

            // 绑定点击事件（使用局部变量避免闭包问题）
            Button btn = btnObj.GetComponent<Button>();
            QuestOption capturedOpt = opt;
            btn.onClick.AddListener(() => qm.ChooseOption(capturedOpt));

            currentButtons.Add(btn);
        }
    }

    /// <summary>
    /// OnDestroy：取消订阅事件，防止内存泄漏
    /// </summary>
    void OnDestroy()
    {
        if (qm != null) qm.OnQuestChanged -= RefreshUI;
    }
}
