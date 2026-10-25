---
name: cgf-res
description: 使用或修改 CommonGameFramework Res。业务加载资源、替换 IResourceLoader（Addressables 等）或禁止直接 Resources.Load 时读取。
---

# Res

## 入口

调用方只碰静态 `Res.Load` / `LoadAsync`。默认 `ResourcesResourceLoader`；启动时可换后端。

## 典型用法

```csharp
Res.Loader = new MyAddressablesLoader();
var clip = Res.Load<AudioClip>(id.Key);
var prefab = await Res.LoadAsync<GameObject>(path);
```

场景固定 UI 的 Icon 在 Inspector 挂好，不要对固定 Icon 赋 `Image.sprite`。仅运行时池化、数据驱动的 Icon 才在 `Refresh` 里经 `Res` 加载。

## 扩展点

游戏层实现 `IResourceLoader`，放在消费项目（不要改框架默认以外的业务 loader）。测试可替换 in-memory loader。

## 禁止

- 业务代码直接 `Resources.Load`
- 用 `Resources.Load<ScriptableObject>` 当数值表
