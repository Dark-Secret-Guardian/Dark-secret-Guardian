using UnityEngine;

namespace Aesheria.Combat
{
    /// <summary>
    /// 战斗单位类 —— D&D 5e 风格的战斗实体
    /// 包含生命值、护甲等级、攻击加值、伤害骰等属性
    /// 支持攻击检定（d20 + 攻击加值 vs AC）和伤害骰解析
    /// </summary>
    [System.Serializable]
    public class Combatant
    {
        public string name;         // 单位名称
        public int maxHp;          // 最大生命值
        public int currentHp;      // 当前生命值
        public int armorClass;     // 护甲等级（AC），攻击需超过此值才能命中
        public int attackBonus;    // 攻击加值，加到 d20 检定结果上
        public string damageDice;  // 伤害骰表达式，如 "1d6+2"

        /// <summary>
        /// 是否存活：当前生命值大于 0
        /// </summary>
        public bool IsAlive => currentHp > 0;

        /// <summary>
        /// 攻击检定（D&D 5e 规则）
        /// 掷 d20 + 攻击加值，与目标护甲等级(AC)比较
        /// 大于等于 AC 则命中
        /// </summary>
        /// <param name="targetAC">目标的护甲等级</param>
        /// <returns>是否命中</returns>
        public bool RollToHit(int targetAC)
        {
            int roll = DiceRoller.RollD20();        // 掷 d20
            int total = roll + attackBonus;          // 加上攻击加值
            return total >= targetAC;                 // 与目标 AC 比较
        }

        /// <summary>
        /// 掷伤害骰
        /// 解析伤害骰表达式（如 "1d6+2"）并计算结果
        /// </summary>
        /// <returns>伤害数值</returns>
        public int RollDamage()
        {
            int damage = DiceRoller.RollDamage(damageDice);
            return damage;
        }

        /// <summary>
        /// 受到伤害：扣除生命值，不低于 0
        /// </summary>
        /// <param name="amount">受到的伤害值</param>
        public void TakeDamage(int amount)
        {
            currentHp = Mathf.Max(0, currentHp - amount);
        }
    }
}
