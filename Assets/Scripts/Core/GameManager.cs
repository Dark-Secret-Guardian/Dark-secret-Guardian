using System;
using UnityEngine;
using Aesheria.Core;
using Aesheria.AI;
using Aesheria.Combat;
using Aesheria.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] public int prosperity;   // 繁荣度 0-100
    [SerializeField] public int ecology;      // 生态值 0-100
    [SerializeField] public int awakening;    // 觉醒值 0-100
    [SerializeField] public int guardianship; // 守护值 0-100

    public CharacterAttributes characterAttributes;
    public Combatant playerCombatant;
    public CombatUI combatUI;

    public event Action OnValuesChanged;
    public event Action<int> OnEcologyThresholdCrossed;

    private int lastEcologyLevel;

    private enum EcologyLevel { Healthy, Warning, Crisis, Collapse, Dead }

    private PlayerAction mineAction;
    private PlayerAction restoreAction;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        prosperity = 50;
        ecology = 75;
        awakening = 0;
        guardianship = 0;
        lastEcologyLevel = GetEcologyLevel(ecology);

        mineAction = new PlayerAction("开采新灵脉点", 15, -10, 5, 0);
        restoreAction = new PlayerAction("修复灵脉网络", -10, 20, 15, 10);

        InitializePlayerCombatant();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            TakeAction(mineAction);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            TakeAction(restoreAction);
        if (Input.GetKeyDown(KeyCode.F5))
            TriggerEnding();
    }

    public void TakeAction(PlayerAction action)
    {
        UpdateValues(
            action.deltaProsperity,
            action.deltaEcology,
            action.deltaAwakening,
            action.deltaGuardianship
        );

    }

    public void UpdateValues(int deltaP, int deltaE, int deltaA, int deltaG)
    {
        prosperity = Mathf.Clamp(prosperity + deltaP, 0, 100);
        int newEcology = Mathf.Clamp(ecology + deltaE, 0, 100);

        int oldLevel = GetEcologyLevel(ecology);
        int newLevel = GetEcologyLevel(newEcology);

        ecology = newEcology;
        awakening = Mathf.Clamp(awakening + deltaA, 0, 100);
        guardianship = Mathf.Clamp(guardianship + deltaG, 0, 100);

        if (oldLevel != newLevel)
        {
            OnEcologyThresholdCrossed?.Invoke(ecology);
        }

        OnValuesChanged?.Invoke();

    }

    private int GetEcologyLevel(int value)
    {
        if (value >= 81) return 4;  // 繁荣期（健康）
        if (value >= 61) return 3;  // 警示期
        if (value >= 41) return 2;  // 危机期
        if (value >= 21) return 1;  // 崩塌期
        return 0;                   // 死寂期
    }

    public void InitializeCharacterAttributes(CharacterAttributes attrs)
    {
        characterAttributes = attrs;
        InitializePlayerCombatant();
        Debug.Log($"角色属性已设置: STR={attrs.strength} DEX={attrs.dexterity} CON={attrs.constitution} INT={attrs.intelligence} WIS={attrs.wisdom} CHA={attrs.charisma}");
    }

    // 供外部调用触发 UI 刷新
    public void RefreshUI()
    {
        OnValuesChanged?.Invoke();
    }

    public void SaveGame()
    {
        SaveData data = new SaveData(this, characterAttributes);
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SaveData"))
        {
            string json = PlayerPrefs.GetString("SaveData");
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            data.ApplyTo(this);
        }
        else
        {
            Debug.LogWarning("没有找到存档");
        }
    }

    // AI 旁白请求
    public void RequestAIDescription(Action<string> onResult)
    {
        string prompt = $"当前繁荣度{prosperity}，生态值{ecology}，觉醒值{awakening}，守护值{guardianship}。请用一句富有哲理和诗意的话描述这个世界此刻的状态，呼应繁荣与生态的矛盾。";

        AIDMClient aiClient = GetComponent<AIDMClient>();
        if (aiClient == null)
        {
            aiClient = gameObject.AddComponent<AIDMClient>();
        }

        aiClient.RequestNarrative(prompt, onResult, (error) => {
            Debug.LogWarning($"AI 请求失败: {error}");
            onResult?.Invoke("[灵脉沉默] 艾瑟瑞亚的意志暂时无法触及。");
        });
    }

    // 战斗系统
    void InitializePlayerCombatant()
    {
        int strMod = characterAttributes != null ? characterAttributes.StrMod : 0;
        int dexMod = characterAttributes != null ? characterAttributes.DexMod : 0;
        int conMod = characterAttributes != null ? characterAttributes.ConMod : 0;
        int hp = Mathf.Max(10, 10 + conMod * 2);
        int ac = Mathf.Max(10, 10 + dexMod);
        int atkBonus = Mathf.Max(2, strMod + 2);
        string dmgDice = "1d8+2";

        playerCombatant = new Combatant
        {
            name = "玩家",
            maxHp = hp,
            currentHp = hp,
            armorClass = ac,
            attackBonus = atkBonus,
            damageDice = dmgDice
        };
    }

    public void StartBattleWith(EnemyData enemy)
    {
        if (playerCombatant == null) InitializePlayerCombatant();

        GameObject cmObj = new GameObject("CombatManager");
        CombatManager cm = cmObj.AddComponent<CombatManager>();
        cm.StartCombat(playerCombatant, enemy);
        cm.OnCombatEnd.AddListener(() => {
            OnBattleEnd(cm.player.IsAlive);
            Destroy(cmObj);
        });

        if (combatUI != null)
        {
            combatUI.ShowCombat(cm);
        }
        else
        {
            combatUI = FindObjectOfType<CombatUI>();
            if (combatUI != null)
                combatUI.ShowCombat(cm);
            else
                Debug.LogWarning("CombatUI not found - battle running without UI");
        }
    }

    void OnBattleEnd(bool playerWon)
    {
        if (playerWon)
        {
            UpdateValues(0, 5, 10, 15);
        }
        else
        {
        }
    }

    public bool MakePerceptionCheck(int dc)
    {
        if (characterAttributes == null) return false;
        int total = DiceRoller.SkillCheck(characterAttributes.WisMod, 0);
        return total >= dc;
    }

    public void TriggerEnding()
    {
        var ending = EndingManager.DetermineEnding(this);
        string title = EndingManager.GetEndingTitle(ending);
        string description = EndingManager.GetEndingDescription(ending, this);

        EndingUI ui = FindObjectOfType<EndingUI>(true);
        if (ui != null)
        {
            ui.ShowEnding(title, description);
        }
        else
        {
        }
    }
}
