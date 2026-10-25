---
name: cgf-pool
description: 使用或修改 CommonGameFramework Pool。游戏层 Pool.Get / Release 预制体、实现 IPoolable，或测试替换 IPoolRegistry 时读取。
---

# Pool

## 入口

调用方只碰静态 `Pool`。类对象走 `IPoolable` + `Get<T>()` / `Release`；场景物体走 `Get(prefab)` / `Release(instance)`。实现经 `IPoolRegistry` → `Pools/` 具体池。

## 典型用法

```csharp
var enemy = Pool.Get(enemyPrefab);
Pool.Release(enemy);

var fx = Pool.Get<HitFx>();
Pool.Release(fx);
```

Game/UI 只 `Pool.Get` **已在 Prefab 配齐组件** 的 `Container_*`。框架首次使用会创建 `[PoolRoot]` / `[Pool]_具体名`。

## 扩展点

测试可 `Pool.Registry = new TestPoolRegistry()`。可池化类实现 `IPoolable`（`OnSpawnFromPool` / `OnReturnToPool`）。

## 禁止

- 游戏层直接依赖 `ClassPool` / `MonoPool`
- Game/UI 用 `Instantiate` 临时拼 Hierarchy 再当池用
- 运行时 `AddComponent` 给池化实例补组件
