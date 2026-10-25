---
name: cgf-command
description: 使用或修改 CommonGameFramework Command。写 IGameCommand、装配 CommandContext services bag、嵌套子 Command 或实现 IRevertibleCommand 时读取。
---

# Command

## 入口

只实现 `IGameCommand.Execute`。上下文用 `CommandContext`（`Source` / `Target` + 按类型注入的 services bag）。需要撤销时额外实现 `IRevertibleCommand`，由调用方自行 `Revert`。

## 典型用法

```csharp
var ctx = new CommandContext { Source = player, Target = enemy };
ctx.SetService(buffContainer);
ctx.SetService(enemyHp);

new DealDamageCommand(12).Execute(ctx);
childCommand.Execute(ctx);
```

## 扩展点

游戏层注入 Buff、Stat 等具体服务；框架不预设服务类型。嵌套子 Command 直接 `Execute`，不经执行器。

## 禁止

- `Execute` 里 `await` 或播动画 / 特效
- 在框架层给 Context 挂 BuffContainer、FloatStat 等具体类型
- 把 services bag 当全局 ServiceLocator
- 经 bag 注入 Command 执行器
