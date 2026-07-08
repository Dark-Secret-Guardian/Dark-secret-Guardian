using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 角色属性分配器 —— D&D 5e 20点购买系统
/// 6 项属性（STR/DEX/CON/INT/WIS/CHA）范围 8-15
/// 总可用点数 20，分配完毕后点击确认按钮创建角色
/// </summary>
public class AttributeAllocator : MonoBehaviour
{
    public Slider[] attributeSliders;      // 6 个属性滑块，顺序: STR, DEX, CON, INT, WIS, CHA
    public TextMeshProUGUI[] valueTexts;   // 6 个属性数值文本
    public TextMeshProUGUI pointsLeftText; // 剩余点数文本
    public Button confirmButton;            // 确认按钮
    public GameObject questPanel;           // 任务面板（角色创建期间隐藏，避免遮挡确认按钮）

    private int[] currentValues = new int[6] { 10, 10, 10, 10, 10, 10 }; // 当前各属性值（默认10）
    private int totalPoints = 20;     // 总可用点数（20点购买）
    private int remainingPoints;      // 剩余可分配点数

    /// <summary>
    /// Start：初始化点数、绑定滑块事件和确认按钮
    /// </summary>
    void Start()
    {
        remainingPoints = totalPoints;

        // 角色创建期间隐藏任务面板，避免其 Image 遮挡 ConfirmButton
        if (questPanel != null)
            questPanel.SetActive(false);

        UpdateSliderValues();
        UpdateTexts();

        // 绑定每个滑块的值变化事件
        for (int i = 0; i < attributeSliders.Length; i++)
        {
            int index = i; // 闭包捕获需要局部变量
            attributeSliders[i].onValueChanged.AddListener((v) => OnSliderChanged(index, (int)v));
        }

        // 绑定确认按钮
        confirmButton.onClick.AddListener(ConfirmAttributes);
    }

    /// <summary>
    /// 滑块值变化回调
    /// 计算点数差值，如果剩余点数不足则回滚滑块
    /// </summary>
    /// <param name="idx">滑块索引</param>
    /// <param name="newValue">新的属性值</param>
    void OnSliderChanged(int idx, int newValue)
    {
        int delta = newValue - currentValues[idx];
        if (remainingPoints - delta < 0)
        {
            // 点数不足，回滚滑块到当前值
            attributeSliders[idx].value = currentValues[idx];
            return;
        }
        currentValues[idx] = newValue;
        remainingPoints -= delta;
        UpdateTexts();
    }

    /// <summary>
    /// 初始化滑块值（仅在 Start 中调用一次）
    /// </summary>
    void UpdateSliderValues()
    {
        for (int i = 0; i < currentValues.Length; i++)
            attributeSliders[i].value = currentValues[i];
    }

    /// <summary>
    /// 刷新文本和按钮状态（拖拽过程中调用，不重置滑块值以免打断拖拽）
    /// 确认按钮仅在所有点数分配完毕（剩余点数为0）时可用
    /// </summary>
    void UpdateTexts()
    {
        for (int i = 0; i < currentValues.Length; i++)
            valueTexts[i].text = currentValues[i].ToString();
        pointsLeftText.text = $"剩余点数: {remainingPoints}";
        confirmButton.interactable = remainingPoints == 0;
    }

    /// <summary>
    /// 确认属性分配
    /// 创建 CharacterAttributes 对象并传递给 GameManager
    /// 隐藏角色创建面板
    /// </summary>
    void ConfirmAttributes()
    {
        // 创建角色属性对象
        CharacterAttributes attrs = new CharacterAttributes(
            currentValues[0], currentValues[1], currentValues[2], // STR, DEX, CON
            currentValues[3], currentValues[4], currentValues[5]  // INT, WIS, CHA
        );

        // 传递给 GameManager（会自动初始化战斗属性）
        FindObjectOfType<GameManager>().InitializeCharacterAttributes(attrs);

        // 恢复任务面板显示
        if (questPanel != null)
            questPanel.SetActive(true);

        // 隐藏角色创建面板
        gameObject.SetActive(false);
    }
}
