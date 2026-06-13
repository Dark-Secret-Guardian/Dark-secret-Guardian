using System;

[Serializable]
public class SaveData
{
    // 核心数值
    public int prosperity;
    public int ecology;
    public int awakening;
    public int guardianship;

    // 角色属性（六项）
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int wisdom;
    public int charisma;

    // 预留：任务进度（可后续扩展）
    // public List<string> completedTasks;

    public SaveData(GameManager gm, CharacterAttributes attrs)
    {
        prosperity = gm.prosperity;
        ecology = gm.ecology;
        awakening = gm.awakening;
        guardianship = gm.guardianship;

        if (attrs != null)
        {
            strength = attrs.strength;
            dexterity = attrs.dexterity;
            constitution = attrs.constitution;
            intelligence = attrs.intelligence;
            wisdom = attrs.wisdom;
            charisma = attrs.charisma;
        }
    }

    public void ApplyTo(GameManager gm)
    {
        gm.prosperity = prosperity;
        gm.ecology = ecology;
        gm.awakening = awakening;
        gm.guardianship = guardianship;

        // 重新创建角色属性对象
        CharacterAttributes loadedAttrs = new CharacterAttributes(
            strength, dexterity, constitution,
            intelligence, wisdom, charisma
        );
        gm.InitializeCharacterAttributes(loadedAttrs);

        // 触发 UI 刷新
        gm.RefreshUI();
    }
}
