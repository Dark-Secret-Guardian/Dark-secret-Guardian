using UnityEngine;

public static class DiceRoller
{
    private static System.Random rng = new System.Random();

    /// <summary>
    /// 掷一个 d20
    /// </summary>
    public static int RollD20()
    {
        return rng.Next(1, 21);
    }

    /// <summary>
    /// 进行技能检定
    /// </summary>
    /// <param name="attributeModifier">属性调整值</param>
    /// <param name="proficiencyBonus">熟练加值（默认0）</param>
    /// <returns>最终结果</returns>
    public static int SkillCheck(int attributeModifier, int proficiencyBonus = 0)
    {
        return RollD20() + attributeModifier + proficiencyBonus;
    }

    /// <summary>
    /// 对抗难度等级（DC）
    /// </summary>
    public static bool IsSuccess(int total, int difficultyClass)
    {
        return total >= difficultyClass;
    }

    /// <summary>
    /// 解析伤害骰子字符串，如 "2d6+3" 或 "1d8"
    /// </summary>
    public static int RollDamage(string diceNotation)
    {
        diceNotation = diceNotation.ToLower().Replace(" ", "");
        int plusIndex = diceNotation.IndexOf('+');
        int minusIndex = diceNotation.IndexOf('-');
        int mod = 0;
        string dicePart;

        if (plusIndex != -1)
        {
            mod = int.Parse(diceNotation.Substring(plusIndex + 1));
            dicePart = diceNotation.Substring(0, plusIndex);
        }
        else if (minusIndex > 0)
        {
            mod = -int.Parse(diceNotation.Substring(minusIndex + 1));
            dicePart = diceNotation.Substring(0, minusIndex);
        }
        else
        {
            dicePart = diceNotation;
        }

        string[] parts = dicePart.Split('d');
        if (parts.Length != 2) return 0;
        int numDice = int.Parse(parts[0]);
        int dieSides = int.Parse(parts[1]);

        int total = 0;
        for (int i = 0; i < numDice; i++)
        {
            total += rng.Next(1, dieSides + 1);
        }
        total += mod;
        return total;
    }
}
