using UnityEngine;
using UnityEngine.Events;

namespace Aesheria.Combat
{
    /// <summary>
    /// 战斗管理器 —— 管理回合制战斗流程
    /// D&D 5e 风格：玩家先攻 → 敌人回合 → 循环
    /// 战斗结束时触发 OnCombatEnd 事件
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        // ========== 战斗单位引用 ==========
        public Combatant player;    // 玩家战斗单位
        public Combatant enemy;     // 敌人战斗单位

        /// <summary>
        /// 战斗结束事件，由 GameManager 监听以发放奖励和清理
        /// </summary>
        public UnityEvent OnCombatEnd;

        // ========== 战斗状态 ==========
        private bool isPlayerTurn = true;   // 是否为玩家回合
        private bool combatActive = false;  // 战斗是否进行中

        // ========== 公开只读属性 ==========
        public bool IsPlayerTurn => isPlayerTurn;   // 当前是否为玩家回合
        public bool IsCombatActive => combatActive;  // 战斗是否进行中

        /// <summary>
        /// 开始战斗
        /// 从 EnemyData ScriptableObject 创建敌人战斗单位
        /// 初始化回合状态为玩家先攻
        /// </summary>
        /// <param name="playerUnit">玩家战斗单位</param>
        /// <param name="enemyData">敌人数据 ScriptableObject</param>
        public void StartCombat(Combatant playerUnit, EnemyData enemyData)
        {
            // 设置玩家战斗单位
            player = playerUnit;

            // 从 EnemyData 创建敌人战斗单位
            enemy = new Combatant
            {
                name = enemyData.enemyName,
                maxHp = enemyData.maxHp,
                currentHp = enemyData.maxHp,
                armorClass = enemyData.armorClass,
                attackBonus = enemyData.attackBonus,
                damageDice = enemyData.damageDice
            };

            // 初始化战斗状态
            isPlayerTurn = true;    // 玩家先攻
            combatActive = true;    // 战斗开始
        }

        /// <summary>
        /// 玩家攻击回合
        /// 1. 攻击检定（d20 + 攻击加值 vs 敌人 AC）
        /// 2. 命中则掷伤害骰并扣除敌人生命值
        /// 3. 敌人被击败则结束战斗
        /// 4. 否则切换到敌人回合
        /// </summary>
        /// <returns>战斗日志文本</returns>
        public string PlayerAttack()
        {
            // 检查战斗状态和回合
            if (!combatActive || !isPlayerTurn) return "";

            string log = "";

            // 攻击检定
            bool hit = player.RollToHit(enemy.armorClass);
            if (hit)
            {
                // 命中：掷伤害骰并扣除敌人生命值
                int damage = player.RollDamage();
                enemy.TakeDamage(damage);
                log = $"{player.name} 命中！造成 {damage} 点伤害";
            }
            else
            {
                // 未命中
                log = $"{player.name} 攻击未命中";
            }

            // 检查敌人是否被击败
            if (!enemy.IsAlive)
            {
                EndCombat(true);    // 玩家胜利
                log += $"\n{enemy.name} 被击败！战斗胜利！";
                return log;
            }

            // 切换到敌人回合
            isPlayerTurn = false;
            return log;
        }

        /// <summary>
        /// 敌人回合
        /// 1. 攻击检定（d20 + 攻击加值 vs 玩家 AC）
        /// 2. 命中则掷伤害骰并扣除玩家生命值
        /// 3. 玩家被击败则结束战斗
        /// 4. 否则切换回玩家回合
        /// </summary>
        /// <returns>战斗日志文本</returns>
        public string EnemyTurn()
        {
            // 检查战斗状态
            if (!combatActive) return "";

            string log = "";

            // 攻击检定
            bool hit = enemy.RollToHit(player.armorClass);
            if (hit)
            {
                // 命中：掷伤害骰并扣除玩家生命值
                int damage = enemy.RollDamage();
                player.TakeDamage(damage);
                log = $"{enemy.name} 命中！造成 {damage} 点伤害";
            }
            else
            {
                // 未命中
                log = $"{enemy.name} 攻击未命中";
            }

            // 检查玩家是否被击败
            if (!player.IsAlive)
            {
                EndCombat(false);   // 玩家战败
                log += $"\n{player.name} 战败...";
                return log;
            }

            // 切换回玩家回合
            isPlayerTurn = true;
            return log;
        }

        /// <summary>
        /// 结束战斗
        /// 设置战斗状态为非活跃，并触发 OnCombatEnd 事件
        /// </summary>
        /// <param name="playerWon">玩家是否胜利</param>
        public void EndCombat(bool playerWon)
        {
            combatActive = false;
            // 触发战斗结束事件（由 GameManager 监听：发放奖励、销毁 CombatManager）
            OnCombatEnd?.Invoke();
        }
    }
}
