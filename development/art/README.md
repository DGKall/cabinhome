# 混元样本整理与本地风格验证

2026-10-19。处理 Blender 5.2.2 LTS 导出的两个首批样本，Unity 6000.3.26f1 / URP。

原始下载保持不变，收件箱、处理后源文件与 `Assets/LocalArt` 为本地忽略目录。公开仓库保存处理配方、统计与交接记录；尚未归档网页版输出条款，不能把社区模型权重许可证当成此处网页版输出的许可。新机器需复制两个 GLB 才能重建风格场景；仅克隆代码仍可运行原灰盒。

## 重建

在仓库根执行（Blender 路径可按本机调整）：

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --python development/art/prepare_hunyuan.py -- assets-source/hunyuan/inbox/PRP_001/9715092750fe0154b1209bc6660c5f3c.glb PRP_001
/Applications/Blender.app/Contents/MacOS/Blender --background --python development/art/prepare_hunyuan.py -- assets-source/hunyuan/inbox/VEG_003/ba611d5e1c1c110b17101bb0332074c2.glb VEG_003
```

配方校验源文件 SHA-256，拒绝对不同输入自动复用样本专用切割。输出到 `assets-source/exports/PRP_001/` 和 `VEG_003/`：可编辑 blend、FBX、GLB、颜色贴图、inspection.json。脚本会重写其明确命名的生成结果；不要在这些文件上直接保存手工修改，应另存版本。

1. Unity 菜单 `CabinHome > Create 009 style study (once)` 首次创建；已有场景会拒绝覆盖，后续直接编辑保存的场景。
2. 打开 `Assets/LocalArt/Style009.unity`，点击 Play。
3. 构建：关闭该工程的 Unity 编辑器，执行 `zsh development/build-mac-style.sh`。
4. 试玩：双击仓库根 `play-style-demo.command`。
5. 自动截图与行走检查：

```sh
Builds/CabinHome-Style009.app/Contents/MacOS/CabinHome -cabinSmoke /private/tmp/cabin-style-check -logFile /private/tmp/cabin-style-player.log
```

`inspect_glb.py -- 模型.glb 输出目录` 可在 Blender 后台渲染模型正反面，输出文件不替代游戏内验证。

## 验证要点与已修问题

- 石块原始模型包含草地底盘/花朵。切除源 Z=0.105 以下部分，保留最大连通块并封底；用中性哑光材质替换带苔藓/固定明暗的原贴图。原始贴图仍保留。
- 树木保留 1K 颜色贴图，不使用生成的高频法线贴图。树冠略规则/玩具感仍待美术评审；未标为最终资产。
- FBX 本身携带轴向/单位转换。Prefab 使用单位变换的外层根节点，FBX 放在子节点，场景放置不得覆盖子节点的变换。
- 双面草花分别使用顶点，避免反向共用三角面的法线相互抵消。远山朝向已检查。
- 三种光照是静态评审预设。没有完整天气、风摆、LOD、移动端输入或性能验收。

实测资产统计见 `asset-inspection.json`；实际运行结果见 `../style-validation.txt`。当前本机截图保存在 `Builds/style-review/`，不随公开仓库上传。
