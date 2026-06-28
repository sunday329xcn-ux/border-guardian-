# 《Grimm Forest 零美术资产技术清单》

**版本：** 1.1  
**日期：** 2026-06-28  
**适用工程：** `边境守卫者-tuanjie/`（Tuanjie / Unity 2D 正交）  
**关联文档：** [《边境守卫者》游戏开发文档 (GDD).md](./《边境守卫者》游戏开发文档%20(GDD).md)  
**定位：** 独立开发者、**零外包原画**前提下，仅使用 **代码 + Unity 内置能力** 提升格林姆森林观感的技术规格与实施清单。  
**状态：** **P-A0 ~ P-A8 全部已实现**（垂直切片已铺开到 6 塔 / 16 敌人 / 全地图 / UI 皮肤 / 氛围层）

---

## 一、目标与风格定位

### 1.1 要解决的问题

当前可玩原型中，地图、塔、敌人、士兵、环境机关均大量使用 `MapGridControllerShared.GetWhiteSprite()` + `SpriteRenderer.color` 纯色块（见 `MapGridController`、`TowerVisualFactory`、`EnemyBase.Spawn`）。玩法与 UI 流程已较完整，但视觉仍像「逻辑调试画面」。

### 1.2 零美术资产的可行目标

| 不追求 | 追求 |
| --- | --- |
| Kingdom Rush 级手绘插画 | **有意为之的几何 Fantasy**（Geometric Fantasy） |
| 每单位独立原画 | **程序组合 Silhouette + 统一调色 + 粒子/投射物/后处理** |
| 大规模换架构 | **逻辑不动，只换 Visual 层与 Presentation 层** |

### 1.3 官方风格一句话

> **Border Guard — Geometric Fantasy：** 深林绿底、几何剪影、描边与光晕区分层级，动效承担「精美感」。

此风格应写入 GDD 1.1/美术说明（实施时再同步），避免玩家误以为是「未完成占位」。

---

## 二、全局技术规格

### 2.1 世界与网格（与现逻辑对齐）

| 项 | 当前值 | 说明 |
| --- | --- | --- |
| 地图尺寸 | 16 × 12 格 | `MapGridSettings.Width/Height` |
| 格距 | 1 world unit | `MapGridSettings.CellSize` |
| 世界原点 | 格心对齐 | `GridToWorld(x,y)` |
| 高台数量 | 21 | `GrimmForestMapLayout.BuildSlotCount` |
| 相机 | Orthographic | `MapGridController.SetupCamera()`，`orthographicSize` 约 6 |

**程序生成纹理建议分辨率：** 单格 64×64 px（PPU=64 → 1 unit 宽），或 32×32 做 retro 像素风（仍无外部图，仅 `Texture2D` 生成）。

### 2.2 Sorting Layer 规范（建议新增常量类 `VisualSorting`）

| Order | 层 | 内容 |
| --- | --- | --- |
| -10 ~ 0 | Background | 视差渐变、远林噪声 |
| 0 | Ground | 草地/路径程序纹理格 |
| 1 | GroundDecor | 路径边暗、落叶点缀 |
| 2 | Zones | `TowerGroundZone`、陷阱范围 |
| 3 | Shadows | 单位脚下椭圆影 |
| 4 | Towers | 塔本体 |
| 5 | Enemies | 敌人 |
| 6 | Soldiers | `SoldierUnit` |
| 7 | Projectiles | 箭/弹/法球 |
| 8 | VFX | 粒子、爆发圈 |
| 9 | Markers | 出生点/终点（备波后可隐藏或弱化） |
| 10+ | UI World | 飘字等（已有 Overlay Canvas） |

现有代码中 `sortingOrder` 分散（塔 4、敌人 5、士兵 6 等），实施时应**集中到一个静态类**，避免新特效乱序。

### 2.3 全局调色板（与 `UiDisplaySettings` 对齐并扩展）

**环境色**

| 名称 | RGBA 参考 | 用途 |
| --- | --- | --- |
| ForestDeep | (0.12, 0.16, 0.12) | 相机背景、地图外缘 |
| GrassBase | (0.22, 0.34, 0.22) | 草地程序纹理基色 |
| GrassLight | (0.28, 0.48, 0.28) | 草地高光噪声 |
| PathBase | (0.72, 0.58, 0.36) | 路径 |
| PathEdge | (0.45, 0.35, 0.22) | 路径描边/阴影 |
| PlatformBase | (0.42, 0.45, 0.48) | 高台（可被木纹理替代） |
| Blocked | (0.18, 0.24, 0.18) | 不可建造区 |

**战斗塔主色（沿用现有，实施时写入 `VisualPalette`）**

| 塔 | 当前色 | 英文名 |
| --- | --- | --- |
| Arrow | (0.82, 0.72, 0.28) | Arrow Tower |
| Frost | (0.45, 0.78, 0.95) | Frost Tower |
| Cannon | (0.55, 0.55, 0.60) | Cannon Tower |
| Arcane | (0.62, 0.35, 0.85) | Arcane Tower |
| Barracks | (0.35, 0.55, 0.95) | Barracks |
| Diamond Mine | (0.45, 0.85, 0.95) | Diamond Mine |

**敌人主色：** 见 `EnemyCatalog.Get()` 各 `Color` 字段 — 实施时只改 Visual 层，不改数值。

**语义色**

| 用途 | 色 |
| --- | --- |
| 选中/建造合法 | 绿 `(0.45, 0.85, 0.55)` — 已有射程圈 |
| 选中塔 | 黄 `(1, 0.92, 0.35)` — 已有 |
| 协同 | 蓝 `(0.55, 0.75, 1)` — 已有 |
| 精英/BOSS 强调 | 金 + Bloom — 已有 `CombatPresentationUI` |
| 危险/拆塔 | 红 `(0.45, 0.08, 0.08)` — 已有 Tower warning |

### 2.4 核心代码挂载点（实施时优先抽象）

| 职责 | 现有入口 | 建议新增（规划） |
| --- | --- | --- |
| 地图格渲染 | `MapGridController` 刷格 | `ProceduralMapVisual` 或替换 `GetCellColor` |
| 塔外观 | `TowerVisualFactory.CreateTowerObject` | `TowerVisualComposer.Compose(TowerType, level, branch)` |
| 敌人外观 | `EnemyBase.Spawn` 内 `SpriteRenderer` | `EnemyVisualComposer.Compose(EnemyType, stats)` |
| 士兵 | `SoldierUnit.Spawn` | 同 Compositor 模式 |
| 地面效果 | `TowerGroundZone.SpawnEffect` | 保留逻辑，换 pulse + 粒子 |
| 投射物 | 无（即时伤害） | `ProjectileVisualFactory` |
| 受击/死亡 | `CombatFeedbackService` | 扩展 ParticlePool |
| 建造预览圈 | `TowerRangePreviewController` | 可选换贴图环 shader |
| UI 皮肤 | `UiDisplaySettings` | `ProceduralUiSkin` 生成 9-slice |

原则：**玩法脚本不直接 `new GameObject` 拼形状**，统一走 Visual 工厂，便于以后换真 Sprite 也只改一处。

---

## 三、分阶段路线图

| 阶段 | 名称 | 预估工作量（solo） | 验收标准 |
| --- | --- | --- | --- |
| **P-A0** | 基础设施 | 0.5–1 天 | Sorting、Palette、Visual 工厂空壳 |
| **P-A1** | 单位通用层 | 1–2 天 | 全体阴影、描边、受击 punch |
| **P-A2** | 战斗投射物 | 1–2 天 | 四种战斗塔有可见弹道 |
| **P-A3** | 地图程序纹理 | 2–3 天 | 草地/路径/高台不再 flat color |
| **P-A4** | 塔 Silhouette | 2–4 天 | 6 塔可一眼区分 |
| **P-A5** | 敌人 Silhouette | 2–4 天 | 10 敌人可一眼区分 |
| **P-A6** | 环境机关 | 1 天 | 古树/陷阱有触发反馈 |
| **P-A7** | 氛围后处理 | 0.5–1 天 | 全图 vignette + 轻微 Bloom |
| **P-A8** | UI 程序皮肤 | 1–2 天 | 面板圆角框 + 建造栏几何 icon |

可并行：**P-A1 + P-A2**；**P-A4 / P-A5** 按波次出场频率 prioritization。

---

## 四、地图（Grimm Forest）

### 4.1 现状

- `GrimmForestMapLayout.CreateCells()`：`Grass / Path / BuildPlatform / Blocked`
- `MapGridController` 每格一个 `SpriteRenderer` + `GetCellColor`
- 出生点/终点：`CreateMarker` 纯色块
- 环境：`MapEnvironmentController` 古树 ×2、猎人陷阱放置格

### 4.2 零资产方案

#### 4.2.1 草地（Grass）

- **技术：** 运行时 `Texture2D` 64×64，Perlin 噪声 + 基色 `GrassBase/GrassLight` 混合，缓存为 `Sprite` 复用到每格（同材质可 `MaterialPropertyBlock` 微调色相，避免 192 张图）。
- **可选：** 每格 `Random.seed = hash(x,y)` 旋转 0/90/180/270，破除重复感。
- **挂钩：** 替换 `GetCellColor` → `GetCellSprite(cellType, x, y)`。

#### 4.2.2 路径（Path）

- **技术：** 程序纹理 + **邻居掩码**（4-way：上/下/左/右是否 Path）决定边缘暗化；或简化为 Path 格中心亮、四边 10% 宽度压暗。
- **挂钩：** 仍用 `MapCellType.Path` 逻辑，仅视觉升级。

#### 4.2.3 高台（BuildPlatform）

- **技术：**  
  - 底座：略大的深色 Sprite（shadow）  
  - 台面：程序「木纹理」噪声（棕灰）  
  - `BuildSlot` 空闲：慢速 pulse 描边环（`LineRenderer` 或 scale 动画），替代纯黄 `SetHighlight`
- **挂钩：** `BuildSlot.Initialize` / `SetHighlight`

#### 4.2.4 阻挡（Blocked）

- 更深色 + 更高频率噪声 → 「密林/岩石」感；不可交互，无需动画。

#### 4.2.5 地标：双出生点 + 终点

| 标记 | 建议几何组合 | 行为 |
| --- | --- | --- |
| Upper Spawn | 蓝色竖条 + 顶部三角（门洞感） | 备波 ? 按钮保留 |
| Lower Spawn | 同上，色相略偏 | 同上 |
| Goal | 红色同心圆 + 中心菱形（核心） | 轻微 pulse |

**挂钩：** `MapGridController.CreateMarker` → `SpawnMarkerVisual.Create(type)`

#### 4.2.6 视差背景（可选 P-A7 前做）

- 2 层 fullscreen Sprite：  
  - 远层：深绿渐变 + 低频噪声（树线剪影可用 sin 叠加）  
  - 近层：更暗，alpha 0.3  
- 相机 `position` 或鼠标偏移 ±0.05 unit parallax。

### 4.3 不建议

- 第一版就上 Tilemap 手绘 Rule Tile（无图则收益不如程序纹理直接）。
- 改动 `GrimmForestMapLayout` 逻辑格坐标。

---

## 五、防御塔（6 种）

### 5.1 通用规则（所有塔）

| 项 | 规格 |
| --- | --- |
| 根节点 | 逻辑 `TowerBase` 不变 |
| Visual 子节点 | `VisualRoot` 下组合，便于 `RefreshPresentation` 缩放整组 |
| 阴影 | 椭圆 Sprite，scale (0.9, 0.35)，色 (0,0,0,0.35)，sorting Shadows |
| 描边 | 子 Sprite scale 1.08，同色更深，或 shader outline |
| 等级 | Lv1–5：`VisualRoot.localScale = 0.85 + level * 0.06`（在现有缩放基础上加装饰） |
| 选中 | 已有黄圈；加 **顶部小三角 marker** |
| 建造落地 | 0.25s：`scale 0→1.1→1` + 环形 LineRenderer alpha 1→0 |
| 受击 | 塔有 HP 时：`TowerBase.OnTowerDamaged` 已有 UI；Visual 加 **红闪 + 微震** |

### 5.2 分塔 Silhouette 规格（白块组合）

以下均为 **子 Sprite 相对 VisualRoot 的 local 布局**（用 `GetWhiteSprite()`，不同 scale/rotation）。颜色用各塔 `normalColor`。

#### Arrow Tower（物理 / 速射）

| 部件 | 形状 | scale 参考 | 备注 |
| --- | --- | --- | --- |
| Base | 正方形 | (0.55, 0.35) | 木台 |
| Body | 竖矩形 | (0.25, 0.5) | |
| Roof | 三角（两小块旋转） | — | Lv3+ 加第二箭槽小矩形 |
| **Lv5 Ranger** | 加高 Body | +15% 高 | 分支仅加宽屋顶 |
| **Lv5 Siege** | 加粗 Base | +20% 宽 | |

**投射物（P-A2）：** 小 elongated 矩形，色 `(0.95,0.9,0.7)`，速度按 `attackInterval` 反比。

#### Frost Tower（减速 / 魔法）

| 部件 | 形状 | 备注 |
| --- | --- | --- |
| Base | 六边形近似（6 小块围圈） | 或用圆 scale 0.5 |
| Crystal | 竖菱形（scale 0.2, 0.55） | 色偏白蓝 |
| Pulse | crystal sin 缩放 1±0.05 | Update 驱动 |

**投射物：** 小圆 + 拖尾 `TrailRenderer`，色 `(0.7,0.9,1)`。

#### Cannon Tower（范围物理）

| 部件 | 形状 | 备注 |
| --- | --- | --- |
| Base | 宽矩形 (0.7, 0.3) | |
| Barrel | 横矩形 (0.55, 0.18) | 射击时 rotation kick -5°→0° |
| **Lv5** | 双管：两 barrel 平行 | |

**投射物：** 黑色圆 `(0.2,0.2,0.2)`；命中点 `TowerGroundZone` 已有，加 **爆炸 ParticleSystem**（橙，0.3s）。

#### Arcane Tower（魔伤 / 拆甲）

| 部件 | 形状 | 备注 |
| --- | --- | --- |
| Base | 圆 | |
| Ring | `LineRenderer` 小圆 | 缓慢旋转 |
| Core | 小菱形 | 紫色 pulse alpha |

**投射物：** 菱形旋转飞行；暴击时 scale 1.5。

#### Barracks（兵营 / 阻挡）

| 部件 | 形状 | 备注 |
| --- | --- | --- |
| Base | 大方 (0.65, 0.45) | |
| Flag | 细杆 + 小三角旗 | sin 摆动 |
| Rally 圈 | 已有 gizmo；运行时虚线圆与 `RallyRange` 一致 | 放置 Rally 时 flash |

**士兵：** 见 5.4。

#### Diamond Mine（经济）

| 部件 | 形状 | 备注 |
| --- | --- | --- |
| Base | 梯形（两矩形错切） | |
| Gem | 小菱形，cyan | 产出钻石时 **向上飘小菱形**（复用 FloatingText 逻辑或短 tween） |
| 无射程圈 | 已有 `ShowsCombatRangeRing` false | 保持 |

### 5.3 分支视觉（Lv.5）

| 分支 | 通用做法 |
| --- | --- |
| Branch A | 暖色 accent 子件 + 一种几何配件 |
| Branch B | 冷色 accent + 另一种配件 |

在 `TowerBase.selectedBranch` 变更时调用 `TowerVisualComposer.RefreshBranch()` — **不改数值**。

### 5.4 士兵（Barracks）

| 项 | 规格 |
| --- | --- |
| 基础 | 小竖矩形 body + 圆头，scale 0.42 保持 |
| 护甲 | armor>0 时加灰色横条 |
| 骑士分支 | 加三角盾（小方块 + 三角） |
| 死亡 | 0.2s alpha→0 + 小尘粒子 |

**挂钩：** `SoldierUnit.Spawn` → `SoldierVisualComposer`

---

## 六、敌人（10 种）

### 6.1 通用规则

| 项 | 规格 |
| --- | --- |
| 组合根 | `EnemyVisualRoot` 子节点 |
| 阴影 | 飞行单位 shadow alpha 0.15 且 Y 偏移 -0.1 |
| 移动 | `PathFollower` 前进方向：VisualRoot Z 轴 rotation 朝向 velocity（2D 转 Z） |
| Idle 动效 | sin 波 bobbing amplitude 0.03 |
| 受击 | 已有 `PlayHitFlash`；加 scale punch |
| 死亡 | `CombatFeedbackService.ReportEnemyDeath` 处加 **色匹配粒子** 8–12 粒 |
| 精英 | 外圈 `LineRenderer` 金环 slow rotate |
| BOSS | scale ×1.1 + 双环 + 已有 `CombatPresentationUI` 血条 |

### 6.2 分敌人 Silhouette

| EnemyType | 英文名 | Scale 参考 | 几何 Silhouette 要点 | 特殊行为视觉 |
| --- | --- | --- | --- | --- |
| Imp | Imp | 0.55 | 小圆头 + 三角耳 | — |
| Orc | Orc | 0.55 | 宽体 + 两短腿 | 受击不易退 |
| GoblinRipper | Ripper | 0.50 | 瘦高 + 前倾 | 偷金 leak 时闪金币色 |
| Wraith | Wraith | 0.50 | 菱形 + 半透明 alpha 0.75 | **飞行**：Y bob 加倍 |
| RockGolem | Rock Golem | 0.85 | 大方块 + 两小方块当手 | 精英金环 |
| FireBomber | Fire Bomber | 0.55 | 圆身 + 顶部橙三角 | 死亡 **预警圈** 0.5s 再爆（已有范围逻辑则挂 VFX） |
| ShadowPriest | Shadow Priest | 0.55 | 高瘦 + 帽子三角 | 治疗时绿十字粒子 |
| WolfRider | Wolf Rider | 0.55 | 椭圆身 + 小头在前 | 移动略快 → 拖尾线 |
| TowerBreaker | Tower Breaker | 0.80 | 高矩形 + 锤头横条 | 拆塔时目标塔闪红（已有 presentation） |
| AncientDragon | Ancient Dragon | 1.10 | 长椭圆身 + 三角翼 ×2 | BOSS 环 + Phase2 时 **色相 shift 红**（配合现有 Phase 演出） |

**颜色：** 一律来自 `EnemyCatalog`，Composer 只负责形状，便于与 Codex 一致。

### 6.3 优先级（按波次首次出现）

1. Imp / Orc / GoblinRipper — 前 3 波  
2. Wraith / FireBomber / WolfRider — 中段  
3. ShadowPriest / RockGolem / TowerBreaker — 后段  
4. AncientDragon — 第 10 波 BOSS  

---

## 七、战斗特效与投射物

### 7.1 投射物矩阵

| 来源 | 类型 | 实现 | 池化 |
| --- | --- | --- | --- |
| Arrow | 线性 | `Transform.Translate` 或 lerp | `ProjectilePool` |
| Frost | 线性 + trail | `TrailRenderer` | 同上 |
| Cannon | 抛物线（假） | lerp + 略抬高 Y sin | 同上 |
| Arcane | 线性旋转 | 菱形 self-rotate | 同上 |
| 士兵 | 近战 | 无飞行；攻击帧 **短距 line 闪烁** | — |

**挂钩：** `CombatTowerBase` 在 `ApplyDamageToEnemy` 前 `SpawnProjectile(this, target)` — **仅 Visual**，伤害时机仍可即时或弹着时结算（建议弹着时，更有感）。

### 7.2 已有反馈扩展

| 现有 | 扩展（零资产） |
| --- | --- |
| `FloatingCombatText` | 暴击加 **scale pop** |
| `HitBurstEffect` | 改为 ParticleSystem 池，2 种：物理白、魔法蓝 |
| `CameraShakeController` | BOSS 出场 shake 加强（已有 spawn banner） |
| `TowerGroundZone` | 边缘 `LineRenderer` pulse + frost/fire 粒子 3/s |

### 7.3 Reduce Motion

`CombatFeedbackService.ReduceMotion == true` 时关闭：粒子、trail、punch、震屏加强 — **保留**飘字与必要 UI。

---

## 八、环境机制

### 8.1 古树（Ancient Tree ×2）

| 状态 | 视觉 |
| --- | --- |
| 未激活 | 棕竖条 + 绿圆冠（两圆叠加） |
| 可激活 | 冠部 slow pulse 绿 |
| 激活 | 根须 `LineRenderer` 从树到 `effectCell` 短暂延伸 + 绿粒子 |
| **挂钩** | `AncientTree` 状态回调 → `EnvironmentVisual` |

### 8.2 猎人陷阱（Hunter Trap）

| 状态 | 视觉 |
| --- | --- |
| 放置格空闲 | 虚线小方框 |
| 已放置 | 深色方块 + 中心 X 形（两 thin 矩形交叉） |
| 触发 | 0.2s 白色 flash 圈 + snap scale |
| CD 中 | 格上 **径向 fill**（`Image` radial 或 LineRenderer 弧） |

**挂钩：** `MapEnvironmentController`、`TrapPlacementSlot`

---

## 九、UI（程序皮肤层）

在现有 `UiDisplaySettings` 令牌之上：

### 9.1 程序 9-slice 面板

- 运行时生成 32×32 圆角 alpha 纹理 → Sprite `border=8`  
- 替换 `ApplyPanelBackground` 的纯色为 sliced Image（可选开关 `UseProceduralSkin`）

### 9.2 建造栏几何 Icon（6 塔）

| 塔 | Icon 几何 | 热键 |
| --- | --- | --- |
| Arrow | 细三角 | 1 |
| Frost | 六边近似圆 | 2 |
| Cannon | 横管 | 3 |
| Arcane | 菱形 | 4 |
| Barracks | 旗 | 5 |
| Mine | 菱形宝石 | 6 |

Icon 置于按钮左上，价格仍在下方 — 与 `TowerBuildDragHandler` ghost 一致。

### 9.3 动效清单

| 界面 | 动效 |
| --- | --- |
| MainMenu Play | 已有 loading bar — 加 panel fade in |
| Pause / Victory | `CanvasGroup` 0→1，0.15s |
| Wave 进度条 | fill 已有 — 100% 时短 flash |
| Codex |  tab 切换 slide 8px |

### 9.4 字体

- 仍用 TMP；从 Unity/TMP 免费字体中选 1 款 Title（如 **LiberationSans SDF** 保留正文，标题用 **Bold** 或另选 **Anton SDF** 等免费资源 — **仍无原画，仅字体文件**）。  
- 若严格零外部文件，则只对 Title **加 Outline** 组件。

---

## 十、后处理与光照（Unity 内置）

### 10.1 推荐栈（URP 2D 或 Volume）

| 效果 | 参数建议 | 目的 |
| --- | --- | --- |
| Vignette | intensity 0.25 | 聚视线到地图中心 |
| Color Adjustments | saturation -0.1, contrast +0.05 | 统一森林感 |
| Bloom | threshold 1.0, intensity 0.3 | 精英/钻石/暴击 |
| （可选）2D Light | Global dark + 塔旁 point | 单位从背景弹出 |

### 10.2 性能（PC Demo）

- 粒子同屏 < 200  
- 程序纹理启动时生成一次，不每帧  
- 投射物池 size 32  

---

## 十一、音频（零预算补充）

非原画，但独立 dev 常忽略：

| 事件 | 做法 |
| --- | --- |
| 建造 | 现有策略外，加 1 个低频 thud（代码 oscillator 或 Unity 免费 clip） |
| 波次开始 | 短 rise 音 |
| BOSS | 已有 banner — 加 1 和弦 stinger |

与 `CombatFeedbackService` 节流策略并存，避免 Arrow 连射音。

---

## 十二、建议目录结构（实施后）

```
边境守卫者-tuanjie/Assets/
  Scripts/
    Visual/                    # 新增
      VisualPalette.cs
      VisualSorting.cs
      ProceduralSpriteFactory.cs
      TowerVisualComposer.cs
      EnemyVisualComposer.cs
      ProjectilePool.cs
      EnvironmentVisual.cs
    Maps/                        # 现有
    Towers/
    UI/
  Resources/                     # 可选：缓存程序纹理 Sprite
    Generated/                   # .gitignore 或提交小纹理均可
```

**不要**把 Visual 逻辑散落到 `EnemyBase`、`ArrowTower` 等各写一份。

---

## 十三、分文件实施检查表

### P-A0 基础设施

- [x] `VisualPalette`：环境 + 塔 + 语义色常量
- [x] `VisualSorting`：layer order 常量
- [x] `ProceduralSpriteFactory`：`GetWhiteSprite()` 包装 + 噪声/圆/影/三角/菱形/环/圆角9-slice 纹理缓存
- [x] `TowerVisualComposer` / `EnemyVisualComposer` 空壳 → 已升级为实装

### P-A1 单位通用

- [x] 阴影工厂（`UnitVisualDecorator` 代码创建软影）
- [x] 受击 scale punch（敌人 `PlayHitFlash` / 塔 `TakeTowerDamage` 触发，尊重 `ReduceMotion`）
- [x] 敌人 idle 挤压 bob（位置由 PathFollower 控制，bob 走 localScale 避免冲突）
- [~] 描边/朝向：为避免与逻辑 sprite（选中/受击/隐身改 root 颜色）冲突，改用「彩色 root + 几何 accent 子件」表达层级，未做整体 outline / root 旋转

### P-A2 投射物

- [x] `ProjectileVisualFactory` 池（size 32）+ Arrow/Frost/Cannon/Arcane 四类（Arrow 旋向、Frost trail、Cannon 抛物线、Arcane 自转）
- [x] `CombatTowerBase.ApplyDamageToEnemy` 挂接（仅视觉、尊重 ReduceMotion）
- [x] Cannon 命中橙色扩散爆点（`ProjectileImpactFx`）

### P-A3 地图

- [x] 草地/路径/高台/阻挡 Perlin 程序纹理（每类型 1 张缓存复用 + grass/blocked 哈希转角）
- [x] BuildSlot 空闲慢速 pulse 环（`BuildSlotPulse`，占用时隐藏）
- [x] 出生点门洞三角 / 终点同心环+菱形 marker（goal pulse）
- [ ] （可选）视差 2 层 — 暂未做（氛围层已用 vignette 收敛视线）

### P-A4 塔 Silhouette

- [x] 6 塔（+3 支援塔）Compose 几何 accent
- [x] Lv 装饰层级（底部 1~5 level pip）
- [x] Lv5 分支 accent（暖/冷 + 形状，随 `RefreshPresentation` 重建）
- [x] 建造落地 0→1.12→1 动画（`TowerLandingAnim`）
- [x] 士兵：阴影 + 头部 accent

### P-A5 敌人 Silhouette

- [x] 16 敌人全部 Compose 几何 accent（清单 10 主敌人 + P2.1 扩展 6 敌人）
- [x] 精英金环 / BOSS 双环
- [x] 死亡粒子按 `DisplayColor`（沿用 `CombatFeedbackService.ReportEnemyDeath`）

### P-A6 环境

- [x] 古树三态（棕干 + 绿冠，ready 时绿冠 pulse，激活时根须线 + 绿扩散环）
- [x] 陷阱放置 X 提示 / 触发白闪环 / CD（沿用槽位状态色）

### P-A7 后处理

- [x] 零依赖屏幕空间 vignette 覆盖层（`AtmosphereController`，渲染于地图之上、HUD 之下，不遮挡 HUD）
- [~] Bloom / URP Volume：当前工程未配置 URP Volume，改用程序 vignette；Bloom 留待接入 URP 后补

### P-A8 UI 皮肤

- [x] 程序圆角 9-slice 面板（`ProceduralUiSkin` + `UiDisplaySettings.ApplyPanelBackground` 全局生效）
- [x] 建造栏 6 塔几何 icon（按钮左上）
- [x] Pause / Victory 面板 `CanvasGroup` fade in（`UiFadeIn`，尊重 ReduceMotion）

---

## 十四、验收标准（整包 Zero-Art Visual Pass）

1. **截图测试：** 新玩家 10 秒内能说出「这是森林里的几何塔防」，而不是「彩色方块测试」。  
2. **可读性：** 6 塔、10 敌人 Silhouette 在暂停状态下可区分 ≥80%（自测或同学试玩）。  
3. **战斗感：** 四种战斗塔均有可见弹道或命中特效；死亡有粒子。  
4. **地图：** 路径与草地无纯 flat 单色满屏；高台可辨认。  
5. **性能：** 1080p 60fps（与现原型同级 PC）。  
6. **兼容：** `Reduce Motion`、倍速、暂停、拖拽建造、射程圈 **行为不变**。  
7. **可扩展：** 将来只替换 `TowerVisualComposer` 内 Sprite 引用即可换真美术，不动 `TowerFactory` 经济逻辑。

---

## 十五、明确不做（本清单范围内）

- 外包原画 / 付费资产包作为主视觉  
- 3D 模型、骨骼动画、Spine  
- 改动 `GrimmForestMapLayout` 路径与 21 高台坐标  
- 为视觉重写 `WaveManager` / 经济 / 协同逻辑  
- 第二张地图（Lavafall Rift）的视觉 — 待 P4 再复制本清单模板  

---

## 十六、与 GDD 第十章关系（建议）

| GDD 阶段 | 本清单 |
| --- | --- |
| P3 UI 与表现 | P-A8、部分 P-A7 — **已完成 UI 逻辑，差皮肤** |
| P1 地图设计深化 | P-A3、P-A6 — 视觉层配合机制  
| 新阶段 **P3.4 Zero-Art Visual**（建议在 GDD 实施时追加） | P-A0 ~ P-A8 全文 |

---

## 版本变更记录

| 版本 | 日期 | 摘要 |
| --- | --- | --- |
| 1.0 | 2026-06-20 | 首版：Grimm Forest 全图零外包资产技术规格、分塔/分敌人 Silhouette、分阶段检查表 |
| 1.1 | 2026-06-28 | P-A0~P-A8 全部落地：`Scripts/Visual/` 新增几何/投射物/地图纹理/单位装饰/环境/氛围/UI 皮肤层，挂接现有塔/敌人/士兵/地图/环境/UI 入口，逻辑不变；勾选检查表并标注 描边整体 outline、视差、URP Bloom 三项为后续可选项 |

---

*文档结束 — 实施前请先选定 P-A0→A2 做垂直切片（例如：Arrow 塔 + Imp + 草地程序纹理），验证风格后再批量铺开到 6 塔 10 敌人。*
