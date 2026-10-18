# 小屋游戏本地开发环境

目标平台：macOS、Windows、iOS。前期在 Mac 开发和试玩。

## 已安装与验证（2026-10-18）

| 工具 | 版本 / 状态 |
|---|---|
| Unity Hub | 3.22.3，Personal 许可已显示 |
| Unity Editor | 6000.3.26f1 LTS，Apple Silicon |
| 平台模块 | Mac 内置；Windows Mono、iOS 已安装；三平台均被临时工程识别 |
| VS Code | 1.142.0 arm64；Unity 1.3.2、C# 2.160.4、C# Dev Kit 3.40.210 |
| .NET SDK | 10.0.402，临时 C# 程序编译运行通过 |
| Blender | 5.2.2 LTS，界面启动和 GLB/FBX 基础导入导出通过 |
| Xcode | 27.1；iOS 27.1、macOS 27.0 SDK 可用 |
| Git / Git LFS | Apple Git 2.54.0；复用 Fork 自带 Git LFS 3.7.0 |
| GitHub CLI | 2.102.0，官方发布包经 SHA-256 校验，安装到 `~/.local/bin/gh` |

临时 Unity 工程成功启动并导入了 Blender 生成的验证 FBX。
详细记录见 `environment.json` 与 `verification-results.txt`。尚未创建正式 URP 游戏工程或进行平台打包和真机测试。

## 工具分工

- Unity Hub 管理 Unity 6.3 LTS Apple Silicon 编辑器与平台模块。
- 新游戏项目选 Universal 3D / URP；当前工作区不是已经创建好的 Unity 项目。
- VS Code 编写 C#；微软 Unity 扩展已包含 C# Dev Kit 等依赖。
- 单独安装的 .NET 10 SDK 用于编辑器工具与普通 C# 开发，不会把 Unity 项目的脚本运行时或语言版本自动升级为 .NET 10。
- Blender 用于模型尺寸、原点、拓扑、UV、材质、减面和导出。
- 混元 3D 使用网页版，不安装本地模型、CUDA、PyTorch 或推理服务。
- Xcode 用于 Apple 平台构建；iOS 签名和真机测试仍需要本人账号及设备。
- Fork 继续作为 Git 图形工具，Git LFS 用于较大的二进制美术文件。

双击根目录 `cabin-game.code-workspace` 打开工作区。终端执行
`zsh development/check-environment.sh` 可重新检查工具路径和版本。

## 混元 3D 到游戏资产的流程

1. 网页生成后，若提供 GLB，则优先保留带材质的 GLB；同时保留原始贴图、参考图和生成参数。
2. 原始下载放入 `assets-source/hunyuan/inbox/资产编号/`，保留原件。
3. Blender 导入后另存到 `assets-source/blender/`。一个单位为一米；检查真实尺寸、底部原点、朝向、法线、UV、材质数量及三角面数。
4. 根据实际模型决定减面、重拓扑、贴图调整及 LOD；生成模型不能仅凭外观认定为可直接发布的资产。
5. 处理后的 FBX/GLB 与贴图放到 `assets-source/exports/资产编号/`。
6. Unity 中先验证一个代表模型的尺寸、材质和碰撞，再批量导入。FBX 是一条可选导入路线；GLB 路线可在实际 Unity 项目中通过 Package Manager 安装官方 `com.unity.cloud.gltfast` 包，并验证所选版本与 URP 的兼容性。不要假定 Blender 节点材质会完整转换成 URP。

优先让混元辅助石头、摆件、装饰道具等独立资产。小屋外墙、门窗、基础和升级模块按设计尺寸制作，避免难以拼接。
不要把下载缓存或全部高模原件直接塞进 Unity 的 Assets 文件夹。

米制空白模板：`assets-source/blender/templates/cabin-meters.blend`。打开后另存为自己的资产文件，不覆盖模板。
已用临时一米立方体验证 Blender GLB 导出/导入和 FBX 导出，往返尺寸为 1×1×1 m；这不是混元真实资产或 Unity 导入验收。

## 新 Unity 项目创建后的配置

- 创建 Universal 3D 项目，使用本次固定的 6.3 LTS 补丁版本。
- 在 External Tools 选择 Visual Studio Code；安装/更新 Visual Studio Editor 包以使用官方 VS Code 集成。
- 保持资源 `.meta` 文件随项目版本管理；场景与 Prefab 采用文本序列化。
- Git 忽略 Library、Temp、Logs、Obj 和生成的构建输出。
- 按实际资产类型配置 LFS，通常包括 `.blend`、`.fbx`、`.glb` 和大贴图/音频；不要用 LFS 管理文本场景、Prefab、代码或 `.meta`。
- 建立 macOS、Windows Mono、iOS 三个平台配置。iOS 真机性能、Windows 兼容性需要对应设备实测。
- 前期只进行空间灰盒与风格验证；设计 brief 中的尺寸、相机和玩法仍保留各自待确认状态。

## 当前配置的边界

工具安装与项目验证是不同阶段。Unity Personal 许可与临时工程启动已经验证；正式 URP 工程、GLB 导入包、构建和设备测试仍应分别记录结果。
Homebrew 原有版本不识别 macOS 27，且原镜像返回 403；本次主要工具采用官方安装包，不依赖 Homebrew。

GitHub 远端使用用户创建的 `DGKall/cabinhome`。终端登录与浏览器/连接器登录相互独立；可用 `gh auth status` 检查，未登录时由用户执行 `gh auth login --hostname github.com --git-protocol https --web`。不要在文档或聊天中保存密码和访问令牌。

自动化 shell 如果未读取 `.zprofile`，需要先执行 `export PATH="$HOME/.local/bin:$PATH"`，保证 Git 的 LFS hooks 能找到 `git-lfs`。

## 官方参考

- https://code.visualstudio.com/docs/other/unity
- https://unity.com/releases/unity-6/support
- https://www.blender.org/download/
- https://developer.apple.com/xcode/resources/
- https://3d.hunyuan.tencent.com/
- https://github.com/Unity-Technologies/com.unity.cloud.gltfast
