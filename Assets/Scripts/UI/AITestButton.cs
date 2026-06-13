using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Aesheria.UI
{
    public class AITestButton : MonoBehaviour
    {
        public TextMeshProUGUI displayText;
        public float clearDelay = 3f;

        private GameManager gm;
        private float clearTimer = 0f;

        void Start()
        {
            gm = FindObjectOfType<GameManager>();
            Button btn = GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            if (displayText == null) return;
            displayText.text = "聆听灵脉低语...";

            gm.RequestAIDescription((result) =>
            {
                displayText.text = result;
                clearTimer = clearDelay;
            });
        }

        void Update()
        {
            if (clearTimer > 0)
            {
                clearTimer -= Time.deltaTime;
                if (clearTimer <= 0 && displayText != null && displayText.text != "聆听灵脉低语...")
                {
                    displayText.text = "";
                }
            }
        }
    }
}
