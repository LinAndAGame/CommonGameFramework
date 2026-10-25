---
name: cgf-stat
description: 使用或修改 CommonGameFramework Stat。写 FloatStat / IntStat / BoolStat、加减修正器或经 CommandContext 注入属性时读取。
---

# Stat

## 入口

游戏层构造 `FloatStat` / `IntStat` / `BoolStat`，经 `CommandContext.SetService` 注入。读写 `BaseValue` / `Value`；修正走 `AddModifier` / `RemoveModifiersFromSource` / `ClearModifiers`。

## 典型用法

```csharp
var hp = new FloatStat();
hp.BaseValue = 100f;
hp.AddModifier(new FloatStatModifier("buff_poison", ModifierType.Flat, -10f));
ctx.SetService(hp);
```

修正器单位勿混用：

| 类型 | Flat | PercentAdd | PercentMult |
|------|------|------------|-------------|
| `FloatStatModifier` | 绝对值 | 小数比例（`0.2` = +20%） | 乘数（`2` = 2 倍） |
| `IntStatModifier` | 绝对值 | 百分点（`20` = +20%） | 百分乘数（`200` = 2 倍） |

`BoolStatModifier`：任一来源 `true` 则结果为 `true`，否则回落 `BaseValue`（`Type` 固定 Flat）。

## 扩展点

可注入自定义 `IStatAggregator`。修正器 `sourceId` 用于按来源成批移除。

## 禁止

- 框架层写 Hp / Attack 等业务属性名
- 混用 Float 与 Int 的百分比含义
- 绕过 Stat 直接 cast 具体 Aggregator 才能清修正
- 使用 `GameStat`、非泛型 `IStatModifier` 别名
