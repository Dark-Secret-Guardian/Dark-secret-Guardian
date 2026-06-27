using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 灵晶交易面板 —— 墨衡交易所
/// 单例，DontDestroyOnLoad
/// 纯代码创建 UI，4 档交易选项
/// </summary>
public class TradeUI : MonoBehaviour
{
    public static TradeUI Instance { get; private set; }

    private const int CANVAS_SORTING_ORDER = 850;

    private Canvas tradeCanvas;
    private TextMeshProUGUI crystalDisplay;
    private Image[] itemBgs = new Image[4];
    private TextMeshProUGUI[] itemNames = new TextMeshProUGUI[4];
    private TextMeshProUGUI[] itemDescs = new TextMeshProUGUI[4];
    private TextMeshProUGUI[] itemCosts = new TextMeshProUGUI[4];
    private Button[] itemButtons = new Button[4];

    // 交易项定义
    private struct TradeItem
    {
        public string name;
        public string desc;
        public int cost;
        public int deltaP;
        public int deltaE;
        public int deltaA;
        public int deltaG;
    }

    private static readonly TradeItem[] TradeItems = {
        new TradeItem { name = "小额兑换", desc = "繁荣+5  生态-2", cost = 3, deltaP = 5, deltaE = -2, deltaA = 0, deltaG = 0 },
        new TradeItem { name = "中额兑换", desc = "繁荣+12  生态-6", cost = 8, deltaP = 12, deltaE = -6, deltaA = 0, deltaG = 0 },
        new TradeItem { name = "大额兑换", desc = "繁荣+25  生态-15", cost = 15, deltaP = 25, deltaE = -15, deltaA = 0, deltaG = 0 },
        new TradeItem { name = "生态修复", desc = "生态+10  繁荣-3", cost = 5, deltaP = -3, deltaE = 10, deltaA = 0, deltaG = 0 },
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupUI();
    }

    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("[TradeUI]");
            go.AddComponent<TradeUI>();
        }
    }

    private void SetupUI()
    {
        GameObject canvasObj = new GameObject("[TradeCanvas]");
        canvasObj.transform.SetParent(transform, false);

        tradeCanvas = canvasObj.AddComponent<Canvas>();
        tradeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tradeCanvas.sortingOrder = CANVAS_SORTING_ORDER;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.SetActive(false);

        // 半透明遮罩
        GameObject maskObj = new GameObject("TradeMask");
        maskObj.transform.SetParent(canvasObj.transform, false);
        Image maskImg = maskObj.AddComponent<Image>();
        maskImg.color = new Color(0, 0, 0, 0.6f);
        RectTransform maskRt = maskImg.GetComponent<RectTransform>();
        maskRt.anchorMin = Vector2.zero;
        maskRt.anchorMax = Vector2.one;
        maskRt.offsetMin = Vector2.zero;
        maskRt.offsetMax = Vector2.zero;

        // 主面板
        GameObject panelObj = new GameObject("TradePanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.14f, 0.12f, 0.10f, 0.95f);
        RectTransform panelRt = panelBg.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta = new Vector2(900, 520);

        // 标题
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.font = TMPFontHelper.GetFont();
        titleText.fontSize = 32;
        titleText.color = new Color(0.95f, 0.82f, 0.55f);
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.enableWordWrapping = false;
        titleText.text = "墨衡 · 灵晶交易所";
        RectTransform titleRt = titleText.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = new Vector2(0, -20);
        titleRt.sizeDelta = new Vector2(0, 50);

        // 灵晶数量
        GameObject crystalObj = new GameObject("CrystalDisplay");
        crystalObj.transform.SetParent(panelObj.transform, false);
        crystalDisplay = crystalObj.AddComponent<TextMeshProUGUI>();
        crystalDisplay.font = TMPFontHelper.GetFont();
        crystalDisplay.fontSize = 26;
        crystalDisplay.color = new Color(0.6f, 0.9f, 1f);
        crystalDisplay.fontStyle = FontStyles.Bold;
        crystalDisplay.alignment = TextAlignmentOptions.Center;
        crystalDisplay.enableWordWrapping = false;
        crystalDisplay.text = "◆ 灵晶: 0";
        RectTransform crystalRt = crystalDisplay.GetComponent<RectTransform>();
        crystalRt.anchorMin = new Vector2(0, 1);
        crystalRt.anchorMax = new Vector2(1, 1);
        crystalRt.pivot = new Vector2(0.5f, 1);
        crystalRt.anchoredPosition = new Vector2(0, -70);
        crystalRt.sizeDelta = new Vector2(0, 40);

        // 4 个交易项卡片
        float cardWidth = 200f;
        float cardHeight = 240f;
        float cardSpacing = 20f;
        float totalWidth = cardWidth * 4 + cardSpacing * 3;
        float startX = -totalWidth / 2f + cardWidth / 2f;

        for (int i = 0; i < 4; i++)
        {
            TradeItem item = TradeItems[i];
            float x = startX + i * (cardWidth + cardSpacing);

            // 卡片背景
            GameObject cardObj = new GameObject("Card_" + i);
            cardObj.transform.SetParent(panelObj.transform, false);
            itemBgs[i] = cardObj.AddComponent<Image>();
            itemBgs[i].color = new Color(0.2f, 0.18f, 0.15f, 0.9f);
            RectTransform cardRt = itemBgs[i].GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.anchoredPosition = new Vector2(x, -20);
            cardRt.sizeDelta = new Vector2(cardWidth, cardHeight);

            // 交易名称
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(cardObj.transform, false);
            itemNames[i] = nameObj.AddComponent<TextMeshProUGUI>();
            itemNames[i].font = TMPFontHelper.GetFont();
            itemNames[i].fontSize = 24;
            itemNames[i].color = new Color(0.95f, 0.82f, 0.55f);
            itemNames[i].fontStyle = FontStyles.Bold;
            itemNames[i].alignment = TextAlignmentOptions.Center;
            itemNames[i].enableWordWrapping = false;
            itemNames[i].text = item.name;
            RectTransform nameRt = itemNames[i].GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 1);
            nameRt.anchorMax = new Vector2(1, 1);
            nameRt.pivot = new Vector2(0.5f, 1);
            nameRt.anchoredPosition = new Vector2(0, -15);
            nameRt.sizeDelta = new Vector2(0, 30);

            // 效果描述
            GameObject descObj = new GameObject("Desc");
            descObj.transform.SetParent(cardObj.transform, false);
            itemDescs[i] = descObj.AddComponent<TextMeshProUGUI>();
            itemDescs[i].font = TMPFontHelper.GetFont();
            itemDescs[i].fontSize = 18;
            itemDescs[i].color = new Color(0.85f, 0.8f, 0.7f);
            itemDescs[i].alignment = TextAlignmentOptions.Center;
            itemDescs[i].enableWordWrapping = true;
            itemDescs[i].text = item.desc;
            RectTransform descRt = itemDescs[i].GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0, 0.5f);
            descRt.anchorMax = new Vector2(1, 0.5f);
            descRt.pivot = new Vector2(0.5f, 0.5f);
            descRt.anchoredPosition = new Vector2(0, 10);
            descRt.sizeDelta = new Vector2(-20, 60);

            // 消耗
            GameObject costObj = new GameObject("Cost");
            costObj.transform.SetParent(cardObj.transform, false);
            itemCosts[i] = costObj.AddComponent<TextMeshProUGUI>();
            itemCosts[i].font = TMPFontHelper.GetFont();
            itemCosts[i].fontSize = 20;
            itemCosts[i].color = new Color(0.6f, 0.9f, 1f);
            itemCosts[i].alignment = TextAlignmentOptions.Center;
            itemCosts[i].enableWordWrapping = false;
            itemCosts[i].text = "◆ " + item.cost + " 灵晶";
            RectTransform costRt = itemCosts[i].GetComponent<RectTransform>();
            costRt.anchorMin = new Vector2(0, 0);
            costRt.anchorMax = new Vector2(1, 0);
            costRt.pivot = new Vector2(0.5f, 0);
            costRt.anchoredPosition = new Vector2(0, 55);
            costRt.sizeDelta = new Vector2(0, 25);

            // 确认按钮
            GameObject btnObj = new GameObject("BuyBtn");
            btnObj.transform.SetParent(cardObj.transform, false);
            itemButtons[i] = btnObj.AddComponent<Button>();
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.3f, 0.25f, 0.18f, 0.9f);
            RectTransform btnRt = btnImg.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0, 0);
            btnRt.anchorMax = new Vector2(1, 0);
            btnRt.pivot = new Vector2(0.5f, 0);
            btnRt.anchoredPosition = new Vector2(0, 10);
            btnRt.sizeDelta = new Vector2(-20, 35);

            GameObject btnTextObj = new GameObject("BtnText");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.font = TMPFontHelper.GetFont();
            btnText.fontSize = 20;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.enableWordWrapping = false;
            btnText.text = "兑换";
            RectTransform btnTextRt = btnText.GetComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero;
            btnTextRt.anchorMax = Vector2.one;
            btnTextRt.offsetMin = Vector2.zero;
            btnTextRt.offsetMax = Vector2.zero;

            var btnColors = itemButtons[i].colors;
            btnColors.highlightedColor = new Color(0.4f, 0.35f, 0.25f, 0.9f);
            btnColors.pressedColor = new Color(0.2f, 0.18f, 0.15f, 0.9f);
            itemButtons[i].colors = btnColors;

            int idx = i; // 闭包捕获
            itemButtons[i].onClick.AddListener(() => OnTradeClicked(idx));
        }

        // 关闭按钮
        GameObject closeObj = new GameObject("CloseBtn");
        closeObj.transform.SetParent(panelObj.transform, false);
        Button closeBtn = closeObj.AddComponent<Button>();
        Image closeImg = closeObj.AddComponent<Image>();
        closeImg.color = new Color(0.5f, 0.3f, 0.3f, 0.9f);
        RectTransform closeRt = closeImg.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1, 1);
        closeRt.anchorMax = new Vector2(1, 1);
        closeRt.pivot = new Vector2(1, 1);
        closeRt.anchoredPosition = new Vector2(-10, -10);
        closeRt.sizeDelta = new Vector2(40, 40);

        GameObject closeTextObj = new GameObject("CloseText");
        closeTextObj.transform.SetParent(closeObj.transform, false);
        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.font = TMPFontHelper.GetFont();
        closeText.fontSize = 24;
        closeText.color = Color.white;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.enableWordWrapping = false;
        closeText.text = "✕";
        RectTransform closeTextRt = closeText.GetComponent<RectTransform>();
        closeTextRt.anchorMin = Vector2.zero;
        closeTextRt.anchorMax = Vector2.one;
        closeTextRt.offsetMin = Vector2.zero;
        closeTextRt.offsetMax = Vector2.zero;

        closeBtn.onClick.AddListener(Hide);
    }

    public void Show()
    {
        EnsureExists();
        if (Instance != this) { Instance.Show(); return; }
        RefreshCrystalDisplay();
        tradeCanvas.gameObject.SetActive(true);
        // 隐藏立绘，避免遮挡交易面板
        DialogueUI.Instance?.HidePortrait();
    }

    public void Hide()
    {
        if (Instance != this) { Instance?.Hide(); return; }
        tradeCanvas.gameObject.SetActive(false);
        // 恢复立绘显示
        DialogueUI.Instance?.ShowPortrait();
        // 交易完成后回到墨衡的菜单节点，继续对话
        DialogueUI.Instance?.ReturnToMenu();
    }

    private void RefreshCrystalDisplay()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null && crystalDisplay != null)
        {
            crystalDisplay.text = $"◆ 灵晶: {gm.spiritCrystals}";
        }
    }

    private void OnTradeClicked(int index)
    {
        TradeItem item = TradeItems[index];
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null) return;

        // 检查灵晶是否足够
        if (gm.spiritCrystals < item.cost)
        {
            FloatingNotification.EnsureExists();
            FloatingNotification.Instance?.Show("灵晶不足！");
            return;
        }

        // 执行交易
        gm.AddSpiritCrystals(-item.cost);
        gm.UpdateValues(item.deltaP, item.deltaE, item.deltaA, item.deltaG);

        // 显示通知
        List<string> msgs = new List<string>();
        if (item.deltaP != 0) msgs.Add($"繁荣 {(item.deltaP >= 0 ? "+" : "")}{item.deltaP}");
        if (item.deltaE != 0) msgs.Add($"生态 {(item.deltaE >= 0 ? "+" : "")}{item.deltaE}");
        msgs.Add($"灵晶 -{item.cost}");
        FloatingNotification.EnsureExists();
        FloatingNotification.Instance?.Show(msgs);

        // 刷新显示
        RefreshCrystalDisplay();
    }
}
