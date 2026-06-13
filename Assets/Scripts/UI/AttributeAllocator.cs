using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttributeAllocator : MonoBehaviour
{
    public Slider[] attributeSliders;      // 顺序: STR, DEX, CON, INT, WIS, CHA
    public TextMeshProUGUI[] valueTexts;
    public TextMeshProUGUI pointsLeftText;
    public Button confirmButton;

    private int[] currentValues = new int[6] { 10, 10, 10, 10, 10, 10 };
    private int totalPoints = 20;
    private int remainingPoints;

    void Start()
    {
        remainingPoints = totalPoints;
        UpdateUI();
        for (int i = 0; i < attributeSliders.Length; i++)
        {
            int index = i;
            attributeSliders[i].onValueChanged.AddListener((v) => OnSliderChanged(index, (int)v));
        }
        confirmButton.onClick.AddListener(ConfirmAttributes);
    }

    void OnSliderChanged(int idx, int newValue)
    {
        int delta = newValue - currentValues[idx];
        if (remainingPoints - delta < 0)
        {
            // 不够点数，回滚滑块
            attributeSliders[idx].value = currentValues[idx];
            return;
        }
        currentValues[idx] = newValue;
        remainingPoints -= delta;
        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < currentValues.Length; i++)
        {
            valueTexts[i].text = currentValues[i].ToString();
            attributeSliders[i].value = currentValues[i];
        }
        pointsLeftText.text = $"剩余点数: {remainingPoints}";
        confirmButton.interactable = remainingPoints == 0;
    }

    void ConfirmAttributes()
    {
        CharacterAttributes attrs = new CharacterAttributes(
            currentValues[0], currentValues[1], currentValues[2],
            currentValues[3], currentValues[4], currentValues[5]
        );
        FindObjectOfType<GameManager>().InitializeCharacterAttributes(attrs);
        gameObject.SetActive(false);
    }
}
