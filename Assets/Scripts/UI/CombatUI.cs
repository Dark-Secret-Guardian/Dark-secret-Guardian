using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aesheria.Combat;

namespace Aesheria.UI
{
    /// <summary>
    /// 战斗 UI —— 管理回合制战斗的界面交互
    /// 显示玩家/敌人 HP、战斗日志、攻击/逃跑按钮
    /// 由 GameManager.StartBattleWith 调用 ShowCombat 激活
    /// </summary>
    public class CombatUI : MonoBehaviour
    {
        public CombatManager combatManager;     // 当前战斗管理器引用
        public TextMeshProUGUI playerHpText;     // 玩家 HP 文本
        public TextMeshProUGUI enemyHpText;      // 敌人 HP 文本
        public TextMeshProUGUI logText;          // 战斗日志文本
        public Button attackButton;               // 攻击按钮
        public Button fleeButton;                 // 逃跑按钮
        public GameObject combatPanel;            // 战斗面板（整体显隐控制）

        private GameManager gm;

        /// <summary>
        /// Start：获取 GameManager 引用，绑定按钮事件，默认隐藏战斗面板
        /// </summary>
        void Start()
        {
            gm = FindObjectOfType<GameManager>();
            attackButton.onClick.AddListener(OnAttackClicked);
            fleeButton.onClick.AddListener(OnFleeClicked);
            combatPanel.SetActive(false);   // 默认隐藏战斗面板
        }

        /// <summary>
        /// 显示战斗面板
        /// 由 GameManager.StartBattleWith 调用
        /// </summary>
        /// <param name="cm">当前战斗管理器</param>
        public void ShowCombat(CombatManager cm)
        {
            combatManager = cm;
            combatPanel.SetActive(true);
            logText.text = $"战斗开始！{cm.player.name} VS {cm.enemy.name}";
            UpdateUI();
        }

        /// <summary>
        /// 刷新战斗 UI：更新 HP 文本和按钮可用状态
        /// 攻击按钮仅在玩家回合且战斗进行中时可用
        /// </summary>
        void UpdateUI()
        {
            if (combatManager == null) return;
            var p = combatManager.player;
            var e = combatManager.enemy;

            // 更新 HP 文本
            if (p != null)
                playerHpText.text = $"{p.name}: {p.currentHp}/{p.maxHp}";
            if (e != null)
                enemyHpText.text = $"{e.name}: {e.currentHp}/{e.maxHp}";

            // 攻击按钮仅在玩家回合且战斗活跃时可用
            attackButton.interactable = combatManager.IsPlayerTurn && combatManager.IsCombatActive;
        }

        /// <summary>
        /// 攻击按钮点击回调
        /// 1. 执行玩家攻击
        /// 2. 如果敌人未被击败，执行敌人回合
        /// 3. 更新 UI
        /// </summary>
        void OnAttackClicked()
        {
            // 检查战斗状态
            if (combatManager == null || !combatManager.IsPlayerTurn || !combatManager.IsCombatActive) return;

            // 玩家攻击回合
            string playerLog = combatManager.PlayerAttack();
            AddLog(playerLog);
            UpdateUI();

            // 战斗已结束，禁用攻击按钮
            if (!combatManager.IsCombatActive)
            {
                attackButton.interactable = false;
                return;
            }

            // 敌人回合
            string enemyLog = combatManager.EnemyTurn();
            AddLog(enemyLog);
            UpdateUI();

            // 战斗已结束
            if (!combatManager.IsCombatActive)
            {
                attackButton.interactable = false;
            }
        }

        /// <summary>
        /// 逃跑按钮点击回调：结束战斗（玩家失败）并隐藏面板
        /// </summary>
        void OnFleeClicked()
        {
            AddLog("你逃跑了...");
            combatManager.EndCombat(false);  // 以失败结束战斗
            HideCombat();
        }

        /// <summary>
        /// 添加战斗日志（新日志在最上方）
        /// </summary>
        /// <param name="msg">日志消息</param>
        void AddLog(string msg)
        {
            logText.text = msg + "\n" + logText.text;
        }

        /// <summary>
        /// 隐藏战斗面板
        /// </summary>
        void HideCombat()
        {
            combatPanel.SetActive(false);
        }
    }
}
