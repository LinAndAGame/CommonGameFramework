# 删除 ICommandRunner / CommandExecutor

Logic Command 的唯一执行入口定为 `IGameCommand.Execute`：删除仅有单一透传 adapter 的 `ICommandRunner` / `CommandExecutor`，`LogicStep` 与嵌套子 Command 均直接调用 `Execute`；`IScheduleStep` 不再携带 runner 参数，避免 Logic 执行细节泄漏进表现 Step。`IRevertibleCommand` 保留，由游戏层自行 `Revert`。日后若出现第二 adapter（如 recording / undo 栈）再重新开 seam。

## Status

accepted

## Considered Options

- 整段删除（采纳）
- 保留类型但折叠执行路径（误导面）
- 加深为可注入唯一入口（扩大 scope，留给真有第二 adapter 时）
