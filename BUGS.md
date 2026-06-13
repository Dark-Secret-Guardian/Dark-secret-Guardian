# Bug 追踪表 - Aesheria

最后更新: 2026-06-13

## 测试结果汇总

| 测试项 | 结果 |
|--------|------|
| 核心数值初始状态 | ✅ 通过 |
| 数值上下限 (0-100) | ✅ 通过 |
| 开采灵脉行动 | ✅ 通过 |
| 修复灵脉行动 | ✅ 通过 |
| 角色属性调整值 | ✅ 通过 |
| D20 骰子范围 | ✅ 通过 |
| 伤害骰子解析 | ✅ 通过 |
| 存档/读档 | ✅ 通过 |
| 战斗单位 HP | ✅ 通过 |
| 战斗管理器完整战斗 | ✅ 通过 |
| 生态阈值检测 | ✅ 通过 |
| WebGL 构建 | ✅ 通过 (8.68 MB) |
| WebGL HTTP 服务 | ✅ 通过 |

## 已修复 Bug

| ID | 描述 | 优先级 | 状态 | 修复方案 |
|----|------|--------|------|----------|
| 1 | `InitializeCharacterAttributes` 不更新 `playerCombatant` | 高 | ✅ 已修复 | 添加 `InitializePlayerCombatant()` 调用 |
| 2 | `StartBattleWith` 中 `combatUI` 可能为 null 导致 NullReferenceException | 高 | ✅ 已修复 | 添加 null 检查 + `FindObjectOfType` 回退 |
| 3 | `CombatManager` GameObject 战斗结束后未销毁 | 中 | ✅ 已修复 | 在 `OnCombatEnd` 回调中 `Destroy(cmObj)` |
| 4 | `SaveData.ApplyTo` 加载后 playerCombatant 未重建 | 高 | ✅ 已修复 | Bug#1 修复后自动解决（`InitializeCharacterAttributes` 现在调用 `InitializePlayerCombatant`） |

## 已知问题（未修复）

| ID | 描述 | 优先级 | 状态 | 临时规避 |
|----|------|--------|------|----------|
| 5 | TMP LiberationSans SDF 不含 CJK 字形，中文显示为方块 | 中 | 待修复 | 需导入 TMP 中文 font asset |
| 6 | DeepSeek API 在 WebGL 中可能遇到 CORS 限制 | 低 | 待修复 | 使用 `useMock=true` 模式演示 |
| 7 | 场景切换后 UIManager/QuestUI/CombatUI 可能丢失引用 | 中 | 待修复 | DontDestroyOnLoad 或场景加载后重新绑定 |
| 8 | QuestManager 不随 GameManager 一起持久化（跨场景） | 中 | 待修复 | 添加 DontDestroyOnLoad 或重新初始化 |
| 9 | WebGL IL2CPP 在 Tuanjie 引擎下不兼容 | 低 | 已规避 | 使用 Mono 后端 |
| 10 | `MakePerceptionCheck` 在 `characterAttributes` 为 null 时崩溃 | 中 | 待修复 | 添加 null 检查 |

## 代码兼容性检查

| 检查项 | 结果 |
|--------|------|
| System.IO / System.Net 使用 | ✅ 无 |
| System.Reflection 使用 | ✅ 无（仅委托 ?.Invoke()） |
| AssetDatabase 运行时使用 | ✅ 无 |
| PlayerPrefs WebGL 兼容性 | ✅ 完全兼容 |
| UnityWebRequest WebGL 兼容性 | ⚠️ 需 CORS 支持（mock 模式安全） |
