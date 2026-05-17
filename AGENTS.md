# OpenIVS AGENTS.md

Open Source Industrial Vision System — 开源工业机器视觉框架（C# WPF）

## 项目结构

| 目录 | 说明 |
|------|------|
| `OpenIVSWPF/` | **主 WPF 界面（启动项目）** — Modbus/相机/AI/主循环管理器 |
| `Simulation/` | **硬件仿真层** — 无需真实硬件即可运行 |
| `DlcvCsharpApi/` | 深度学习推理 C# 封装（`dlcv_infer.dll`） |
| `ImageViewer/` | 自定义图像显示 WPF 控件 |
| `ModbusApi/` | Modbus RTU 串口通信（EasyModbus） |
| `SimpleLogger/` | 日志工具 |

## 构建方式

```bash
# C# 项目（.NET Framework 4.7.2，Windows 仅）
msbuild OpenIVS.sln /p:Configuration=Debug /p:Platform=x64
```

NuGet 包自动还原：OpenCvSharp4, EasyModbus, Newtonsoft.Json

## 仿真模式（无需硬件）

`Simulation/` 目录提供完整硬件模拟：

```
Simulation/
├── SimulatedCamera.cs         # 模拟工业相机（生成 PCB/金属合成图像）
├── SimulatedModbusPLC.cs      # 模拟 PLC（内存状态机）
├── SimulatedModel.cs          # 模拟推理模型（像素分析）
├── DefectImageGenerator.cs    # 缺陷图像生成器（5 种缺陷）
├── settings_sim.xml           # 仿真模式配置文件
└── SimImages/                 # 默认测试图像
```

### 启用仿真

1. 在 VS 中将 `Simulation/settings_sim.xml` 复制到输出目录 `OpenIVSWPF/bin/x64/Debug/settings.xml`
2. 按 F5 运行，仿真模式自动激活
3. 或使用菜单：**工具 → 仿真模式**
4. 硬件初始化失败也会自动回退到仿真模式

### 仿真原理

| 硬件 | 仿真方式 | 行为 |
|------|----------|------|
| 工业相机 | 合成 PCB/金属表面图像 | 随机注入 5 种缺陷 |
| PLC | 内存状态机，位置 1-2-3-2-1 循环 | 20ms 步进，支持线圈/寄存器读写 |
| 推理模型 | 基于像素分析 | 30-80ms，含类别/置信度/bbox |

## 系统流程

```
PLC 驱动运动 → 到达位置 → 软触发拍照 → 图像采集
→ AI 模型推理 → 结果判定 (OK/NG) → 图像保存 → 下一位置
```

位置序列: 220 → 330 → 440 → 330（循环）

## 依赖

| 依赖 | 用途 | 仿真时是否需要 |
|------|------|---------------|
| .NET Framework 4.7.2 | 运行时 | ✅ 需要 |
| Visual Studio 2022 | 编译 | ✅ 需要 |
| OpenCvSharp4 (NuGet) | 图像处理 | ✅ 需要 |
| EasyModbus (NuGet) | Modbus 通信 | ❌ 仿真用内存状态机 |
| 海康 MVS SDK + 相机 | 真实图像采集 | ❌ 不需要 |
| DLCV 推理引擎 | 真实 AI 推理 | ❌ 不需要 |

## 已知约束

- .NET Framework 4.7.2 WPF → **Windows 仅**
