---
name: cgf-drag-place
description: 使用或修改 CommonGameFramework DragPlace。接 DragSource / DragPlace / DragController、改命中与回源、或替换 DragSettings 时读取。
---

# DragPlace

命名空间 `CommonGameFramework.DragPlace`。对外脚本放在 `Component/ui/DragPlace/` 根目录，内部实现放在 `Internal/`。不为单文件再拆无意义的小文件夹。命名空间不随目录变。不依赖游戏 Mid / UIWidget。框架依赖 DOTween 与 UniTask：失败回源用 DOTween（`DOMove` / `DOAnchorPos` / `DOScale` / `DOLocalRotate`、`Sequence`、`SetEase`、`Kill`），完成等待用 UniTask `await` DOTween 的 `AsyncWaitForCompletion()`。

## 入口

`DragSource` 是拖拽源，`DragPlace` 是放置点，`DragController` 负责当前这一次拖拽。两者用 `groupId`（空字符串也算一组）配对，并在 `OnEnable` 时向控制器注册。

查找控制器的顺序：组件上指定的控制器、父节点上的 `DragController`、该 Canvas 已有的控制器、场景里唯一的控制器。都没有时，运行时在所属 Canvas 下创建 `[DragPlace]`（拖拽层就是这个节点）。不在 Canvas 下且没有控制器时不会拖拽。

## 典型用法

```csharp
source.SetGroupId("hand");
source.SetUserData(card);
source.DragSucceeded += args => { /* 只读结果；玩法数据在游戏逻辑里改 */ };

place.SetGroupId("hand");
place.SetCanAccept(incoming => incoming.UserData != null);

controller.Settings = dragSettings;          // 空则用 DragSettings 默认值
controller.DragLayer = overlayRect;
controller.SetCanPlaceHandler((incoming, target) => true);
controller.DragSucceeded += args => { };
```

射线目标可以在子节点上。子节点自己实现了拖拽接口时，在那个节点挂 `DragPointerBridge` 并指向 `DragSource`。

## 一次拖拽

`DragController` 只记当前这一次拖拽。开始：记录原点。`CreateFakeOnDrag` 为真时克隆跟随指针，原件留在原地；否则直接移动原件。`BringToFront` 把跟随物放到拖拽层最后。

移动：跟随指针，并用射线、矩形、吸附范围（像素，矩形外到边缘的距离）检测同组放置点，派发进入/离开。悬停不要求 `CanAccept`，松开才要求。

松开：同组且 `DragPlace.CanAccept` 与控制器 `SetCanPlaceHandler` 都通过则成功。有克隆时销毁克隆，原件不动；没有克隆时按配置换父并吸附到放置点。失败则按 `ReturnDuration` / `ReturnEase` 回源（时长 0 立即回去）。有副本时 `Sequence` 里 `DOMove` 飞回原件世界坐标，结束后销毁副本；没有副本时先挂回原父节点，再对 `RectTransform` 做 `DOAnchorPos` / `DOScale` / `DOLocalRotate`。当前回源 tween 存在 `Tween` 字段里，`StopDrag()` 或控制器销毁会 `Kill()`。缓动走 DOTween 默认更新（受 `timeScale` 影响）。回源结束前不能开始下一次拖拽；`StopDrag()` 会立刻收回并允许再拖。`SnapEnabled` 同时决定吸附范围和成功时是否贴到放置点坐标。

## 扩展点

`DragSettings`（ScriptableObject，菜单 CommonGameFramework/Drag Settings）配置吸附、克隆、置顶、成功换父、回源时长与缓动。`ReturnEase` 是 DOTween 的 `Ease`，默认 `OutQuad`。

回调是 C# event：源上 `BeginDrag` / `Dragging` / `EndDrag` / `DragSucceeded` / `DragFailed`；放置点上 `Entered` / `Exited` / `DragSucceeded` / `DragFailed`；控制器上成功、失败、进入、离开。自定义指针可以调用 `DragSource.ProcessBeginDrag` / `ProcessDrag` / `ProcessEndDrag`，或 `DragController.HandleBeginDrag` / `HandleDrag` / `HandleEndDrag`。

## 目录

按「外部能用 / 外部不能用」分开放，不为单文件再拆无意义的小文件夹。命名空间仍是 `CommonGameFramework.DragPlace`。

对外脚本（模块根，游戏层会挂组件或订阅）：`DragController`、`DragSource`、`DragPlace`、`DragSettings`、`DragEventArgs`、`DragPointerBridge`。

内部实现（`Internal/`）：`CurrentDrag` 是当前这一次拖拽（原先的 `DragSession`）；`DragHitTester` 做射线、矩形和吸附范围；`DragVisual` 管副本、跟随和回源；`DragCloneGuard` 在克隆时避免把副本登记成正式的源或放置点；`DragRegistry` 按 `groupId` 登记；`DragControllerLookup` 查找或创建控制器；`DragSettingsRuntime` 在配置为空时给出默认值。不要再用 `Session` 做类型名。

## 依赖

框架依赖 DOTween 与 UniTask。`CommonGameFramework.asmdef` 引用程序集 `UniTask`、`DOTween.Modules`，并预编译引用 `DOTween.dll`。`Tween` / `Ease` / `DOTween.Sequence` 在 `DOTween.dll`；`DOMove`、`DOAnchorPos`、`DOScale`、`DOLocalRotate`、`AsyncWaitForCompletion` 在 `DOTween.Modules`。用 DOTween Utility Panel 生成 ASMDEF 后，程序集名必须保持 `DOTween.Modules`。

## 风格

遵守 `.cursor/rules/csharp-style.mdc`。开括号与声明同一行，禁止 Allman。布尔写成 `== true` / `== false`。每个方法头部一行 `//`。静态方法放在 `static class`：查找控制器是 `DragControllerLookup`，空配置默认值是 `DragSettingsRuntime`。`DragController` 与 `DragSettings` 上不再挂静态方法。

## 禁止

- 依赖 `UIWidget`、`WidgetType`、游戏 `UIEvent` / `Language.Config`
- 用手写插值代替 DOTween 回源
- 用框架 enum 预设卡牌槽、战斗阶段等业务分组（分组只是字符串 `groupId`）
- 在拖拽回调里把表现当权威状态写回战斗结算
- 为了能拖而运行时给业务物体 `AddComponent` 拼 UI；源上需要能被射线打中的 `Graphic`
