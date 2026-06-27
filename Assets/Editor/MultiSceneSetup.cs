#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 多场景一键搭建工具 —— Tools > 场景管理 > 一键搭建所有场景
/// 创建4个场景（酒馆/街道/灵脉交易所/野外），自动配置背景、相机、玩家、出生点、触发区
/// 并注册到 Build Settings
/// </summary>
public static class MultiSceneSetup
{
    // 场景定义
    private struct SceneConfig
    {
        public string sceneName;
        public string bgImageName;
        public SpawnDef[] spawns;
        public TriggerDef[] triggers;
        public NpcDef[] npcs;
        public bool needsGameManager;
        public bool needsTransitionManager;
        public bool hasRandomEvents;
    }

    private struct SpawnDef
    {
        public string id;
        public Vector2 pos;
    }

    private struct TriggerDef
    {
        public string name;
        public Vector2 pos;
        public Vector2 size;
        public string targetScene;
        public string spawnId;
        public string buttonText;
        public SceneTransitionTrigger.Edge edge;
    }

    private struct NpcDef
    {
        public string name;
        public Vector2 pos;
        public string dialogueFile;
        public string displayName;
        public string spritePath;  // PixelSprites 下的精灵图路径（不含扩展名），留空则用占位方块
        public string questCompleteFlag;     // 任务完成 PlayerPrefs key，留空不检测
        public string postQuestStartNodeId;  // 任务完成后起始节点ID
    }

    private const string SCENES_DIR = "Assets/Scenes/";
    private const string BG_DIR = "Assets/Resources/Backgrounds/";
    private const string CHAR_DIR = "Assets/Resources/Characters/";

    [MenuItem("Tools/场景管理/一键搭建所有场景")]
    public static void SetupAllScenes()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("无法在运行时操作", "请先停止 Play 模式。", "知道了");
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog("一键搭建所有场景",
            "将创建/覆盖以下4个场景：\n\n" +
            "1. Tavern.unity（酒馆 - 初始场景）\n" +
            "2. Street.unity（街道）\n" +
            "3. Exchange.unity（灵脉交易所）\n" +
            "4. Wilderness.unity（野外）\n\n" +
            "每个场景将自动配置：背景、相机、玩家、出生点、场景触发区。\n" +
            "已手动调整的出生点位置会自动保留。\n" +
            "确认继续？",
            "开始搭建", "取消");
        if (!confirmed) return;

        SceneConfig[] configs = BuildSceneConfigs();

        try
        {
            // 确保目录存在
            if (!AssetDatabase.IsValidFolder(SCENES_DIR))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            for (int i = 0; i < configs.Length; i++)
            {
                EditorUtility.DisplayProgressBar("搭建场景", $"正在创建: {configs[i].sceneName}", (float)i / configs.Length);

                // 搭建前读取已有出生点位置和角色缩放，搭建后还原
                Dictionary<string, Vector2> preserved = ReadExistingSpawnPositions(configs[i]);
                Dictionary<string, Vector3> preservedScales = ReadExistingScales(configs[i]);
                SetupScene(configs[i], preserved, preservedScales);
            }

            RegisterBuildSettings();

            // 重置全局碰撞矩阵，确保触发区和玩家能正常交互
            ResetCollisionMatrix();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("完成", "4个场景已创建并配置完成！\n\n" +
                "Build Settings 已更新。\n" +
                "初始场景为 Tavern（酒馆）。\n" +
                "已保留手动调整的出生点位置和角色大小。", "好的");

            // 打开酒馆场景
            EditorSceneManager.OpenScene(SCENES_DIR + "Tavern.unity", OpenSceneMode.Single);
        }
        catch (Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("出错", "搭建过程中出错:\n" + e.Message, "知道了");
            Debug.LogError("[MultiSceneSetup] " + e);
        }
    }

    /// <summary>
    /// 读取场景中已有的出生点位置
    /// </summary>
    private static Dictionary<string, Vector2> ReadExistingSpawnPositions(SceneConfig config)
    {
        var result = new Dictionary<string, Vector2>();
        string scenePath = SCENES_DIR + config.sceneName + ".unity";

        if (!System.IO.File.Exists(scenePath)) return result;

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var spawnPoints = UnityEngine.Object.FindObjectsOfType<SceneSpawnPoint>();
        foreach (var sp in spawnPoints)
        {
            result[sp.spawnPointId] = sp.transform.position;
            Debug.Log($"[MultiSceneSetup] 保留出生点: {sp.spawnPointId} @ {sp.transform.position}");
        }
        return result;
    }

    /// <summary>
    /// 读取场景中已有的 Player 和 NPC 缩放值
    /// 返回字典：key 为物体名称，value 为 localScale
    /// </summary>
    private static Dictionary<string, Vector3> ReadExistingScales(SceneConfig config)
    {
        var result = new Dictionary<string, Vector3>();
        string scenePath = SCENES_DIR + config.sceneName + ".unity";

        if (!System.IO.File.Exists(scenePath)) return result;

        // 场景已由 ReadExistingSpawnPositions 打开，直接查找物体
        // Player
        GameObject existingPlayer = GameObject.Find("Player");
        if (existingPlayer != null)
        {
            result["Player"] = existingPlayer.transform.localScale;
            Debug.Log($"[MultiSceneSetup] 保留 Player 缩放: {existingPlayer.transform.localScale}");
        }

        // NPC
        if (config.npcs != null)
        {
            foreach (var npc in config.npcs)
            {
                GameObject existingNpc = GameObject.Find(npc.name);
                if (existingNpc != null)
                {
                    result[npc.name] = existingNpc.transform.localScale;
                    Debug.Log($"[MultiSceneSetup] 保留 {npc.name} 缩放: {existingNpc.transform.localScale}");
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 定义4个场景的配置
    /// </summary>
    private static SceneConfig[] BuildSceneConfigs()
    {
        return new SceneConfig[]
        {
            // ===== 酒馆 =====
            new SceneConfig
            {
                sceneName = "Tavern",
                bgImageName = "tavern",
                needsGameManager = true,
                needsTransitionManager = true,
                spawns = new SpawnDef[]
                {
                    new SpawnDef { id = "Tavern_Start", pos = new Vector2(0f, -2.5f) },
                    new SpawnDef { id = "Tavern_FromStreet", pos = new Vector2(-6f, -2.5f) }
                },
                triggers = new TriggerDef[]
                {
                    new TriggerDef
                    {
                        name = "Exit_ToStreet",
                        pos = Vector2.zero,
                        size = new Vector2(2f, 14f),
                        targetScene = "Street",
                        spawnId = "Street_FromTavern",
                        buttonText = "进入街道",
                        edge = SceneTransitionTrigger.Edge.Right
                    }
                },
                npcs = new NpcDef[]
                {
                    new NpcDef { name = "NPC_TavernKeeper", pos = new Vector2(-3.99f, -1.43f), dialogueFile = "Dialogues/tavern_keeper", displayName = "老板娘·红袖", questCompleteFlag = "tavern_quest_complete", postQuestStartNodeId = "t0" }
                }
            },

            // ===== 街道 =====
            new SceneConfig
            {
                sceneName = "Street",
                bgImageName = "street",
                spawns = new SpawnDef[]
                {
                    new SpawnDef { id = "Street_FromTavern", pos = new Vector2(6f, -2.5f) },
                    new SpawnDef { id = "Street_FromExchange", pos = new Vector2(-6f, -2.5f) },
                    new SpawnDef { id = "Street_FromWild", pos = new Vector2(0f, -2.5f) }
                },
                triggers = new TriggerDef[]
                {
                    new TriggerDef
                    {
                        name = "Exit_ToTavern",
                        pos = Vector2.zero,
                        size = new Vector2(2f, 14f),
                        targetScene = "Tavern",
                        spawnId = "Tavern_FromStreet",
                        buttonText = "进入酒馆",
                        edge = SceneTransitionTrigger.Edge.Left
                    },
                    new TriggerDef
                    {
                        name = "Exit_ToExchange",
                        pos = Vector2.zero,
                        size = new Vector2(2f, 14f),
                        targetScene = "Exchange",
                        spawnId = "Exchange_FromStreet",
                        buttonText = "进入灵脉交易所",
                        edge = SceneTransitionTrigger.Edge.Right
                    },
                    new TriggerDef
                    {
                        name = "Exit_ToWilderness",
                        pos = Vector2.zero,
                        size = new Vector2(4f, 7f),
                        targetScene = "Wilderness",
                        spawnId = "Wild_FromStreet",
                        buttonText = "前往野外",
                        edge = SceneTransitionTrigger.Edge.CenterTop
                    }
                },
                hasRandomEvents = true
            },

            // ===== 灵脉交易所（上方中间大门进出） =====
            new SceneConfig
            {
                sceneName = "Exchange",
                bgImageName = "exchange",
                spawns = new SpawnDef[]
                {
                    // 出生点：远离上方出口，在画面下方
                    new SpawnDef { id = "Exchange_FromStreet", pos = new Vector2(0f, -3f) }
                },
                triggers = new TriggerDef[]
                {
                    // 出口触发：大门处（什么地方进就什么地方出）
                    new TriggerDef
                    {
                        name = "Exit_ToStreet",
                        pos = Vector2.zero,
                        size = new Vector2(4f, 2f),
                        targetScene = "Street",
                        spawnId = "Street_FromExchange",
                        buttonText = "返回街道",
                        edge = SceneTransitionTrigger.Edge.CenterTop
                    }
                },
                npcs = new NpcDef[]
                {
                    new NpcDef { name = "NPC_ExchangeMaster", pos = new Vector2(0f, 0.16f), dialogueFile = "Dialogues/exchange_master", displayName = "会长·墨衡", spritePath = "PixelSprites/exchange" }
                }
            },

            // ===== 野外（左下角进出） =====
            new SceneConfig
            {
                sceneName = "Wilderness",
                bgImageName = "wilderness",
                spawns = new SpawnDef[]
                {
                    // 出生点：远离左下出口，在画面右上方
                    new SpawnDef { id = "Wild_FromStreet", pos = new Vector2(6f, 3f) }
                },
                triggers = new TriggerDef[]
                {
                    // 出口触发：左下角（什么地方进就什么地方出）
                    new TriggerDef
                    {
                        name = "Exit_ToStreet",
                        pos = Vector2.zero,
                        size = new Vector2(3f, 2f),
                        targetScene = "Street",
                        spawnId = "Street_FromWild",
                        buttonText = "返回街道",
                        edge = SceneTransitionTrigger.Edge.BottomLeft
                    }
                },
                npcs = new NpcDef[]
                {
                    new NpcDef { name = "NPC_TreeSpirit", pos = new Vector2(1.7f, -0.16f), dialogueFile = "Dialogues/wild_tree_spirit", displayName = "古树之灵·青檀" }
                }
            }
        };
    }

    /// <summary>
    /// 创建或更新一个场景
    /// </summary>
    /// <param name="preservedSpawns">搭建前读取的已有出生点位置，null 则用默认值</param>
    /// <param name="preservedScales">搭建前读取的已有角色缩放，null 则用默认值</param>
    private static void SetupScene(SceneConfig config, Dictionary<string, Vector2> preservedSpawns = null, Dictionary<string, Vector3> preservedScales = null)
    {
        string scenePath = SCENES_DIR + config.sceneName + ".unity";

        // 创建新场景
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // SceneRoot
        GameObject root = new GameObject(config.sceneName + "_Root");

        // 背景
        CreateBackground(root, config.bgImageName, config.sceneName);

        // 边界墙（防止玩家走出画面）
        CreateBoundaryWalls(root, config.sceneName);

        // 相机
        SetupCamera();

        // 玩家
        Vector3 playerScale = new Vector3(0.1f, 0.1f, 1f);
        if (preservedScales != null && preservedScales.ContainsKey("Player"))
            playerScale = preservedScales["Player"];
        CreatePlayer(root, playerScale);

        // 出生点 —— 优先使用保留的位置，没有则用默认值
        foreach (var spawn in config.spawns)
        {
            Vector2 pos = spawn.pos;
            if (preservedSpawns != null && preservedSpawns.ContainsKey(spawn.id))
                pos = preservedSpawns[spawn.id];
            CreateSpawnPoint(root, spawn.id, pos);
        }

        // 触发区
        foreach (var trigger in config.triggers)
            CreateTrigger(root, trigger);

        // NPC
        if (config.npcs != null)
        {
            foreach (var npc in config.npcs)
            {
                Vector3 npcScale = default;
                if (preservedScales != null && preservedScales.ContainsKey(npc.name))
                    npcScale = preservedScales[npc.name];
                CreateNPC(root, npc, npcScale);
            }
        }

        // GameManager（仅酒馆）
        if (config.needsGameManager)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        // QuestManager（仅酒馆，DontDestroyOnLoad 跨场景持久化）
        if (config.needsGameManager)
        {
            GameObject qm = new GameObject("QuestManager");
            qm.AddComponent<QuestManager>();
        }

        // SceneTransitionManager（仅酒馆）
        if (config.needsTransitionManager)
        {
            GameObject stm = new GameObject("SceneTransitionManager");
            stm.AddComponent<SceneTransitionManager>();
        }

        // 随机事件触发器（仅街道）
        if (config.hasRandomEvents)
        {
            GameObject evtObj = new GameObject("RandomEventTrigger");
            evtObj.transform.SetParent(root.transform, false);
            evtObj.transform.position = Vector3.zero;
            var col = evtObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(20f, 10f);
            col.isTrigger = true;
            evtObj.AddComponent<RandomEventTrigger>();
            Debug.Log("[MultiSceneSetup] 随机事件触发器已添加到街道");
        }

        // 保存场景
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[MultiSceneSetup] 场景已创建: {scenePath}");
    }

    /// <summary>
    /// 创建背景 SpriteRenderer + BackgroundAutoFit
    /// </summary>
    private static void CreateBackground(GameObject parent, string bgName, string sceneName)
    {
        string bgPath = BG_DIR + bgName + ".jpg";

        // 自动修复导入设置：确保是 Sprite 类型，并统一世界宽度
        EnsureSpriteImport(bgPath, normalizePPU: true);

        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);

        if (bgSprite == null)
        {
            // 尝试 .png
            bgPath = BG_DIR + bgName + ".png";
            EnsureSpriteImport(bgPath, normalizePPU: true);
            bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
        }

        GameObject bgObj = new GameObject(sceneName + "_Background");
        bgObj.transform.SetParent(parent.transform, false);
        bgObj.transform.position = Vector3.zero;

        var sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -10;

        if (bgSprite != null)
        {
            sr.sprite = bgSprite;
            bgObj.AddComponent<BackgroundAutoFit>();
            Debug.Log($"[MultiSceneSetup] 背景已加载: {bgPath}");
        }
        else
        {
            Debug.LogWarning($"[MultiSceneSetup] 未找到背景图: {BG_DIR}{bgName}.jpg/png，请放入后再执行");
        }
    }

    /// <summary>
    /// 配置相机
    /// </summary>
    private static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            cam = camObj.GetComponent<Camera>();
        }

        cam.orthographic = true;
        cam.orthographicSize = 7.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.transform.position = new Vector3(0f, 0f, -10f);

        // 确保 FixedCamera 存在
        if (cam.GetComponent<FixedCamera>() == null)
            cam.gameObject.AddComponent<FixedCamera>();
    }

    /// <summary>
    /// 创建玩家
    /// </summary>
    /// <param name="scale">手动指定的 localScale，Vector3.zero 表示用默认值</param>
    private static void CreatePlayer(GameObject parent, Vector3 scale = default)
    {
        GameObject player = new GameObject("Player");
        player.transform.SetParent(parent.transform, false);
        player.transform.position = new Vector3(0f, -2.5f, 0f);
        player.tag = "Player";

        // SpriteRenderer
        var sr = player.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;

        // 加载方向图
        Sprite spriteFront  = AssetDatabase.LoadAssetAtPath<Sprite>(CHAR_DIR + "正面.png");
        Sprite spriteBack   = AssetDatabase.LoadAssetAtPath<Sprite>(CHAR_DIR + "背面.png");
        Sprite spriteLeft   = AssetDatabase.LoadAssetAtPath<Sprite>(CHAR_DIR + "左立.png");
        Sprite spriteRight  = AssetDatabase.LoadAssetAtPath<Sprite>(CHAR_DIR + "右立.png");
        Sprite spriteLWalk  = AssetDatabase.LoadAssetAtPath<Sprite>(CHAR_DIR + "左走.png");
        Sprite spriteRWalk  = AssetDatabase.LoadAssetAtPath<Sprite>(CHAR_DIR + "右走.png");

        if (spriteFront != null) sr.sprite = spriteFront;
        player.transform.localScale = (scale == default) ? new Vector3(0.1f, 0.1f, 1f) : scale;

        // Rigidbody2D
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Collider
        var col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;

        // PlayerMovement
        var movement = player.AddComponent<PlayerMovement>();
        movement.spriteFront = spriteFront;
        movement.spriteBack = spriteBack;
        movement.spriteLeft = spriteLeft;
        movement.spriteRight = spriteRight;
        movement.spriteLeftWalk = spriteLWalk;
        movement.spriteRightWalk = spriteRWalk;
    }

    /// <summary>
    /// 创建出生点
    /// </summary>
    private static void CreateSpawnPoint(GameObject parent, string id, Vector2 pos)
    {
        GameObject go = new GameObject("Spawn_" + id);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        var sp = go.AddComponent<SceneSpawnPoint>();
        sp.spawnPointId = id;
    }

    /// <summary>
    /// 创建场景切换触发区
    /// </summary>
    private static void CreateTrigger(GameObject parent, TriggerDef trigger)
    {
        GameObject go = new GameObject(trigger.name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = new Vector3(trigger.pos.x, trigger.pos.y, 0f);

        var col = go.AddComponent<BoxCollider2D>();
        col.size = trigger.size;
        col.isTrigger = true;

        var t = go.AddComponent<SceneTransitionTrigger>();
        t.targetSceneName = trigger.targetScene;
        t.spawnPointId = trigger.spawnId;
        t.buttonText = trigger.buttonText;
        t.edge = trigger.edge;
        t.triggerSize = trigger.size;
    }

    /// <summary>
    /// 创建 NPC 物体（SpriteRenderer + Rigidbody2D + 碰撞触发区 + 对话触发器）
    /// NPC 场景占位图用代码生成的彩色方块，Portraits 立绘仅在对话触发时由 DialogueUI 加载
    /// </summary>
    /// <param name="scale">手动指定的 localScale，Vector3.zero 表示用自动计算值</param>
    private static void CreateNPC(GameObject parent, NpcDef npc, Vector3 scale = default)
    {
        GameObject go = new GameObject(npc.name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = new Vector3(npc.pos.x, npc.pos.y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        // 优先使用指定的像素精灵图
        Sprite npcSprite = null;
        if (!string.IsNullOrEmpty(npc.spritePath))
        {
            string spriteFullPath = CHAR_DIR + npc.spritePath + ".png";

            // 去除白底（将不透明的近白色像素改为透明）
            RemoveWhiteBg(spriteFullPath);

            EnsureSpriteImport(spriteFullPath);
            npcSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteFullPath);
        }

        if (npcSprite != null)
        {
            sr.sprite = npcSprite;
            if (scale != default)
            {
                // 使用保留的手动缩放
                go.transform.localScale = scale;
            }
            else
            {
                // 动态计算缩放：匹配玩家世界高度（玩家 384px @ 0.1 scale @ 100PPU ≈ 0.384）
                float targetHeight = 0.5f;
                float spriteHeight = npcSprite.bounds.size.y;
                if (spriteHeight > 0.001f)
                {
                    float s = targetHeight / spriteHeight;
                    go.transform.localScale = new Vector3(s, s, 1f);
                }
                else
                {
                    go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
                }
            }
        }
        else
        {
            // 没有指定精灵图，使用代码生成的占位方块
            string placeholderPath = CHAR_DIR + "Portraits/_npc_placeholder.png";
            if (!System.IO.File.Exists(placeholderPath))
            {
                Texture2D tex = new Texture2D(32, 48);
                Color head = new Color(0.9f, 0.75f, 0.6f, 1f);
                Color body = new Color(0.6f, 0.45f, 0.3f, 1f);
                Color empty = new Color(0, 0, 0, 0);
                Color[] pixels = new Color[32 * 48];
                for (int y = 0; y < 48; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        int i = y * 32 + x;
                        if (y >= 36 && y < 46)
                        {
                            int cx = 16, cy = 40;
                            if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= 25)
                                pixels[i] = head;
                            else
                                pixels[i] = empty;
                        }
                        else if (y >= 14 && y < 36 && x >= 10 && x < 22)
                        {
                            pixels[i] = body;
                        }
                        else
                        {
                            pixels[i] = empty;
                        }
                    }
                }
                tex.SetPixels(pixels);
                tex.Apply();
                System.IO.File.WriteAllBytes(placeholderPath, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(placeholderPath);
            }
            EnsureSpriteImport(placeholderPath);
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(placeholderPath);
        }

        // Rigidbody2D（Kinematic）—— 确保触发检测稳定可靠
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        // 碰撞触发区（IsTrigger，检测玩家靠近）—— 世界空间 4×5
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(4f, 5f);
        col.isTrigger = true;

        // 对话触发器
        var trigger = go.AddComponent<NpcDialogueTrigger>();
        trigger.dialogueFile = npc.dialogueFile;
        trigger.npcDisplayName = npc.displayName;
        if (!string.IsNullOrEmpty(npc.questCompleteFlag))
            trigger.questCompleteFlag = npc.questCompleteFlag;
        if (!string.IsNullOrEmpty(npc.postQuestStartNodeId))
            trigger.postQuestStartNodeId = npc.postQuestStartNodeId;

        Debug.Log($"[MultiSceneSetup] NPC 已创建: {npc.name} @ {npc.pos}, 对话文件: {npc.dialogueFile}");
    }

    /// <summary>
    /// 去除 PNG 图片的白底（将不透明的近白色像素改为透明）
    /// </summary>
    private static void RemoveWhiteBg(string assetPath)
    {
        string fullPath = System.IO.Path.GetFullPath(assetPath);
        if (!System.IO.File.Exists(fullPath)) return;

        byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);

        Color32[] pixels = tex.GetPixels32();
        int changed = 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 c = pixels[i];
            if (c.a > 200 && c.r > 230 && c.g > 230 && c.b > 230)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
                changed++;
            }
        }

        if (changed > 0)
        {
            tex.SetPixels32(pixels);
            tex.Apply();
            System.IO.File.WriteAllBytes(fullPath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath);
            Debug.Log($"[MultiSceneSetup] 去除白底: {assetPath} ({changed} 像素)");
        }
    }

    /// <summary>
    /// 创建场景四周的隐形边界墙，防止玩家走出画面
    /// 墙的位置由 SceneBoundary 在运行时根据相机可见范围动态调整
    /// </summary>
    private static void CreateBoundaryWalls(GameObject parent, string sceneName)
    {
        GameObject walls = new GameObject("BoundaryWalls");
        walls.transform.SetParent(parent.transform, false);

        // 四面墙的初始位置（运行时由 SceneBoundary 动态修正）
        CreateWallObj(walls, "Wall_Left", new Vector2(-15f, 0f), new Vector2(1f, 20f));
        CreateWallObj(walls, "Wall_Right", new Vector2(15f, 0f), new Vector2(1f, 20f));
        CreateWallObj(walls, "Wall_Top", new Vector2(0f, 10f), new Vector2(30f, 1f));
        CreateWallObj(walls, "Wall_Bottom", new Vector2(0f, -10f), new Vector2(30f, 1f));

        // 挂载 SceneBoundary，运行时根据相机实际可见范围调整墙位置
        walls.AddComponent<SceneBoundary>();
    }

    private static void CreateWallObj(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = pos;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
    }

    /// <summary>
    /// 重置全局碰撞矩阵 —— 清除 TavernColliderGenerator 设置的层忽略
    /// 确保所有层之间的碰撞和触发都能正常工作
    /// </summary>
    private static void ResetCollisionMatrix()
    {
        for (int i = 0; i < 32; i++)
            for (int j = 0; j < 32; j++)
                Physics2D.IgnoreLayerCollision(i, j, false);
        Debug.Log("[MultiSceneSetup] 全局碰撞矩阵已重置");
    }

    /// <summary>
    /// 背景图标准世界宽度（以最大背景 2560px @ 100PPU 为基准）
    /// 所有背景图通过调整 PPU 统一到此宽度，确保相机视野一致
    /// </summary>
    private const float TARGET_WORLD_WIDTH = 25.6f;

    /// <summary>
    /// 确保纹理导入设置为 Sprite 类型
    /// </summary>
    /// <param name="normalizePPU">为 true 时按图片宽度计算 PPU，使世界宽度统一为 25.6</param>
    private static void EnsureSpriteImport(string assetPath, bool normalizePPU = false)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        // 计算目标 PPU（仅对背景图）
        int targetPPU = 100;
        if (normalizePPU)
        {
            importer.GetSourceTextureWidthAndHeight(out int texW, out int texH);
            targetPPU = Mathf.Max(1, Mathf.RoundToInt(texW / TARGET_WORLD_WIDTH));
        }

        bool needsReimport = false;

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            needsReimport = true;
        }
        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            needsReimport = true;
        }
        if (normalizePPU && importer.spritePixelsPerUnit != targetPPU)
        {
            importer.spritePixelsPerUnit = targetPPU;
            needsReimport = true;
        }
        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            needsReimport = true;
        }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            needsReimport = true;
        }
        if (importer.maxTextureSize != 4096)
        {
            importer.maxTextureSize = 4096;
            needsReimport = true;
        }
        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            needsReimport = true;
        }

        if (needsReimport)
        {
            importer.SaveAndReimport();
            if (normalizePPU)
                Debug.Log($"[MultiSceneSetup] 已修复导入设置: {assetPath} (PPU={targetPPU})");
            else
                Debug.Log($"[MultiSceneSetup] 已修复导入设置: {assetPath}");
        }
    }

    /// <summary>
    /// 注册4个场景到 Build Settings
    /// </summary>
    private static void RegisterBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(SCENES_DIR + "Tavern.unity", true),
            new EditorBuildSettingsScene(SCENES_DIR + "Street.unity", true),
            new EditorBuildSettingsScene(SCENES_DIR + "Exchange.unity", true),
            new EditorBuildSettingsScene(SCENES_DIR + "Wilderness.unity", true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[MultiSceneSetup] Build Settings 已更新");
    }
}
#endif
