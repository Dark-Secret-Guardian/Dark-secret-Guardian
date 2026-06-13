using UnityEngine;

namespace Aesheria.Combat
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Aesheria/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName;
        public int maxHp;
        public int armorClass;
        public int attackBonus;
        public string damageDice;
    }
}
