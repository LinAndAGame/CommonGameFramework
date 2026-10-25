---
name: cgf-buff
description: 使用或修改 CommonGameFramework Buff。经 CommandContext 注入 IBuffRegistry / IBuffContainer，创建、合并、TickRealtime / AdvanceTurn 或派生 Duration / MergePolicy 时读取。
---

# Buff

## 入口

游戏层只碰 `IBuffRegistry` / `IBuffContainer`，经 `CommandContext` 注入。创建走 `TryCreate`；入容器走 `TryAdd`。时长：`TickRealtime` 推 `IRealtimeDuration`，`AdvanceTurn` 推 `ITurnBasedDuration`。

## 典型用法

```csharp
var registry = new BuffRegistry();
registry.Register(poisonDef);
var container = new BuffContainer();
ctx.SetService<IBuffRegistry>(registry);
ctx.SetService<IBuffContainer>(container);

if (context.GetService<IBuffRegistry>().TryCreate("poison", context.Target, out var instance))
    context.GetService<IBuffContainer>().TryAdd(instance);
```

`MergeResult`：`Merged`（incoming 走 `OnMergeDiscarded`）、`Replaced`、`NotMerged`（同 ID 可共存，Find 仍匹配首个）。`Duration == null` 表示永不过期。

## 扩展点

游戏项目派生 `BaseBuff` / `BuffInstance`、`BuffMergePolicy`、`IRealtimeDuration` / `ITurnBasedDuration`。框架不预设业务 Buff。未激活默认不推进 Duration（`ShouldAdvanceDuration => Active`）。

## 禁止

- 框架层写 Poison / Stun 等业务类型
- 在表现回调里改 Buff 权威状态
- 绕过 Registry 直接 `new` 实例塞进容器
- services bag 只注入 `BuffContainer` 具体类
- Policy 直接 Remove 容器内实例；在 Container 内 cast Duration
