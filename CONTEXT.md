# CommonGameFramework

通用 Unity 游戏框架库：提供对象池、资源加载、逻辑 Command、Buff/Stat、时钟/随机/音频/冷却、视图流程、UI 拖放等**机制**；卡牌战斗、UI 层级等**语义**由游戏项目扩展。存档用游戏层 EasySave，文案用 Unity Localization；Event 总线暂缓。

## Language

**Logic Command**:
同步执行的逻辑动作，通过 `IGameCommand` 修改权威游戏状态。
_Avoid_: 把异步动效写成 Command

**ViewLayer**:
视图栈与 Canvas 挂载层的标识；框架提供抽象基类，游戏项目派生子类并注册 Canvas。
_Avoid_: UILayer enum

**ViewFlowController**:
视图编排入口：Open 入栈，Hide 出栈并恢复下层；日常只用 OpenAsync / HideAsync。
_Avoid_: 绕过 Flow 直接改 ViewStackManager；Hide 只藏不 Pop

**MvcScope**：
实例级 MVC 装配入口；登记外部模型并拥有控制器生命周期，初始化失败自动清理、退出逆序释放。
_Avoid_: 全局静态模型仓库；框架预设业务 Model；作用域销毁注入的 View/Model

**MvcModel / IMvcView / MvcController**：
模型同步结算后发布只读快照，视图通过 Refresh 消费快照，控制器订阅交互意图及模型变化并成对注销。
_Avoid_: View 修改权威状态；Controller 代绑按钮；Model 等待动画或依赖具体 View

**CommandContext**:
逻辑 Command 的运行上下文；仅含 Source/Target 与 lazy-create 的 services bag。
_Avoid_: 在框架层挂载 BuffContainer、FloatStat 等具体类型

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
对象池静态门面；实现经 `IPoolRegistry`（`Registry/`）→ `Pools/` 具体池。
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

**DragController**:
UI 拖放会话。`DragSource` / `DragPlace` 按 `groupId` 登记；负责副本跟随、命中、成功吸附与失败回源。未指定控制器时，运行时在 Canvas 下创建 `[DragPlace]`。
_Avoid_: 游戏 Mid / UIWidget 中间层；DOTween；用 enum 预设业务槽位

**DragSource / DragPlace**:
拖拽源与放置点。回调是 C# event。只有相同 `groupId`（含空字符串默认组）才能配对。
_Avoid_: 在拖拽回调里改战斗权威状态；为了能拖而给业务物体运行时拼 UI

**Extensions**:
`Extensions/System`（C#）、`Extensions/Unity`（Unity API）；每扩展类一文件；命名空间 `CommonGameFramework.Extensions`。
_Avoid_: 在扩展里塞业务逻辑

**Utils**:
与类型无关的算法/工具（`MathUtil`、`WeightedRandom`、`StringUtil`）。
_Avoid_: 把扩展方法塞进 Utils；Utils 依赖具体游戏类型
