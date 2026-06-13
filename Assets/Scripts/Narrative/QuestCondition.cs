using System;

/// <summary>
/// 任务条件抽象基类 —— 用于条件式选项过滤
/// 可在 QuestNode.conditions 中配置，控制选项是否可见
/// </summary>
[Serializable]
public abstract class QuestCondition
{
    /// <summary>
    /// 判断条件是否满足
    /// </summary>
    /// <param name="gm">GameManager 引用，用于读取当前数值</param>
    /// <returns>条件是否满足</returns>
    public abstract bool IsMet(GameManager gm);
}

/// <summary>
/// 觉醒值阈值条件 —— 当觉醒值达到指定最低值时满足
/// </summary>
[Serializable]
public class AwakeningThresholdCondition : QuestCondition
{
    public int minAwakening;    // 最低觉醒值要求
    public override bool IsMet(GameManager gm) => gm.awakening >= minAwakening;
}

/// <summary>
/// 生态值阈值条件 —— 当生态值达到指定最低值时满足
/// </summary>
[Serializable]
public class EcologyThresholdCondition : QuestCondition
{
    public int minEcology;     // 最低生态值要求
    public override bool IsMet(GameManager gm) => gm.ecology >= minEcology;
}
