using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Aesheria.Combat;

public class QuestManager : MonoBehaviour
{
    public List<QuestNode> allNodes;
    public QuestNode currentNode;

    private GameManager gm;

    public event System.Action OnQuestChanged;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();

        if (allNodes == null || allNodes.Count == 0)
            InitializeQuestData();

        currentNode = allNodes.Find(n => n.nodeId == "start");
        if (currentNode == null) Debug.LogError("没有起始任务节点！");
        OnQuestChanged?.Invoke();
    }

    private void InitializeQuestData()
    {
        allNodes = new List<QuestNode>
        {
            new QuestNode
            {
                nodeId = "start",
                description = "你在酒馆听到争论：商会和德鲁伊都在招募你。商会承诺繁荣，德鲁伊警告灵脉枯竭。",
                options = new List<QuestOption>
                {
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
            new QuestNode
            {
                nodeId = "node_commerce",
                description = "商会让你去开采新灵脉点。矿工们已经整装待发，但空气中弥漫着不安的气息。",
                options = new List<QuestOption>
                {
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
            new QuestNode
            {
                nodeId = "node_druid",
                description = "德鲁伊请求你修复受损灵脉。古老的仪式需要消耗大量资源，但灵脉的呼声越来越弱。",
                options = new List<QuestOption>
                {
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
            new QuestNode
            {
                nodeId = "node_encounter_beast",
                description = "开采的轰鸣惊扰了沉睡的灵兽！它正朝矿工冲来！",
                options = new List<QuestOption>
                {
                    new QuestOption
                    {
                        optionText = "战斗！迎战狂暴灵兽",
                        targetNodeId = "node_ending_check",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.StartCombat, stringValue = "FeralSpiritBeast" }
                        }
                    },
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
            new QuestNode
            {
                nodeId = "node_encounter_guardian",
                description = "灵脉深处传来低沉的咆哮——一个灵脉守卫挡在面前，考验你的决心。",
                options = new List<QuestOption>
                {
                    new QuestOption
                    {
                        optionText = "接受试炼！与灵脉守卫战斗",
                        targetNodeId = "node_ending_check",
                        actions = new List<QuestAction>
                        {
                            new QuestAction { type = QuestAction.ActionType.StartCombat, stringValue = "SpiritVeinGuardian" }
                        }
                    },
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
            new QuestNode
            {
                nodeId = "node_withered_depths",
                description = "守卫让开了路，但你发现灵脉深处有枯萎的痕迹。一只枯萎魔物正在侵蚀灵脉的核心。",
                options = new List<QuestOption>
                {
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
            new QuestNode
            {
                nodeId = "node_ending_check",
                description = "你的旅程已经影响这片土地深远。灵脉低语着，似乎在做最后的判断。",
                options = new List<QuestOption>
                {
                    new QuestOption
                    {
                        optionText = "继续探索",
                        targetNodeId = "start",
                        actions = new List<QuestAction>()
                    },
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

    public void ChooseOption(QuestOption option)
    {
        if (option.actions != null)
        {
            foreach (var action in option.actions)
            {
                ExecuteAction(action);
            }
        }

        QuestNode nextNode = allNodes.Find(n => n.nodeId == option.targetNodeId);
        if (nextNode != null)
        {
            currentNode = nextNode;
            OnQuestChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"未找到目标节点: {option.targetNodeId}");
        }
    }

    private void ExecuteAction(QuestAction action)
    {
        switch (action.type)
        {
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
            case QuestAction.ActionType.LoadScene:
                SceneManager.LoadScene(action.stringValue);
                break;
            case QuestAction.ActionType.TriggerEnding:
                gm.TriggerEnding();
                break;
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
        }
    }

    public List<QuestOption> GetAvailableOptions()
    {
        if (currentNode == null) return new List<QuestOption>();
        return currentNode.options ?? new List<QuestOption>();
    }
}
