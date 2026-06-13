using System;

[Serializable]
public class CharacterAttributes
{
    public int strength;      // 力量
    public int dexterity;     // 敏捷
    public int constitution;  // 体质
    public int intelligence;  // 智力
    public int wisdom;        // 感知
    public int charisma;      // 魅力

    public CharacterAttributes(int str, int dex, int con, int intel, int wis, int cha)
    {
        strength = str; dexterity = dex; constitution = con;
        intelligence = intel; wisdom = wis; charisma = cha;
    }

    // 根据属性值计算调整值：(value - 10) / 2，向下取整
    public int GetModifier(int value) => (int)Math.Floor((value - 10) / 2.0);

    // 便捷方法获取各调整值
    public int StrMod => GetModifier(strength);
    public int DexMod => GetModifier(dexterity);
    public int ConMod => GetModifier(constitution);
    public int IntMod => GetModifier(intelligence);
    public int WisMod => GetModifier(wisdom);
    public int ChaMod => GetModifier(charisma);
}
