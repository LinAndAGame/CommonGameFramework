---
name: cgf-achievement
description: 使用或修改 CommonGameFramework Achievement。派生 AchievementId、经 AchievementService 注册/解锁/改进度，或挂 Steam 等平台 adapter 时读取。
---

# Achievement

## 入口

游戏只调 `AchievementService`。顺序：`Register` → `Load` →（可选 `AttachPlatform`）→ `AddProgress` / `Unlock` → `Save`。本地存档与平台同步是两个独立 seam。

## 典型用法

```csharp
public sealed class FirstWinId : AchievementId
{
    public static FirstWinId Instance { get; } = new();
    public override string Key => "first_win";
}

var achievements = new AchievementService(new PlayerPrefsAchievementLocal());
achievements.Register(new AchievementDefinition(FirstWinId.Instance));
achievements.Register(new AchievementDefinition(Kill100Id.Instance, targetValue: 100));
achievements.Load();
achievements.OnUnlocked += id => { /* 弹窗 */ };
achievements.Unlock(FirstWinId.Instance);
achievements.AddProgress(Kill100Id.Instance, 1);
achievements.Save();
```

`TargetValue == 1` 为一次性，`> 1` 为进度型。离线仅本地：不调用 `AttachPlatform`，或 `AttachPlatform(null)`。

## 扩展点

游戏层派生 `AchievementId`（`Key` 做存档与平台映射）。`IAchievementLocal` 放 `Local/`；`IAchievementPlatform` 放 `Platform/`。SDK 配置写在 adapter 构造函数。可用 `CompositeAchievementPlatform` 叠多个平台。

## 禁止

- 框架层成就 enum
- 未 Register 就改进度 / 解锁
- 游戏逻辑直接写 `IAchievementLocal` 改解锁状态
- 发明 `OfflineAchievementPlatform`（用 null）
