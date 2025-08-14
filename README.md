# CuttingOptimizer - 一维下料优化系统

## 项目概述

基于.NET开发的一维下料优化系统，采用改进的贪心算法，支持大规模数据处理、特殊约束处理和数据库集成。系统能够高效处理超过3000个部件的优化任务，特别针对导轨类部件的孔洞约束进行了专门优化。

## 功能特点

- **智能算法**：改进的贪心算法，支持大数据集优化
- **特殊约束**：导轨部件孔洞配置处理
- **数据库集成**：支持多种数据库的数据导入导出
- **Excel导入**：支持从Excel文件批量导入部件数据
- **结果统计**：详细的材料利用率和浪费统计
- **模块化设计**：清晰的项目架构，便于扩展和维护

## 系统要求

- .NET 6.0 或更高版本
- Windows 10/11 或 Windows Server 2016+
- 内存：建议8GB以上（处理大数据集时）
- 硬盘：至少100MB可用空间

## 安装说明

### 源码编译

1. 克隆项目到本地：
```bash
git clone [项目地址]
cd CuttingOptimizerSolution
```

2. 使用Visual Studio 2022或VS Code打开解决方案：
```bash
CuttingOptimizerSolution.sln
```

3. 还原NuGet包：
```bash
dotnet restore
```

4. 编译项目：
```bash
dotnet build
```

5. 运行控制台应用：
```bash
dotnet run --project CuttingOptimizer.Console
```

### 数据库配置（可选）

如需使用数据库功能，请配置相应的连接字符串和执行数据库脚本。

## 使用方法

### 基本使用

1. **启动程序**：运行CuttingOptimizer.Console.exe
2. **输入材料参数**：设置原材料长度和切割损耗
3. **导入数据**：选择Excel文件或手动输入部件信息
4. **执行优化**：系统自动进行切割方案优化
5. **查看结果**：显示详细的切割方案和统计信息

### 数据格式要求

Excel文件应包含以下列：
- 部件名称
- 长度（mm）
- 数量
- 功能定义（可选）
- 部件编号（可选）

### 特殊功能

**导轨部件处理**：
- 支持孔洞位置配置
- 自动处理孔洞约束
- 优化孔洞对齐策略

**大数据集处理**：
- 超过3000个部件时自动启用简化算法
- 按功能定义和部件编号分组处理
- 内存优化和性能提升

## 项目结构
├── CuttingOptimizer.Console/                # 控制台应用程序
│   └── Program.cs                           # 程序入口点
├── CuttingOptimizer.Core/                   # 核心算法模块
│   └── Core/
│       ├── AdvancedGreedyAlgorithm.cs       # 改进贪心算法
│       └── ICuttingAlgorithm.cs             # 算法接口
├── CuttingOptimizer.Models/                 # 数据模型
│   └── Models/
│       ├── Part.cs                          # 部件模型
│       ├── CuttingResult.cs                 # 切割结果模型
│       ├── CuttingPlan.cs                   # 切割方案模型
│       ├── HoleConfiguration.cs             # 孔洞配置模型
│       ├── CutPart.cs                       # 切割部件模型
│       ├── OptimizationModels.cs            # 优化模型
│       └── OversizedPart.cs                 # 超长部件模型
├── CuttingOptimizer.DataProcessor/          # 数据处理模块
│   └── DataProcessor/
│       ├── ExcelReader.cs                   # Excel读取器
│       ├── IExcelReader.cs                  # Excel读取接口
│       ├── ResultFormatter.cs               # 结果格式化器
│       └── IResultFormatter.cs              # 结果格式化接口
├── CuttingOptimizer.IOHelper/               # 输入输出辅助
│   └── IOHelper/
│       ├── ConsoleInputHelper.cs            # 控制台输入辅助
│       ├── IInputHelper.cs                  # 输入辅助接口
│       └── LicenseHelper.cs                 # 许可证辅助
├── DatabaseScripts/                         # 数据库脚本
├── Templates/                               # 模板文件
└── README.md                                # 项目说明

## 算法特点

### 改进贪心算法

- **分组策略**：按功能定义和部件编号进行智能分组
- **双重优化**：小组使用精确算法，大组使用简化策略
- **约束处理**：特殊处理导轨部件的孔洞约束
- **性能优化**：针对大数据集进行内存和时间优化

### 算法流程

1. 数据预处理和分组
2. 超长部件识别和处理
3. 按组执行优化算法
4. 结果合并和统计计算
5. 输出格式化和保存

## 配置示例

### 基本配置

```csharp
var config = new OptimizationConfig
{
    MaterialLength = 6000,  // 原材料长度(mm)
    CuttingLoss = 3,        // 切割损耗(mm)
    EnableHoleProcessing = true,  // 启用孔洞处理
    MaxGroupSize = 3000     // 最大组大小
};
```

### 孔洞配置

```csharp
var holeConfig = new HoleConfiguration
{
    HolePositions = new[] { 100, 200, 300 },  // 孔洞位置(mm)
    HoleDiameter = 8,       // 孔径(mm)
    MinDistance = 50        // 最小间距(mm)
};
```

## 性能特点

- **处理能力**：支持10000+部件的大规模优化
- **内存效率**：优化的内存使用，避免内存溢出
- **计算速度**：大数据集下仍保持较快的计算速度
- **结果质量**：在性能和优化质量间取得良好平衡

## 输出结果

### 控制台输出

- 详细的切割方案
- 材料利用率统计
- 浪费材料统计
- 处理时间信息

### 文件输出

- Excel格式的详细结果
- 统计报表
- 错误日志（如有）

## 扩展功能

### 数据库集成

- 支持从数据库读取部件信息
- 结果自动保存到数据库
- 历史记录查询和管理

### 报表生成

- 自动生成优化报表
- 图表化显示统计信息
- 导出多种格式报告

## 故障排除

### 常见问题

1. **内存不足**：减少批处理大小或增加系统内存
2. **Excel读取失败**：检查文件格式和权限
3. **算法超时**：启用简化模式或减少数据量
4. **结果异常**：检查输入数据的有效性

### 日志查看

系统会在控制台和日志文件中记录详细信息，便于问题诊断。

## 开发环境

- **IDE**：Visual Studio 2022 / VS Code
- **框架**：.NET 6.0+
- **语言**：C# 10.0
- **包管理**：NuGet

## 贡献指南

1. Fork 项目
2. 创建功能分支
3. 提交更改
4. 推送到分支
5. 创建 Pull Request

## 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 联系信息


- 邮箱：[bingxinzuo0@gmail.com]


## 更新日志

### v1.0.0 (2024-01-01)
- 初始版本发布
- 实现基础切割优化功能
- 支持Excel数据导入
- 添加控制台界面

### v1.1.0 (2024-02-01)
- 添加导轨孔洞处理功能
- 优化大数据集处理性能
- 改进算法效率
- 增加详细统计信息

## 致谢

感谢所有为本项目做出贡献的开发者和测试人员。
