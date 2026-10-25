# 删除 Presentation 模块

串行、并行、延迟、取消都能用 UniTask 直接表达（`await` / `WhenAll` / `Delay` / `CancellationToken`）。`ActionScheduler`、`ActionBus`、`IPresentationAction`、`PresentationChannel` 只是一层薄封装，游戏层 Director 自己写异步方法即可，不再需要框架调度器。

Logic / Presentation **分层原则保留**：Command 同步改权威状态；动效用游戏层 UniTask 播放且不回写权威状态。

## Status

accepted

## Considered Options

- 整段删除模块（采纳）
- 保留接口但改成对 UniTask 的薄包装（仍增加学习成本）
- 仅删除 Channel / Bus，保留 Scheduler（剩余价值不足）
