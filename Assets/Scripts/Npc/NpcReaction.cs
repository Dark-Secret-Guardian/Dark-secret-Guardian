using UnityEngine;

/// <summary>
/// NPC 反应系统 —— 监听生态值阈值变化，生成对应的 NPC 对话文本
/// 生态等级：繁荣期(≥81) → 警示期(≥61) → 危机期(≥41) → 崩塌期(≥21) → 死寂期(<21)
/// </summary>
public class NpcReaction : MonoBehaviour
{
    private GameManager gm;

    /// <summary>
    /// Start：获取 GameManager 引用并订阅生态阈值事件
    /// </summary>
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            // 订阅生态阈值跨越事件
            gm.OnEcologyThresholdCrossed += OnEcologyChanged;
        }
        else
        {
            Debug.LogError("未找到 GameManager！");
        }
    }

    /// <summary>
    /// 生态值变化回调 —— 根据当前生态值等级生成 NPC 反应文本
    /// </summary>
    /// <param name="currentEcology">当前生态值</param>
    private void OnEcologyChanged(int currentEcology)
    {
        string message = "";

        // 根据生态值范围生成对应的 NPC 对话
        if (currentEcology >= 81)
            message = "【繁荣期】商人笑容满面：'灵脉贸易带来了前所未有繁荣！'";
        else if (currentEcology >= 61)
            message = "【警示期】老德鲁伊皱眉：'灵脉变得不稳定，动物开始迁徙...'";
        else if (currentEcology >= 41)
            message = "【危机期】卫兵紧张报告：'野外出现狂暴魔物，请小心！'";
        else if (currentEcology >= 21)
            message = "【崩塌期】灵使莉莉安哀叹：'大地在哭泣，灵脉快要枯竭了...'";
        else
            message = "【死寂期】死寂中仿佛听见：'太迟了...文明已随灵脉一同逝去。'";

        // 当前仅输出到控制台，可后续接入对话 UI
    }

    /// <summary>
    /// OnDestroy：取消订阅事件，防止内存泄漏
    /// </summary>
    void OnDestroy()
    {
        if (gm != null)
            gm.OnEcologyThresholdCrossed -= OnEcologyChanged;
    }
}
