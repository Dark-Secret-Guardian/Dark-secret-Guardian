using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI descriptionText;
    public Transform optionsContainer;
    public GameObject optionButtonPrefab;

    private QuestManager qm;
    private List<Button> currentButtons = new List<Button>();

    void Start()
    {
        qm = FindObjectOfType<QuestManager>();
        if (qm != null)
        {
            qm.OnQuestChanged += RefreshUI;
            RefreshUI();
        }
    }

    void RefreshUI()
    {
        if (qm == null || qm.currentNode == null) return;

        descriptionText.text = qm.currentNode.description;

        foreach (var btn in currentButtons)
            Destroy(btn.gameObject);
        currentButtons.Clear();

        foreach (var opt in qm.GetAvailableOptions())
        {
            GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = opt.optionText;
            Button btn = btnObj.GetComponent<Button>();
            QuestOption capturedOpt = opt; // avoid closure issue
            btn.onClick.AddListener(() => qm.ChooseOption(capturedOpt));
            currentButtons.Add(btn);
        }
    }

    void OnDestroy()
    {
        if (qm != null) qm.OnQuestChanged -= RefreshUI;
    }
}
