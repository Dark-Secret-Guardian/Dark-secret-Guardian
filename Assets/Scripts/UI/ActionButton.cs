using UnityEngine;
using UnityEngine.UI;
using Aesheria.Core;

/// <summary>
/// 行动按钮 —— 绑定一个 PlayerAction，点击时执行该行动
/// 用于 HUD 中的快捷行动按钮（开采灵脉、修复灵脉等）
/// </summary>
public class ActionButton : MonoBehaviour
{
    public PlayerAction action;     // 绑定的玩家行动
    private GameManager gm;

    /// <summary>
    /// Start：获取 GameManager 引用，绑定按钮点击事件
    /// </summary>
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        Button btn = GetComponent<Button>();
        if (btn != null && action != null)
        {
            btn.onClick.AddListener(() => gm.TakeAction(action));
        }
    }
}
