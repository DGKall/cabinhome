# Cabin Game · 山脚小屋

在开阔草甸与近山之间，制作一款可逐步扩建小屋的独立游戏。当前先验证空间、画风与制作流程；玩法细节尚未定稿。

GitHub：[DGKall/cabinhome](https://github.com/DGKall/cabinhome)（用户创建的公开仓库）。

目标平台：**macOS、Windows、iOS**。前期使用 Mac 开发、调试和试玩。技术路线为 **Unity 6000.3.26f1 LTS + URP + C#**，建模使用 Blender，混元 3D 网页版辅助制作独立道具。

## 从这里接手

1. 阅读 [AGENTS.md](AGENTS.md)：开发与交接约定。
2. 阅读 [项目简要](docs/PROJECT.md)：目标、约束和阶段验收。
3. 阅读 [进度与下一步](docs/PROGRESS.md)：实际完成内容、下一项任务及阻塞。
4. 涉及模型或场景时，阅读 [素材需求](docs/ASSETS.md) 和 [设计简报](docs/reference/05-design-brief/017_cabin-game_agent-build-brief_v01.md)。配套 PDF 包含关键视觉参考。

**当前已有正式 URP 工程、空间灰盒和本地 009 风格试玩版。** 双击 `play-style-demo.command` 启动风格版；场景为 `Assets/LocalArt/Style009.unity`，素材整理/重建见 [美术流程](development/art/README.md)。风格版素材暂存本机，公开仓库仅克隆时仍可运行灰盒。

**原灰盒保留。** 工程位于 `unity/CabinGame/`，入口场景为 `Assets/Scenes/Meadow.unity`。打开场景点击 Play；本机独立包构建后也可双击根目录 `play-demo.command`。详细操作与构建方法见 [试玩说明](docs/DEMO.md)。

## 目录

| 路径 | 用途 |
|---|---|
| `docs/` | 项目简要、进度、决策和素材需求 |
| `docs/reference/` | 设计来源快照，避免依赖原机器的桌面路径 |
| `development/` | 开发环境说明、检查脚本和已有验证证据 |
| `assets-source/` | Blender 源文件与导出资产；下载收件箱不上传 |
| `unity/CabinGame/` | 正式 Unity URP 工程，代码、场景、资源、包锁和项目设置 |

当前目录还保留 `city-demo/`、`art-direction/`、`output/` 等早期演示或生成输出，不属于当前游戏实现，暂不纳入本仓库。选定参考资料已单独归档到 `docs/reference/`。

## 本地工作

打开 `cabin-game.code-workspace`。环境检查：

```sh
zsh development/check-environment.sh
```

克隆后先确保 Git LFS 可用，再执行 `git lfs install` 和 `git lfs pull`。工具版本与安装状态见 [环境说明](development/README.md)；其中绝对路径是当前开发机记录，换机器时需要调整。

代码、文档、Unity 文本资源用普通 Git；模型、纹理和音频等二进制资源按 `.gitattributes` 使用 LFS。不要提交缓存、密钥、签名文件、构建产物或未整理的素材下载。
