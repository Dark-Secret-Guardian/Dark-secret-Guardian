using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI prosperityText;
    public TextMeshProUGUI ecologyText;
    public TextMeshProUGUI awakeningText;
    public TextMeshProUGUI guardianshipText;

    private GameManager gm;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("未找到 GameManager！");
            return;
        }

        gm.OnValuesChanged += RefreshUI;
        RefreshUI();
    }

    private void RefreshUI()
    {
        prosperityText.text = $"繁荣: {gm.prosperity}";
        ecologyText.text = $"生态: {gm.ecology}";
        awakeningText.text = $"觉醒: {gm.awakening}";
        guardianshipText.text = $"守护: {gm.guardianship}";
    }

    private void OnDestroy()
    {
        if (gm != null)
            gm.OnValuesChanged -= RefreshUI;
    }
}
