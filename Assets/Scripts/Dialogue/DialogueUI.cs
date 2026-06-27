using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 对话界面 UI 控制器 —— 纯代码创建古风羊皮纸风格对话框
/// 基于 QuestNode/QuestOption/QuestAction 数据结构
/// 单例，DontDestroyOnLoad，sortingOrder=900（低于黑屏1000）
/// </summary>
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    private const int CANVAS_SORTING_ORDER = 900;

    // 配色（古风羊皮纸风格）
    private static readonly Color PARCHMENT_BG = new Color(0.96f, 0.94f, 0.90f);  // #F5F0E6
    private static readonly Color BORDER_COLOR = new Color(0.55f, 0.49f, 0.42f);    // #8B7D6B
    private static readonly Color TEXT_COLOR = new Color(0.23f, 0.19f, 0.14f);      // #3B3024
    private static readonly Color NAME_COLOR = new Color(0.55f, 0.35f, 0.15f);     // 深褐橙色
    private static readonly Color DECORATION_BAND = new Color(0.83f, 0.79f, 0.72f); // #D4C9B8
    private static readonly Color CHOICE_BG = new Color(0.55f, 0.49f, 0.42f, 0.85f);
    private static readonly Color CHOICE_HOVER = new Color(0.40f, 0.34f, 0.27f, 0.85f);

    [Header("立绘设置")]
    [Tooltip("立绘缩放倍数，1=原始大小")]
    public float portraitScale = 3.0f;
    [Tooltip("立绘水平偏移（正=右移，负=左移）")]
    public float portraitOffsetX = -111f;
    [Tooltip("立绘底部距离对话框顶部的间距")]
    public float portraitBottomGap = 0f;

    // 各 NPC 立绘参数（按 portraitPath 区分）
    private static readonly Dictionary<string, Vector3> PortraitParams = new Dictionary<string, Vector3>
    {
        // x = scale, y = offsetX, z = bottomGap
        { "Characters/Portraits/tavern",   new Vector3(2.5f, -111f, 0f) },
        { "Characters/Portraits/exchange", new Vector3(3.0f, -111f, 0f) },
        { "Characters/Portraits/wildness", new Vector3(3.0f, -111f, 0f) },
    };

    private Canvas dialogueCanvas;
    private GameObject dialoguePanel;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI dialogueText;
    private Image portraitImage;
    private Transform choicesContainer;
    private Button clickToContinueButton;

    private DialogueTreeData currentTree;
    private string currentNodeId;
    private bool isTyping = false;
    private bool skipTyping = false;
    private bool isDialogueActive = false;

    private GameObject player;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupCanvas();
    }

    /// <summary>
    /// 确保 UI 引用有效，跨场景后可能丢失需重建
    /// </summary>
    private void EnsureCanvasValid()
    {
        if (dialogueCanvas == null || choicesContainer == null ||
            nameText == null || dialogueText == null ||
            portraitImage == null || clickToContinueButton == null)
        {
            Debug.Log("[DialogueUI] UI 引用丢失，重建 Canvas");
            SetupCanvas();
        }
    }

    /// <summary>
    /// 确保单例存在
    /// </summary>
    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("[DialogueUI]");
            go.AddComponent<DialogueUI>();
        }
    }

    /// <summary>
    /// 创建对话 Canvas 和所有 UI 元素
    /// </summary>
    private void SetupCanvas()
    {
        // 清理旧 Canvas（EnsureCanvasValid 重建时避免残留）
        if (dialogueCanvas != null && dialogueCanvas.gameObject != null)
        {
            DestroyImmediate(dialogueCanvas.gameObject);
        }

        GameObject canvasObj = new GameObject("[DialogueCanvas]");
        canvasObj.transform.SetParent(transform, false);

        dialogueCanvas = canvasObj.AddComponent<Canvas>();
        dialogueCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dialogueCanvas.sortingOrder = CANVAS_SORTING_ORDER;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.SetActive(false);

        // === 外层边框（深褐色，比内层大 10px） ===
        GameObject borderObj = new GameObject("DialogueBorder");
        borderObj.transform.SetParent(canvasObj.transform, false);
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = BORDER_COLOR;
        RectTransform borderRt = borderImg.GetComponent<RectTransform>();
        borderRt.anchorMin = new Vector2(0, 0);
        borderRt.anchorMax = new Vector2(1, 0);
        borderRt.pivot = new Vector2(0.5f, 0);
        borderRt.anchoredPosition = new Vector2(0, 20);
        borderRt.sizeDelta = new Vector2(60, 460);

        // === 内层面板（米白色羊皮纸） ===
        dialoguePanel = new GameObject("DialoguePanel");
        dialoguePanel.transform.SetParent(borderObj.transform, false);
        Image panelImg = dialoguePanel.AddComponent<Image>();
        panelImg.color = PARCHMENT_BG;
        RectTransform panelRt = panelImg.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = new Vector2(10, 10);
        panelRt.offsetMax = new Vector2(-10, -10);

        // === 顶部装饰带 ===
        CreateDecorationBand(panelImg.transform, true);

        // === 底部装饰带 ===
        CreateDecorationBand(panelImg.transform, false);

        // === NPC 名字标签 ===
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(panelImg.transform, false);
        nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.font = TMPFontHelper.GetFont();
        nameText.fontSize = 32;
        nameText.color = NAME_COLOR;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.MidlineLeft;
        nameText.enableWordWrapping = false;
        nameText.overflowMode = TextOverflowModes.Overflow;
        RectTransform nameRt = nameText.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0, 1);
        nameRt.anchorMax = new Vector2(0.6f, 1);
        nameRt.pivot = new Vector2(0, 1);
        nameRt.anchoredPosition = new Vector2(40, -30);
        nameRt.sizeDelta = new Vector2(0, 44);

        // === 对话文本区域 ===
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(panelImg.transform, false);
        dialogueText = textObj.AddComponent<TextMeshProUGUI>();
        dialogueText.font = TMPFontHelper.GetFont();
        dialogueText.fontSize = 32;
        dialogueText.color = TEXT_COLOR;
        dialogueText.lineSpacing = 1.5f;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        dialogueText.enableWordWrapping = true;
        dialogueText.overflowMode = TextOverflowModes.Overflow;
        dialogueText.raycastTarget = false;
        RectTransform textRt = dialogueText.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0, 0);
        textRt.anchorMax = new Vector2(0.7f, 1);
        textRt.pivot = new Vector2(0, 0.5f);
        textRt.offsetMin = new Vector2(40, 30);
        textRt.offsetMax = new Vector2(-10, -90);

        // === 立绘区域（对话框上方，底边紧贴对话框顶部） ===
        GameObject portraitObj = new GameObject("Portrait");
        portraitObj.transform.SetParent(canvasObj.transform, false); // 直接挂在 Canvas 下，不在面板内
        portraitImage = portraitObj.AddComponent<Image>();
        portraitImage.color = new Color(0.7f, 0.6f, 0.5f, 0.3f); // 半透明占位
        portraitImage.preserveAspect = true; // 保持原始比例，不拉伸变形
        portraitImage.raycastTarget = false;
        RectTransform portraitRt = portraitImage.GetComponent<RectTransform>();
        portraitRt.anchorMin = new Vector2(0.7f, 0);
        portraitRt.anchorMax = new Vector2(1, 1);
        portraitRt.pivot = new Vector2(0.5f, 0); // pivot 在底部中心
        portraitRt.offsetMin = new Vector2(portraitOffsetX, 480 + portraitBottomGap);
        portraitRt.offsetMax = new Vector2(portraitOffsetX, 0);
        portraitRt.localScale = new Vector3(portraitScale, portraitScale, 1f);

        // === 选项容器（底部，向上排列） ===
        // 注意：必须先 AddComponent<RectTransform> 再取 transform 引用，
        // 否则 AddComponent 会替换 Transform 导致旧引用失效（Unity fake-null）
        GameObject choicesObj = new GameObject("ChoicesContainer");
        choicesObj.transform.SetParent(panelImg.transform, false);
        RectTransform choicesRt = choicesObj.AddComponent<RectTransform>();
        choicesContainer = choicesObj.transform; // 取替换后的新引用
        choicesRt.anchorMin = new Vector2(0, 0);
        choicesRt.anchorMax = new Vector2(0.7f, 0);
        choicesRt.pivot = new Vector2(0, 0);
        choicesRt.offsetMin = new Vector2(40, 30);
        choicesRt.offsetMax = new Vector2(-10, 30);
        choicesRt.sizeDelta = new Vector2(0, 160);

        // === 点击继续按钮（全对话框可点击，当无选项时激活） ===
        GameObject clickObj = new GameObject("ClickToContinue");
        clickObj.transform.SetParent(panelImg.transform, false);
        clickToContinueButton = clickObj.AddComponent<Button>();
        Image clickImg = clickObj.AddComponent<Image>();
        clickImg.color = new Color(0, 0, 0, 0); // 透明
        RectTransform clickRt = clickImg.GetComponent<RectTransform>();
        clickRt.anchorMin = Vector2.zero;
        clickRt.anchorMax = Vector2.one;
        clickRt.offsetMin = Vector2.zero;
        clickRt.offsetMax = Vector2.zero;
        clickObj.SetActive(false);
    }

    /// <summary>
    /// 创建顶部或底部装饰带
    /// </summary>
    private void CreateDecorationBand(Transform parent, bool isTop)
    {
        GameObject band = new GameObject(isTop ? "TopBand" : "BottomBand");
        band.transform.SetParent(parent, false);
        Image bandImg = band.AddComponent<Image>();
        bandImg.color = DECORATION_BAND;
        RectTransform rt = bandImg.GetComponent<RectTransform>();
        if (isTop)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -12);
        }
        else
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 12);
        }
        rt.sizeDelta = new Vector2(-20, 6);
    }

    /// <summary>
    /// 显示对话，从指定起始节点开始
    /// </summary>
    /// <param name="startNodeIdOverride">覆盖起始节点ID，null 则用 tree.startNodeId，再为空用 nodes[0]</param>
    public void ShowDialogue(DialogueTreeData tree, string startNodeIdOverride = null)
    {
        if (tree == null || tree.nodes == null || tree.nodes.Count == 0) return;

        // 确保 UI 引用有效（跨场景后可能丢失）
        EnsureCanvasValid();

        currentTree = tree;

        // 确定起始节点：参数覆盖 > JSON startNodeId > nodes[0]
        string startId = startNodeIdOverride;
        if (string.IsNullOrEmpty(startId))
            startId = tree.startNodeId;
        if (string.IsNullOrEmpty(startId))
            startId = tree.nodes[0].nodeId;

        currentNodeId = startId;
        isDialogueActive = true;

        // 禁用玩家移动
        player = GameObject.Find("Player");
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

        // 加载立绘（对话树级别默认立绘）
        LoadPortrait(tree.portraitPath);

        dialogueCanvas.gameObject.SetActive(true);
        ShowNode(currentNodeId);
    }

    /// <summary>
    /// 加载立绘
    /// </summary>
    private void LoadPortrait(string path)
    {
        // 应用该 NPC 的立绘参数
        if (!string.IsNullOrEmpty(path) && PortraitParams.TryGetValue(path, out var p))
        {
            portraitScale = p.x;
            portraitOffsetX = p.y;
            portraitBottomGap = p.z;
        }

        if (!string.IsNullOrEmpty(path))
        {
            Sprite portrait = Resources.Load<Sprite>(path);
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.color = Color.white;
                return;
            }
        }
        portraitImage.color = new Color(0.7f, 0.6f, 0.5f, 0.3f);
    }

    /// <summary>
    /// 显示指定节点（按 nodeId 查找）
    /// </summary>
    private void ShowNode(string nodeId)
    {
        QuestNode node = currentTree.FindNode(nodeId);
        if (node == null)
        {
            HideDialogue();
            return;
        }

        currentNodeId = nodeId;

        // 先设置名字和文本
        if (nameText != null)
            nameText.text = node.speaker ?? "";

        if (dialogueText != null)
            dialogueText.text = "";

        // 节点级立绘覆盖
        if (!string.IsNullOrEmpty(node.portraitPath))
            LoadPortrait(node.portraitPath);

        // 清除旧选项（安全检查）
        if (choicesContainer != null)
            ClearChoices();

        // 点击继续按钮先关闭（安全检查）
        if (clickToContinueButton != null && clickToContinueButton.gameObject != null)
            clickToContinueButton.gameObject.SetActive(false);

        // 打字机效果
        if (dialogueText != null && !string.IsNullOrEmpty(node.description))
        {
            StartCoroutine(TypewriterEffect(node.description, () =>
            {
                OnTypingComplete(node);
            }));
        }
        else
        {
            Debug.LogWarning($"[DialogueUI] description 为空或 dialogueText 为 null");
            OnTypingComplete(node);
        }
    }

    /// <summary>
    /// 打字机效果协程
    /// </summary>
    private IEnumerator TypewriterEffect(string fullText, System.Action onComplete)
    {
        isTyping = true;
        skipTyping = false;
        dialogueText.text = "";

        // 等待一帧，避免按钮点击的 Input 残留导致立即跳过
        yield return null;

        foreach (char c in fullText)
        {
            if (skipTyping)
            {
                dialogueText.text = fullText;
                break;
            }
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        isTyping = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 打字完成后处理选项或继续按钮
    /// </summary>
    private void OnTypingComplete(QuestNode node)
    {
        if (node.options != null && node.options.Count > 0)
        {
            ShowOptions(node.options);
        }
        else
        {
            // 无选项，点击继续 → 结束对话
            if (clickToContinueButton == null) return;
            clickToContinueButton.onClick.RemoveAllListeners();
            clickToContinueButton.onClick.AddListener(() =>
            {
                HideDialogue();
            });
            clickToContinueButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 显示选项按钮（基于 QuestOption）
    /// </summary>
    private void ShowOptions(List<QuestOption> options)
    {
        if (choicesContainer == null)
        {
            Debug.LogError("[DialogueUI] choicesContainer 为 null，无法显示选项");
            return;
        }

        ClearChoices();

        float buttonHeight = 54f;
        float spacing = 10f;

        for (int i = 0; i < options.Count; i++)
        {
            QuestOption opt = options[i];
            GameObject choiceObj = new GameObject("Choice_" + i);
            choiceObj.transform.SetParent(choicesContainer, false);

            Image choiceBg = choiceObj.AddComponent<Image>();
            choiceBg.color = CHOICE_BG;

            Button choiceBtn = choiceObj.AddComponent<Button>();
            var colors = choiceBtn.colors;
            colors.normalColor = CHOICE_BG;
            colors.highlightedColor = CHOICE_HOVER;
            colors.pressedColor = new Color(0.25f, 0.20f, 0.15f, 0.85f);
            choiceBtn.colors = colors;

            RectTransform choiceRt = choiceObj.GetComponent<RectTransform>();
            choiceRt.anchorMin = new Vector2(0, 0);
            choiceRt.anchorMax = new Vector2(1, 0);
            choiceRt.pivot = new Vector2(0.5f, 0);
            // 从底部向上排列
            choiceRt.anchoredPosition = new Vector2(0, i * (buttonHeight + spacing));
            choiceRt.sizeDelta = new Vector2(0, buttonHeight);

            // 选项文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(choiceObj.transform, false);
            TextMeshProUGUI choiceText = textObj.AddComponent<TextMeshProUGUI>();
            choiceText.font = TMPFontHelper.GetFont();
            choiceText.fontSize = 24;
            choiceText.color = Color.white;
            choiceText.alignment = TextAlignmentOptions.Center;
            choiceText.enableWordWrapping = false;
            choiceText.overflowMode = TextOverflowModes.Overflow;
            RectTransform ctRt = textObj.GetComponent<RectTransform>();
            ctRt.anchorMin = Vector2.zero;
            ctRt.anchorMax = Vector2.one;
            ctRt.offsetMin = Vector2.zero;
            ctRt.offsetMax = Vector2.zero;
            choiceText.text = opt.optionText;

            choiceBtn.onClick.AddListener(() =>
            {
                OnOptionSelected(opt);
            });
        }
    }

    /// <summary>
    /// 选项点击处理：执行动作 → 跳转目标节点
    /// </summary>
    private void OnOptionSelected(QuestOption option)
    {
        ClearChoices();

        // 执行选项附带的动作（数值变化、战斗等）
        List<string> notifMessages = new List<string>();

        if (option.actions != null && option.actions.Count > 0)
        {
            QuestManager qm = QuestManager.Instance;
            if (qm != null)
            {
                qm.ExecuteActions(option.actions);
            }
            else
            {
                Debug.LogWarning("[DialogueUI] QuestManager 未找到，无法执行对话动作");
            }

            // 生成浮动提示
            foreach (var action in option.actions)
            {
                switch (action.type)
                {
                    case QuestAction.ActionType.AddProsperity:
                        notifMessages.Add($"繁荣度 {(action.intValue >= 0 ? "+" : "")}{action.intValue}");
                        break;
                    case QuestAction.ActionType.AddEcology:
                        notifMessages.Add($"生态值 {(action.intValue >= 0 ? "+" : "")}{action.intValue}");
                        break;
                    case QuestAction.ActionType.AddAwakening:
                        notifMessages.Add($"觉醒值 {(action.intValue >= 0 ? "+" : "")}{action.intValue}");
                        break;
                    case QuestAction.ActionType.AddGuardianship:
                        notifMessages.Add($"守护值 {(action.intValue >= 0 ? "+" : "")}{action.intValue}");
                        break;
                }
            }
        }

        // 显示浮动提示（如"繁荣度 +10"）
        if (notifMessages.Count > 0)
        {
            FloatingNotification.EnsureExists();
            FloatingNotification.Instance?.Show(notifMessages);
        }

        // 跳转目标节点
        if (string.IsNullOrEmpty(option.targetNodeId) || option.targetNodeId == "-1")
        {
            HideDialogue();
        }
        else
        {
            // 墨衡交易：跳转到交易节点时弹出交易面板
            if (option.targetNodeId == "trade_panel")
            {
                TradeUI.EnsureExists();
                TradeUI.Instance?.Show();
                return; // 不跳转对话节点，等待交易面板关闭
            }

            // 红袖任务触发：跳转到接任务节点时显示任务追踪
            if (option.targetNodeId == "n4" || option.targetNodeId == "n2b")
            {
                QuestTrackerUI.EnsureExists();
                QuestTrackerUI.Instance?.ShowQuest(
                    "疏通受损灵脉",
                    "前往野外找到古树青檀，帮助它疏通淤堵的灵脉。"
                );
            }

            // 青檀任务完成：跳转到告别节点时更新任务追踪
            if (option.targetNodeId == "n7" || option.targetNodeId == "n6")
            {
                QuestTrackerUI.EnsureExists();
                QuestTrackerUI.Instance?.ShowQuest(
                    "返回酒馆复命",
                    "灵脉已疏通，回到酒馆找红袖复命。"
                );
            }

            ShowNode(option.targetNodeId);
        }
    }

    /// <summary>
    /// 清除所有选项按钮
    /// </summary>
    private void ClearChoices()
    {
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(choicesContainer.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 隐藏对话 UI，恢复玩家控制
    /// </summary>
    public void HideDialogue()
    {
        isDialogueActive = false;
        dialogueCanvas.gameObject.SetActive(false);
        ClearChoices();

        // 恢复玩家移动
        if (player != null)
        {
            var movement = player.GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = true;
        }

        // 对话结束后尝试触发 AI DM 中途旁白
        AIDMManager.Instance?.TryMidGameCommentary();

        // 红袖领取奖励（t3 节点）→ 额外给予 10 灵晶
        if (currentNodeId == "t3")
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.AddSpiritCrystals(10);
                FloatingNotification.EnsureExists();
                FloatingNotification.Instance?.Show("获得灵晶 +10");
            }
        }

        // 红袖任务完成后对话结束 → 隐藏任务追踪
        if (currentNodeId == "t1" || currentNodeId == "t2" || currentNodeId == "t3" || currentNodeId == "n_end")
        {
            // t1/t2/t3 是红袖任务完成后的对话节点，n_end 可能是任意结束节点
            // 只有在 t 系列节点时才隐藏
            if (currentNodeId.StartsWith("t"))
            {
                QuestTrackerUI.Instance?.HideQuest();
            }
        }
    }

    /// <summary>
    /// 对话是否正在进行
    /// </summary>
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    /// <summary>
    /// 隐藏立绘（交易面板等弹出时调用）
    /// </summary>
    public void HidePortrait()
    {
        if (portraitImage != null)
            portraitImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 恢复立绘显示
    /// </summary>
    public void ShowPortrait()
    {
        if (portraitImage != null)
            portraitImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// 交易面板关闭后回到菜单节点（如墨衡的 n0b）
    /// </summary>
    public void ReturnToMenu()
    {
        if (currentTree == null) return;

        // 查找菜单节点：优先 n0b，否则 n0
        string menuId = "n0b";
        QuestNode menuNode = currentTree.FindNode(menuId);
        if (menuNode == null)
            menuNode = currentTree.FindNode("n0");

        if (menuNode != null)
        {
            ShowNode(menuNode.nodeId);
        }
        else
        {
            // 找不到菜单节点，结束对话
            HideDialogue();
        }
    }

    void Update()
    {
        // 仅鼠标左键跳过打字效果（避免 Input.anyKeyDown 误触按钮）
        if (isTyping && Input.GetMouseButtonDown(0))
        {
            skipTyping = true;
        }

        // 运行时实时调整立绘参数（Play 模式下在 Inspector 调整即可生效）
        if (portraitImage != null && dialogueCanvas != null && dialogueCanvas.gameObject.activeSelf)
        {
            RectTransform rt = portraitImage.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.offsetMin = new Vector2(portraitOffsetX, 480 + portraitBottomGap);
                rt.offsetMax = new Vector2(portraitOffsetX, 0);
                rt.localScale = new Vector3(portraitScale, portraitScale, 1f);
            }
        }
    }
}
