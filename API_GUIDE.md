# Slay the Spire 2 Modding API Guide

基于解包源码整理的 API 参考，适用于 BaseLib 0.1.4 + .NET 9.0 + Godot 4.5 的 mod 开发。

## Table of Contents
1. [项目结构与入口](#1-项目结构与入口)
2. [模型系统 (ModelDb)](#2-模型系统-modeldb)
3. [Hook 系统 (事件钩子)](#3-hook-系统-事件钩子)
4. [遗物 (Relic) 开发](#4-遗物-relic-开发)
5. [卡牌 (Card) 开发](#5-卡牌-card-开发)
6. [能力 (Power) 开发](#6-能力-power-开发)
7. [命令系统 (Cmd)](#7-命令系统-cmd)
8. [DynamicVar 动态变量](#8-dynamicvar-动态变量)
9. [角色修改 (Harmony Patch)](#9-角色修改-harmony-patch)
10. [本地化 (Localization)](#10-本地化-localization)
11. [资源路径 (Asset)](#11-资源路径-asset)
12. [常见踩坑与注意事项](#12-常见踩坑与注意事项)
13. [角色一览](#13-角色一览)

---

## 1. 项目结构与入口

### Mod 入口
```csharp
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace ABStS2Mod;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "ABStS2Mod";
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; }
        = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}
```

> **踩坑警告：`harmony.PatchAll()` 必须传入程序集参数！**
>
> 游戏通过反射 (`method.Invoke`) 调用 mod 的 `Initialize()` 方法。无参的 `harmony.PatchAll()` 内部使用 `Assembly.GetCallingAssembly()` 来确定扫描哪个程序集，但反射调用时该方法返回的是**游戏主程序集 (sts2)** 而非你的 mod 程序集，导致所有 `[HarmonyPatch]` 类都不会被发现和应用。
>
> **正确写法：** `harmony.PatchAll(Assembly.GetExecutingAssembly())`
> **错误写法：** `harmony.PatchAll()` — Patch 静默失败，不报错但不生效

### 日志
```csharp
MainFile.Logger.Info("message");
MainFile.Logger.Warn("warning");
MainFile.Logger.Error("error");
```

---

## 2. 模型系统 (ModelDb)

`MegaCrit.Sts2.Core.Models.ModelDb` 是所有模型的注册中心。

### 获取模型实例
```csharp
ModelDb.Relic<DivineRight>()       // 获取遗物的规范实例
ModelDb.Card<StrikeRegent>()       // 获取卡牌
ModelDb.Character<Regent>()        // 获取角色
ModelDb.CardPool<RegentCardPool>() // 获取卡池
ModelDb.RelicPool<RegentRelicPool>()
ModelDb.GetById<RelicModel>(modelId) // 按 ID 获取
```

### 模型注入 (Mod 扩展)
BaseLib 的 `[Pool]` 属性会通过 `ModHelper.AddModelToPool<TPoolType, TModelType>()` 将自定义模型注册到对应池中。必须在游戏初始化前完成注册。

---

## 3. Hook 系统 (事件钩子)

所有继承 `AbstractModel` 的类(遗物、卡牌、能力)均可 override 以下虚方法来响应游戏事件。方法均为 `virtual async Task`。

### 战斗阶段
| 方法 | 说明 |
|------|------|
| `BeforeCombatStart()` | 战斗开始前 |
| `BeforeCombatStartLate()` | 战斗开始前(晚期) |
| `AfterCombatEnd(CombatRoom room)` | 战斗结束后 |
| `AfterCombatVictory(CombatRoom room)` | 战斗胜利后 |
| `AfterCombatVictoryEarly(CombatRoom room)` | 战斗胜利后(早期) |

### 回合管理
| 方法 | 说明 |
|------|------|
| `BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, CombatState state)` | 回合开始前 |
| `AfterSideTurnStart(CombatSide side, CombatState state)` | 回合开始后 |
| `BeforeSideTurnEnd(CombatSide side, CombatState state)` | 回合结束前 |
| `AfterEnergyReset(Player player)` | 能量重置后 |
| `AfterEnergyResetLate(Player player)` | 能量重置后(晚期) |

### 攻击与伤害
| 方法 | 说明 |
|------|------|
| `BeforeAttack(AttackCommand command)` | 攻击前 |
| `AfterAttack(AttackCommand command)` | 攻击后 |
| `BeforeDamageReceived(Creature target, ...)` | 受伤前 |
| `AfterDamageReceived(Creature target, ...)` | 受伤后 |
| `AfterDamageGiven(Creature target, ...)` | 造成伤害后 |

### 格挡
| 方法 | 说明 |
|------|------|
| `BeforeBlockGained(Creature creature, decimal amount, ...)` | 获得格挡前 |
| `AfterBlockGained(...)` | 获得格挡后 |
| `AfterBlockCleared(Creature creature)` | 格挡清除后 |
| `AfterBlockBroken(Creature creature)` | 格挡被击破后 |

### 卡牌事件
| 方法 | 说明 |
|------|------|
| `BeforeCardPlayed(CardPlay cardPlay)` | 打出卡牌前 |
| `AfterCardPlayed(PlayerChoiceContext ctx, CardPlay play)` | 打出卡牌后 |
| `AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)` | 抽牌后 |
| `AfterCardExhausted(PlayerChoiceContext ctx, CardModel card, bool causedByEthereal)` | 卡牌消耗后 |
| `AfterCardRetained(CardModel card)` | 卡牌保留后 |
| `AfterCardChangedPiles(CardModel card)` | 卡牌换堆后 |

### 能力 (Power) 事件
| 方法 | 说明 |
|------|------|
| `BeforePowerAmountChanged(Creature target, PowerModel power, ...)` | 能力层数变化前 |
| `AfterPowerAmountChanged(Creature target, PowerModel power, ...)` | 能力层数变化后 |

### 星辰与能量
| 方法 | 说明 |
|------|------|
| `AfterStarsGained(int amount, Player player)` | 获得星辰后 |
| `AfterStarsSpent(int amount, Player player)` | 消耗星辰后 |
| `AfterEnergySpent(Player player, int amount)` | 消耗能量后 |

### 房间与地图
| 方法 | 说明 |
|------|------|
| `AfterRoomEntered(AbstractRoom room)` | 进入房间后 |
| `AfterRestSiteHeal(Player player, decimal amount)` | 休息点回血后 |

### 死亡
| 方法 | 说明 |
|------|------|
| `BeforeDeath(Creature creature)` | 死亡前 |
| `AfterDeath(Creature creature)` | 死亡后 |
| `ShouldDie(Creature creature) -> bool` | 是否应该死亡 |
| `AfterPreventingDeath(Creature creature)` | 阻止死亡后 |

### 修改器 (Modifier Hooks)
这些方法返回修改后的数值：
```csharp
// 伤害修改
ModifyDamageAdditive(Creature target, decimal damage, ...) -> decimal
ModifyDamageMultiplicative(Creature target, decimal damage, ...) -> decimal

// 格挡修改
ModifyBlockAdditive(Creature target, decimal block, ...) -> decimal
ModifyBlockMultiplicative(Creature target, decimal block, ...) -> decimal

// 治疗修改
ModifyHealAmount(Creature creature, decimal amount) -> decimal

// 抽牌修改
ModifyHandDraw(Player player, decimal count) -> decimal

// 能力层数修改
ModifyPowerAmountGiven(Creature target, decimal amount, ...) -> decimal
ModifyPowerAmountReceived(Creature target, decimal amount, ...) -> decimal

// 商店价格修改
ModifyMerchantPrice(int price, ...) -> int
```

### 条件谓词 (Predicate Hooks)
返回布尔值控制游戏行为：
```csharp
ShouldAddToDeck(CardModel card) -> bool
ShouldDraw(Player player, bool fromHandDraw) -> bool
ShouldFlush(Player player) -> bool
ShouldPlay(CardModel card, AutoPlayType type) -> bool
ShouldDie(Creature creature) -> bool
```

---

## 4. 遗物 (Relic) 开发

### 基本结构
使用 BaseLib 的 `CustomRelicModel`：
```csharp
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ABStS2Mod.Relics;

[Pool(typeof(RegentRelicPool))]  // 注册到储君遗物池
public class MyRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;
    // Common / Uncommon / Rare / Starter / Shop / Event / Ancient / None
}
```

### 可用遗物池
| Pool 类 | 角色 |
|---------|------|
| `IroncladRelicPool` | 铁甲战士 |
| `SilentRelicPool` | 静默猎手 |
| `DefectRelicPool` | 机器人缺陷 |
| `NecrobinderRelicPool` | 死灵缚者 |
| `RegentRelicPool` | 储君 |
| `SharedRelicPool` | 通用 |
| `EventRelicPool` | 事件遗物 |

### RelicRarity 枚举
```csharp
public enum RelicRarity
{
    None,      // 无
    Starter,   // 初始遗物
    Common,    // 普通
    Uncommon,  // 罕见
    Rare,      // 稀有
    Shop,      // 商店
    Event,     // 事件
    Ancient    // 先古
}
```

### 遗物图标路径
遗物需要提供三种图标：
```csharp
// 小图标(遗物栏)
public override string PackedIconPath { get; }
// 小图标轮廓
protected override string PackedIconOutlinePath { get; }
// 大图标(详情页)
protected override string BigIconPath { get; }
```

通用模板(找不到资源时使用默认图):
```csharp
public override string PackedIconPath
{
    get
    {
        var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();
        return ResourceLoader.Exists(path) ? path : "relic.png".RelicImagePath();
    }
}
```

### 遗物常用属性
```csharp
Owner              // Player - 拥有者
Owner.Creature     // Creature - 拥有者的战斗实体
Owner.PlayerCombatState.Hand.Cards  // 手牌
Owner.PlayerCombatState.DrawPile    // 抽牌堆
Owner.PlayerCombatState.DiscardPile // 弃牌堆
Owner.PlayerCombatState.Energy      // 当前能量

Flash()            // 遗物闪烁动画
DynamicVars        // 动态变量集合
IsWax / IsMelted   // 蜡制/融化状态
Status             // RelicStatus: Normal / Active / Disabled
```

### 完整遗物示例 (源码参考: DivineRight)
```csharp
// 储君的初始遗物：进入战斗房间时获得3颗星辰
public sealed class DivineRight : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars
        => new ReadOnlySingleElementList<DynamicVar>(new StarsVar(3));

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            await PlayerCmd.GainStars(DynamicVars.Stars.BaseValue, Owner);
        }
    }
}
```

### 完整遗物示例 (源码参考: MiniRegent)
```csharp
// 每回合第一次消耗星辰时，获得1层力量
public sealed class MiniRegent : RelicModel
{
    private bool _usedThisTurn;

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars
        => new ReadOnlySingleElementList<DynamicVar>(new PowerVar<StrengthPower>(1m));

    protected override IEnumerable<IHoverTip> ExtraHoverTips
        => new ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

    public override async Task AfterStarsSpent(int amount, Player spender)
    {
        if (spender == Owner && !_usedThisTurn)
        {
            _usedThisTurn = true;
            Flash();
            await PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, null);
        }
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext ctx, CombatSide side, CombatState state)
    {
        if (side == Owner.Creature.Side) _usedThisTurn = false;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        _usedThisTurn = false;
        return Task.CompletedTask;
    }
}
```

---

## 5. 卡牌 (Card) 开发

### 基本结构
使用 BaseLib 的 `CustomCardModel`：
```csharp
using BaseLib.Abstracts;

// 构造函数参数: (cost, type, rarity, targetType)
public class MyCard() : CustomCardModel(
    1,                      // 能量消耗
    CardType.Attack,        // Attack / Skill / Power / Status / Curse
    CardRarity.Common,      // Basic / Common / Uncommon / Rare / Special / Curse
    TargetType.AnyEnemy     // Self / AnyEnemy / AllEnemies / None
)
```

### CardType 卡牌类型
- `Attack` - 攻击牌
- `Skill` - 技能牌
- `Power` - 能力牌
- `Status` - 状态牌
- `Curse` - 诅咒牌

### TargetType 目标类型
- `Self` - 自身
- `AnyEnemy` - 任一敌人
- `AllEnemies` - 所有敌人
- `None` - 无目标

### 卡牌核心方法
```csharp
// 打出卡牌时执行的效果
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    // cardPlay.Target - 选定的目标 (可能为 null)
    // Owner - 拥有者
}

// 升级时的变化
protected override void OnUpgrade()
{
    DynamicVars.Damage.UpgradeValueBy(3m);
}
```

### 卡牌源码示例 (StrikeRegent - 储君打击)
```csharp
public sealed class StrikeRegent : CardModel
{
    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

    protected override IEnumerable<DynamicVar> CanonicalVars
        => new ReadOnlySingleElementList<DynamicVar>(new DamageVar(6m, ValueProp.Move));

    public StrikeRegent() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
```

### 卡牌源码示例 (DefendRegent - 储君防御)
```csharp
public sealed class DefendRegent : CardModel
{
    public override bool GainsBlock => true;
    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Defend };
    protected override IEnumerable<DynamicVar> CanonicalVars
        => new ReadOnlySingleElementList<DynamicVar>(new BlockVar(5m, ValueProp.Move));

    public DefendRegent() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
```

### 卡牌源码示例 (FallingStar - 坠星)
```csharp
// 多 DynamicVar + 星辰消耗 + 多种效果
public sealed class FallingStar : CardModel
{
    public override int CanonicalStarCost => 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<WeakPower>(1m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    };

    public FallingStar() : base(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_starry_impact", "blunt_attack.mp3")
            .Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(cardPlay.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
```

### 卡牌源码示例 (Venerate - 崇敬)
```csharp
// 获得星辰的技能牌
public sealed class Venerate : CardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars
        => new ReadOnlySingleElementList<DynamicVar>(new StarsVar(2));

    public Venerate() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PlayerCmd.GainStars(DynamicVars.Stars.BaseValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Stars.UpgradeValueBy(1m);
}
```

### 修改卡牌费用
```csharp
// 完全免费（能量 + 星辰等所有消耗归零）
card.SetToFreeThisTurn();             // 本回合完全免费（推荐）
card.SetToFreeThisCombat();           // 本场战斗完全免费

// 仅修改能量费用（不影响星辰等其他消耗）
card.EnergyCost.SetThisTurn(0);       // 本回合能量费用设为 0
card.EnergyCost.SetUntilPlayed(0);    // 直到打出前能量费用为 0
card.EnergyCost.SetThisCombat(0);     // 本场战斗能量费用为 0
card.EnergyCost.AddThisTurn(-1);      // 本回合能量费用 -1
```

> **注意：** 如果想让卡牌完全免费，应使用 `SetToFreeThisTurn()` 而非 `EnergyCost.SetThisTurn(0)`。后者只清除能量消耗，不会清除星辰等其他资源消耗（如储君的坠星需要 2 星辰）。

---

## 6. 能力 (Power) 开发

### 基本结构
使用 BaseLib 的 `CustomPowerModel`：
```csharp
using BaseLib.Abstracts;

public class MyPower : CustomPowerModel
{
    // 能力同样可 override AbstractModel 中的所有 Hook 方法
}
```

### 施加能力
```csharp
// 对目标施加能力
await PowerCmd.Apply<StrengthPower>(target, amount, source, cardSource);
await PowerCmd.Apply<WeakPower>(target, amount, source, cardSource);
await PowerCmd.Apply<VulnerablePower>(target, amount, source, cardSource);
```

### 常用内置能力 (MegaCrit.Sts2.Core.Models.Powers)
- `StrengthPower` - 力量
- `DexterityPower` - 敏捷
- `WeakPower` - 虚弱
- `VulnerablePower` - 易伤
- `VigorPower` - 活力
- `IntangiblePower` - 无实体
- `BarricadePower` - 壁垒

---

## 7. 命令系统 (Cmd)

游戏效果通过命令系统执行，位于 `MegaCrit.Sts2.Core.Commands`：

### DamageCmd - 伤害命令
```csharp
await DamageCmd.Attack(damage)
    .FromCard(this)          // 伤害来源
    .Targeting(target)       // 目标
    .WithHitFx("vfx/...")   // 命中特效
    .Execute(choiceContext);
```

### CreatureCmd - 生物命令
```csharp
await CreatureCmd.GainBlock(creature, dynamicVar, cardPlay);  // 获得格挡
await CreatureCmd.Heal(creature, amount);                     // 治疗
await CreatureCmd.TriggerAnim(creature, "Cast", delay);       // 播放动画
```

### PlayerCmd - 玩家命令
```csharp
await PlayerCmd.GainStars(amount, owner);  // 获得星辰
```

### PowerCmd - 能力命令
```csharp
await PowerCmd.Apply<TPower>(target, amount, source, cardSource);
```

---

## 8. DynamicVar 动态变量

DynamicVar 系统用于管理可在本地化文本中引用的动态数值。

### 常用 DynamicVar 子类
| 类 | 说明 | 本地化引用 |
|---|---|---|
| `DamageVar(6m, ValueProp.Move)` | 伤害值 | `{Damage}` |
| `BlockVar(5m, ValueProp.Move)` | 格挡值 | `{Block}` |
| `HealVar(6m)` | 治疗值 | `{Heal}` |
| `StarsVar(3)` | 星辰数 | `{Stars}` / `{Stars:starIcons()}` |
| `PowerVar<T>(1m)` | 能力层数 | `{StrengthPower}` / `{WeakPower}` 等 |
| `EnergyVar(1m)` | 能量值 | `{Energy:energyIcons()}` |
| `GoldVar(50m)` | 金币数 | `{Gold}` |
| `SummonVar(1m)` | 召唤数 | `{Summon}` |

### 在模型中定义 DynamicVar
```csharp
protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
{
    new DamageVar(6m, ValueProp.Move),
    new PowerVar<WeakPower>(1m)
};
```

### 访问 DynamicVar 值
```csharp
DynamicVars.Damage.BaseValue   // 基础伤害值
DynamicVars.Block.BaseValue    // 基础格挡值
DynamicVars.Stars.BaseValue    // 星辰数
DynamicVars.Strength.BaseValue // 力量值 (PowerVar<StrengthPower>)
DynamicVars.Weak.BaseValue     // 虚弱值 (PowerVar<WeakPower>)
```

### 升级时修改
```csharp
protected override void OnUpgrade()
{
    DynamicVars.Damage.UpgradeValueBy(3m);  // 伤害 +3
    DynamicVars.Block.UpgradeValueBy(3m);   // 格挡 +3
}
```

---

## 9. 角色修改 (Harmony Patch)

### 添加初始遗物
```csharp
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;

[HarmonyPatch(typeof(Regent), nameof(Regent.StartingRelics), MethodType.Getter)]
public static class RegentStartingRelicPatch
{
    public static void Postfix(ref IReadOnlyList<RelicModel> __result)
    {
        var newRelics = new List<RelicModel>(__result);
        newRelics.Add(ModelDb.Relic<MyCustomRelic>());
        __result = newRelics;
    }
}
```

### 修改初始牌组
```csharp
[HarmonyPatch(typeof(Regent), nameof(Regent.StartingDeck), MethodType.Getter)]
public static class RegentStartingDeckPatch
{
    public static void Postfix(ref IEnumerable<CardModel> __result)
    {
        var newDeck = new List<CardModel>(__result);
        newDeck.Add(ModelDb.Card<MyCustomCard>());
        __result = newDeck;
    }
}
```

### 可用角色类 (MegaCrit.Sts2.Core.Models.Characters)
| 类 | 角色 |
|---|---|
| `Ironclad` | 铁甲战士 |
| `Silent` | 静默猎手 |
| `Defect` | 缺陷 |
| `Necrobinder` | 死灵缚者 |
| `Regent` | 储君 |

---

## 10. 本地化 (Localization)

### JSON 格式
**重要：** 游戏使用**扁平的点号分隔键**，不是嵌套 JSON！

文件放在 `ABStS2Mod/localization/{lang}/` 目录下。文件名即表名(如 `relics.json` -> 表名 `"relics"`)。

#### relics.json
```json
{
  "MODID-RELIC_NAME.title": "遗物名称",
  "MODID-RELIC_NAME.flavor": "趣味文字",
  "MODID-RELIC_NAME.description": "遗物效果描述。"
}
```

可选键:
- `{ID}.eventDescription` - 事件中的描述文本(不设则回退到 description)
- `{ID}.selectionScreenPrompt` - 选择界面提示
- `{ID}.additionalRestSiteHealText` - 休息点额外文本

#### cards.json
```json
{
  "MODID-CARD_NAME.title": "卡牌名称",
  "MODID-CARD_NAME.description": "造成[blue]{Damage}[/blue]点伤害。"
}
```

#### powers.json
```json
{
  "MODID-POWER_NAME.title": "能力名称",
  "MODID-POWER_NAME.description": "能力效果描述。",
  "MODID-POWER_NAME.smartDescription": "含动态变量的能力效果描述，如 [blue]{Amount}[/blue]。"
}
```

### 富文本格式 (BBCode)
```
[blue]...[/blue]     - 蓝色 (数值)
[gold]...[/gold]     - 金色 (关键词)
[red]...[/red]       - 红色 (负面)
[green]...[/green]   - 绿色 (正面，如治疗)
[purple]...[/purple] - 紫色
```

### 动态变量引用
```
{Damage}              - 伤害数值
{Block}               - 格挡数值
{Heal}                - 治疗数值
{Stars:starIcons()}   - 星辰图标
{Energy:energyIcons()} - 能量图标
{StrengthPower}       - 力量层数
{WeakPower}           - 虚弱层数
{VulnerablePower}     - 易伤层数
{Cards}               - 卡牌数量
{Gold}                - 金币数量
```

### 复数形式
```
{Cards:plural:card|cards}        - 根据数值选择单复数
{PotionSlots:plural:slot|slots}
```

### 条件格式
```
{Value:cond:true text|false text}
```

### 本地化键的 ID 规则
BaseLib 生成的 mod 模型 ID 格式为 `MODID-CLASS_NAME`，例如：
- Mod ID: `ABStS2Mod`，类名: `PuppyPowerRelic`
- 生成 ID: `ABStS2Mod-PUPPY_POWER_RELIC`
- 本地化键: `ABStS2Mod-PUPPY_POWER_RELIC.title` 等

---

## 11. 资源路径 (Asset)

### StringExtensions 辅助方法
```csharp
"file.png".RelicImagePath()       // -> ABStS2Mod/images/relics/file.png
"file.png".BigRelicImagePath()    // -> ABStS2Mod/images/relics/big/file.png
"file.png".CardImagePath()        // -> ABStS2Mod/images/card_portraits/file.png
"file.png".BigCardImagePath()     // -> ABStS2Mod/images/card_portraits/big/file.png
"file.png".PowerImagePath()       // -> ABStS2Mod/images/powers/file.png
"file.png".BigPowerImagePath()    // -> ABStS2Mod/images/powers/big/file.png
"file.png".CharacterUiPath()      // -> ABStS2Mod/images/charui/file.png
"file.png".ImagePath()            // -> ABStS2Mod/images/file.png
```

### 目录结构
```
ABStS2Mod/
├── images/
│   ├── relics/          # 遗物小图标
│   │   └── big/         # 遗物大图标
│   ├── card_portraits/  # 卡牌图标
│   │   └── big/         # 卡牌大图标
│   ├── powers/          # 能力图标
│   │   └── big/         # 能力大图标
│   └── charui/          # 角色 UI 图标
└── localization/
    ├── eng/             # English
    │   ├── relics.json
    │   ├── cards.json
    │   └── ui.json
    └── zhs/             # 简体中文
        ├── relics.json
        ├── cards.json
        └── ui.json
```

注意：图片资源需要在 Godot 编辑器中导入一次才能被识别。

---

## 12. 常见踩坑与注意事项

### 12.1 `harmony.PatchAll()` 必须传程序集参数

见 [1. 项目结构与入口](#1-项目结构与入口) 中的踩坑警告。

### 12.2 `dotnet build` 不会重建 PCK

- `dotnet build` 只编译 DLL 并部署到游戏 mods 目录
- **`dotnet publish` 才会触发 Godot PCK 导出**，将本地化 JSON、图片等资源打包到 `.pck` 文件中
- 如果你新增或修改了本地化文件 (`localization/` 下的 JSON) 或图片资源，**必须运行 `dotnet publish`** 才能在游戏中生效
- 症状：DLL 功能正常但本地化文字缺失，游戏可能在抽到缺失本地化的卡牌时冻结

### 12.3 本地化键名必须使用 `.title` 而非 `.name`

游戏中**所有模型**（卡牌、能力、遗物）的显示名称键均使用 `.title` 后缀：
```
MODID-CLASS_NAME.title         // 正确
MODID-CLASS_NAME.name          // 错误 - 不会被识别
```

构建时的 `STS001` 错误会提示缺少本地化键，注意核对键名。

### 12.4 能力(Power)本地化必须包含 `.smartDescription`

能力有三个本地化键：
```json
{
  "MODID-POWER_NAME.title": "能力名称",
  "MODID-POWER_NAME.description": "固定描述文本（不带动态变量）",
  "MODID-POWER_NAME.smartDescription": "含动态变量的描述（战斗中实际显示的）"
}
```

- `.description` 是基础描述，使用**硬编码数值**（如 `[blue]2[/blue]`）
- `.smartDescription` 使用**动态占位符**（如 `[blue]{Amount}[/blue]`），战斗中根据实际层数渲染
- 如果缺少 `.smartDescription`，能力获得后鼠标悬停不会显示描述

### 12.5 能力描述中使用 `{Amount}` 而非 `{0}`

能力描述使用 SmartFormat 语法，`{0}` 位置参数**不被支持**，会导致显示为 `System.Collections.Generic.Dictionary...`。

正确使用 `{Amount}` 引用能力的当前层数：
```json
"smartDescription": "每当你[gold]锻造[/gold]时，获得锻造量[blue]{Amount}[/blue]倍的[gold]格挡[/gold]。"
```

错误写法（会显示 Dictionary.ToString()）：
```json
"smartDescription": "每当你[gold]锻造[/gold]时，获得锻造量[blue]{0}[/blue]倍的[gold]格挡[/gold]。"
```

#### SmartFormat 常用语法参考
```
{Amount}                              // 能力层数
{Amount:abs()}                        // 绝对值（用于可能为负的值如力量）
{Amount:plural:time|times}            // 复数形式
{Amount:cond:<0?Decreases|Increases}  // 条件判断
{Amount:energyIcons()}                // 能量图标
{singleStarIcon}                      // 星辰图标（内置变量）
```

游戏自动注入的能力描述变量：
| 变量 | 类型 | 说明 |
|------|------|------|
| `{Amount}` | decimal | 能力当前层数 |
| `{OnPlayer}` | bool | 持有者是否为玩家 |
| `{IsMultiplayer}` | bool | 是否多人模式 |
| `{PlayerCount}` | int | 玩家数量 |
| `{OwnerName}` | string | 持有者名称 |
| `{singleStarIcon}` | string | 星辰图标 |
| `{energyPrefix}` | string | 能量前缀图标 |

此外，能力的 `CanonicalVars` 中定义的 DynamicVar 也会被注入到描述中。

### 12.6 能力卡(Power)不能是 Common 稀有度

`CardType.Power`（能力牌）的稀有度**最低为 `Uncommon`**。如果设为 `Common`，构建时会报错。

### 12.7 卡牌免费应使用 `SetToFreeThisTurn()`

见 [5. 卡牌开发 - 修改卡牌费用](#修改卡牌费用) 中的注意事项。

### 12.8 禁止直接 `new` Power 实例传给 `PowerCmd.Apply` —— 必须使用泛型 `Apply<T>`

**这是最容易犯的严重 bug，会导致卡牌打出后卡死在屏幕上。**

#### 错误原因

游戏的模型系统基于 **Canonical / Mutable 模式**：
- `new SomePower()` 创建的对象 `IsMutable = false`（canonical 状态）
- `PowerCmd.Apply(power, ...)` 的 instance 重载内部会调用 `power.AssertMutable()`，对 canonical 对象直接抛出 `CanonicalModelException`
- 如果 Power 使用了 `GetInternalData<T>()`（通过 `InitInternalData()` 初始化），直接 `new` 出来的对象 `_internalData = null`，因为 `InitInternalData()` 只在 `DeepCloneFields()` 中被调用（即只有通过 `MutableClone()` 克隆时才会初始化）

异常被抛出后，`OnPlay` 的 async Task 中断，卡牌停留在 Play 堆无法移动到结果堆（Exhaust/Discard），**表现为卡图卡在屏幕上不消失、效果不生效**。

#### 错误写法
```csharp
// 错误！直接 new 实例 + 对象初始化器
var power = new MyPower { TrackedCard = card, Amount = 5 };
await PowerCmd.Apply(power, Owner.Creature, power.Amount, Owner.Creature, this);
```

#### 正确写法（参考游戏源码 Nightmare.cs）
```csharp
// 正确！使用泛型 Apply<T>，它会内部从 canonical 做 MutableClone()
// 返回值就是已施加到目标上的 mutable Power 实例
var power = await PowerCmd.Apply<MyPower>(Owner.Creature, 5, Owner.Creature, this);
power.SetTrackedCard(card);  // 在 Apply 之后设置自定义数据
```

#### Power 中的数据设置方法
```csharp
// 错误！属性 setter 在对象初始化器中调用时 InternalData 尚未初始化
public CardModel TrackedCard
{
    set { GetInternalData<Data>().trackedCard = value; }
}

// 正确！公开方法，在 Apply<T> 返回后由卡牌代码调用
public void SetTrackedCard(CardModel card)
{
    Data data = GetInternalData<Data>();
    data.trackedCard = card;
}
```

#### 完整示例（带 InternalData 的 Instanced Power）

Power 定义：
```csharp
public class MyTrackingPower : CustomPowerModel
{
    private class Data { public CardModel trackedCard; }

    public override bool IsInstanced => true;
    // ... Type, StackType ...

    protected override object InitInternalData() => new Data();

    public void SetTrackedCard(CardModel card)
    {
        GetInternalData<Data>().trackedCard = card;
    }

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != Owner.Side) return;
        var data = GetInternalData<Data>();
        // 使用 data.trackedCard 做逻辑...
        await PowerCmd.Remove(this);
    }
}
```

卡牌中使用：
```csharp
var power = await PowerCmd.Apply<MyTrackingPower>(Owner.Creature, amount, Owner.Creature, this);
power.SetTrackedCard(selectedCard);
```

#### 不带自定义数据的 Power（更简单）
```csharp
// 不需要返回值，直接一行
await PowerCmd.Apply<MySimplePower>(Owner.Creature, amount, Owner.Creature, this);
```

### 12.9 修改原版卡牌基础费用需用反射

`CardEnergyCost` 没有公开的 `SetBaseValue` 方法。`SetCustomBaseCost()` 虽然存在但内部会调用 `AssertMutable()`，在构造函数 Postfix 中 canonical 实例尚未标记为 mutable，会抛异常。

正确做法是通过反射直接设置私有字段 `_base`：
```csharp
using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;

internal static class EnergyCostHelper
{
    private static readonly FieldInfo BaseField =
        typeof(CardEnergyCost).GetField("_base", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public static void SetBaseCost(CardEnergyCost cost, int value)
    {
        BaseField.SetValue(cost, value);
    }
}
```

然后在 Harmony Postfix 中使用：
```csharp
[HarmonyPatch(typeof(SomeCard), MethodType.Constructor)]
public static class SomeCardCostPatch
{
    public static void Postfix(SomeCard __instance)
    {
        EnergyCostHelper.SetBaseCost(__instance.EnergyCost, 0);
    }
}
```

注意事项：
- **不要用** `EnergyCost.SetCustomBaseCost()` — 在构造函数 Postfix 中会因 `AssertMutable` 失败
- **不要用** `EnergyCost.SetThisTurn()` / `SetThisCombat()` — 这些是临时修改，不是永久改基础值
- **不要用** `EnergyCost.UpgradeBy()` — 内部也调用 `SetCustomBaseCost`，同样受 `AssertMutable` 限制

### 12.10 BaseLib 版本必须固定，禁止使用 `Version="*"`

csproj 中 BaseLib 的 PackageReference **必须**固定到已知可用的版本号，例如 `0.1.6`。

**问题**：如果使用 `Version="*"`（NuGet 浮动版本），每次 `dotnet build` / `dotnet restore` 会自动拉取最新版 BaseLib。当 BaseLib 发布了不兼容的新版本（如 0.1.7 中 `PrefixIdPatch` 的 Harmony patch 失效），所有 mod 卡片/遗物/能力的 ID 前缀（如 `ABStS2Mod-`）会丢失，导致本地化键找不到、图鉴崩溃等问题。

**更隐蔽的坑**：csproj 的 `CopyToModsFolderOnBuild` target 会在每次 build 时把 NuGet 缓存中的 BaseLib.dll / BaseLib.pck / BaseLib.json 复制到游戏 mods 目录。这意味着即使你回退了自己的代码，只要 NuGet 缓存里已经是新版 BaseLib，重新 build 也会把坏版本部署到游戏中。

**正确写法**：
```xml
<!-- PackageReference 固定版本 -->
<PackageReference Include="Alchyr.Sts2.BaseLib" Version="0.1.6" PrivateAssets="All" />

<!-- BaseLibFiles glob 也要固定版本路径 -->
<BaseLibFiles Include="$(NuGetPackageRoot)/alchyr.sts2.baselib/0.1.6/lib/**/BaseLib.dll;
                        $(NuGetPackageRoot)/alchyr.sts2.baselib/0.1.6/Content/BaseLib.pck;
                        $(NuGetPackageRoot)/alchyr.sts2.baselib/0.1.6/Content/BaseLib.json;"/>
```

**错误写法**：
```xml
<!-- 危险：浮动版本 -->
<PackageReference Include="Alchyr.Sts2.BaseLib" Version="*" PrivateAssets="All" />

<!-- 危险：通配符路径会匹配到任意版本 -->
<BaseLibFiles Include="$(NuGetPackageRoot)/alchyr.sts2.baselib/**/lib/**/BaseLib.dll;..."/>
```

**排查方法**：如果所有 mod 内容突然全部失效（本地化找不到、图鉴崩溃），检查游戏 mods/BaseLib/BaseLib.dll 的文件大小或 MD5 是否与已知可用版本一致。

### 12.11 `CardFactory.GetForCombat` 会过滤掉 Basic/Ancient/Event 稀有度的卡牌

`CardFactory.GetForCombat(player, cards, count, rng)` 内部调用 `FilterForCombat()`，会移除以下稀有度的卡牌：
- `CardRarity.Basic`（基础卡，如打击、防御）
- `CardRarity.Ancient`（远古卡）
- `CardRarity.Event`（事件卡）

如果传入的卡牌列表在过滤后为空，`rng.NextItem()` 会返回 `null`，后续 `CreateCard(null)` 导致 `NullReferenceException`。

**典型场景**：能力/遗物效果"下回合生成一张与消耗卡同稀有度的随机攻击牌"，如果玩家消耗了基础攻击牌（如打击），用 `CardRarity.Basic` 去查询角色卡池，过滤后为空列表 → 崩溃。

**正确做法**：调用 `GetForCombat` 前检查稀有度是否会被过滤，如果是则回退到 Common 稀有度：
```csharp
// Basic/Ancient/Event 会被 FilterForCombat 移除，回退到 Common
var safeRarity = rarity;
if (rarity == CardRarity.Basic || rarity == CardRarity.Ancient || rarity == CardRarity.Event)
{
    safeRarity = CardRarity.Common;
}
```

---

## 13. 角色一览

### Regent (储君)
| 属性 | 值 |
|------|-----|
| 初始 HP | 75 |
| 初始金币 | 99 |
| 能量色 | 橙色 (regent) |
| 初始遗物 | DivineRight (天命) |
| 解锁条件 | 以 Silent 完成一局 |

#### 储君初始牌组 (10 张)
- 4x StrikeRegent (打击-储君) - 1费攻击, 6伤害
- 4x DefendRegent (防御-储君) - 1费技能, 5格挡
- 1x FallingStar (坠星) - 0费攻击(2星辰), 7伤害 + 虚弱 + 易伤
- 1x Venerate (崇敬) - 1费技能, 获得2星辰

#### 储君遗物池 (8 种)
DivineRight, FencingManual, GalacticDust, LunarPastry, MiniRegent, OrangeDough, Regalite, VitruvianMinion

### Ironclad (铁甲战士)
| 属性 | 值 |
|------|-----|
| 初始 HP | 80 |
| 初始金币 | 99 |
| 初始遗物 | BurningBlood (燃烧之血) |

### Silent (静默猎手)
| 属性 | 值 |
|------|-----|
| 初始 HP | 70 |
| 初始金币 | 99 |
| 初始遗物 | MercuryHourglass |
