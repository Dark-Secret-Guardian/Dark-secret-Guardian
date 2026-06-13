using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aesheria.Combat;

namespace Aesheria.UI
{
    public class CombatUI : MonoBehaviour
    {
        public CombatManager combatManager;
        public TextMeshProUGUI playerHpText;
        public TextMeshProUGUI enemyHpText;
        public TextMeshProUGUI logText;
        public Button attackButton;
        public Button fleeButton;
        public GameObject combatPanel;

        private GameManager gm;

        void Start()
        {
            gm = FindObjectOfType<GameManager>();
            attackButton.onClick.AddListener(OnAttackClicked);
            fleeButton.onClick.AddListener(OnFleeClicked);
            combatPanel.SetActive(false);
        }

        public void ShowCombat(CombatManager cm)
        {
            combatManager = cm;
            combatPanel.SetActive(true);
            logText.text = $"战斗开始！{cm.player.name} VS {cm.enemy.name}";
            UpdateUI();
        }

        void UpdateUI()
        {
            if (combatManager == null) return;
            var p = combatManager.player;
            var e = combatManager.enemy;
            if (p != null)
                playerHpText.text = $"{p.name}: {p.currentHp}/{p.maxHp}";
            if (e != null)
                enemyHpText.text = $"{e.name}: {e.currentHp}/{e.maxHp}";

            attackButton.interactable = combatManager.IsPlayerTurn && combatManager.IsCombatActive;
        }

        void OnAttackClicked()
        {
            if (combatManager == null || !combatManager.IsPlayerTurn || !combatManager.IsCombatActive) return;

            string playerLog = combatManager.PlayerAttack();
            AddLog(playerLog);
            UpdateUI();

            if (!combatManager.IsCombatActive)
            {
                attackButton.interactable = false;
                return;
            }

            // Enemy turn after short delay
            string enemyLog = combatManager.EnemyTurn();
            AddLog(enemyLog);
            UpdateUI();

            if (!combatManager.IsCombatActive)
            {
                attackButton.interactable = false;
            }
        }

        void OnFleeClicked()
        {
            AddLog("你逃跑了...");
            combatManager.EndCombat(false);
            HideCombat();
        }

        void AddLog(string msg)
        {
            logText.text = msg + "\n" + logText.text;
        }

        void HideCombat()
        {
            combatPanel.SetActive(false);
        }
    }
}
