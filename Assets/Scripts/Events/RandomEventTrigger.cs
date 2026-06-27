using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 街道随机事件触发器 —— 玩家进入街道后延迟触发随机事件
/// 挂在街道场景中央区域，使用 BoxCollider2D 检测玩家
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class RandomEventTrigger : MonoBehaviour
{
    // 事件池：JSON 文件路径（Resources 下，不含扩展名） + 防重复标志 key
    private static readonly string[] EventPool = {
        "Dialogues/Events/street_beggar",
        "Dialogues/Events/street_merchant",
        "Dialogues/Events/street_ranger",
        "Dialogues/Events/street_miner",
        "Dialogues/Events/street_beast",
    };

    // 触发延迟（秒），等场景加载完毕
    private const float TRIGGER_DELAY = 1.5f;

    // 触发概率（0-1），不是每次进街都触发
    private const float TRIGGER_CHANCE = 0.7f;

    private bool hasTriggeredThisVisit = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggeredThisVisit) return;
        if (!other.CompareTag("Player") && other.gameObject.name != "Player") return;

        hasTriggeredThisVisit = true;
        StartCoroutine(DelayedTrigger());
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && other.gameObject.name != "Player") return;

        // 离开街道后重置，下次进入可再次触发
        hasTriggeredThisVisit = false;
    }

    private IEnumerator DelayedTrigger()
    {
        yield return new WaitForSeconds(TRIGGER_DELAY);

        // 概率检查
        if (Random.value > TRIGGER_CHANCE)
        {
            Debug.Log("[RandomEvent] 本次未触发随机事件");
            yield break;
        }

        // 对话进行中则不触发
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive())
        {
            Debug.Log("[RandomEvent] 对话进行中，跳过本次随机事件");
            yield break;
        }

        // 从事件池中选取未触发过的事件
        List<int> available = new List<int>();
        for (int i = 0; i < EventPool.Length; i++)
        {
            string flag = "street_event_" + i;
            if (!QuestState.HasFlag(flag))
                available.Add(i);
        }

        // 全部触发过，50% 概率重置后重选
        if (available.Count == 0)
        {
            if (Random.value < 0.5f)
            {
                Debug.Log("[RandomEvent] 所有事件已触发，本次不重置");
                yield break;
            }
            Debug.Log("[RandomEvent] 所有事件已触发，重置事件池");
            for (int i = 0; i < EventPool.Length; i++)
            {
                // 无法直接清除单个标志，标记为"可重复"即可
                available.Add(i);
            }
        }

        // 随机选取一个
        int eventIndex = available[Random.Range(0, available.Count)];
        string eventPath = EventPool[eventIndex];
        string eventFlag = "street_event_" + eventIndex;

        Debug.Log($"[RandomEvent] 触发事件: {eventPath} (index={eventIndex})");

        // 加载对话树
        DialogueTreeData tree = DialogueTreeData.LoadFromJSON(eventPath);
        if (tree != null)
        {
            // 标记已触发
            QuestState.SetFlag(eventFlag);

            // 禁用玩家移动
            GameObject player = GameObject.Find("Player");
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var movement = player.GetComponent<PlayerMovement>();
                if (movement != null) movement.enabled = false;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;
            }

            // 隐藏场景切换按钮
            SceneTransitionManager.Instance?.HideTransitionButton();

            // 显示事件对话
            DialogueUI.EnsureExists();
            DialogueUI.Instance.ShowDialogue(tree);
        }
        else
        {
            Debug.LogError($"[RandomEvent] 无法加载事件文件: {eventPath}");
        }
    }
}
