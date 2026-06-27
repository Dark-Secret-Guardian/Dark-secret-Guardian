using UnityEngine;

/// <summary>
/// 结局管理器 —— 静态工具类，根据四维数值判定结局类型
/// 
/// 结局优先级：
/// 1. 崩塌结局（任意一维归零）—— 最高优先级
/// 2. 均衡共生：觉醒≥85 且 守护≥70
/// 3. 繁荣之囚：繁荣≥80 且 生态≤30
/// 4. 从零重生：生态≤20 且 繁荣≤30
/// 5. 未竟之途：以上条件均不满足
/// </summary>
public class EndingManager : MonoBehaviour
{
    /// <summary>
    /// 结局类型枚举
    /// </summary>
    public enum EndingType { ProsperityPrison, RebirthFromZero, BalancedSymbiosis, Unfinished, ZeroCollapse }

    /// <summary>
    /// 判定结局类型
    /// </summary>
    public static EndingType DetermineEnding(GameManager gm)
    {
        // 最高优先级：任意一维归零 → 崩塌结局
        if (gm.prosperity <= 0 || gm.ecology <= 0 || gm.awakening <= 0 || gm.guardianship <= 0)
        {
            return EndingType.ZeroCollapse;
        }

        // 均衡共生（最佳结局）
        if (gm.awakening >= 85 && gm.guardianship >= 70)
            return EndingType.BalancedSymbiosis;
        // 繁荣之囚：过度开发导致生态崩溃
        else if (gm.prosperity >= 80 && gm.ecology <= 30)
            return EndingType.ProsperityPrison;
        // 从零重生：文明与自然双双崩溃后的重生
        else if (gm.ecology <= 20 && gm.prosperity <= 30)
            return EndingType.RebirthFromZero;
        // 未竟之途：以上条件均不满足
        else
            return EndingType.Unfinished;
    }

    /// <summary>
    /// 获取结局标题
    /// </summary>
    public static string GetEndingTitle(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.ProsperityPrison: return "永续繁荣囚笼";
            case EndingType.RebirthFromZero: return "归零重生";
            case EndingType.BalancedSymbiosis: return "平衡共生";
            case EndingType.ZeroCollapse: return "崩塌";
            default: return "未竟之路";
        }
    }

    /// <summary>
    /// 获取结局描述文字
    /// </summary>
    public static string GetEndingDescription(EndingType ending, GameManager gm)
    {
        switch (ending)
        {
            case EndingType.ZeroCollapse:
                return GetZeroCollapseDescription(gm);

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

    /// <summary>
    /// 获取崩塌结局的具体描述（根据哪个维度归零）
    /// </summary>
    private static string GetZeroCollapseDescription(GameManager gm)
    {
        if (gm.ecology <= 0)
        {
            return $"灵脉断绝，草木枯死，走兽绝迹。\n\n" +
                   $"生态值已归零。大地的血脉不再流淌，这片土地变成了无生之域。\n\n" +
                   $"繁荣 {gm.prosperity}，生态 0，觉醒 {gm.awakening}，守护 {gm.guardianship}。\n\n" +
                   $"你听见了青檀最后的叹息，然后是一切归于沉寂。";
        }
        if (gm.prosperity <= 0)
        {
            return $"商号倒闭，街道荒废，百姓离散。\n\n" +
                   $"繁荣度已归零。文明的灯火熄灭，村镇回归蛮荒。\n\n" +
                   $"繁荣 0，生态 {gm.ecology}，觉醒 {gm.awakening}，守护 {gm.guardianship}。\n\n" +
                   $"山还在，水还在，但人走了。也许这是自然的选择。";
        }
        if (gm.awakening <= 0)
        {
            return $"你闭上双眼，不再追问真相。\n\n" +
                   $"觉醒值已归零。你选择了沉睡，不愿看见繁华背后的代价。\n\n" +
                   $"繁荣 {gm.prosperity}，生态 {gm.ecology}，觉醒 0，守护 {gm.guardianship}。\n\n" +
                   $"无知是福，直到灾难来临，你才发现自己从未真正醒来。";
        }
        if (gm.guardianship <= 0)
        {
            return $"你放弃了守护，转身离去。\n\n" +
                   $"守护值已归零。古树青檀独自枯萎，再无人为自然发声。\n\n" +
                   $"繁荣 {gm.prosperity}，生态 {gm.ecology}，觉醒 {gm.awakening}，守护 0。\n\n" +
                   $"大地沉默地接受了命运，而你成为了沉默的帮凶。";
        }
        return "一切归于虚无。";
    }

    /// <summary>
    /// 获取结局对应的画面色调
    /// </summary>
    public static Color GetEndingColor(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.ProsperityPrison:
                return new Color(0.4f, 0.35f, 0.1f, 1f); // 暗金
            case EndingType.RebirthFromZero:
                return new Color(0.15f, 0.25f, 0.15f, 1f); // 暗绿
            case EndingType.BalancedSymbiosis:
                return new Color(0.15f, 0.2f, 0.3f, 1f); // 深蓝紫
            case EndingType.ZeroCollapse:
                return new Color(0.12f, 0.08f, 0.08f, 1f); // 暗红黑
            default:
                return new Color(0.1f, 0.1f, 0.12f, 1f); // 深灰蓝
        }
    }
}
