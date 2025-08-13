using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CuttingOptimizer.Models;
using CuttingOptimizer.DataProcessor;

namespace CuttingOptimizer.IOHelper
{
    public class ConsoleInputHelper : IInputHelper
    {
        private readonly IExcelReader _excelReader;

        public ConsoleInputHelper()
        {
            _excelReader = new ExcelReader();
        }

        public double GetMaterialLength()
        {
            while (true)
            {
                Console.Write("请输入原材料长度 (mm): ");
                var input = Console.ReadLine();

                if (double.TryParse(input, out double length) && length > 0)
                {
                    return length;
                }

                Console.WriteLine("请输入有效的正数！");
            }
        }

        public double GetMinJointLength()
        {
            while (true)
            {
                Console.Write("请输入最小拼接长度 (mm): ");
                var input = Console.ReadLine();

                if (double.TryParse(input, out double length) && length >= 0)
                {
                    return length;
                }

                Console.WriteLine("请输入有效的非负数！");
            }
        }

        public HoleConfiguration GetHoleConfiguration(double materialLength)
        {
            Console.WriteLine("\n=== 导轨孔洞配置 ===");
            Console.WriteLine("注意：此配置仅适用于导轨部件。");

            var holeSpacing = GetPositiveDouble("请输入孔洞间距 (mm): ");
            var leftMargin = GetNonNegativeDouble("请输入左边距 (mm): ");
            var rightMargin = GetNonNegativeDouble("请输入右边距 (mm): ");

            if (leftMargin + rightMargin >= materialLength)
            {
                Console.WriteLine("警告：左边距和右边距之和不能大于等于材料长度！");
                return GetHoleConfiguration(materialLength);
            }

            var config = new HoleConfiguration
            {
                HoleSpacing = holeSpacing,
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                MaterialLength = materialLength
            };

            var holes = config.GetHolePositions();
            Console.WriteLine($"\n孔洞分布信息:");
            Console.WriteLine($"孔洞数量: {holes.Count}");
            Console.WriteLine($"有效切割区域: {leftMargin}mm - {materialLength - rightMargin}mm");

            return config;
        }

        private double GetPositiveDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();
                if (double.TryParse(input, out double value) && value > 0)
                    return value;
                Console.WriteLine("请输入有效的正数！");
            }
        }

        private double GetNonNegativeDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();
                if (double.TryParse(input, out double value) && value >= 0)
                    return value;
                Console.WriteLine("请输入有效的非负数！");
            }
        }

        public List<Part> ImportExcelFile()
        {
            while (true)
            {
                Console.Write("请输入Excel文件路径: ");
                var filePath = Console.ReadLine()?.Trim('"');

                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("文件路径不能为空！");
                    continue;
                }

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"文件不存在: {filePath}");
                    continue;
                }

                try
                {
                    var parts = _excelReader.ReadExcelFile(filePath);
                    if (parts.Any())
                    {
                        var typeGroups = parts.GroupBy(p => p.FunctionDefinition).ToList();
                        Console.WriteLine("\n导入的部件类型:");
                        foreach (var group in typeGroups)
                        {
                            var isRailType = group.Key?.Contains("导轨") == true;
                            var cuttingMethod = isRailType ? "(孔洞约束切割)" : "(传统切割)";
                            Console.WriteLine($"  {group.Key}: {group.Count()} 个部件 {cuttingMethod}");
                        }

                        return parts;
                    }
                    else
                    {
                        Console.WriteLine("Excel文件中没有有效数据，请检查文件内容。");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"读取Excel文件失败: {ex.Message}");
                }
            }
        }

        public void DisplayResults(CuttingResult result, string functionType)
        {
            var isRailType = functionType?.Contains("导轨") == true;
            var cuttingMethod = isRailType ? "孔洞约束切割" : "传统切割";

            Console.WriteLine($"\n=== {functionType} 优化结果 ({cuttingMethod}) ===");
            Console.WriteLine($"使用材料数量: {result.MaterialsUsed}");
            Console.WriteLine($"总使用长度: {Math.Round(result.TotalUsedLength, 0)} mm");
            Console.WriteLine($"材料利用率: {result.MaterialUtilization:P2}");
            Console.WriteLine($"总浪费长度: {Math.Round(result.TotalWaste, 0)} mm");

            if (isRailType && result.TotalHoleWaste > 0)
            {
                Console.WriteLine($"孔洞约束浪费: {Math.Round(result.TotalHoleWaste, 0)} mm");
            }

            Console.WriteLine("\n切割计划:");
            foreach (var plan in result.CuttingPlans)
            {
                Console.WriteLine($"\n{plan.MaterialId} (长度: {plan.MaterialLength}mm, 利用率: {plan.Utilization:P2})");

                if (isRailType && plan.HoleConstraintWaste > 0)
                {
                    Console.WriteLine($"  孔洞约束浪费: {Math.Round(plan.HoleConstraintWaste, 0)}mm");
                }

                foreach (var cut in plan.CutParts)
                {
                    Console.WriteLine($"  {cut.Id}. {cut.PartName} - {Math.Round(cut.Length, 0)}mm");
                    Console.WriteLine($"     位置: {Math.Round(cut.StartPosition, 0)}mm - {Math.Round(cut.EndPosition, 0)}mm");

                    // 新增：显示详细部件信息
                    Console.WriteLine($"     图例位置: {cut.DiagramPosition ?? "未指定"}");
                    Console.WriteLine($"     布局空间: {cut.LayoutSpace ?? "未指定"}");
                    Console.WriteLine($"     安装板: {cut.MountingBoard ?? "未指定"}");
                    Console.WriteLine($"     设备标识符: {cut.DeviceIdentifier ?? "未指定"}");
                    Console.WriteLine($"     部件编号: {cut.PartNumber ?? "未指定"}");
                    Console.WriteLine($"     功能定义: {cut.FunctionDefinition ?? "未指定"}");
                }
                Console.WriteLine($"  剩余长度: {Math.Round(plan.RemainingLength, 0)}mm");
            }

            if (result.OversizedParts.Any())
            {
                Console.WriteLine("\n超长部件分段:");
                foreach (var oversized in result.OversizedParts)
                {
                    Console.WriteLine($"\n{oversized.OriginalPart.Name} (原长度: {Math.Round(oversized.OriginalPart.Length, 0)}mm)");
                    Console.WriteLine($"  图例位置: {oversized.OriginalPart.DiagramPosition ?? "未指定"}");
                    Console.WriteLine($"  布局空间: {oversized.OriginalPart.LayoutSpace ?? "未指定"}");
                    Console.WriteLine($"  安装板: {oversized.OriginalPart.MountingBoard ?? "未指定"}");
                    Console.WriteLine($"  设备标识符: {oversized.DeviceIdentifier ?? "未指定"}");
                    Console.WriteLine($"  部件编号: {oversized.PartNumber ?? "未指定"}");
                    Console.WriteLine($"  分段数量: {oversized.TotalSegments}, 拼接点: {oversized.SpliceCount}");
                    foreach (var segment in oversized.Segments)
                    {
                        Console.WriteLine($"  段{segment.SegmentIndex}: {Math.Round(segment.Length, 0)}mm");
                    }
                }
            }
        }
    }
}