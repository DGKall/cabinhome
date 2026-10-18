# 空间灰盒 demo · v0.1

正式工程：`unity/CabinGame/`。场景：`Assets/Scenes/Meadow.unity`。Unity 6000.3.26f1、URP 17.3.0、Input System 1.20.0。输入包使用当前编辑器附带版本，修复旧模板 1.12.0 对已移除枚举的编译错误。

## 启动与试玩

本机完成构建后，双击根目录 `play-demo.command`，或直接打开 `Builds/CabinHome.app`。构建目录不上传 GitHub。

从 GitHub 克隆时，先运行 `git lfs pull`；用 Unity Hub 添加 `unity/CabinGame/`，打开 `Assets/Scenes/Meadow.unity` 并点击 Play。只需修改代码或场景时，不必先构建独立应用。

| 操作 | 按键 |
|---|---|
| 观察 / 步行切换 | Tab，或底部 Walk around / Overview 按钮 |
| 步行移动；观察模式平移 | W A S D |
| 看向 / 环绕 | 按住鼠标右键拖动（触控板可用双指辅助点按并拖动） |
| 观察模式缩放 | 滚轮 / 双指滚动 |
| 加速步行 | 左 Shift |
| 显示 / 隐藏规划边界 | B |
| 四个固定评审镜头 | 数字 1–4，或底部 View 按钮 |
| 返回初始观察视角 | R / Esc |
| 显示 / 隐藏界面 | H |
| 退出独立应用 | macOS 的 Command-Q |

步行从南侧步道开始，面朝小屋。沿步道直行，上台阶穿过敞开的门口，可以进入小屋。当前没有跳跃、建造或摆放交互；观察视角可以查看规划范围。

## 当前内容与边界

- 160×120 m 地形按简报公式生成，2 m 网格；保留台地缓坡与北侧坡体。
- 6×8 m 初始小屋，完成地坪为 Unity Y=2；南侧入口、实际窗洞、基础、台阶与简易室内长凳。
- 台地、阶段 2/3、露台和工坊/温室/菜园/弹性区为可切换轮廓，不是已实现的扩建玩法。
- 少量原始几何树石、共享哑光材质、南侧场外湖面与远山占位。均为开发侧占位，不是正式美术资产。
- 四个镜头使用同一几何与统一下午光照。本轮没有实现黄昏/蓝调/雨天切换。CAM04 为避开入口并对齐真实窗口，临时改为 Unity (73.8,3.6,60) 看向 (82,6,10)。
- UI 暂用英文以避免引入未确认字体；中文化、触屏控制和最终游戏相机留到后续。
- 当前 Mac 开发包经检查包含 arm64/x86_64，仅在 Apple M3 Pro 实测；Intel Mac、Windows/iOS 构建和设备验收尚未进行。这个 demo 不代表最终画面或性能承诺。

## 实现入口

| 文件 | 用途 |
|---|---|
| `Assets/Scripts/SitePlan.cs` | 地形公式与评审坐标 |
| `Assets/Scripts/WalkMotor.cs` | 与输入设备分离的步行、重力和场地边界处理 |
| `Assets/Scripts/DemoController.cs` | 桌面输入、观察镜头与临时界面 |
| `Assets/Editor/GrayboxBuilder.cs` | 首次生成、空间验证与 Mac 构建入口 |
| `Assets/Scripts/DemoSmokeCheck.cs` | 显式命令行启动的独立应用验证与截图，普通试玩不运行 |

场景、材质、网格均为保存的 Unity 资源，可以在编辑器直接修改。生成器只用于初始化，发现场景已存在时会拒绝覆盖。不要为了修改一个物体删除整个场景重建，也不要把生成器改动当成现有场景已更新。

## 构建与验证

关闭此工程的 Unity 编辑器实例后，在仓库根目录运行：

```sh
zsh development/build-mac-demo.sh
```

编辑器菜单 `CabinHome/Validate graybox` 检查关键尺寸、URP 与入口碰撞；`CabinHome/Build Mac demo` 可在编辑器中构建。

独立应用的自动验证需要有图形桌面，不要使用 `-nographics`：

```sh
Builds/CabinHome.app/Contents/MacOS/CabinHome \
  -cabinSmoke /private/tmp/cabinhome-smoke \
  -logFile /private/tmp/cabinhome-player.log
```

它会截图、使用实际步行控制器通过台阶进入室内、检查北墙阻挡，再输出 `report.txt` 并以退出码报告结果。这个测试不替代真实输入和视觉检查；验证记录见进度表。
