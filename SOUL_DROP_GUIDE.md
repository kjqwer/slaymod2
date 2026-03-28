# 摄魂掉落系统说明（后续可扩展）

本文档说明当前“摄魂”击杀掉落的实现方式，以及后续新增普通/稀有变体时需要修改的位置。

## 1. 入口流程

摄魂牌在命中后，如果目标被击杀，会进入奖励流程：

- 入口：`SoulCapture.OnPlay`
- 判断击杀：`target.IsDead || attack.Results.Any(result => result.WasTargetKilled)`
- 触发：`RemoveDeckVersionAndAddRewardCard(...)`

核心代码位置：

- `Cards/SoulCapture.cs`

## 2. 奖励如何组成

在 `RemoveDeckVersionAndAddRewardCard` 中，奖励由两部分组成：

1. 怪物魂奖励（可 1~3 张）  
   来源：`MonsterSoulCardRegistry.CreateRewardCards(owner, defeatedMonsterId)`
2. 额外附带 1 张新的摄魂牌  
   来源：`owner.RunState.CreateCard<SoulCapture>(owner)`

最后把两部分合并进同一个 `CardReward`。

这意味着：**一次摄魂击杀，确实可能出现多张奖励卡**。  
其中怪物魂卡数量取决于掉落数量逻辑，此外固定再加 1 张摄魂。

## 3. 掉落数量逻辑（怪物魂部分）

数量逻辑在 `MonsterSoulCardRegistry.RollRewardCount`：

- 若该怪物只有 1 个候选：必定掉 1 张
- 若该怪物有 2 个候选：从 `[1, 1, 1, 2]` 取样
  - 1 张：75%
  - 2 张：25%
- 若该怪物有 3 个或更多候选：从 `[1, 1, 1, 1, 2, 2, 2, 3]` 取样
  - 1 张：50%
  - 2 张：37.5%
  - 3 张：12.5%
  - 最终会被 `Math.Min(rolled, maxOptions)` 限制，不会超过候选上限

## 4. 权重逻辑（怪物魂部分）

权重在 `MonsterSoulCardRegistry.RollWeighted`：

- 每个候选有 `Weight`（decimal）
- 转换为抽样池复制次数：`copies = Max(1, Round(weight * 10, AwayFromZero))`
- 在复制池中 `NextItem` 均匀抽 1 个
- 每抽中一个候选就从本次池里移除，因此同一轮奖励不会重复同一张

当前配置常量：

- `DefaultWeight = 1m`
- `WeightScale = 10m`

含义示例：

- `1.0` => 10 份
- `0.2` => 2 份
- `100` => 1000 份

注意：由于有 `Max(1, ...)`，极小权重仍至少有 1 份，不会变成绝对 0 概率。

## 5. 当前已接入的多版本怪物

在 `VariantOptions` 中，`TOADPOLE` 已配置 3 张候选：

- `SoulMonsterToadpole`（普通）
- `SoulMonsterToadpoleSpined`（变体）
- `SoulMonsterToadpoleRoyal`（变体）

其它怪物若未在 `VariantOptions` 配置，会自动走原来的单卡映射 `CreateSingle(...)`。

## 6. 后续新增某怪物“普通+稀有”步骤

以怪物 `XXX` 为例：

1. 新建卡类（可选 1~N 张变体）  
   放在 `Cards/MonsterSouls/`，命名建议 `SoulMonsterXxxRareA`、`SoulMonsterXxxRareB`
2. 补本地化  
   - `ABStS2Mod/localization/zhs/cards.json`
   - `ABStS2Mod/localization/eng/cards.json`
3. 在 `MonsterSoulCardRegistry.VariantOptions` 加配置  
   例：
   - 普通卡权重 `1m`
   - 稀有卡权重 `0.2m` 或更低
   - 想更常见可提高到 `2m/3m`，极端可到 `100m`
4. 不需要改 `SoulCapture` 主流程  
   `CreateRewardCards` 会自动处理数量与权重

## 7. 关键文件索引

- `Cards/SoulCapture.cs`
- `Cards/MonsterSouls/MonsterSoulCardRegistry.cs`
- `Cards/MonsterSouls/SoulMonsterToadpoleSpined.cs`
- `Cards/MonsterSouls/SoulMonsterToadpoleRoyal.cs`
- `ABStS2Mod/localization/zhs/cards.json`
- `ABStS2Mod/localization/eng/cards.json`
