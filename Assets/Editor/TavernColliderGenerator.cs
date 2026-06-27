#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 酒馆场景生成器 —— Tools > 生成酒馆场景
/// 一键创建背景+碰撞体+玩家，自动保存场景
/// </summary>
public static class TavernColliderGenerator
{
    private const string PARENT_NAME = "TavernColliders";
    private const string LAYER_WALL = "Wall";
    private const string LAYER_FURNITURE = "Furniture";
    private const string LAYER_NPC = "NPC";
    private const string LAYER_PLAYER = "Player";

    [MenuItem("Tools/删除碰撞体")]
    public static void RemoveColliders()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("无法在运行时操作", "请先停止 Play 模式。", "知道了");
            return;
        }

        string[] containers = { "Colliders_Walls", "Colliders_Furniture", "Colliders_Npcs" };
        GameObject root = GameObject.Find(PARENT_NAME);
        if (root == null)
        {
            Debug.LogWarning("[TavernGenerator] 未找到 TavernColliders");
            return;
        }

        int count = 0;
        foreach (string name in containers)
        {
            Transform t = root.transform.Find(name);
            if (t != null)
            {
                Object.DestroyImmediate(t.gameObject);
                count++;
                Debug.Log("[TavernGenerator] 已删除: " + name);
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log($"[TavernGenerator] 已删除 {count} 个碰撞体容器，场景已保存");
    }

    [MenuItem("Tools/生成酒馆场景")]
    public static void Generate()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("无法在运行时生成",
                "请先停止 Play 模式，再执行生成。", "知道了");
            return;
        }

        // 询问是否保留已调整的碰撞体
        bool keepColliders = false;
        GameObject existingRoot = GameObject.Find(PARENT_NAME);
        if (existingRoot != null)
        {
            keepColliders = EditorUtility.DisplayDialog("保留碰撞体？",
                "检测到已存在的酒馆场景。\n\n" +
                "「保留碰撞体」：只更新背景图和相机，保留你手动调整过的碰撞体和玩家。\n" +
                "「全部重新生成」：删除所有内容，从头创建（之前调整的碰撞体将丢失）。",
                "保留碰撞体", "全部重新生成");
        }

        try
        {
            if (keepColliders)
            {
                // 只更新背景和相机，不碰碰撞体
                UpdateBackgroundOnly(existingRoot);
            }
            else
            {
                CleanupOldObjects();
                EnsureLayers();

                GameObject root = new GameObject(PARENT_NAME);
                Undo.RegisterCreatedObjectUndo(root, "Generate Tavern Scene");

                CreateBackground(root);
                CreateWalls(root);
                CreateFurniture(root);
                CreateNpcs(root);
                CreatePlayer(root);
                ConfigureCollisionMatrix();
                SetupCamera();

                Selection.activeGameObject = root;
                Debug.Log("[TavernGenerator] 全部生成完成");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[TavernGenerator] 生成出错: " + e.Message);
        }
        finally
        {
            // 无论是否出错都保存场景，确保已生成的物体不丢失
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[TavernGenerator] 场景已保存");
        }
    }

    /// <summary>
    /// 只更新背景图和相机，保留已有碰撞体和玩家
    /// </summary>
    private static void UpdateBackgroundOnly(GameObject root)
    {
        // 查找或重建背景
        Transform bgT = root.transform.Find("TavernBackground");
        if (bgT != null)
        {
            // 已有背景，确保 BackgroundAutoFit 在
            if (bgT.GetComponent<BackgroundAutoFit>() == null)
                bgT.gameObject.AddComponent<BackgroundAutoFit>();
            Debug.Log("[TavernGenerator] 背景已保留，BackgroundAutoFit 已确认");
        }
        else
        {
            // 背景丢了，重新创建
            CreateBackground(root);
        }

        // 更新相机
        SetupCamera();

        Selection.activeGameObject = root;
        Debug.Log("[TavernGenerator] 仅更新背景和相机，碰撞体已保留");
    }

    // ===== 背景图 =====
    private static void CreateBackground(GameObject root)
    {
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Backgrounds/tavern.jpg");
        if (bgSprite == null)
        {
            Debug.LogWarning("[TavernGenerator] 未找到背景图");
            return;
        }

        GameObject bgObj = new GameObject("TavernBackground");
        bgObj.transform.SetParent(root.transform, false);
        bgObj.transform.position = Vector3.zero;

        var sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sprite = bgSprite;
        sr.sortingOrder = -10;
        // 缩放铺满相机视野：图片 2560x1440 / PPU=100 = 25.6x14.4 世界单位
        // 相机 Size=7.2 → 视野高度=14.4，宽度=14.4*aspect
        // Free Aspect 下 aspect 可能很大，所以稍大一点确保铺满
        bgObj.transform.localScale = new Vector3(1f, 1f, 1f);
        bgObj.AddComponent<BackgroundAutoFit>();  // 自动缩放铺满屏幕，适配任何分辨率
    }

    // ===== 墙体 =====
    private static void CreateWalls(GameObject root)
    {
        GameObject container = CreateContainer(root, "Colliders_Walls");
        CreateBox(container, "Wall_Left",   new Vector2(-12.2f, 0f),    new Vector2(1.2f, 14.4f), LAYER_WALL);
        CreateBox(container, "Wall_Right",  new Vector2(12.2f, 0f),     new Vector2(1.2f, 14.4f), LAYER_WALL);
        CreateBox(container, "Wall_Back",   new Vector2(0f, 6.8f),     new Vector2(25.6f, 1.0f), LAYER_WALL);
        CreateBox(container, "Wall_Front",  new Vector2(0f, -6.8f),    new Vector2(25.6f, 1.0f), LAYER_WALL);
        CreateBox(container, "Wall_Door",   new Vector2(-11.0f, -3.5f), new Vector2(2.0f, 2.5f), LAYER_WALL);
        CreateBox(container, "Wall_Window", new Vector2(-11.5f, 1.5f),  new Vector2(1.5f, 4.0f), LAYER_WALL);
    }

    // ===== 家具 =====
    private static void CreateFurniture(GameObject root)
    {
        GameObject container = CreateContainer(root, "Colliders_Furniture");

        CreateBox(container, "BarCounter_Main",  new Vector2(-8.0f, 0.5f),   new Vector2(6.0f, 1.5f), LAYER_FURNITURE);
        CreateBox(container, "BarCounter_Side", new Vector2(-5.5f, -1.5f),  new Vector2(1.5f, 3.0f), LAYER_FURNITURE);
        CreateBox(container, "Fireplace",        new Vector2(-1.5f, 4.0f),   new Vector2(3.0f, 2.5f), LAYER_FURNITURE);

        CreateBox(container, "Table_1",  new Vector2(-2.5f, 2.5f),  new Vector2(1.8f, 1.2f), LAYER_FURNITURE);
        CreateBox(container, "Table_2",  new Vector2(2.5f, 2.0f),   new Vector2(1.8f, 1.2f), LAYER_FURNITURE);
        CreateBox(container, "Table_3",  new Vector2(4.5f, 0.5f),   new Vector2(2.0f, 1.5f), LAYER_FURNITURE);
        CreateBox(container, "Table_4",  new Vector2(1.0f, -1.0f),  new Vector2(2.5f, 1.5f), LAYER_FURNITURE);
        CreateBox(container, "Table_5",  new Vector2(-2.0f, -4.5f), new Vector2(3.0f, 1.5f), LAYER_FURNITURE);
        CreateBox(container, "Table_6",  new Vector2(6.5f, -5.0f),  new Vector2(2.0f, 2.0f), LAYER_FURNITURE);
        CreateBox(container, "Table_7",  new Vector2(8.0f, -1.0f),  new Vector2(1.5f, 1.5f), LAYER_FURNITURE);

        string[] chairs = {
            "Chair_1|-5.5,0.8", "Chair_2|-2.0,3.2", "Chair_3|3.0,2.6", "Chair_4|4.8,0.8",
            "Chair_5|1.5,-0.3", "Chair_6|-0.5,-5.2", "Chair_7|7.5,-5.8", "Chair_8|8.5,-0.3",
        };
        foreach (var c in chairs)
        {
            var parts = c.Split('|');
            CreateBox(container, parts[0], ParseVec2(parts[1]), new Vector2(0.7f, 0.7f), LAYER_FURNITURE);
        }

        CreateCircle(container, "Barrel_1", new Vector2(-11.0f, -5.5f), 0.6f, LAYER_FURNITURE);
        CreateCircle(container, "Barrel_2", new Vector2(-9.5f, -6.2f),  0.6f, LAYER_FURNITURE);
        CreateCircle(container, "Barrel_3", new Vector2(-5.5f, 3.5f),   0.6f, LAYER_FURNITURE);
        CreateCircle(container, "Barrel_4", new Vector2(4.5f, 4.0f),    0.6f, LAYER_FURNITURE);
        CreateCircle(container, "Barrel_5", new Vector2(8.5f, 1.5f),    0.6f, LAYER_FURNITURE);
        CreateCircle(container, "Barrel_6", new Vector2(10.5f, -4.0f),  0.6f, LAYER_FURNITURE);

        CreateBox(container, "Shelf",         new Vector2(6.0f, 3.5f),  new Vector2(3.0f, 1.5f), LAYER_FURNITURE);
        CreateBox(container, "CeilingBeam_1", new Vector2(-5.0f, 5.0f), new Vector2(8.0f, 0.8f), LAYER_FURNITURE);
        CreateBox(container, "CeilingBeam_2", new Vector2(5.0f, 5.0f),  new Vector2(8.0f, 0.8f), LAYER_FURNITURE);
    }

    // ===== NPC =====
    private static void CreateNpcs(GameObject root)
    {
        GameObject container = CreateContainer(root, "Colliders_Npcs");
        CreateBox(container, "NPC_Bartender",        new Vector2(-8.5f, 1.5f),  new Vector2(0.9f, 1.0f), LAYER_NPC);
        CreateBox(container, "NPC_SleepingCustomer", new Vector2(-6.0f, 0.8f),  new Vector2(1.2f, 1.0f), LAYER_NPC);
        CreateBox(container, "NPC_ForegroundChar",   new Vector2(-2.0f, -5.0f), new Vector2(1.0f, 1.2f), LAYER_NPC);
        CreateBox(container, "NPC_LoneDrinker",      new Vector2(4.0f, 0.8f),   new Vector2(0.9f, 1.0f), LAYER_NPC);
        CreateCircle(container, "NPC_Cat",           new Vector2(-9.5f, -1.0f),  0.35f, LAYER_NPC);
    }

    // ===== 玩家 =====
    private static void CreatePlayer(GameObject root)
    {
        GameObject player = new GameObject("Player");
        player.transform.SetParent(root.transform);
        player.transform.position = new Vector3(0f, -2.5f, 0f);
        player.layer = GetLayer(LAYER_PLAYER);

        // SpriteRenderer
        var sr = player.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;  // 置于背景之上

        // 加载角色方向图
        Sprite spriteFront  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Characters/正面.png");
        Sprite spriteBack   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Characters/背面.png");
        Sprite spriteLeft   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Characters/左立.png");
        Sprite spriteRight  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Characters/右立.png");
        Sprite spriteLWalk  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Characters/左走.png");
        Sprite spriteRWalk  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Characters/右走.png");

        if (spriteFront != null) sr.sprite = spriteFront;  // 默认正面
        // 缩放角色：236x320px / PPU=32 = 7.375x10 世界单位，太大了
        // 缩放到 0.15 使角色约 1.5 世界单位高，和桌子差不多
        player.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

        // Rigidbody2D
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Collider
        var col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;  // 角色碰撞半径（缩放后）

        // PlayerMovement 脚本并绑定方向图
        var movement = player.AddComponent<PlayerMovement>();
        movement.spriteFront  = spriteFront;
        movement.spriteBack   = spriteBack;
        movement.spriteLeft   = spriteLeft;
        movement.spriteRight  = spriteRight;
        movement.spriteLeftWalk  = spriteLWalk;
        movement.spriteRightWalk = spriteRWalk;

        Debug.Log("[TavernGenerator] 玩家角色已创建，方向图已绑定");
    }

    // ===== 相机：正交 + 清除色改为纯黑（不显示天空盒） =====
    private static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindObjectOfType<Camera>();
        if (cam == null)
        {
            Debug.LogWarning("[TavernGenerator] 未找到 Camera");
            return;
        }

        cam.orthographic = true;
        cam.orthographicSize = 7.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.transform.position = new Vector3(0f, 0f, -10f);

        EditorUtility.SetDirty(cam.gameObject);
        Debug.Log("[TavernGenerator] 相机: Orthographic, Size=7.2, ClearColor=Black");
    }

    // ===== 清理旧物体 =====
    private static void CleanupOldObjects()
    {
        string[] namesToRemove = { PARENT_NAME, "[BackgroundManager]", "[BackgroundCanvas]", "TavernBackground" };

        // 用 FindObjectsOfType（Unity 2022 兼容）
        var allObjects = Object.FindObjectsOfType<GameObject>();
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            foreach (string name in namesToRemove)
            {
                if (go.name == name)
                {
                    Debug.Log("[TavernGenerator] 清理: " + go.name);
                    Object.DestroyImmediate(go);
                    break;
                }
            }
        }
    }

    // ===== 辅助方法 =====
    private static GameObject CreateContainer(GameObject parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    private static void CreateBox(GameObject parent, string name, Vector2 pos, Vector2 size, string layerName)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = pos;
        go.layer = GetLayer(layerName);
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
    }

    private static void CreateCircle(GameObject parent, string name, Vector2 pos, float radius, string layerName)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = pos;
        go.layer = GetLayer(layerName);
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = radius;
    }

    private static Vector2 ParseVec2(string s)
    {
        var p = s.Split(',');
        return new Vector2(float.Parse(p[0]), float.Parse(p[1]));
    }

    private static int GetLayer(string name)
    {
        int layer = LayerMask.NameToLayer(name);
        return layer >= 0 ? layer : 0;
    }

    private static void EnsureLayers()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layersProp = tagManager.FindProperty("layers");
        EnsureLayer(layersProp, 8, LAYER_WALL);
        EnsureLayer(layersProp, 9, LAYER_FURNITURE);
        EnsureLayer(layersProp, 10, LAYER_NPC);
        EnsureLayer(layersProp, 11, LAYER_PLAYER);
        tagManager.ApplyModifiedProperties();
    }

    private static void EnsureLayer(SerializedProperty layersProp, int index, string name)
    {
        var entry = layersProp.GetArrayElementAtIndex(index);
        if (entry.stringValue != name)
            entry.stringValue = name;
    }

    private static void ConfigureCollisionMatrix()
    {
        int wall = GetLayer(LAYER_WALL);
        int furniture = GetLayer(LAYER_FURNITURE);
        int npc = GetLayer(LAYER_NPC);
        int player = GetLayer(LAYER_PLAYER);

        for (int i = 0; i < 32; i++)
            for (int j = 0; j < 32; j++)
                Physics2D.IgnoreLayerCollision(i, j, true);

        Physics2D.IgnoreLayerCollision(player, wall, false);
        Physics2D.IgnoreLayerCollision(player, furniture, false);
        Physics2D.IgnoreLayerCollision(player, npc, false);
    }
}
#endif
