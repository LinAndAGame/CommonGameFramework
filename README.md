# CommonGameFramework

通用 Unity 游戏框架库（`CommonGameFramework` 程序集）。领域术语见 [CONTEXT.md](CONTEXT.md)。

## 模块概览

| 模块 | 职责 | 目录要点 |
|------|------|----------|
| `Pool` | 对象池；`IPoolRegistry` + 静态 `Pool` 门面 | 根 `Pool`；`Registry/`；`Core/`；`Internal/` |
| `Res` | 资源加载；`IResourceLoader` + 静态 `Res` 门面 | 根 `Res`；`Loader/` |
| `Time` | 游戏时钟；Pause / TimeScale | 根 `Clock`；`Core/`（含 `ClockDriver`） |
| `Random` | 可种子随机 | 根 `Rng`；`Core/` |
| `Audio` | 音效 / BGM | 根 `Audio`；`Core/` |
| `Cooldown` | 冷却计时 | 根 `CooldownTracker`；`Core/` |
| `Command` | 同步 Logic Command；services bag | 三文件，无子目录 |
| `Presentation` | 回合制动效队列 | 根 `ActionScheduler`；`Core/`；`Bus/`；`Actions/`；`Steps/` |
| `Buff` | Buff 容器 / 注册 / 时长 / 合并 | `Core/`；`Duration/`；`Merge/`；`Factory/` |
| `Stat` | 属性聚合 | `Core/`；`Float/`；`Int/`；`Bool/` |
| `Achievement` | 本地权威 + 可选平台 | 根 `AchievementService`；`Core/`；`Local/`；`Platform/` |
| `View` | BaseView 异步 Open/Hide | 根 `ViewFlowController`；`Core/`；`Stack/`；`Canvas/` |
| `StateMachine` | 通用 FSM | 扁平 |
| `Style` | 样式切换 | 扁平 |
| `Extensions` | C# / Unity API 扩展方法 | `System/`；`Unity/` |
| `Utils` | 通用算法与工具类 | 扁平 |

存档用游戏层 **EasySave**；文案用 **Unity Localization**；Event 总线暂缓。

## CommandContext（Services Bag）

框架层**不**挂载 BuffContainer、FloatStat 等具体类型。游戏层 lazy 注入：

```csharp
var ctx = new CommandContext { Source = player, Target = enemy };
ctx.SetService(buffContainer);
ctx.SetService(enemyHp); // FloatStat

// Command 内
var hp = context.GetService<FloatStat>();
childCommand.Execute(context); // 嵌套子 Command 直接 Execute
```

## Presentation 动效队列

### 三层职责

| 层 | 类型 | 说明 |
|----|------|------|
| 编排 | `ActionScheduler` + `IScheduleStep` | 主序列串行 |
| 逻辑 | `IGameCommand` | 同步改状态 |
| 表现 | `IPresentationAction` | 异步 UniTask 播动效 |

### PresentationChannel（游戏层扩展）

```csharp
public sealed class BattlePresentationChannel : PresentationChannel
{
    public static BattlePresentationChannel Default { get; } = new();
}
```

无并行需求时用 `DefaultPresentationChannel.Instance`。

### 典型用法

```csharp
var ctx = new CommandContext { Source = player, Target = enemy };
ctx.SetService(enemyHp);
ctx.SetService(buffContainer);

await new ActionScheduler()
    .Clear()
    .EnqueueLogic(new DealDamageCommand(12))
    .EnqueueParallel(
        (BattlePresentationChannel.Default, new HitFlashAction(enemy)),
        (UiPresentationChannel.Default,     new HpBarShakeAction(enemy)))
    .EnqueuePresentation(UiPresentationChannel.Default, new DamageNumberAction(12, enemy))
    .RunAsync(new PresentationContext(ctx));
```

`EnqueueGroup` 内部转为 `SerialGroupStep`，与 `EnqueueStep(new SerialGroupStep(...))` 语义一致。

`Cancel()` 或 `RunAsync` 被 token 取消时会**清空**未执行主序列，不会从断点续跑。`IsBusy == true` 时游戏层应锁定输入。

## View（游戏层扩展）

`UILayer` enum 已移除。游戏项目派生 `ViewLayer` 并在 `UICanvasRoot.RegisterCanvas` 绑定 Canvas：

```csharp
public sealed class MainViewLayer : ViewLayer
{
    public static MainViewLayer Instance { get; } = new();
}

// 启动时
canvasRoot.RegisterCanvas(MainViewLayer.Instance, mainCanvas);
```

`ViewFlowController`：`OpenAsync` 按 Push/Replace 入栈；`HideAsync` 出栈（栈顶 Pop，非栈顶从栈移除），若关闭的是栈顶则恢复下层 `OnDisplay`。日常只用这两个 API。

## Res / Pool Adapter

```csharp
Res.Loader = new MyAddressablesLoader();      // 替换资源后端（实现放 Loader/）
Pool.Registry = new TestPoolRegistry();       // 测试用 in-memory 池
```

## Clock / Rng / Audio / Cooldown

```csharp
Clock.IsPaused = true;   // 首次访问自动创建 [Clock] + ClockDriver
Clock.TimeScale = 0.5f;

Rng.Reseed(12345);       // 默认 UnityRandom；单测可 Rng.Current = new CSharpRandom(12345)
var roll = Rng.Range(0, 100);

Audio.Player = new UnityAudioPlayer(sfxSource, bgmSource, id => Res.Load<AudioClip>(id.Key));
Audio.PlaySfx(HitSfxId.Instance);

var cooldowns = new CooldownTracker();
cooldowns.Start(DashCooldownId.Instance, 3f);
cooldowns.Tick(Clock.DeltaTime);
```

`AudioId` / `CooldownId` 均为抽象基类。框架运行时物体命名：`[Clock]`、`[PoolRoot]`、`[Pool]_Xxx`。

## Extensions / Utils

- `Extensions/System`：`String` / `Collection` / `Dictionary` / `List`（含 `Shuffle`）
- `Extensions/Unity`：`Transform` / `GameObject` / `Component` / `RectTransform`
- `Utils`：`MathUtil.Remap`、`WeightedRandom.Pick`、`StringUtil.JoinNonEmpty`

## Buff

### 时长驱动

| 入口 | 用途 |
|------|------|
| `BuffContainer.TickRealtime` | `IRealtimeDuration`（如 RealtimeDuration） |
| `BuffContainer.AdvanceTurn` | `ITurnBasedDuration`（如 TurnDuration） |

未激活 Buff 默认不推进 Duration（`ShouldAdvanceDuration => Active`）；子类可 override。

### 创建与合并

```csharp
var registry = new BuffRegistry();
registry.Register(poisonDef);

var container = new BuffContainer();
container.BuffAdded += inst => { /* Presentation adapter */ };

ctx.SetService<IBuffRegistry>(registry);
ctx.SetService<IBuffContainer>(container);

// Command 内
if (context.GetService<IBuffRegistry>().TryCreate("poison", context.Target, out var instance))
    context.GetService<IBuffContainer>().TryAdd(instance);
```

`MergeResult`：`Merged`（OnMergeDiscarded incoming）、`Replaced`、`NotMerged`（同 BuffId 可共存，Find 仍匹配首个）。

`Duration == null` 表示永不过期。错配 Duration 类型时 Editor/Development 下 LogWarning。

TurnManager / Update 经 `IBuffContainer` 调用 `TickRealtime` / `AdvanceTurn`。

## Stat

```csharp
var hp = new FloatStat();
hp.BaseValue = 100f;
hp.AddModifier(new FloatStatModifier("buff_poison", ModifierType.Flat, -10f));
ctx.SetService(hp);
```

修正器单位对照（Float / Int 不同，勿混用）：

| 类型 | Flat | PercentAdd | PercentMult |
|------|------|------------|-------------|
| `FloatStatModifier` | 绝对值 | 小数比例（`0.2` = +20%） | 乘数（`2` = 2 倍） |
| `IntStatModifier` | 绝对值 | 百分点（`20` = +20%） | 百分乘数（`200` = 2 倍） |

`BoolStatModifier`：任一来源 `true` 则结果为 `true`，否则回落 `BaseValue`（`Type` 固定 Flat）。`ClearModifiers` 在 `IStatAggregator` / `StatBase` 上可用。

## Achievement

游戏只调 `AchievementService`；本地存档与平台同步是两个独立 seam。

```csharp
// 游戏层派生 Id
public sealed class FirstWinId : AchievementId
{
    public static FirstWinId Instance { get; } = new();
    public override string Key => "first_win";
}

var achievements = new AchievementService(new PlayerPrefsAchievementLocal());
achievements.Register(new AchievementDefinition(FirstWinId.Instance));
achievements.Register(new AchievementDefinition(Kill100Id.Instance, targetValue: 100));
achievements.Load();

// 可选：挂平台（SDK 配置写在 adapter 构造函数里）
var composite = new CompositeAchievementPlatform();
composite.Add(new SteamAchievementPlatform(appId));
achievements.AttachPlatform(composite);

achievements.OnUnlocked += id => { /* 弹窗 */ };
achievements.Unlock(FirstWinId.Instance);
achievements.AddProgress(Kill100Id.Instance, 1);
achievements.Save();
```

离线仅本地：不调用 `AttachPlatform`，或 `AttachPlatform(null)`。

## 框架设计原则

详见 `%USERPROFILE%\.cursor\rules-library\unity\framework-design.mdc`。

架构决策见 [docs/adr/](docs/adr/)。

## 依赖

- UniTask
- Odin Inspector Attributes（预编译 DLL）
