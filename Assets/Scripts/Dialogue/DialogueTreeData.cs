using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC 对话树数据 —— 基于 QuestNode 的 JSON 包装器
/// 替代旧的 DialogueData，使 NPC 对话复用 QuestNode/QuestOption/QuestAction 结构
/// 对话选项可通过 QuestAction 直接影响四维数值、触发战斗等
/// </summary>
[Serializable]
public class DialogueTreeData
{
    public string npcName;              // NPC 名称
    public string portraitPath;         // 立绘 Resources 路径
    public List<QuestNode> nodes;       // 对话节点列表
    public string startNodeId;          // 起始节点ID（留空则用 nodes[0]）

    /// <summary>
    /// 从 Resources 加载 JSON 并反序列化
    /// </summary>
    /// <param name="resourcePath">Resources 路径（不含扩展名），如 Dialogues/tavern_keeper</param>
    public static DialogueTreeData LoadFromJSON(string resourcePath)
    {
        if (string.IsNullOrEmpty(resourcePath)) return null;

        TextAsset json = Resources.Load<TextAsset>(resourcePath);
        if (json == null)
        {
            Debug.LogError($"[DialogueTree] 无法加载对话文件: {resourcePath}");
            return null;
        }

        DialogueTreeData data = JsonUtility.FromJson<DialogueTreeData>(json.text);
        if (data == null || data.nodes == null || data.nodes.Count == 0)
        {
            Debug.LogError($"[DialogueTree] 对话文件格式错误或无节点: {resourcePath}");
            return null;
        }

        return data;
    }

    /// <summary>
    /// 根据 nodeId 查找节点
    /// </summary>
    public QuestNode FindNode(string nodeId)
    {
        if (nodes == null) return null;
        return nodes.Find(n => n.nodeId == nodeId);
    }
}
