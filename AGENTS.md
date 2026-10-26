# CommonGameFramework

先读 `CONTEXT.md`（术语）。只打开当前在改的模块 `SKILL.md`，不要一次读完。

改了模块的公开入口、扩展点或禁止事项：先改该目录 `SKILL.md`，再改调用方。
契约与代码冲突时以代码为准，并立刻改契约。

框架依赖 DOTween 与 UniTask。DragPlace 的失败回源用 DOTween，等待完成用 UniTask。

命名优先用业务能直接读懂的词（当前拖拽、放置点、回源、吸附等）。禁止用 `Session` / `session` 表示临时运行时状态。类型、目录、字段、注释都遵守。

C# 风格见 `.cursor/rules/csharp-style.mdc`。开括号与声明同一行，禁止 Allman。布尔写成 `== true` / `== false`，禁止 `if (flag)` / `if (!flag)`。每个方法头部一行 `//`。局部函数放在宿主方法末尾。互斥业务用 `if / else if / else`，分支前写注释，禁止中间 `return` 后再写同级业务分支。静态方法只能在 `static class`。`abstract class` 以 `Base` 开头。DragPlace 已按此写；旧模块不要顺手全库重排。

| 模块 | 契约 | 何时打开 |
|------|------|----------|
| Command | `Command/SKILL.md` | 改 CommandContext / IGameCommand，或游戏层写 Logic Command |
| Buff | `Buff/SKILL.md` | 改 Buff，或游戏层加 / 合并 / 推进时长 |
| Stat | `Stat/SKILL.md` | 改属性或修正器 |
| View | `View/SKILL.md` | 改视图栈 / ViewLayer / BaseView |
| MVC | `MVC/SKILL.md` | 改 Model/View/Controller/Scope，或游戏层接 MVC |
| Achievement | `Achievement/SKILL.md` | 改成就或接平台 |
| Pool | `Pool/SKILL.md` | 改对象池，或游戏层 Get / Release |
| Res | `Res/SKILL.md` | 改资源加载，或替换 Addressables 后端 |
| DragPlace | `Component/ui/DragPlace/SKILL.md` | 改拖拽源 / 放置点 / DragController，或游戏层接拖放。对外脚本在模块根，内部实现在 `Internal/`，不为单文件再拆小文件夹 |

其余门面无独立 Skill，只走静态入口：

- `Clock`：Pause / TimeScale / DeltaTime；首次访问创建 `[Clock]`
- `Rng`：可种子随机；单测换 `CSharpRandom`
- `Audio`：启动注入 `IAudioPlayer`；游戏层派生 `AudioId`
- `CooldownTracker`：`Tick(Clock.DeltaTime)`；游戏层派生 `CooldownId`
- `Extensions` / `Utils` / `Style` / `StateMachine`：按现有类型用，不扩业务语义

表现编排不在框架内。游戏层 Director 在 Logic 结算后直接 `await` / `UniTask.WhenAll`；取消用 `CancellationToken`。
