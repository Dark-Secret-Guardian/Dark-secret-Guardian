using UnityEngine;

/// <summary>
/// 骰子工具类 —— D&D 5e 骰子模拟器
/// 提供 d20 检定、技能检定、伤害骰解析等静态方法
/// 伤害骰表达式格式："XdY+Z"（X个Y面骰+Z修正值），如 "2d6+3"、"1d8"
/// </summary>
public static class DiceRoller
{
    // 使用 System.Random 避免与 Unity 帧同步问题
    private static System.Random rng = new System.Random();

    /// <summary>
    /// 掷一个 d20（1-20）
    /// </summary>
    /// <returns>1-20 的随机整数</returns>
    public static int RollD20()
    {
        return rng.Next(1, 21);
    }

    /// <summary>
    /// 进行技能检定
    /// 公式：d20 + 属性调整值 + 熟练加值
    /// </summary>
    /// <param name="attributeModifier">属性调整值（如力量调整值、感知调整值等）</param>
    /// <param name="proficiencyBonus">熟练加值（默认 0）</param>
    /// <returns>检定结果</returns>
    public static int SkillCheck(int attributeModifier, int proficiencyBonus = 0)
    {
        return RollD20() + attributeModifier + proficiencyBonus;
    }

    /// <summary>
    /// 判断检定是否成功（结果 ≥ 难度等级）
    /// </summary>
    /// <param name="total">检定结果</param>
    /// <param name="difficultyClass">难度等级(DC)</param>
    /// <returns>是否成功</returns>
    public static bool IsSuccess(int total, int difficultyClass)
    {
        return total >= difficultyClass;
    }

    /// <summary>
    /// 解析并掷伤害骰子
    /// 支持格式："XdY+Z" 或 "XdY-Z" 或 "XdY"
    /// 例如："2d6+3" = 2个6面骰+3修正，"1d8" = 1个8面骰无修正
    /// </summary>
    /// <param name="diceNotation">伤害骰表达式</param>
    /// <returns>伤害数值</returns>
    public static int RollDamage(string diceNotation)
    {
        // 标准化：转小写、去空格
        diceNotation = diceNotation.ToLower().Replace(" ", "");

        // 解析修正值（+Z 或 -Z）
        int plusIndex = diceNotation.IndexOf('+');
        int minusIndex = diceNotation.IndexOf('-');
        int mod = 0;
        string dicePart;

        if (plusIndex != -1)
        {
            // 有加号修正，如 "2d6+3"
            mod = int.Parse(diceNotation.Substring(plusIndex + 1));
            dicePart = diceNotation.Substring(0, plusIndex);
        }
        else if (minusIndex > 0)
        {
            // 有减号修正，如 "2d6-1"（minusIndex > 0 避免匹配负号开头）
            mod = -int.Parse(diceNotation.Substring(minusIndex + 1));
            dicePart = diceNotation.Substring(0, minusIndex);
        }
        else
        {
            // 无修正值，如 "1d8"
            dicePart = diceNotation;
        }

        // 解析骰子部分 "XdY"
        string[] parts = dicePart.Split('d');
        if (parts.Length != 2) return 0;   // 格式无效，返回 0

        int numDice = int.Parse(parts[0]);     // 骰子数量 X
        int dieSides = int.Parse(parts[1]);    // 骰子面数 Y

        // 逐个掷骰并累加
        int total = 0;
        for (int i = 0; i < numDice; i++)
        {
            total += rng.Next(1, dieSides + 1);
        }

        // 加上修正值
        total += mod;
        return total;
    }
}
