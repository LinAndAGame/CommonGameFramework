# CommonGameFramework

通用 Unity 游戏框架库：提供对象池、资源加载、逻辑 Command、表现队列、Buff/Stat、时钟/随机/音频/冷却、视图流程等**机制**；卡牌战斗、UI 层级、Channel 分区等**语义**由游戏项目扩展。存档用游戏层 EasySave，文案用 Unity Localization；Event 总线暂缓。

## Language

**Logic Command**:
同步执行的逻辑动作，通过 `IGameCommand` 修改权威游戏状态。
_Avoid_: Action（与 Presentation Action 混淆）

**Presentation Action**:
异步表现动作，通过 `IPresentationAction` 播放动效；不持有权威状态。
_Avoid_: AnimationCommand, VFXCommand

**PresentationChannel**:
表现并行车道的标识；框架提供抽象基类，游戏项目派生子类并按实例路由。
_Avoid_: ActionChannel enum, Layer enum

**ActionScheduler**:
表现主序列编排入口；根目录门面。`ActionBus` / `IScheduleStep` / 内置 Action 分属 `Core/`、`Steps/`、`Actions/`。
_Avoid_: 游戏层直接拼 ActionQueue；BuiltIn 笼统目录名

**ViewLayer**:
视图栈与 Canvas 挂载层的标识；框架提供抽象基类，游戏项目派生子类并注册 Canvas。
_Avoid_: UILayer enum

**ViewFlowController**:
视图编排入口：Open 入栈，Hide 出栈并恢复下层；日常只用 OpenAsync / HideAsync。
_Avoid_: 绕过 Flow 直接改 ViewStackManager；Hide 只藏不 Pop

**CommandContext**:
逻辑 Command 的运行上下文；仅含 Source/Target 与 lazy-create 的 services bag。
_Avoid_: 在框架层挂载 BuffContainer、FloatStat 等具体类型

**Schedule Step**:
主序列调度单元，编排 Logic 与 Presentation 的执行顺序。
_Avoid_: GameAction, AbstractGameAction；子命名空间 Presentation.Steps

**Services Bag**:
CommandContext 内按类型 lazy 注册/获取的服务容器；游戏层注入 Buff、Hp 等。
_Avoid_: ServiceLocator（全局）；经 bag 注入 Command 执行器（嵌套子 Command 直接 `Execute`）

**IBuffRegistry**:
Buff 定义注册表 seam；`Register` / `TryCreate` 显式失败语义，经 services bag 注入。
_Avoid_: `Create` 返回 null；IBuffFactory（双路径）

**IBuffContainer**:
Buff 容器 seam；TryAdd / Remove / TickRealtime / AdvanceTurn + 生命周期事件，测试可 mock。
_Avoid_: services bag 仅注入 BuffContainer 具体类

**MergeResult**:
同 ID Buff 合并结果（Merged / Replaced / NotMerged）；BuffContainer 编排 Remove / Add；Merged 时 incoming 走 OnMergeDiscarded。
_Avoid_: TryMerge 返回 bool；Policy 直接 Remove 容器内实例

**BuffInstance**:
运行时 Buff 实例；Init / Active / TryTrigger / Duration 驱动 / OnMergeDiscarded；CurLayer / Duration 经 mutator 修改。
_Avoid_: 在 BuffContainer 内 cast Duration；public set CurLayer / Duration

**FloatStat**:
浮点属性；经 services bag 注入。修正器用 `FloatStatModifier` / `IStatModifier<float>`。
_Avoid_: GameStat、StatModifier、非泛型 IStatModifier 别名

**IStatAggregator**:
属性聚合 seam；含 ClearModifiers；Stat 持有唯一实例，可注入自定义实现。
_Avoid_: 绕过 Stat 直接 cast 具体 Aggregator 才能清修正

**Pool**:
对象池静态门面；实现经 `IPoolRegistry`（`Registry/`）→ `Internal/` 池。
_Avoid_: 游戏层直接依赖 ClassPool / MonoPool

**Res**:
资源加载静态门面；实现经 `IResourceLoader`（`Loader/`），默认可换 Addressables。
_Avoid_: 直接 `Resources.Load`

**AchievementId**:
成就标识基类；游戏项目派生子类，以 `Key` 做本地存档与平台映射。
_Avoid_: 框架层成就 enum

**AchievementDefinition**:
成就定义（Id + TargetValue）；TargetValue == 1 为一次性，> 1 为进度型。
_Avoid_: 未 Register 即改进度

**AchievementService**:
成就唯一入口：本地权威、可选平台同步、解锁/进度事件。
_Avoid_: 游戏逻辑直接写 IAchievementLocal；OfflineAchievementPlatform（用 null）

**IAchievementLocal**:
本地存档 seam（进度 / 解锁）；InMemory 与 PlayerPrefs 等实现放在 `Local/`。
_Avoid_: 绕过 AchievementService 改解锁状态

**IAchievementPlatform**:
外部平台同步 seam；SDK 配置在各 adapter 构造时注入；不挂载即仅本地；Composite 等放在 `Platform/`。
_Avoid_: IAchievementPlatformWithProfile；AchievementPlatformProfile；BuiltIn 笼统目录名

**Clock**:
游戏时钟门面；`IClock` / `UnityClock`；首次使用创建 `[Clock]` + `ClockDriver`；Pause 时 DeltaTime=0。
_Avoid_: 框架强制改 `UnityEngine.Time.timeScale`；Game 层手搓同类运行时物体

**Rng**:
可种子随机门面；`IRandom` + `UnityRandom`（默认）/ `CSharpRandom`（单测）。
_Avoid_: 与 `System.Random` / `UnityEngine.Random` 混用无种子；业务里直接调引擎 Random

**Audio**:
音频门面；`AudioId` 基类 + `IAudioPlayer`；默认 `NullAudioPlayer`，启动注入 `UnityAudioPlayer`。
_Avoid_: 框架层音效 enum；运行时 AddComponent AudioSource

**CooldownTracker**:
多冷却管理；`CooldownId` 基类 + `CooldownTimer`；Tick 传入 `Clock.DeltaTime`。
_Avoid_: 冷却里写死技能名 enum；Tracker 内部引用 Clock 具体类

**Extensions**:
`Extensions/System`（C#）、`Extensions/Unity`（Unity API）；每扩展类一文件；命名空间 `CommonGameFramework.Extensions`。
_Avoid_: 在扩展里塞业务逻辑

**Utils**:
与类型无关的算法/工具（`MathUtil`、`WeightedRandom`、`StringUtil`）。
_Avoid_: 把扩展方法塞进 Utils；Utils 依赖具体游戏类型
