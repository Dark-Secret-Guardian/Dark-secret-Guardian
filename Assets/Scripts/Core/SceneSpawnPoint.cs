using UnityEngine;

/// <summary>
/// 场景出生点标记 —— 标记玩家在新场景中的出生位置
/// 由 SceneTransitionManager 在场景加载后查找匹配的 spawnPointId
/// </summary>
public class SceneSpawnPoint : MonoBehaviour
{
    [Tooltip("出生点ID，需与 SceneTransitionTrigger 的 spawnPointId 对应")]
    public string spawnPointId;
}
