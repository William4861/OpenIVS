# OpenIVS — Open Source Industrial Vision System

开源工业机器视觉检测系统，覆盖**图像采集 → AI 推理 → 结果判定 → 运动控制**全流程。支持硬件仿真模式，无需真实相机、PLC 或推理引擎即可运行。

## 快速开始

```bash
# 1. 克隆
git clone https://github.com/William4861/OpenIVS.git
cd OpenIVS

# 2. 用 Visual Studio 2022 打开
OpenIVS.sln

# 3. 设置启动项目为 OpenIVS
# 4. 复制仿真配置文件到输出目录
copy Simulation\settings_sim.xml OpenIVSWPF\bin\x64\Debug\settings.xml

# 5. 按 F5 运行
```

> 如果无硬件（海康相机、DLCV 模型、Modbus PLC），系统会自动降级到仿真模式。

## 系统流程

```
PLC 驱动运动 → 到达工位 → 软触发拍照 → 图像采集
→ AI 模型推理 → 结果判定 (OK/NG) → 图像保存 → 下一工位
```

位置序列: `220 → 330 → 440 → 330`（循环）

## 项目结构

```
OpenIVS/
├── OpenIVSWPF/              # 主 WPF 界面（启动项目）
│   ├── MainWindow.xaml      # 主界面
│   ├── Managers/            # 各管理器
│   │   ├── MainLoopManager.cs     # 主循环（运动→拍照→推理→保存）
│   │   ├── SimulationModeManager.cs  # 仿真模式桥接层
│   │   ├── SettingsManager.cs     # 配置管理
│   │   └── ...
│   └── CameraManagerStub.cs # 相机 SDK 桩（无需真实 SDK）
├── Simulation/              # 硬件仿真层（无需真实设备）
│   ├── SimulatedCamera.cs        # 模拟工业相机（生成合成图像）
│   ├── SimulatedModbusPLC.cs     # 模拟 PLC（内存状态机）
│   ├── SimulatedModel.cs         # 模拟推理模型（像素分析）
│   ├── DefectImageGenerator.cs   # PCB 缺陷图像生成器
│   ├── settings_sim.xml          # 仿真模式配置
│   └── SimImages/                # 默认测试图像
├── DlcvCsharpApi/           # 深度学习推理 C# 封装
├── ImageViewer/             # 图像显示控件
├── ModbusApi/               # Modbus RTU 串口通信
├── SimpleLogger/            # 日志工具
└── OpenIVS.sln
```

## 仿真模式

| 硬件 | 仿真实现 | 行为 |
|------|----------|------|
| 工业相机 | 生成 PCB/金属表面合成图像 | 随机注入划痕/凹坑/污渍/裂纹/偏移 |
| PLC | 内存状态机 | 支持位置 1-2-3-2-1 循环，20ms 步进（模拟节拍） |
| 推理模型 | 基于像素分析 | 模拟 30-80ms 推理耗时，含类别/置信度/bbox |

启用方式：
- 配置 `settings.xml` 中 `<UseSimulation>true</UseSimulation>`
- 或直接在菜单：**工具 → 仿真模式**

## 依赖

| 依赖 | 用途 | 仿真时 |
|------|------|--------|
| .NET Framework 4.7.2 | 运行时 | 需要 |
| Visual Studio 2022 | 编译 | 需要 |
| NuGet 包（自动还原） | OpenCvSharp4, Newtonsoft.Json, EasyModbus | 需要 |
| 海康 MVS SDK | 真实相机 | **不需要** |
| DLCV 推理引擎 | 真实 AI 推理 | **不需要** |
| Modbus PLC 硬件 | 真实运动控制 | **不需要** |

## 截图 / GIF

（运行截图待补充）

## 许可

Apache 2.0 — 详见 [LICENSE](LICENSE)
