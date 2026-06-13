using UnityEngine;

namespace Aesheria.Combat
{
    [System.Serializable]
    public class Combatant
    {
        public string name;
        public int maxHp;
        public int currentHp;
        public int armorClass;
        public int attackBonus;
        public string damageDice;

        public bool IsAlive => currentHp > 0;

        public bool RollToHit(int targetAC)
        {
            int roll = DiceRoller.RollD20();
            int total = roll + attackBonus;
            return total >= targetAC;
        }

        public int RollDamage()
        {
            int damage = DiceRoller.RollDamage(damageDice);
            return damage;
        }

        public void TakeDamage(int amount)
        {
            currentHp = Mathf.Max(0, currentHp - amount);
        }
    }
}
