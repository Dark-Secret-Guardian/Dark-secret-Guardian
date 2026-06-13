using UnityEngine;

namespace Aesheria.Combat
{
    /// <summary>
    /// 敌人数据 ScriptableObject
    /// 在 Unity Inspector 中通过右键 → Create → Aesheria → Enemy Data 创建
    /// 存储在 Resources/Enemies/ 目录下，供 QuestManager 的 StartCombat 动作通过名称加载
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy", menuName = "Aesheria/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName;     // 敌人名称
        public int maxHp;           // 最大生命值
        public int armorClass;      // 护甲等级（AC）
        public int attackBonus;     // 攻击加值
        public string damageDice;   // 伤害骰表达式，如 "1d6+2"
    }
}
