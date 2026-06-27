using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Aesheria.Combat;

/// <summary>
/// 任务管理器 —— 管理叙事分支流程
/// 维护所有任务节点和当前节点，处理选项选择和动作执行
/// 支持通过 Inspector 手动配置节点，或通过代码初始化默认节点
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public List<QuestNode> allNodes;    // 所有任务节点列表
    public QuestNode currentNode;        // 当前激活的节点

    private GameManager gm;              // GameManager 引用

    /// <summary>
    /// 任务节点变化事件，供 QuestUI 监听以刷新界面
    /// </summary>
    public event System.Action OnQuestChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Start：初始化 GameManager 引用，加载任务数据，定位起始节点
    /// </summary>
    void Start()
    {
        // 获取 GameManager 引用
        gm = FindObjectOfType<GameManager>();

        // 如果 Inspector 中未配置节点，使用代码初始化默认任务数据
        if (allNodes == null || allNodes.Count == 0)
            InitializeQuestData();

        // 查找起始节点（nodeId = "start"）
        currentNode = allNodes.Find(n => n.nodeId == "start");
        if (currentNode == null) Debug.LogError("没有起始任务节点！");

        // 广播初始状态
        OnQuestChanged?.Invoke();
    }

    /// <summary>
    /// 初始化默认任务数据（7个节点的演示流程）
    /// 流程：酒馆选择 → 商会/德鲁伊分支 → 遭遇战 → 结局判定
    /// 如果 Inspector 中已配置节点，此方法不会被调用
    /// </summary>
    private void InitializeQuestData()
    {
        allNodes = new List<QuestNode>
        {
            // ========== 起始节点：酒馆选择 ==========
            new QuestNode
            {
                nodeId = "start",
                description = "你在酒馆听到争论：商会和德鲁伊都在招募你。商会承诺繁荣，德鲁伊警告灵脉枯竭。",
                options = new List<QuestOption>
                {
                    // 选项1：帮助商会（繁荣+10，生态-5）
                    new QuestOption
                    {
                        optionText = "帮助商会开采灵脉",
                        targetNodeId = "node_commerce",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.AddProsperity, intValue = 10 },
                            new QuestAction { type = QuestAction.ActionType.AddEcology, intValue = -5 }
                        }
                    },
                    // 选项2：帮助德鲁伊（繁荣-5，生态+10）
                    new QuestOption
                    {
                        optionText = "帮助德鲁伊修复灵脉",
                        targetNodeId = "node_druid",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.AddProsperity, intValue = -5 },
                            new QuestAction { type = QuestAction.ActionType.AddEcology, intValue = 10 }
                        }
                    }
                }
            },

            // ========== 商会路线：开采灵脉 ==========
            new QuestNode
            {
                nodeId = "node_commerce",
                description = "商会让你去开采新灵脉点。矿工们已经整装待发，但空气中弥漫着不安的气息。",
                options = new List<QuestOption>
                {
                    // 选项1：开采（繁荣+15，生态-10，觉醒+5）→ 触发灵兽遭遇
                    new QuestOption
                    {
                        optionText = "开采灵脉（繁荣+15，生态-10，觉醒+5）",
                        targetNodeId = "node_encounter_beast",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.AddProsperity, intValue = 15 },
                            new QuestAction { type = QuestAction.ActionType.AddEcology, intValue = -10 },
                            new QuestAction { type = QuestAction.ActionType.AddAwakening, intValue = 5 }
                        }
                    },
                    // 选项2：拒绝，转向德鲁伊（繁荣-5，生态+5，觉醒+10）
                    new QuestOption
                    {
                        optionText = "拒绝开采，转而帮助德鲁伊（繁荣-5，生态+5，觉醒+10）",
                        targetNodeId = "node_druid",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.AddProsperity, intValue = -5 },
                            new QuestAction { type = QuestAction.ActionType.AddEcology, intValue = 5 },
                            new QuestAction { type = QuestAction.ActionType.AddAwakening, intValue = 10 }
                        }
                    }
                }
            },

            // ========== 德鲁伊路线：修复灵脉 ==========
            new QuestNode
            {
                nodeId = "node_druid",
                description = "德鲁伊请求你修复受损灵脉。古老的仪式需要消耗大量资源，但灵脉的呼声越来越弱。",
                options = new List<QuestOption>
                {
                    // 选项1：修复灵脉（繁荣-10，生态+15，觉醒+10，守护+5）→ 触发守卫遭遇
                    new QuestOption
                    {
                        optionText = "修复灵脉（繁荣-10，生态+15，觉醒+10，守护+5）",
                        targetNodeId = "node_encounter_guardian",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.AddProsperity, intValue = -10 },
                            new QuestAction { type = QuestAction.ActionType.AddEcology, intValue = 15 },
                            new QuestAction { type = QuestAction.ActionType.AddAwakening, intValue = 10 },
                            new QuestAction { type = QuestAction.ActionType.AddGuardianship, intValue = 5 }
                        }
                    },
                    // 选项2：无视，转向商会（繁荣+5，生态-5）
                    new QuestOption
                    {
                        optionText = "无视请求，转向商会（繁荣+5，生态-5）",
                        targetNodeId = "node_commerce",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.AddProsperity, intValue = 5 },
                            new QuestAction { type = QuestAction.ActionType.AddEcology, intValue = -5 }
                        }
                    }
                }
            },

            // ========== 遭遇：狂暴灵兽（商会路线触发） ==========
            new QuestNode
            {
                nodeId = "node_encounter_beast",
                description = "开采的轰鸣惊扰了沉睡的灵兽！它正朝矿工冲来！",
                options = new List<QuestOption>
                {
                    // 选项1：战斗！使用狂暴灵兽数据
                    new QuestOption
                    {
                        optionText = "战斗！迎战狂暴灵兽",
                        targetNodeId = "node_ending_check",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.StartCombat, stringValue = "FeralSpiritBeast" }
                        }
                    },
                    // 选项2：撤退（生态+5，觉醒+5）
                    new QuestOption
                    {
                        optionText = "撤退，放弃开采",
                        targetNodeId = "node_druid",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.AddEcology, intValue = 5 },
                            new QuestAction { type = QuestAction.ActionType.AddAwakening, intValue = 5 }
                        }
                    }
                }
            },

            // ========== 遭遇：灵脉守卫（德鲁伊路线触发） ==========
            new QuestNode
            {
                nodeId = "node_encounter_guardian",
                description = "灵脉深处传来低沉的咆哮——一个灵脉守卫挡在面前，考验你的决心。",
                options = new List<QuestOption>
                {
                    // 选项1：战斗！使用灵脉守卫数据
                    new QuestOption
                    {
                        optionText = "接受试炼！与灵脉守卫战斗",
                        targetNodeId = "node_ending_check",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.StartCombat, stringValue = "SpiritVeinGuardian" }
                        }
                    },
                    // 选项2：以感知说服（觉醒+10，守护+10）→ 进入深层
                    new QuestOption
                    {
                        optionText = "以感知说服守卫让路",
                        targetNodeId = "node_withered_depths",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.AddAwakening, intValue = 10 },
                            new QuestAction { type = QuestAction.ActionType.AddGuardianship, intValue = 10 }
                        }
                    }
                }
            },

            // ========== 灵脉深处：枯萎魔物（守卫让路后） ==========
            new QuestNode
            {
                nodeId = "node_withered_depths",
                description = "守卫让开了路，但你发现灵脉深处有枯萎的痕迹。一只枯萎魔物正在侵蚀灵脉的核心。",
                options = new List<QuestOption>
                {
                    // 选项1：消灭枯萎魔物（战斗 + 生态+5，守护+5）
                    new QuestOption
                    {
                        optionText = "消灭枯萎魔物",
                        targetNodeId = "node_ending_check",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.StartCombat, stringValue = "WitheredAbomination" },
                            new QuestAction { type = QuestAction.ActionType.AddEcology, intValue = 5 },
                            new QuestAction { type = QuestAction.ActionType.AddGuardianship, intValue = 5 }
                        }
                    },
                    // 选项2：净化而非消灭（觉醒+15，守护+10）
                    new QuestOption
                    {
                        optionText = "尝试净化而非消灭（觉醒+15）",
                        targetNodeId = "node_ending_check",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.AddAwakening, intValue = 15 },
                            new QuestAction { type = QuestAction.ActionType.AddGuardianship, intValue = 10 }
                        }
                    }
                }
            },

            // ========== 结局判定节点 ==========
            new QuestNode
            {
                nodeId = "node_ending_check",
                description = "你的旅程已经影响这片土地深远。灵脉低语着，似乎在做最后的判断。",
                options = new List<QuestOption>
                {
                    // 选项1：继续探索（回到起点循环）
                    new QuestOption
                    {
                        optionText = "继续探索",
                        targetNodeId = "start",
                        actions = new List<QuestAction>()
                    },
                    // 选项2：触发结局判定
                    new QuestOption
                    {
                        optionText = "结束旅程，聆听灵脉的裁决",
                        targetNodeId = "node_ending_check",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.TriggerEnding }
                        }
                    }
                }
            }
        };
    }

    /// <summary>
    /// 选择一个选项
    /// 1. 执行选项附带的所有动作（数值变化、战斗、结局等）
    /// 2. 跳转到目标节点
    /// 3. 广播 OnQuestChanged 事件刷新 UI
    /// </summary>
    /// <param name="option">玩家选择的选项</param>
    public void ChooseOption(QuestOption option)
    {
        // 执行选项附带的所有动作
        if (option.actions != null)
        {
            foreach (var action in option.actions)
            {
                ExecuteAction(action);
            }
        }

        // 查找并跳转到目标节点
        QuestNode nextNode = allNodes.Find(n => n.nodeId == option.targetNodeId);
        if (nextNode != null)
        {
            currentNode = nextNode;
            OnQuestChanged?.Invoke();   // 广播节点变化事件
        }
        else
        {
            Debug.LogWarning($"未找到目标节点: {option.targetNodeId}");
        }
    }

    /// <summary>
    /// 执行一组任务动作（公开方法，供 NPC 对话系统调用）
    /// NPC 对话选项选中后调用此方法执行数值变化、战斗等动作
    /// 不会改变 QuestManager 当前主线节点
    /// </summary>
    public void ExecuteActions(List<QuestAction> actions)
    {
        if (actions == null) return;
        foreach (var action in actions)
        {
            ExecuteAction(action);
        }
    }

    /// <summary>
    /// 执行单个任务动作
    /// 根据动作类型调用对应的 GameManager 方法
    /// </summary>
    private void ExecuteAction(QuestAction action)
    {
        switch (action.type)
        {
            // 四维数值变化：调用 GameManager.UpdateValues
            case QuestAction.ActionType.AddProsperity:
                gm.UpdateValues(action.intValue, 0, 0, 0);
                break;
            case QuestAction.ActionType.AddEcology:
                gm.UpdateValues(0, action.intValue, 0, 0);
                break;
            case QuestAction.ActionType.AddAwakening:
                gm.UpdateValues(0, 0, action.intValue, 0);
                break;
            case QuestAction.ActionType.AddGuardianship:
                gm.UpdateValues(0, 0, 0, action.intValue);
                break;

            // 场景切换
            case QuestAction.ActionType.LoadScene:
                SceneManager.LoadScene(action.stringValue);
                break;

            // 触发结局判定
            case QuestAction.ActionType.TriggerEnding:
                gm.TriggerEnding();
                break;

            // 开始战斗：从 Resources/Enemies/ 加载敌人数据
            case QuestAction.ActionType.StartCombat:
                EnemyData enemy = Resources.Load<EnemyData>($"Enemies/{action.stringValue}");
                if (enemy != null)
                {
                    gm.StartBattleWith(enemy);
                }
                else
                {
                    Debug.LogWarning($"未找到敌人数据: {action.stringValue}");
                }
                break;

            // 设置任务标志（内存中的静态字典，重启自动重置）
            case QuestAction.ActionType.SetQuestFlag:
                QuestState.SetFlag(action.stringValue);
                break;
        }
    }

    /// <summary>
    /// 获取当前节点的可用选项列表
    /// 供 QuestUI 动态生成选项按钮
    /// </summary>
    /// <returns>当前节点的选项列表（不会返回 null）</returns>
    public List<QuestOption> GetAvailableOptions()
    {
        if (currentNode == null) return new List<QuestOption>();
        return currentNode.options ?? new List<QuestOption>();
    }
}
