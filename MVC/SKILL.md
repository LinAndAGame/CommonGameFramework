---
name: cgf-mvc
description: 使用或修改 CommonGameFramework MVC。接 Model/View/Controller、MvcScope 装配，或让现有 Panel 实现 IMvcView.Refresh 消费快照时读取。
---

# MVC

命名空间 `CommonGameFramework.MVC`。纯 C#，不依赖 Unity 组件或其他框架模块。根目录只留 `MvcScope`。

## 入口

从 `MvcScope` 装配。游戏层先 `RegisterModel` / `RegisterController` 并初始化视图，再 `Initialize()`；退出 `Dispose()`。View 只实现 `IMvcView<TSnapshot>.Refresh`。

## 典型用法

```csharp
var scope = new MvcScope();
scope.RegisterModel(hpModel);
scope.RegisterController(new HpController(hpModel, hpPanel));
scope.Initialize();
// 作用域退出
scope.Dispose();
```

- Model 同步结算后 `PublishChanged()`；`TSnapshot` 必须是不可变值或独立副本。
- Controller 构造注入 Model 与 View；`Init` / `OnDispose` 成对订阅视图意图。
- 重复注册抛错；初始化开始后冻结装配；初始化失败自动清理，失败实例不可重用。
- 控制器逆序释放；模型与视图属外部注入，作用域不销毁 Unity 对象。

## 扩展点

游戏层提供具体 Model / View / Controller。现有 Panel 可实现 `IMvcView`。不替代 `ViewFlowController`，也不预设 `BasePanel`。

## 禁止

- View 改权威状态；Model 等待动画或依赖具体 View
- Controller 代绑 `Button.onClick`
- 全局静态模型仓库；框架预设业务 Model
- 作用域销毁外部注入的 View / Model
- 把可修改的状态集合或 Model 引用当作 Snapshot
