# ADR-0001: CommandContext 使用 Services Bag

## Status

Accepted

## Context

CommandContext 曾直接持有 BuffContainer、GameStat、CommandExecutor，导致 Command 模块 interface 过宽，Presentation 间接耦合 Buff/Stat，与 framework-design 原则冲突。

## Decision

CommandContext 仅保留 object Source/Target，以及 lazy-create 的 services bag（SetService / GetService）。游戏层在战斗初始化时注入 BuffContainer、FloatStat 等具体类型。

## Consequences

- 框架 Command/Presentation 模块不再引用 Buff/Stat 具体类型（除游戏层 Command 实现自身）。
- 测试 Command 时可 mock services bag，无需构造完整 Buff 图。
- 游戏层需自行 SetService，多一行装配代码。
