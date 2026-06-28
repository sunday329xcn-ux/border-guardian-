# 进度记录 / Progress

> 本文件由 AI 工程师持续维护，记录三阶段任务的进展、决策与未解决问题。

## 当前阶段

**阶段 1：代码健康检查与错误修复** — 进行中

## 总览

| 阶段 | 主题 | 状态 |
|---|---|---|
| 1 | 代码健康检查与错误修复 | 🚧 扫描中 |
| 2 | P4.x 功能实现（挑战 / 排行榜） | ⏳ 未开始 |
| 3 | 零美术资产清单 P-A0 ~ P-A8 | ⏳ 未开始 |

约束：
- 忽略 `ThirdParty/`、`Plugins/` 目录。
- 不修改任何美术资源 / `.meta` 文件。
- 不实现：无尽模式、P4.4 交互（地图二仅显示解锁状态）。
- 每个独立步骤完成后暂停，等待用户确认。

---

## 阶段 1 — 代码健康检查

### 1.1 扫描范围
- `边境守卫者-tuanjie/Assets/Scripts/`，共 104 个 `.cs` 文件。
- 子目录：Core / Maps / Towers / Eneimes / UI。

### 1.2 问题清单（已完成扫描，2026-06-28）

扫描方式：5 个只读探查代理按 Core/Maps/Towers/Eneimes/UI 分目录逐文件审查，致命项已由主控亲自核验。

#### 致命（4）
- F1 `Towers/SoldierUnit.cs:144-151` + `Eneimes/PathFollower.cs:78-81`：士兵死亡不调用 `engagedEnemy.SetBlocked(false)`；PathFollower 在 IsBlocked 时直接 return，永不清除阻挡 → 敌人永久冻结 → 波次软锁。
- F2 `Towers/BarracksTower.cs`（TakeTowerDamage 摧毁路径）：兵营被导弹/Tower Breaker 摧毁时不清士兵，仅出售路径清。孤儿士兵继续战斗，叠加 F1。
- F3 `Maps/MapPlatformUnlockService.cs:8` `ResetSession()` 全项目无调用；`WaveManager.ResetLevel`/`VictoryResultUI`/`MainMenuUI` 重载场景后 static `UnlockedLatentCells` 残留 → 第二局起 wave6/11 的 4 个高台永不解锁。
- F4 `Eneimes/EnemyBehaviors.cs:207-209`(WolfRider) + `EnemyBase.Spawn`/`PathFollower.Begin(preservePosition=false)`：召唤的 Imp 被传送到路径起点而非骑乘者旁，召唤机制失效。

#### 普通（精选，需确认）
- N1 `Towers/TowerBase.cs` 升级后不重应用 `PlatformTerrainCatalog` 地形加成（射程/攻速/Fragile +15% 丢失）。
- N2 `Towers/ArcaneTower.cs` armorPenetration 仅对 Physical 生效，Arcane 是 Magic → 穿透数值实际无效。
- N3 `Towers/TowerBase.cs` 出售只退金币，不退钻石（Lv4/5 投入损失）。
- N4 `Eneimes/EnemyBase.cs:370-374` 无 damageSource 的物理伤害绕过护盾兵正面减伤。
- N5 `Eneimes/EnemyBehaviors.cs`(SplitSlime) 子体 HP=MaxHP/2 而非剩余 HP；`EnemyBase.cs:453-461` 分裂奖励翻倍（经济偏肥）。
- N6 `Core/GameDifficultyService.cs` 难度无 PlayerPrefs 持久化，重启回 Normal（设置页与实际不符）。
- N7 `Core/GameDebugInput.cs` 无 `#if UNITY_EDITOR` 守卫，Release 包仍开 G/D/L/R/K 作弊键。
- N8 `Core/GamePauseController.cs` Pause/Resume 未校验上下文：主菜单按 Esc 可解冻；Victory 时 waveManager 查找脆弱可能仍可暂停。
- N9 `UI/GameplayExitUI.cs:104-108` 裸写 `Time.timeScale=0` 绕过 GamePauseController，IsPaused 与时间状态脱节。
- N10 `Maps/MapEnvironmentController.cs:96-100` 猎手陷阱格与 Block 格重叠，2/3 陷阱位点击只开 Block 面板无法放陷阱。
- N11 `Maps/MapRouteController.cs:60,80,105` routeCache.Clear 只清字典不 Destroy → 路线 GameObject 累积泄漏。
- N12 `Eneimes/EnemyBehaviors.cs:345-374` Nullifier 压制 static 字典积累已销毁塔引用。
- N13 多处 UI/控制器「Start 时管理器为 null 即永不订阅」(GameUiController/GameHUD/TowerInfoPanelUI/RouteControlUI/LevelProgressHudUI)。
- N14 多个 static 选中态跨场景不重置（Enemy/BuildSlot/Tower SelectionController），重载后引用已销毁对象。
- N15 `Maps/EasterEggController.cs:47-51` 早期触发角落彩蛋且 WaveManager 未就绪 → Call Early +50% 奖励永久丢失。
- N16 `UI/TowerRangePreviewController.cs` `new Material` 无 OnDestroy 销毁 → Material 泄漏。
- N17 `UI/CombatFeedbackService.cs` 飘字 Canvas 固定 1920×1080 不跟随 UI Scale；worldCamera 仅取一次。

#### 优化建议（单列，本阶段默认不改）
- 每帧 Refresh/Find/字符串 GC：GameUiController、WaveTimelineUI、EnemyInfoPanelUI、PlatformTerrainInfoUI(+UiInputUtility 每帧 new List)、GoblinMissileUI、RouteControlUI、GameplayExitUI。
- 每帧/高频 GetComponent 与全表扫描：PathFollower、RootEntangleZone、TowerGroundZone、TowerSynergyService、SupportTowerService、ShadeBehavior、CombatTowerBase 目标选取 O(塔×敌)、GoblinMissile 爆炸帧全量扫描。
- CodexMenuUI 列表全量重建；ConfigureCanvas 重复调用；AncientTree/MapEnvironment 每帧改 color；各 Find 未缓存。

#### 需设计决策（无法 100% 确定修法，待确认）
- D1 FireBomber 漏怪到达终点时是否仍触发 60 真伤 AOE？（当前会触发）
- D2 分裂史莱姆奖励与子体血量的目标数值？（按 GDD「4 金」还是当前翻倍）
- D3 出售退款是否应含钻石？退多少比例？
- D4 Arcane 穿透修复方向：改伤害类型为含穿透的物理，还是改为「降低魔抗」debuff？
- D5 士兵攻击间隔是否应随兵营等级变化？（当前恒为 respawnHint 传入值）

### 1.3 修复计划
- 用户确认：批次 A（4 致命）先做并报告，再做批次 B（普通）。D1-D5 用户将回答。
- 项目无自动化测试框架（无 Test asmdef），验证以 Unity Editor 播放为准。

### 1.4 修复执行记录

#### 批次 A — 致命（已完成，2026-06-28）
- **F1** `Towers/SoldierUnit.cs`：新增 `ReleaseEngagedEnemy()`，在 `OnDisable()` 与 `Die()` 中调用，确保士兵消失时对交战敌人 `SetBlocked(false)`，消除「敌人永久冻结 → 波次软锁」。
- **F2** `Towers/BarracksTower.cs`：新增 `protected override void OnDestroy()` → `ClearSoldiers()` + `base.OnDestroy()`，兵营被摧毁（非出售）时清理士兵；与 F1 联合杜绝孤儿士兵。
- **F3** `Eneimes/WaveManager.cs`：`Start()` 顶部调用 `MapPlatformUnlockService.ResetSession()`，每次场景加载重置延迟高台解锁状态，修复重开后 wave6/11 高台不再解锁。
- **F4** `Eneimes/EnemyBase.cs` + `EnemyBehaviors.cs`：`Spawn` 增加 `preservePosition`/`pathProgress` 可选参数；WolfRider 召唤 Imp 传 `preservePosition:true` + 当前 `PathProgress`，召唤物在骑乘者旁生成并沿正确路点前进（不再瞬移回起点）。
- 编译检查：5 个文件 ReadLints 无错误。
- 验证方式：Unity 播放（手动）——待用户确认。

#### 批次 B — 普通 + D1-D5 决策（已完成，2026-06-28）
决策落地：D1 漏怪仍触发 AOE（保持现状）；D2 分裂史莱姆总奖励 4 金（子体奖励清零）、子体血量 = 母体上限一半（保留）；D3 出售退款含钻石 50%；D4 Arcane 改为含穿透物理伤害；D5 士兵攻速随等级递增（1.0→0.9→0.8→0.7；Lv5 BranchA 0.65、BranchB 0.5）。

- **N1** `Towers/TowerBase.cs`：新增 `ReapplyTerrainBonuses()`，三条升级路径（金币升级 / Lv4 / Lv5 分支）在 `ApplyLevelStats()` 后重应用 `PlatformTerrainCatalog`，修复升级后地形加成丢失。
- **N2/D4** `Towers/CombatTowerBase.cs`：`GetDamageType()` 仅 Frost 为 Magic，Arcane 改为 Physical → `armorPenetration` 实际生效。
- **N3/D3** `Towers/TowerBase.cs`：`TrySell` 退款增加 `AddDiamonds(totalDiamondSpent/2)`。
- **N4** `Towers/BarracksTower.cs`：兵营自爆伤害补传 `damageSource: deathPosition`，护盾兵正面减伤判定恢复。
- **N5/D2** `Eneimes/EnemyBase.cs` 新增 `SuppressRewards()`；`EnemyBehaviors.cs` 分裂子体 `MarkAsMini()` 调用之，子体不再给金/钻 → 总奖励回到 4 金。
- **N6** `Core/GameDifficultyService.cs`：难度写入 `PlayerPrefs`（键 `bg.difficulty`）并在加载时读取，重启保持。
- **N7** `Core/GameDebugInput.cs`：`Update` 体包入 `#if UNITY_EDITOR || DEVELOPMENT_BUILD`，Release 关闭作弊键。
- **N8** `Core/GamePauseController.cs`：Esc 仅在 `MainMenuUI.IsSessionStarted` 且非 modal 冻结时切换暂停；`CanPause` 在 waveManager 为 null 时重新查找。
- **N9** `Core/GamePauseController.cs` 新增 `BeginModalFreeze/EndModalFreeze`（`IsPaused` 计入 modalFreeze，不触发 OnPauseChanged）；`UI/GameplayExitUI.cs` 退出确认改走 modal 冻结，暂停态与时间一致。
- **N10** `Maps/MapEnvironmentController.cs`：`TryHandleClick` 改为陷阱放置优先于路线控制，解决陷阱格与 Block 格重叠。
- **N11** `Maps/MapRouteController.cs`：移除三处 `routeCache.Clear()`（路由 key 已含 fork/block/branch/spawn，按 key 永久缓存），消除路线对象泄漏且不破坏在途敌人引用。
- **N12** `Eneimes/EnemyBehaviors.cs`：`NullifierSuppressionService.Reset()`，`WaveManager.Start` 调用，跨场景清空压制字典。
- **N14** 三个 SelectionController 新增 `ResetState()`，`WaveManager.Start` 统一重置选中态。
- **N15** `Maps/EasterEggController.cs`：角落彩蛋应用奖励时若 waveManager 为 null 重新查找，避免奖励永久丢失。
- **N16** `UI/TowerRangePreviewController.cs`：`OnDestroy` 销毁运行时创建的 `lineMaterial`，修复材质泄漏。
- **N17** `UI/CombatFeedbackService.cs`：飘字 Canvas 改用 `UiDisplaySettings.ConfigureCanvas`，随 UI Scale 一致。
- **N13** `UI/GameHUD.cs`/`TowerInfoPanelUI.cs`/`RouteControlUI.cs`：资源订阅改为「未绑定则在 Update 重试」的惰性绑定（防御性，当前架构下管理器均先于 UI 创建）。
- 编译检查：全部 ReadLints 无错误。

---

## 阶段 2 — P4.x 功能实现
状态：🚧 进行中（已读取 GDD P4.1–P4.4 规格）。

### P4.1 英雄单位 + 主动技能（已完成，2026-06-28）
新增 `Assets/Scripts/Hero/`：
- `HeroSkillId`：Meteor / Freeze / Reinforce。
- `HeroAuraService`：静态被动光环，塔在英雄 1.5 格内 +伤害（基础 +5%，每级 +1%）。
- `HeroUnit`：可右键移动；自动近战交战最近可阻挡敌人（攻速随等级递增）；有限 HP，受交战接触伤害（普/精英/BOSS 6/12/22 DPS）；死亡后 25s 在出生点复活；击杀获 XP 升级（满血、强化近战/HP/光环）。
- `HeroController`：运行时生成英雄（地图中下 `(W/2,3)`）；右键移动；技能冷却（Meteor 12s / Freeze 20s / Reinforce 18s）；订阅 `EnemyBase.OnEnemyKilledByPlayer` 给 XP（普 1 / 精英 5 / BOSS 20）。
- `HeroSkillBarUI`：底部居中 HUD（不与左侧导弹 / 右侧建造栏重叠），HP/等级/复活倒计时 + 三技能按钮（实时 CD）。
集成改动：
- `EnemyBase`：新增 `OnEnemyKilledByPlayer` 事件，`Die(!leaked)` 触发。
- `SoldierUnit`：新增 `SetTemporaryLifetime`（Reinforce 临时士兵 12s 自毁，支持 `owner==null`）。
- `CombatTowerBase.ApplyOutgoingDamageSynergies`：末尾乘 `HeroAuraService.GetDamageMultiplier`。
- `TowerBuildController`：左键链最前加 `TryHandleHeroSkillCast`（Meteor 瞄准点施放，复用导弹式 armed 消费点击）。
- `GameUiController`：装配 `HeroController` + `HeroSkillBarUI`。
- 技能：Meteor 瞄准 AOE 真伤 `80+20*Lv`（半径 1.8）；Freeze 全场减速 60%×3s；Reinforce 英雄旁召 2 临时士兵。
验收：ReadLints 全绿；🚧 待 Unity 播放手验（英雄移动/技能/复活/升级/光环加伤）。

### P4.2 Roguelike 词条 + 单图变体（已完成，2026-06-28）
新增 `Assets/Scripts/Meta/`：
- `RoguelikeBuffId`：TowerDamage / KillGold / SynergyRange / CallEarly / HeroAura / FieldMedic。
- `RoguelikeModifierService`：静态 buff 叠层（开局 `Reset`）；查询 `TowerDamageMultiplier(+10%/层)`、`KillGoldBonus(+1/层)`、`SynergyRangeBonus(+0.5/层)`、`CallEarlyMultiplier(+25%/层)`、`HeroAuraBonus(+5%/层)`；FieldMedic 即时回满生命；`RollChoices(3)` 无重复抽取。
- `MapModifierService`：None/Night/Fog/Rain，PlayerPrefs 持久化；`SlowMultiplier`（Rain 1.1）。
新增 UI/控制器（`Assets/Scripts/UI/`）：
- `WaveBuffSelectionUI`：订阅 `WaveManager.OnWaveCleared`，**屏幕居中 modal**（全屏 dim 拦截输入 + modal-freeze 暂停），三按钮选 1，落定后 `EndModalFreeze`；不遮挡战斗 HUD 集群。
- `MapModifierController`：全屏 tint 叠加（`raycastTarget=false`、置于 HUD 之下，纯视觉不挡操作）；按当前 modifier 改色，Active 变化即刷新。
- `MapModifierHudUI`：**顶部居中**切换按钮（顶部中区空闲），循环 None→Night→Fog→Rain 即时生效。
集成改动：
- `WaveManager`：新增 `OnWaveCleared(int)`（非终局波清理时触发）；`Start` 追加 `RoguelikeModifierService.Reset()` + `MapModifierService.Load()`；`CallWaveEarly` 金额乘 `CallEarlyMultiplier`。
- `CombatTowerBase.ApplyOutgoingDamageSynergies`：乘 `TowerDamageMultiplier`（与英雄光环相乘）。
- `EnemyBase.Die`：非泄漏且 `goldReward>0` 时加 `KillGoldBonus`（迷你史莱姆 0 金不受影响，守 D2）。
- `TowerBase.SynergyRange`：基础>0 时 +`SynergyRangeBonus`（无协同塔不误启）。
- `HeroUnit.AuraDamageBonus`：+`HeroAuraBonus`。
- `EnemySlowEffect.ApplySlow`：入参乘 `MapModifierService.SlowMultiplier`（Rain 全局放大减速的单一接入点）。
- `GameUiController`：装配 `MapModifierController` + `MapModifierHudUI` + `WaveBuffSelectionUI`。
验收：ReadLints 全绿；🚧 待 Unity 播放手验（每波弹三选一并暂停、buff 生效、天气切换 tint 与 Rain 减速）。

### P4.3 无尽模式 + 挑战 + 排行榜（已完成，2026-06-28）
新增 `Assets/Scripts/Meta/`：
- `GameModeService`：`GameMode`(Normal/Endless) + `ChallengeId`(None/ThreeTowers/NoBarracks/EarlyBoss)，PlayerPrefs 持久化；约束查询 `IsTowerDisabled` / `MaxTowers` / `EarlyBoss`。
- `EndlessScalingService`：全局敌人 HP 倍率（每轮 +25%），WaveManager 每波设值、EnemyBase 读取。
- `EndlessWaveGenerator`：按 round 确定性生成波（组数/数量随 round 递增，每 5 轮加 AncientDragon Boss）。
- `LeaderboardService`：本地最高分（按模式 key），`ComputeScore = 波次×100 + 剩余生命×5`。
新增 UI：`ScoreHudUI`（**顶部居中、置于天气按钮下方**，实时 Score/Best，不遮挡侧栏）。
集成改动：
- `WaveManager`：`Start` 追加 `GameModeService.Load()` + `EndlessScalingService.Reset()` + 订阅 `OnGameOver`；新增 `GetWaveDefinition`（>15 波走生成器，EarlyBoss 在 W7 注入 Boss）；`SpawnWave` 按波设 HP 倍率；`StartNextWave`/`CompleteCurrentWave` 在 Endless 不触发胜利（无限续波）；`GetWaveCounterText` 显示 `Endless N`；新增 `ClearedWaves`/`CurrentRunScore`；`SubmitScore`（OnGameOver / 胜利 / OnDestroy 退出，单次守卫）。
- `EnemyBase.ApplyStats`：`maxHealth` 乘 `EndlessScalingService.HealthMultiplier`。
- `TowerBuildService.TryBuild`：拦截 `IsTowerDisabled`（禁 Barracks）+ `MaxTowers` 上限（仅 3 塔）。
- `MainMenuUI`：面板放大，新增 **Mode / Challenge 循环行 + 最高分标签**（预开局选择，主菜单空间充足无遮挡）；返回主页刷新。
- `GameUiController`：装配 `ScoreHudUI`。
验收：ReadLints 全绿；🚧 待 Unity 播放手验（Endless 续波与 HP 递增、3 挑战约束生效、排行榜记分、Score HUD 刷新）。
备注：Endless 中途退出经 `WaveManager.OnDestroy` 提交当前分；满 15 波 Normal 胜利照常评星。

### P4.4 元进度：天赋树（钥匙新用途）（已完成，2026-06-28）
新增 `Assets/Scripts/Meta/TalentService.cs`：
- 4 永久天赋 `TalentId`：StartingGold(War Chest, +50 金, 1 钥) / CallEarly(Forced March, +20% Call Early 金, 1 钥) / EnvironmentCooldown(Forest Attunement, 古树·陷阱 CD ×0.9, 1 钥) / HeroStartLevel(Veteran Hero, 英雄起始 Lv.2, 2 钥)。
- PlayerPrefs 按天赋持久化；`AvailableKeys = LevelProgressService.TotalKeys - SpentKeys`；`CanPurchase/Purchase/IsPurchased/GetCost/GetTitle/GetDescription` + 效果查询（`StartingGoldBonus`/`CallEarlyTalentMultiplier`/`EnvironmentCooldownMultiplier`/`HeroStartLevelBonus`）+ `OnChanged`。
新增 UI `Assets/Scripts/UI/TalentMenuUI.cs`：
- 主菜单居中页（Settings/Codex 同模式 Show(onBack)/Hide），列出 4 天赋（标题+描述+花费），按钮三态：Buy（可购买）/ Owned（已拥有，绿禁用）/ Need keys（钥匙不足，灰禁用）；顶部显示可用钥匙；Back 返回。**不遮挡 HUD/地图**（前端覆盖层）。
集成改动：
- `GameManager.ApplySessionStart`：起始金 + `StartingGoldBonus`。
- `WaveManager.CallWaveEarly`：金额再乘 `CallEarlyTalentMultiplier`。
- `AncientTree.TryActivate` / `MapEnvironmentController.OnTrapTriggered`：冷却乘 `EnvironmentCooldownMultiplier`。
- `HeroUnit.Spawn`：`level = 1 + HeroStartLevelBonus` 后再 `ApplyLevelStats`。
- `MainMenuUI`：面板 470→520，新增 **Talents** 按钮（Play 下方），装配 `TalentMenuUI`，ShowFrontEnd/ShowHome 同步 Hide。
- `VictoryResultUI.BuildUnlockText`：由「Lavafall Coming soon」改为 **Talents 解锁进度 + 可用钥匙提示**。
- `LevelProgressService.ResetAllProgress`：追加 `TalentService.ResetAll()`，进度重置同步清空天赋。
验收：ReadLints 全绿；🚧 待 Unity 播放手验（购买天赋后起始金/CallEarly/环境 CD/英雄起始等级生效、钥匙数与已购态持久化、胜利面板文案）。
备注：钥匙当前仅来自单图新星（最多 3），4 天赋共需 5 钥 → 设计为「取舍式」元进度；`LavafallRiftKeyCost` 常量保留备用。

至此 **P4.1~P4.4 全部完成**。

---

## 阶段 3 — P-A0 ~ P-A8
### P-A0 基础设施（已完成，2026-06-28）
- 新增 `Assets/Scripts/Visual/`：
  - `VisualSorting`：集中排序层常量（Background…OverlayCanvas）。
  - `VisualPalette`：环境 / 塔 / 支援塔 / 语义 / 单位共享色 + `ForTower(TowerType)`。
  - `ProceduralSpriteFactory`：白图包装 + 圆形 / 软阴影 / 值噪声纹理缓存。
  - `TowerVisualComposer` / `EnemyVisualComposer`：Compose 空壳（仅设排序，行为中性，待 P-A4/A5 填充）。
- 验收：4 类基础设施到位，ReadLints 无错误。🚧 单测场景未建（项目无 Test asmdef，按 Editor 播放验证）。

### P-A1~P-A8 零美术视觉升级（已完成，2026-06-28）
> 原则：**逻辑零改动**，只在 `Scripts/Visual/` 新增层并挂接现有创建入口；可随时换真美术只改 Visual 层。

**P-A1 单位通用** — `UnitVisualDecorator`
- 软影子件 + 受击 scale punch + 敌人 idle 挤压 bob；punch/bob 仅在动画激活时写 `localScale`，避免与塔升级 `RefreshPresentation` / PathFollower 位置写入冲突；全程尊重 `ReduceMotion`。
- 挂接：敌人 `EnemyBase.PlayHitFlash` → `Punch()`；塔 `TowerBase.TakeTowerDamage` → `Punch()`；Attach 由各 Composer / SoldierUnit 调用。
- 取舍：放弃整体 outline 与 root 旋转朝向（选中/受击/隐身均改 root 颜色/alpha，旋转或描边会与之冲突），改用「彩色 root + 几何 accent 子件」表达层级。

**P-A2 投射物** — `ProjectileVisual` / `ProjectileVisualFactory`（池 size 32）
- 四类：Arrow（朝向旋转、细长）/ Frost（圆 + `TrailRenderer`）/ Cannon（黑圆 + 抛物线 + 命中 `ProjectileImpactFx` 橙扩散）/ Arcane（菱形自转）。
- 挂接：`CombatTowerBase.ApplyDamageToEnemy` 起点处 `Fire`（仅视觉、尊重 ReduceMotion）；用 scaled time → 暂停/倍速一致。

**P-A3 地图程序纹理** — `ProceduralMapVisual`
- 草/路/高台/阻挡按类型各生成 1 张 Perlin 纹理复用到全部 tile（草/阻挡哈希 0/90/180/270 转角破重复）；出生点门洞三角 + 终点同心环+菱形（`MarkerPulse`）。
- `BuildSlotPulse`：空闲高台慢速 pulse 描边环，占用即隐藏（替代纯黄高亮的 idle 提示）。
- 挂接：`MapGridController.Awake` 末 `ProceduralMapVisual.Apply(this)`；`CreateBuildSlot` 加 `BuildSlotPulse`（新增 `TilesRoot`/`MarkersRoot` 访问器）。

**P-A4 塔 Silhouette** — `TowerVisualComposer`（含 `TowerLandingAnim`）
- 6 战斗/经济塔 + 3 支援塔几何 accent（箭塔屋顶三角、霜塔水晶菱、炮塔横管、奥术核心+环、兵营旗、矿井宝石…）；底部 1~5 level pip；Lv5 分支暖/冷 accent；建造 0→1.12→1 落地动画。
- decor 子件 `sortingOrder=Towers` 且 z=-0.05（同层靠前），随 `RefreshPresentation` 幂等重建。
- 挂接：`TowerBase.RefreshPresentation` 末 `Compose`；`Setup` 末 `PlayLanding`；士兵 `SoldierUnit.Spawn` 加影 + 头部 accent。

**P-A5 敌人 Silhouette** — `EnemyVisualComposer`
- 16 敌人（清单 10 + P2.1 扩展 6）几何 accent；精英金环 / BOSS 双环（环 z=+0.05 居后）；颜色取自 body（与 Codex 一致），Composer 只负责形状。
- 死亡粒子沿用 `CombatFeedbackService.ReportEnemyDeath`（已按 `DisplayColor`）。
- 挂接：`EnemyBase.Spawn` / `SpawnSplitChild` 于 `ApplyStats` 后 `Compose`。

**P-A6 环境机关** — `EnvironmentVisual`
- 古树：棕干 + 双绿冠，ready 时 `TreeCanopyPulse` 绿冠脉动，激活时 `EnvironmentLineFx` 根须线 + `EnvironmentRingFx` 绿扩散。
- 陷阱：放置 X 十字（两旋转细条）+ 触发白闪环；放置槽状态色沿用原逻辑。
- 挂接：`AncientTree.Initialize/TryActivate`、`HunterTrap.Initialize/TriggerOn`（仅加视觉调用）。

**P-A7 氛围后处理** — `AtmosphereController`
- 零依赖屏幕空间 vignette（独立 Overlay Canvas，`sortingOrder=-5`：在地图之上、HUD（≥0）之下，**不遮挡/不变暗 HUD**）；`GetVignetteSprite` 中心透明→边缘压暗。
- URP Volume/Bloom 当前工程未配置，留待接入后补（已在清单标注 [~]）。
- 挂接：`GameUiController.Start` 装配。

**P-A8 UI 程序皮肤** — `ProceduralUiSkin`（含 `UiFadeIn`）
- 运行时圆角 9-slice 面板（`GetRoundedRectSprite` border=14）经 `UiDisplaySettings.ApplyPanelBackground` 全局生效；建造栏按钮左上几何 icon（箭=三角/霜=圆/炮=横条/奥术=菱/兵营=三角/矿=菱）；Pause、Victory overlay `CanvasGroup` 0→1 fade（尊重 ReduceMotion）。
- 挂接：`UiDisplaySettings.ApplyPanelBackground`、`GameUiController.CreateTowerBuildButton`、`PauseMenuUI`/`VictoryResultUI` overlay 创建处。

**新增文件**：`Visual/{VisualPrimitives, UnitVisualDecorator, TowerVisualComposer(重写), EnemyVisualComposer(重写), ProjectileVisual, ProceduralMapVisual, EnvironmentVisual, AtmosphereController, ProceduralUiSkin}.cs` + `ProceduralSpriteFactory` 扩展（三角/菱形/环/圆角9-slice/vignette）。
**UI 不遮挡守则**：vignette 在 HUD 之下；塔/敌人 decor 为世界空间子件不入屏幕 UI；建造 icon 在按钮自身左上角；fade 仅作用于既有面板。
验收：全部 ReadLints 绿。🚧 待 Unity 播放手验（投射物/纹理/Silhouette 可读性、vignette 不压 HUD、fade 与倍速/暂停一致）。

---

## 变更日志（按对话）

### 2026-06-28
- 建立 `memory-bank/progress.md`，启动阶段 1 代码扫描。
