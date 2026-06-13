# 《艾瑟瑞亚：繁荣悖论》- 黑客松演示版

> *灵脉回响，繁荣与枯寂同源。你的每一个选择，都将重塑这片大地。*

基于 D&D 5e 规则的叙事驱动 RPG，融合生态管理与 AI 动态旁白。玩家在繁荣与生态的矛盾中做出抉择，最终走向四种不同结局之一。

---

## 🎮 如何运行

### 方式一：本地 HTTP 服务器（推荐）

1. 下载 `WebGL_Final.zip` 并解压
2. 安装 [Node.js](https://nodejs.org/)（或使用 Python 3）
3. 启动本地服务器：

**Node.js：**
```bash
npx http-server WebGL_Final -p 8080 --cors
```

**Python 3：**
```bash
cd WebGL_Final
python -m http.server 8080
```

4. 浏览器打开 `http://localhost:8080`
5. ⚠️ 必须通过 HTTP 服务器访问，`file://` 协议无法加载 WebGL

### 方式二：Unity Editor

1. 使用 Unity 2021.3+ 打开项目
2. 打开 `Assets/Scenes/SampleScene.scene`
3. 点击 Play 运行

---

## 🎲 核心机制

### 四维动态影响系统
| 数值 | 含义 | 影响 |
|------|------|------|
| 繁荣度 | 文明与经济的发展程度 | 高繁荣带来资源，但可能牺牲生态 |
| 生态值 | 灵脉与自然的健康状态 | 生态值过低触发崩塌 |
| 觉醒值 | 对灵脉本质的理解深度 | 影响结局走向 |
| 守护值 | 对自然平衡的投入程度 | 影响结局走向 |

### D&D 5e 角色创建
- **20 点购买**系统，6 项属性（STR/DEX/CON/INT/WIS/CHA）
- 属性范围 8-15，直接影响战斗属性
- 力量 → 攻击加值，敏捷 → 护甲等级，体质 → 生命值

### D20 检定与回合制战斗
- d20 攻击检定 vs 护甲等级 (AC)
- 伤害骰子系统（1d4/1d6/1d8 + 修正值）
- 三种敌人：
  - 🐺 狂暴灵兽 (HP12, AC13, +4, 1d6+2)
  - 🛡️ 灵脉守卫 (HP18, AC15, +5, 1d8+3)
  - ☠️ 枯萎魔物 (HP8, AC11, +3, 1d4+1)

### AI 动态旁白
- 集成 **DeepSeek API** 生成诗意环境描述
- Mock 模式：无需 API 密钥即可体验完整流程
- 根据当前游戏状态生成不同风格的旁白

### 多结局系统
| 结局 | 条件 | 含义 |
|------|------|------|
| 🌿 均衡共生 | 觉醒≥85 且 守护≥70 | 文明与自然的完美平衡 |
| ⛓️ 繁荣之囚 | 繁荣≥80 且 觉醒<40 | 牺牲自然的文明巅峰 |
| 🔥 从零重生 | 生态≤15 且 觉醒≥50 | 文明崩溃后的新起点 |
| ❓ 未竟之途 | 以上条件均不满足 | 旅途尚未结束 |

### 存档系统
- PlayerPrefs 实现跨平台存档/读档
- 保存全部数值与角色属性

---

## 🕹️ 操作说明

| 操作 | 功能 |
|------|------|
| 鼠标点击 | UI 交互（按钮、滑块） |
| F5 | 触发结局 |
| 角色创建 | 分配属性点后点击确认 |
| 任务选择 | 点击选项按钮推进剧情 |

---

## 🛠️ 技术栈

| 组件 | 技术 |
|------|------|
| 游戏引擎 | Unity (Tuanjie Engine 兼容) |
| 脚本语言 | C# |
| UI 框架 | Unity UI + TextMeshPro |
| AI 旁白 | DeepSeek API (OpenAI 兼容格式) |
| 网络请求 | UnityWebRequest |
| 存档方案 | PlayerPrefs + JsonUtility |
| 构建目标 | WebGL (Mono Backend, Gzip 压缩) |

---

## 📁 项目结构

```
Assets/
├── Scripts/
│   ├── Core/              GameManager, PlayerAction, CharacterAttributes, SaveData, EndingManager
│   ├── AI/                AIDMClient (DeepSeek 集成)
│   ├── Combat/            Combatant, CombatManager, EnemyData
│   ├── Narrative/         QuestManager, QuestNode, QuestCondition
│   ├── NPC/               NpcReaction
│   ├── UI/                UIManager, AttributeAllocator, QuestUI, CombatUI, EndingUI
│   └── Utils/             DiceRoller
├── Resources/Enemies/     EnemyData ScriptableObject 资源
└── Scenes/                SampleScene, Scene_Second
```

---

## ⚠️ 已知问题与限制

1. **CJK 字体缺失** — TMP 默认字体不含中文字符，显示为方框。需导入 CJK 字体资源。
2. **WebGL 不支持 DeepSeek 实时调用** — 浏览器 CORS 限制，演示使用 Mock 模式。
3. **WebGL 使用 Mono 后端** — IL2CPP 与 Tuanjie Engine WebGL 后处理器不兼容。
4. **UI 组件非持久化** — 场景切换时 UIManager/QuestUI/CombatUI 需重新绑定。
5. **部分 UI 为占位符** — 无最终美术资源，界面为功能演示状态。

---

## 🏗️ WebGL 构建配置

- Scripting Backend: **Mono**
- Compression: **Gzip**
- Managed Stripping Level: **Medium**
- Strip Engine Code: ✅
- Development Build: ❌
- Template: APPLICATION:Default

---

## 📦 交付物

```
Aesheria_Submission/
├── WebGL_Final/              完整 WebGL 构建文件夹
├── demo_video.mp4            演示视频 (3-5 分钟)
├── README.md                 本文件
└── source_code_link.txt      GitHub 仓库链接
```

---

## 🎯 黑客松演示流程

1. **角色创建**（30秒）— 分配属性点，确认进入游戏
2. **任务选择**（1分钟）— 选择商会/德鲁伊路线，观察数值变化
3. **AI 旁白**（30秒）— 点击 AI 旁白按钮，查看动态生成文本
4. **战斗系统**（1分钟）— 触发战斗，展示回合制 D20 检定
5. **结局触发**（30秒）— 按 F5 触发结局，展示结局报告
