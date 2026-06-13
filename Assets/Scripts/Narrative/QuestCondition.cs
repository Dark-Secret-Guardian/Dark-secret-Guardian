using System;

[Serializable]
public abstract class QuestCondition
{
    public abstract bool IsMet(GameManager gm);
}

[Serializable]
public class AwakeningThresholdCondition : QuestCondition
{
    public int minAwakening;
    public override bool IsMet(GameManager gm) => gm.awakening >= minAwakening;
}

[Serializable]
public class EcologyThresholdCondition : QuestCondition
{
    public int minEcology;
    public override bool IsMet(GameManager gm) => gm.ecology >= minEcology;
}
