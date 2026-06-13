using UnityEngine;
using UnityEngine.Events;

namespace Aesheria.Combat
{
    public class CombatManager : MonoBehaviour
    {
        public Combatant player;
        public Combatant enemy;
        public UnityEvent OnCombatEnd;

        private bool isPlayerTurn = true;
        private bool combatActive = false;

        public bool IsPlayerTurn => isPlayerTurn;
        public bool IsCombatActive => combatActive;

        public void StartCombat(Combatant playerUnit, EnemyData enemyData)
        {
            player = playerUnit;
            enemy = new Combatant
            {
                name = enemyData.enemyName,
                maxHp = enemyData.maxHp,
                currentHp = enemyData.maxHp,
                armorClass = enemyData.armorClass,
                attackBonus = enemyData.attackBonus,
                damageDice = enemyData.damageDice
            };
            isPlayerTurn = true;
            combatActive = true;
        }

        public string PlayerAttack()
        {
            if (!combatActive || !isPlayerTurn) return "";

            string log = "";
            bool hit = player.RollToHit(enemy.armorClass);
            if (hit)
            {
                int damage = player.RollDamage();
                enemy.TakeDamage(damage);
                log = $"{player.name} 命中！造成 {damage} 点伤害";
            }
            else
            {
                log = $"{player.name} 攻击未命中";
            }

            if (!enemy.IsAlive)
            {
                EndCombat(true);
                log += $"\n{enemy.name} 被击败！战斗胜利！";
                return log;
            }

            isPlayerTurn = false;
            return log;
        }

        public string EnemyTurn()
        {
            if (!combatActive) return "";

            string log = "";
            bool hit = enemy.RollToHit(player.armorClass);
            if (hit)
            {
                int damage = enemy.RollDamage();
                player.TakeDamage(damage);
                log = $"{enemy.name} 命中！造成 {damage} 点伤害";
            }
            else
            {
                log = $"{enemy.name} 攻击未命中";
            }

            if (!player.IsAlive)
            {
                EndCombat(false);
                log += $"\n{player.name} 战败...";
                return log;
            }

            isPlayerTurn = true;
            return log;
        }

        public void EndCombat(bool playerWon)
        {
            combatActive = false;
            OnCombatEnd?.Invoke();
        }
    }
}
