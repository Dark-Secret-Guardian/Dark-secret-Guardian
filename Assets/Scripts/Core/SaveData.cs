using System;

/// <summary>
/// 存档数据类 —— 可序列化为 JSON 保存到 PlayerPrefs
/// 包含四维核心数值和角色六项属性
/// </summary>
[Serializable]
public class SaveData
{
    // ========== 四维核心数值 ==========
    public int prosperity;    // 繁荣度
    public int ecology;       // 生态值
    public int awakening;    // 觉醒值
    public int guardianship; // 守护值

    // ========== 角色六项属性 ==========
    public int strength;      // 力量
    public int dexterity;     // 敏捷
    public int constitution;  // 体质
    public int intelligence;  // 智力
    public int wisdom;        // 感知
    public int charisma;      // 魅力

    // 预留：任务进度（可后续扩展）
    // public List<string> completedTasks;

    /// <summary>
    /// 构造函数：从 GameManager 和角色属性中提取存档数据
    /// </summary>
    public SaveData(GameManager gm, CharacterAttributes attrs)
    {
        // 保存四维数值
        prosperity = gm.prosperity;
        ecology = gm.ecology;
        awakening = gm.awakening;
        guardianship = gm.guardianship;

        // 保存角色属性（如果已初始化）
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

    /// <summary>
    /// 将存档数据应用到 GameManager
    /// 1. 恢复四维数值
    /// 2. 重建角色属性对象（并重新计算战斗属性）
    /// 3. 触发 UI 刷新
    /// </summary>
    public void ApplyTo(GameManager gm)
    {
        // 恢复四维数值
        gm.prosperity = prosperity;
        gm.ecology = ecology;
        gm.awakening = awakening;
        gm.guardianship = guardianship;

        // 重新创建角色属性对象（会触发战斗属性重建）
        CharacterAttributes loadedAttrs = new CharacterAttributes(
            strength, dexterity, constitution,
            intelligence, wisdom, charisma
        );
        gm.InitializeCharacterAttributes(loadedAttrs);

        // 触发 UI 刷新
        gm.RefreshUI();
    }
}
