using System;
using System.Collections.Generic;

[Serializable]
public class QuestNode
{
    public string nodeId;
    public string description;
    public List<QuestCondition> conditions;
    public List<QuestOption> options;
}

[Serializable]
public class QuestOption
{
    public string optionText;
    public string targetNodeId;
    public List<QuestAction> actions;
}

[Serializable]
public class QuestAction
{
    public enum ActionType { AddProsperity, AddEcology, AddAwakening, AddGuardianship, LoadScene, TriggerEnding, StartCombat }
    public ActionType type;
    public int intValue;
    public string stringValue;
}
