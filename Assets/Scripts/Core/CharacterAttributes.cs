using System;

/// <summary>
/// 角色属性类 —— D&D 5e 标准六项属性
/// 属性值范围 8-15（20点购买系统）
/// 调整值 = Math.Floor((属性值 - 10) / 2)
/// </summary>
[Serializable]
public class CharacterAttributes
{
    public int strength;      // 力量：影响攻击加值
    public int dexterity;     // 敏捷：影响护甲等级(AC)
    public int constitution;  // 体质：影响生命值(HP)
    public int intelligence;  // 智力：影响知识检定
    public int wisdom;        // 感知：影响察觉检定
    public int charisma;      // 魅力：影响交涉检定

    /// <summary>
    /// 构造函数：创建角色属性
    /// </summary>
    public CharacterAttributes(int str, int dex, int con, int intel, int wis, int cha)
    {
        strength = str; dexterity = dex; constitution = con;
        intelligence = intel; wisdom = wis; charisma = cha;
    }

    /// <summary>
    /// 根据 D&D 5e 规则计算属性调整值
    /// 公式：(value - 10) / 2，向下取整
    /// 例：10→0, 12→+1, 14→+2, 8→-1
    /// </summary>
    public int GetModifier(int value) => (int)Math.Floor((value - 10) / 2.0);

    // ========== 便捷属性：直接获取各属性的调整值 ==========
    public int StrMod => GetModifier(strength);    // 力量调整值
    public int DexMod => GetModifier(dexterity);   // 敏捷调整值
    public int ConMod => GetModifier(constitution); // 体质调整值
    public int IntMod => GetModifier(intelligence); // 智力调整值
    public int WisMod => GetModifier(wisdom);      // 感知调整值
    public int ChaMod => GetModifier(charisma);    // 魅力调整值
}
