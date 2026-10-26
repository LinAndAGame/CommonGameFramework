# CommonGameFramework

通用 Unity 游戏框架库（`CommonGameFramework` 程序集）。AI 入口见 [AGENTS.md](AGENTS.md)；术语见 [CONTEXT.md](CONTEXT.md)。

## 模块概览

| 模块 | 职责 | 目录要点 |
|------|------|----------|
| `Pool` | 对象池；`IPoolRegistry` + 静态 `Pool` 门面 | 根 `Pool`；`Registry/`；`Core/`；`Pools/` |
| `Res` | 资源加载；`IResourceLoader` + 静态 `Res` 门面 | 根 `Res`；`Loader/` |
| `Clock` | 游戏时钟；Pause / TimeScale | 根 `Clock`；`Core/`（含 `ClockDriver`） |
| `Rng` | 可种子随机 | 根 `Rng`；`Core/` |
| `Audio` | 音效 / BGM | 根 `Audio`；`Core/` |
| `Cooldown` | 冷却计时 | 根 `CooldownTracker`；`Core/` |
| `Command` | 同步 Logic Command；services bag | 三文件，无子目录 |
| `Buff` | Buff 容器 / 注册 / 时长 / 合并 | `Core/`；`Duration/`；`Merge/`；`Factory/` |
| `Stat` | 属性聚合 | `Core/`；`Float/`；`Int/`；`Bool/` |
| `Achievement` | 本地权威 + 可选平台 | 根 `AchievementService`；`Core/`；`Local/`；`Platform/` |
| `View` | BaseView 异步 Open/Hide | 根 `ViewFlowController`；`Core/`；`Stack/`；`Canvas/` |
| `DragPlace` | UI 拖放会话；`groupId` 配对源和放置点 | `Component/ui/DragPlace/`；根 `DragController`；`Source/`；`Place/`；`Session/` |
| `MVC` | 模型快照、视图刷新、控制器生命周期 | 根 `MvcScope`；`Model/`；`View/`；`Controller/` |
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

## 表现编排

框架不提供动效队列。游戏层 Director 在 Logic 结算后直接 `await` / `UniTask.WhenAll`；取消用 `CancellationToken`。

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

## DragPlace

命名空间 `CommonGameFramework.DragPlace`。不依赖游戏 UI 中间层。框架依赖 DOTween 与 UniTask：失败回源用 DOTween（`Ease` 默认 `OutQuad`），等待完成用 UniTask。

`DragSource` 与 `DragPlace` 写上相同的 `groupId` 就会配对（留空则同属默认组）。它们在启用时登记到 `DragController`。找不到已有控制器时，运行时在所属 Canvas 下创建 `[DragPlace]`。

```csharp
source.SetGroupId("hand");
source.SetUserData(card);
source.DragSucceeded += args => { /* 玩法数据在游戏逻辑里改 */ };

place.SetGroupId("hand");
place.SetCanAccept(incoming => incoming.UserData != null);

controller.Settings = dragSettings; // 空则用默认吸附范围 80、回源 0.25 秒
controller.SetCanPlaceHandler((incoming, target) => true);
```

源物体（或其子节点）需要能被射线打中的 `Graphic`。子节点自己吃掉拖拽事件时，加 `DragPointerBridge` 指向 `DragSource`。配置资产菜单：`CommonGameFramework/Drag Settings`。

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

框架必要插件：DOTween 与 UniTask。

- UniTask（程序集名 `UniTask`）
- DOTween：预编译 `DOTween.dll`，以及模块程序集 `DOTween.Modules`（Utility Panel 生成 ASMDEF 后保持该程序集名）。`DOMove`、`DOAnchorPos`、`AsyncWaitForCompletion` 在模块程序集里
- Odin Inspector Attributes（预编译 DLL）
