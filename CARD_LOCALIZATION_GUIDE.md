# 卡牌本地化编写规范（描述 / 升级 / 必写与禁写）

本文档只讲 `cards.json` 的写法，目标是让后续新增卡牌时，本地化一次写对。

## 1. 文件与最小要求

卡牌本地化文件：

- `ABStS2Mod/localization/zhs/cards.json`
- `ABStS2Mod/localization/eng/cards.json`

每张卡至少写 2 个键：

- `{CARD_ID}.title`
- `{CARD_ID}.description`

缺任何一个都会导致显示异常或缺文案。

## 2. 键名规则（ID 怎么来）

本项目卡牌 ID 由类名转为大写下划线，并带前缀 `ABSTS2MOD-`。

示例：

- 类：`SoulMonsterToadpoleRoyal`
- 键前缀：`ABSTS2MOD-SOUL_MONSTER_TOADPOLE_ROYAL`
- 最终键：
  - `ABSTS2MOD-SOUL_MONSTER_TOADPOLE_ROYAL.title`
  - `ABSTS2MOD-SOUL_MONSTER_TOADPOLE_ROYAL.description`

## 3. 动态数值占位（描述里怎么接代码变量）

描述里常见写法：

- `{Damage}`、`{Block}`、`{Heal}`
- `{SomePower}`（如 `{ThornsPower}`、`{ArtifactPower}`）
- 自定义变量名（如 `{Increase}`、`{Replay}`）

变量名来源于卡牌类里的 `CanonicalVars`：

- `new DamageVar(...)` -> `{Damage}`
- `new BlockVar(...)` -> `{Block}`
- `new PowerVar<ArtifactPower>(...)` -> `{ArtifactPower}`
- `new DynamicVar("Increase", ...)` 或 `new IntVar("Replay", ...)` -> `{Increase}` / `{Replay}`

如果升级会影响该数值，建议使用 `:diff()`：

- `{Damage:diff()}`
- `{Block:diff()}`
- `{ArtifactPower:diff()}`
- `{Increase:diff()}`

`diff()` 的作用是让升级后的差异展示更直观，项目中大量卡牌已采用该写法。

## 4. 升级文案的三种写法

### 4.1 仅数值变化（最常见）

代码里 `OnUpgrade()` 只改数值时，描述用动态变量即可，通常不需要额外条件分支。

- 推荐：`{Damage:diff()}` / `{Block:diff()}`

### 4.2 升级改变文本分支（条件描述）

当升级改变“牌名、词句或行为分支”时，使用：

- `{IfUpgraded:show:升级时文本|未升级文本}`

这在项目内已被用于“升级后插入不同卡名”或“升级后显示升级标签”。

### 4.3 升级改变固定常量（非 DynamicVar）

若你描述里用了固定数字而非变量，也可用条件分支：

- `{IfUpgraded:show:16|12}`

但更推荐把数值放进 `DynamicVar`，避免代码与文案双维护。

## 5. 必须写什么 / 不要写什么

### 必须写

- 实际效果文本（伤害、格挡、施加能力、抽牌、生成牌等）
- 需要让玩家理解的机制词（如 [gold]斩杀[/gold]、[gold]中毒[/gold]、[gold]脆弱[/gold]）
- 会随升级变化的核心信息（通过 `:diff()` 或 `IfUpgraded`）

### 不要写（重点）

- 不要重复写“系统会自动显示的卡牌关键词行”
  - 典型：`Exhaust` / `Ethereal` / `Retain` 这类由卡牌关键词系统展示的内容
- 不要在描述里额外补一行“虚无/消耗/保留”来重复系统信息

原因：重复写会出现双重提示，且与实际关键词状态（例如升级后移除 Exhaust）容易不一致。

## 6. 判断“该不该写关键词”的实用规则

看这个词属于哪一类：

1. **卡牌自身标签关键词（通常自动展示）**  
   例如 `CardKeyword.Exhaust / Ethereal / Retain`  
   -> 通常不写到 `description` 里

2. **效果机制关键词（属于动作描述）**  
   例如“施加脆弱”“施加中毒”“获得人工制品”  
   -> 应该写在 `description` 里

简化记忆：  
“卡角标/关键词行会自动出”的，不手写；  
“打出后到底做了什么”的，要手写。

## 7. 本项目已验证的参考样例

- 关键词自动展示，不在描述重复写：
  - `SoulMonsterCrusher`（有 `Exhaust` 关键词，描述不重复写“消耗”）
  - `SoulMonsterCeremonialBeast`（有 `Ethereal` 关键词，描述不重复写“虚无”）
- 升级条件文案：
  - `SoulCapture`、`SoulMonsterLivingFog`、`SoulMonsterThievingHopper` 使用 `IfUpgraded:show:...`
- 升级数值差异：
  - 多张怪物魂卡使用 `{Xxx:diff()}`（如 `{Block:diff()}`、`{Damage:diff()}`）

## 8. 新增卡牌本地化检查清单（提交前）

1. zhs/eng 两个 `cards.json` 都已加 `title + description`
2. 键名与卡牌真实 ID 一致（尤其大小写与下划线）
3. 描述中的变量名都存在于 `CanonicalVars`
4. 升级变化已体现（`diff()` 或 `IfUpgraded`）
5. 没有手写重复的自动关键词（如虚无/消耗/保留）
6. 中英文语义一致，不出现一边改了另一边漏改
