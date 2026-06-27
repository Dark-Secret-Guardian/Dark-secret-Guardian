using System;
using System.Collections.Generic;

/// <summary>
/// 任务节点 —— 叙事系统的基本单元
/// 每个节点包含描述文本和可选的选项列表
/// 玩家通过选择选项推进剧情，选项会触发数值变化、战斗或结局
/// </summary>
[Serializable]
public class QuestNode
{
    public string nodeId;                   // 节点唯一标识符
    public string description;              // 节点描述文本（显示给玩家）
    public string speaker;                  // 说话者名字（NPC 对话用，主线任务可留空）
    public string portraitPath;             // 立绘 Resources 路径（NPC 对话用）
    public List<QuestOption> options;       // 可选选项列表

    // 条件系统已移至 QuestManager 内部管理，不在此类中存储
    // （QuestCondition 是抽象类，JsonUtility 无法反序列化）
}

/// <summary>
/// 任务选项 —— 玩家可选择的行为
/// 每个选项指向下一个节点，并可附带多个动作（数值变化、战斗、结局等）
/// </summary>
[Serializable]
public class QuestOption
{
    public string optionText;              // 选项显示文本
    public string targetNodeId;            // 选择后跳转的目标节点 ID
    public List<QuestAction> actions;       // 选择后执行的动作列表
}

/// <summary>
/// 任务动作 —— 选项选中后执行的效果
/// 支持 7 种动作类型：修改四维数值、切换场景、触发结局、开始战斗
/// </summary>
[Serializable]
public class QuestAction
{
    /// <summary>
    /// 动作类型枚举
    /// </summary>
    public enum ActionType
    {
        AddProsperity,    // 增加繁荣度（intValue 为变化量，可为负）
        AddEcology,       // 增加生态值
        AddAwakening,     // 增加觉醒值
        AddGuardianship,  // 增加守护值
        LoadScene,        // 加载场景（stringValue 为场景名称）
        TriggerEnding,    // 触发结局判定
        StartCombat,      // 开始战斗（stringValue 为敌人资源名称）
        SetQuestFlag      // 设置任务标志（stringValue 为 PlayerPrefs key）
    }

    public ActionType type;    // 动作类型
    public int intValue;       // 数值型参数（用于数值变化量）
    public string stringValue; // 字符串型参数（用于场景名称、敌人名称等）
}
