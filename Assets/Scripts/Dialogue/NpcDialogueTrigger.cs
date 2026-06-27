using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NPC 对话触发器 —— 挂在 NPC 物体上
/// 玩家靠近时在 NPC 头顶显示"对话"按钮，点击后加载 JSON 并打开对话界面
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class NpcDialogueTrigger : MonoBehaviour
{
    [Header("对话设置")]
    [Tooltip("Resources 下的 JSON 对话文件路径（不含扩展名），如 Dialogues/tavern_keeper")]
    public string dialogueFile = "";

    [Tooltip("NPC 显示名称")]
    public string npcDisplayName = "NPC";

    [Header("任务完成后对话")]
    [Tooltip("任务完成标志（PlayerPrefs key），留空则不检测")]
    public string questCompleteFlag = "";

    [Tooltip("任务完成后从哪个节点开始对话，如 t0")]
    public string postQuestStartNodeId = "";

    private bool playerInRange = false;
    private GameObject dialogueButtonObj;
    private Button dialogueButton;
    private TextMeshProUGUI dialogueButtonText;

    void Start()
    {
        SetupDialogueButton();
    }

    /// <summary>
    /// 创建"对话"按钮 UI（跟随 NPC 头顶）
    /// </summary>
    private void SetupDialogueButton()
    {
        DialogueUI.EnsureExists();

        dialogueButtonObj = new GameObject("[NpcDialogueButton_" + npcDisplayName + "]");
        dialogueButtonObj.transform.SetParent(DialogueUI.Instance?.transform, false);

        Canvas canvas = dialogueButtonObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 899;

        var scaler = dialogueButtonObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        dialogueButtonObj.AddComponent<GraphicRaycaster>();

        // 按钮背景
        GameObject btnObj = new GameObject("DialogueButton");
        btnObj.transform.SetParent(canvas.transform, false);
        dialogueButton = btnObj.AddComponent<Button>();
        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = Vector2.zero;
        btnRt.sizeDelta = new Vector2(120, 40);

        // 按钮文字
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        dialogueButtonText = textObj.AddComponent<TextMeshProUGUI>();
        dialogueButtonText.font = TMPFontHelper.GetFont();
        dialogueButtonText.fontSize = 20;
        dialogueButtonText.color = Color.white;
        dialogueButtonText.alignment = TextAlignmentOptions.Center;
        dialogueButtonText.enableWordWrapping = false;
        dialogueButtonText.overflowMode = TextOverflowModes.Overflow;
        dialogueButtonText.text = "对话";

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        var colors = dialogueButton.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.85f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        dialogueButton.colors = colors;

        dialogueButtonObj.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[NpcDialogue] {npcDisplayName} 触发检测: collider={other.gameObject.name}, tag={other.gameObject.tag}");

        if (!other.CompareTag("Player") && other.gameObject.name != "Player") return;
        if (playerInRange) return;
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive()) return;

        playerInRange = true;
        ShowDialogueButton();
        Debug.Log($"[NpcDialogue] 玩家进入 {npcDisplayName} 触发区，显示对话按钮");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && other.gameObject.name != "Player") return;
        if (!playerInRange) return;

        playerInRange = false;
        HideDialogueButton();
        Debug.Log($"[NpcDialogue] 玩家离开 {npcDisplayName} 触发区");
    }

    /// <summary>
    /// 显示对话按钮
    /// </summary>
    private void ShowDialogueButton()
    {
        if (dialogueButtonObj == null) return;
        dialogueButton.onClick.RemoveAllListeners();
        dialogueButton.onClick.AddListener(OnDialogueButtonClicked);
        dialogueButtonObj.SetActive(true);
    }

    /// <summary>
    /// 隐藏对话按钮
    /// </summary>
    private void HideDialogueButton()
    {
        if (dialogueButtonObj == null) return;
        dialogueButtonObj.SetActive(false);
        dialogueButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 每帧将按钮跟随 NPC 头顶（世界坐标 → 屏幕坐标）
    /// </summary>
    void Update()
    {
        if (dialogueButtonObj == null || !dialogueButtonObj.activeSelf) return;

        // 在 NPC 头顶上方偏移 2 单位
        Vector3 worldPos = transform.position + new Vector3(0, 2f, 0);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // 转换为 Canvas 内的局部坐标
        RectTransform btnRt = dialogueButton.GetComponent<RectTransform>();
        btnRt.position = screenPos;

        // 如果 NPC 在相机背后，隐藏按钮
        if (screenPos.z < 0)
            dialogueButtonObj.SetActive(false);
    }

    /// <summary>
    /// 对话按钮点击回调
    /// </summary>
    private void OnDialogueButtonClicked()
    {
        HideDialogueButton();

        DialogueUI.EnsureExists();

        DialogueTreeData tree = DialogueTreeData.LoadFromJSON(dialogueFile);
        if (tree != null)
        {
            // 检查任务完成状态，决定从哪个节点开始
            string startNode = null;
            if (!string.IsNullOrEmpty(questCompleteFlag) && !string.IsNullOrEmpty(postQuestStartNodeId))
            {
                if (QuestState.HasFlag(questCompleteFlag))
                {
                    startNode = postQuestStartNodeId;
                    Debug.Log($"[NpcDialogue] {npcDisplayName} 任务已完成，从节点 {startNode} 开始对话");
                }
            }
            DialogueUI.Instance.ShowDialogue(tree, startNode);
        }
        else
        {
            Debug.LogError($"[NpcDialogue] 无法加载对话文件: {dialogueFile}");
        }
    }

    void OnDestroy()
    {
        if (dialogueButtonObj != null)
            Destroy(dialogueButtonObj);
    }
}
