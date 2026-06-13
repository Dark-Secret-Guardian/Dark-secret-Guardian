using UnityEngine;

/// <summary>
/// 结局管理器 —— 静态工具类，根据四维数值判定结局类型
/// 4 种结局：
/// - 均衡共生（最高优先级）：觉醒≥85 且 守护≥70
/// - 繁荣之囚：繁荣≥80 且 生态≤30
/// - 从零重生：生态≤20 且 繁荣≤30
/// - 未竟之途：以上条件均不满足
/// </summary>
public class EndingManager : MonoBehaviour
{
    /// <summary>
    /// 结局类型枚举
    /// </summary>
    public enum EndingType { ProsperityPrison, RebirthFromZero, BalancedSymbiosis, Unfinished }

    /// <summary>
    /// 根据当前四维数值判定结局类型
    /// 注意：均衡共生优先级最高，代表玩家达成了最佳结局
    /// </summary>
    public static EndingType DetermineEnding(GameManager gm)
    {
        // 优先检查均衡共生（最佳结局）
        if (gm.awakening >= 85 && gm.guardianship >= 70)
            return EndingType.BalancedSymbiosis;
        // 繁荣之囚：过度开发导致生态崩溃
        else if (gm.prosperity >= 80 && gm.ecology <= 30)
            return EndingType.ProsperityPrison;
        // 从零重生：文明与自然双双崩溃后的重生
        else if (gm.ecology <= 20 && gm.prosperity <= 30)
            return EndingType.RebirthFromZero;
        // 未竟之途：故事尚未终结
        else
            return EndingType.Unfinished;
    }

    /// <summary>
    /// 获取结局标题文本
    /// </summary>
    public static string GetEndingTitle(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.ProsperityPrison: return "永续繁荣囚笼";
            case EndingType.RebirthFromZero: return "归零重生";
            case EndingType.BalancedSymbiosis: return "平衡共生";
            default: return "未竟之路";
        }
    }

    /// <summary>
    /// 获取结局描述文本，包含当前数值摘要
    /// </summary>
    public static string GetEndingDescription(EndingType ending, GameManager gm)
    {
        switch (ending)
        {
            case EndingType.ProsperityPrison:
                return $"你曾以为繁荣是文明的勋章，却不知它正成为自然的墓志铭。\n\n" +
                       $"繁荣度 {gm.prosperity}，生态值 {gm.ecology}。\n\n" +
                       $"地表繁华，地下死寂。你赢了文明的竞赛，却输给了生命本身。";
            case EndingType.RebirthFromZero:
                return $"你放下掌控欲，给了自然重生的机会。\n\n" +
                       $"繁荣度 {gm.prosperity}，生态值 {gm.ecology}。\n\n" +
                       $"荒芜大地，百年后复苏。代价惨烈，但希望犹存。";
            case EndingType.BalancedSymbiosis:
                return $"你走出了第三条路——清醒的克制，才是终极答案。\n\n" +
                       $"觉醒值 {gm.awakening}，守护值 {gm.guardianship}。\n\n" +
                       $"文明与生态和谐共存，艾瑟瑞亚的未来在你手中延续。";
            default:
                return $"故事尚未终结，你的旅程还在继续。\n\n" +
                       $"繁荣 {gm.prosperity}，生态 {gm.ecology}，觉醒 {gm.awakening}，守护 {gm.guardianship}。\n\n" +
                       $"也许下一次选择，能带来真正的平衡。";
        }
    }
}
