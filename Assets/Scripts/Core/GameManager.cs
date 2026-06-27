using System;
using UnityEngine;
using Aesheria.Core;       // 核心系统命名空间
using Aesheria.AI;         // AI 旁白命名空间
using Aesheria.Combat;     // 战斗系统命名空间
using Aesheria.UI;         // UI 系统命名空间

/// <summary>
/// 游戏主控制器 —— 管理四维数值、角色属性、战斗、存档、AI旁白和结局触发
/// 使用 DontDestroyOnLoad 确保跨场景持久化
/// </summary>
public class GameManager : MonoBehaviour
{
    // ========== 四维核心数值（0-100） ==========
    [SerializeField] public int prosperity;   // 繁荣度：文明与经济的发展程度
    [SerializeField] public int ecology;      // 生态值：灵脉与自然的健康状态
    [SerializeField] public int awakening;    // 觉醒值：对灵脉本质的理解深度
    [SerializeField] public int guardianship; // 守护值：对自然平衡的投入程度

    // ========== 货币 ==========
    [SerializeField] public int spiritCrystals = 20; // 灵晶：交易货币

    // ========== 角色与战斗引用 ==========
    public CharacterAttributes characterAttributes; // 玩家角色属性（D&D 5e 六项属性）
    public Combatant playerCombatant;               // 玩家战斗单位（HP、AC、攻击等）
    public CombatUI combatUI;                       // 战斗 UI 面板引用

    // ========== 事件系统 ==========
    public event Action OnValuesChanged;            // 数值变化时触发，供 UI 刷新监听
    public event Action<int> OnEcologyThresholdCrossed; // 生态值跨越阈值时触发，供 NPC 反应监听

    // ========== 内部状态 ==========
    private int lastEcologyLevel;                   // 上一帧的生态等级，用于检测阈值跨越

    // 生态等级枚举：5 个阶段，从健康到死亡
    private enum EcologyLevel { Healthy, Warning, Crisis, Collapse, Dead }

    // 预定义的玩家行动（快捷键触发）
    private PlayerAction mineAction;      // 开采灵脉行动
    private PlayerAction restoreAction;   // 修复灵脉行动

    /// <summary>
    /// Awake：确保 GameManager 跨场景不被销毁
    /// </summary>
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Start：初始化四维数值、创建行动对象、初始化战斗属性
    /// </summary>
    void Start()
    {
        // 初始数值设定：四维均不为0，任意一个归零则触发结局
        prosperity = 50;
        ecology = 75;
        awakening = 10;
        guardianship = 10;
        lastEcologyLevel = GetEcologyLevel(ecology);

        // 创建两个可用行动
        mineAction = new PlayerAction("开采新灵脉点", 15, -10, 5, 0);
        restoreAction = new PlayerAction("修复灵脉网络", -10, 20, 15, 10);

        // 根据角色属性初始化战斗数值
        InitializePlayerCombatant();

        // 初始化四维数值 HUD
        StatsHUDUI.EnsureExists();

        // 初始化 AI DM 管理器
        AIDMManager.EnsureExists();
    }

    /// <summary>
    /// Update：监听快捷键
    /// 数字键1/2：执行预设行动
    /// F5：触发结局
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            TakeAction(mineAction);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            TakeAction(restoreAction);
        if (Input.GetKeyDown(KeyCode.F5))
            TriggerEnding();
    }

    /// <summary>
    /// 执行一个玩家行动，更新四维数值
    /// </summary>
    public void TakeAction(PlayerAction action)
    {
        UpdateValues(
            action.deltaProsperity,
            action.deltaEcology,
            action.deltaAwakening,
            action.deltaGuardianship
        );
    }

    /// <summary>
    /// 核心数值更新方法
    /// 1. 更新四维数值（0-100 钳制）
    /// 2. 检测生态等级变化，触发阈值事件
    /// 3. 广播 OnValuesChanged 事件供 UI 刷新
    /// </summary>
    public void UpdateValues(int deltaP, int deltaE, int deltaA, int deltaG)
    {
        // 更新繁荣度（钳制到 0-100）
        prosperity = Mathf.Clamp(prosperity + deltaP, 0, 100);

        // 暂存新生态值，用于阈值检测
        int newEcology = Mathf.Clamp(ecology + deltaE, 0, 100);

        // 获取变化前后的生态等级
        int oldLevel = GetEcologyLevel(ecology);
        int newLevel = GetEcologyLevel(newEcology);

        // 应用新生态值
        ecology = newEcology;

        // 更新觉醒值和守护值
        awakening = Mathf.Clamp(awakening + deltaA, 0, 100);
        guardianship = Mathf.Clamp(guardianship + deltaG, 0, 100);

        // 生态等级发生变化时，触发阈值事件（供 NPC 反应系统监听）
        if (oldLevel != newLevel)
        {
            OnEcologyThresholdCrossed?.Invoke(ecology);
        }

        // 广播数值变化事件，通知所有 UI 刷新
        OnValuesChanged?.Invoke();

        // 任意一维归零 → 触发结局
        if (prosperity <= 0 || ecology <= 0 || awakening <= 0 || guardianship <= 0)
        {
            TriggerEnding();
        }
    }

    /// <summary>
    /// 增减灵晶（交易货币）
    /// </summary>
    public void AddSpiritCrystals(int amount)
    {
        spiritCrystals = Mathf.Max(0, spiritCrystals + amount);
        OnValuesChanged?.Invoke();
    }
    /// 4=繁荣期, 3=警示期, 2=危机期, 1=崩塌期, 0=死寂期
    /// </summary>
    private int GetEcologyLevel(int value)
    {
        if (value >= 81) return 4;  // 繁荣期（健康）
        if (value >= 61) return 3;  // 警示期
        if (value >= 41) return 2;  // 危机期
        if (value >= 21) return 1;  // 崩塌期
        return 0;                   // 死寂期
    }

    /// <summary>
    /// 初始化角色属性，并重建战斗属性
    /// 由 AttributeAllocator（角色创建面板）在确认属性后调用
    /// </summary>
    public void InitializeCharacterAttributes(CharacterAttributes attrs)
    {
        characterAttributes = attrs;
        // 属性变化后需要重建战斗数值（HP、AC、攻击加值等）
        InitializePlayerCombatant();
        Debug.Log($"角色属性已设置: STR={attrs.strength} DEX={attrs.dexterity} CON={attrs.constitution} INT={attrs.intelligence} WIS={attrs.wisdom} CHA={attrs.charisma}");
    }

    /// <summary>
    /// 供外部调用的 UI 刷新方法（因为事件无法从外部直接触发）
    /// </summary>
    public void RefreshUI()
    {
        OnValuesChanged?.Invoke();
    }

    /// <summary>
    /// 保存游戏：将四维数值和角色属性序列化为 JSON，存入 PlayerPrefs
    /// </summary>
    public void SaveGame()
    {
        SaveData data = new SaveData(this, characterAttributes);
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 加载游戏：从 PlayerPrefs 读取 JSON，反序列化并应用到当前状态
    /// </summary>
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

    /// <summary>
    /// 请求 AI 旁白描述
    /// 构建包含当前四维数值的提示词，通过 AIDMClient 发送给 DeepSeek API
    /// 如果 API 请求失败，返回默认的模拟文本
    /// </summary>
    public void RequestAIDescription(Action<string> onResult)
    {
        // 构建提示词：包含当前四维数值
        string prompt = $"当前繁荣度{prosperity}，生态值{ecology}，觉醒值{awakening}，守护值{guardianship}。请用一句富有哲理和诗意的话描述这个世界此刻的状态，呼应繁荣与生态的矛盾。";

        // 获取或添加 AIDMClient 组件
        AIDMClient aiClient = GetComponent<AIDMClient>();
        if (aiClient == null)
        {
            aiClient = gameObject.AddComponent<AIDMClient>();
        }

        // 发送请求，提供成功和失败回调
        aiClient.RequestNarrative(prompt, onResult, (error) => {
            Debug.LogWarning($"AI 请求失败: {error}");
            onResult?.Invoke("[灵脉沉默] 艾瑟瑞亚的意志暂时无法触及。");
        });
    }

    // ========== 战斗系统 ==========

    /// <summary>
    /// 根据角色属性初始化玩家战斗单位
    /// D&D 5e 规则：
    /// - HP = 10 + 体质调整值 × 2
    /// - AC = 10 + 敏捷调整值
    /// - 攻击加值 = 力量调整值 + 2
    /// - 伤害骰 = 1d8+2（基础武器）
    /// 如果角色属性尚未初始化，使用保底值
    /// </summary>
    void InitializePlayerCombatant()
    {
        // 获取属性调整值，如果属性未初始化则默认为 0
        int strMod = characterAttributes != null ? characterAttributes.StrMod : 0;
        int dexMod = characterAttributes != null ? characterAttributes.DexMod : 0;
        int conMod = characterAttributes != null ? characterAttributes.ConMod : 0;

        // 根据 D&D 5e 规则计算战斗属性，使用 Mathf.Max 保证最低值
        int hp = Mathf.Max(10, 10 + conMod * 2);   // 生命值
        int ac = Mathf.Max(10, 10 + dexMod);        // 护甲等级
        int atkBonus = Mathf.Max(2, strMod + 2);    // 攻击加值
        string dmgDice = "1d8+2";                    // 伤害骰

        // 创建玩家战斗单位
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

    /// <summary>
    /// 开始一场战斗
    /// 1. 确保玩家战斗属性已初始化
    /// 2. 创建 CombatManager 并初始化战斗
    /// 3. 注册战斗结束回调（销毁 CombatManager 并发放奖励）
    /// 4. 显示战斗 UI
    /// </summary>
    public void StartBattleWith(EnemyData enemy)
    {
        // 确保玩家战斗属性已初始化
        if (playerCombatant == null) InitializePlayerCombatant();

        // 动态创建 CombatManager GameObject（战斗结束后自动销毁）
        GameObject cmObj = new GameObject("CombatManager");
        CombatManager cm = cmObj.AddComponent<CombatManager>();
        cm.StartCombat(playerCombatant, enemy);

        // 注册战斗结束回调：发放奖励并销毁 CombatManager
        cm.OnCombatEnd.AddListener(() => {
            OnBattleEnd(cm.player.IsAlive);
            Destroy(cmObj); // 防止内存泄漏
        });

        // 显示战斗 UI（优先使用缓存的引用，否则动态查找）
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

    /// <summary>
    /// 战斗结束回调
    /// 胜利：生态值+5，觉醒+10，守护+15
    /// 失败：暂无惩罚逻辑
    /// </summary>
    void OnBattleEnd(bool playerWon)
    {
        if (playerWon)
        {
            UpdateValues(0, 5, 10, 15);
        }
        else
        {
            // 战败暂无惩罚逻辑，可后续扩展
        }
    }

    /// <summary>
    /// 进行察觉检定（D&D 5e 技能检定）
    /// d20 + 感知调整值 vs 难度等级(DC)
    /// </summary>
    public bool MakePerceptionCheck(int dc)
    {
        if (characterAttributes == null) return false;
        int total = DiceRoller.SkillCheck(characterAttributes.WisMod, 0);
        return total >= dc;
    }

    /// <summary>
    /// 触发结局
    /// 根据当前四维数值由 EndingManager 判定结局类型
    /// 显示结局 UI 面板
    /// </summary>
    public void TriggerEnding()
    {
        // 判定结局类型
        var ending = EndingManager.DetermineEnding(this);
        string title = EndingManager.GetEndingTitle(ending);
        string description = EndingManager.GetEndingDescription(ending, this);
        Color bgColor = EndingManager.GetEndingColor(ending);

        // 显示结局 UI（纯代码单例，自动创建）
        EndingUI.EnsureExists();
        EndingUI.Instance?.ShowEnding(title, description, bgColor);
    }
}
