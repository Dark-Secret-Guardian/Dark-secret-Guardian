using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 任务状态管理 —— 使用内存中的静态字典，游戏重启自然重置
/// 不依赖 PlayerPrefs，避免跨会话残留
/// </summary>
public static class QuestState
{
    private static readonly HashSet<string> _flags = new HashSet<string>();

    /// <summary>
    /// 设置任务标志
    /// </summary>
    public static void SetFlag(string flag)
    {
        if (!string.IsNullOrEmpty(flag))
        {
            _flags.Add(flag);
            Debug.Log($"[QuestState] 设置标志: {flag}");
        }
    }

    /// <summary>
    /// 检查任务标志是否已设置
    /// </summary>
    public static bool HasFlag(string flag)
    {
        return !string.IsNullOrEmpty(flag) && _flags.Contains(flag);
    }

    /// <summary>
    /// 清除所有任务标志
    /// </summary>
    public static void ClearAll()
    {
        _flags.Clear();
        Debug.Log("[QuestState] 所有任务标志已清除");
    }
}
