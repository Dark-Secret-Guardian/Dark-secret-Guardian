using UnityEngine;
using UnityEngine.UI;
using Aesheria.Core;

public class ActionButton : MonoBehaviour
{
    public PlayerAction action;
    private GameManager gm;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        Button btn = GetComponent<Button>();
        if (btn != null && action != null)
        {
            btn.onClick.AddListener(() => gm.TakeAction(action));
        }
    }
}
