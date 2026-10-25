---
name: cgf-view
description: 使用或修改 CommonGameFramework View。派生 BaseView / ViewLayer、用 ViewFlowController 做 OpenAsync / HideAsync，或向 UICanvasRoot 注册 Canvas 时读取。
---

# View

## 入口

日常只用 `ViewFlowController.OpenAsync` / `HideAsync`。游戏项目派生 `ViewLayer`，启动时 `UICanvasRoot.RegisterCanvas`。视图挂到 `GetLayerRoot` 下。无多层时用 `DefaultViewLayer.Instance`。

## 典型用法

```csharp
public sealed class MainViewLayer : ViewLayer
{
    public static MainViewLayer Instance { get; } = new();
}

canvasRoot.RegisterCanvas(MainViewLayer.Instance, mainCanvas);
await flow.OpenAsync(shopView, new ViewOpenOptions
{
    Layer = MainViewLayer.Instance,
    StackMode = ViewStackMode.Push,
    Param = shopId
});
await flow.HideAsync(shopView);
```

`Push`：压栈并隐藏下层；已在栈中则提到栈顶。`Replace`：清空该层再打开。`Hide` 栈顶则 Pop 并恢复下层 `OnDisplay`；非栈顶只从栈移除。

## 扩展点

游戏层派生 `BaseView`（实现 `OnCreate` / `OnDisplay` / `OnHide` 及 Async 对）。勿在 `Awake` 初始化。

## 禁止

- 框架层用 `enum` 预设 UI 槽位（已删除 `UILayer`）
- 绕过 Flow 直接改 `ViewStackManager`
- Hide 只藏不 Pop
- Game/UI 运行时 `new GameObject` 拼视图 Hierarchy
