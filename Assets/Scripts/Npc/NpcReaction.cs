using UnityEngine;

public class NpcReaction : MonoBehaviour
{
    private GameManager gm;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.OnEcologyThresholdCrossed += OnEcologyChanged;
        }
        else
        {
            Debug.LogError("未找到 GameManager！");
        }
    }

    private void OnEcologyChanged(int currentEcology)
    {
        string message = "";

        if (currentEcology >= 81)
            message = "【繁荣期】商人笑容满面：'灵脉贸易带来了前所未有繁荣！'";
        else if (currentEcology >= 61)
            message = "【警示期】老德鲁伊皱眉：'灵脉变得不稳定，动物开始迁徙...'";
        else if (currentEcology >= 41)
            message = "【危机期】卫兵紧张报告：'野外出现狂暴魔物，请小心！'";
        else if (currentEcology >= 21)
            message = "【崩塌期】灵使莉莉安哀叹：'大地在哭泣，灵脉快要枯竭了...'";
        else
            message = "【死寂期】死寂中仿佛听见：'太迟了...文明已随灵脉一同逝去。'";

    }

    private void OnDestroy()
    {
        if (gm != null)
            gm.OnEcologyThresholdCrossed -= OnEcologyChanged;
    }
}
