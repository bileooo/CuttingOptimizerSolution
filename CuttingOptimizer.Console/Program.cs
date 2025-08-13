using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CuttingOptimizer.Core;
using CuttingOptimizer.DataProcessor;
using CuttingOptimizer.IOHelper;
using CuttingOptimizer.Models;

namespace CuttingOptimizer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                System.Console.OutputEncoding = Encoding.UTF8;
                LicenseHelper.SetEPPlusLicense();

                System.Console.WriteLine("=== 一维下料优化系统 ===");
                System.Console.WriteLine();

                var inputHelper = new ConsoleInputHelper();
                var resultFormatter = new ResultFormatter();

                // 获取基本参数
                var materialLength = inputHelper.GetMaterialLength();
                var minJointLength = inputHelper.GetMinJointLength();
                var parts = inputHelper.ImportExcelFile();

                if (parts == null || !parts.Any())
                {
                    System.Console.WriteLine("没有有效的部件数据，程序退出。");
                    return;
                }

                System.Console.WriteLine($"\n成功导入 {parts.Count} 个部件");

                // 按功能类型分组处理
                var functionGroups = parts.GroupBy(p => p.FunctionDefinition ?? "未知类型").ToList();
                var allResults = new List<CuttingResult>();

                foreach (var group in functionGroups)
                {
                    var functionType = group.Key;
                    var functionParts = group.ToList();

                    //System.Console.WriteLine($"\n=== 处理 {functionType} 部件 ({functionParts.Count} 个) ===");

                    // 检查是否需要孔洞配置
                    HoleConfiguration holeConfig = null;
                    if (functionType.Contains("导轨"))
                    {
                        System.Console.WriteLine("检测到导轨部件，需要配置孔洞参数。");
                        holeConfig = inputHelper.GetHoleConfiguration(materialLength);
                    }

                    //System.Console.WriteLine($" {functionType}");

                    // 执行优化算法
                    var algorithm = new AdvancedGreedyAlgorithm();
                    var result = algorithm.OptimizeCutting(functionParts, materialLength, minJointLength, holeConfig);
                    result.FunctionType = functionType;

                    // 显示该类型的结果
                    inputHelper.DisplayResults(result, functionType);
                    allResults.Add(result);
                }

                // 显示总体统计
                System.Console.WriteLine("\n=== 总体统计 ===");
                var totalMaterials = allResults.Sum(r => r.MaterialsUsed);
                var totalUsedLength = allResults.Sum(r => r.TotalUsedLength);
                var totalWaste = allResults.Sum(r => r.TotalWaste);
                var overallUtilization = totalMaterials > 0 ? totalUsedLength / (totalMaterials * materialLength) : 0;

                System.Console.WriteLine($"总使用材料数量: {totalMaterials}");
                System.Console.WriteLine($"总使用长度: {Math.Round(totalUsedLength, 0)} mm");
                System.Console.WriteLine($"总体利用率: {overallUtilization:P2}");
                System.Console.WriteLine($"总浪费长度: {Math.Round(totalWaste, 0)} mm");

                System.Console.WriteLine("\n=== 各类型利用率对比 ===");
                foreach (var result in allResults)
                {
                    System.Console.WriteLine($"{result.FunctionType}: {result.MaterialUtilization:P2} (材料数: {result.MaterialsUsed})");
                }

                // 保存结果
                resultFormatter.SaveResults(allResults, materialLength, minJointLength);

                System.Console.WriteLine("\n按任意键退出...");
                System.Console.ReadKey();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"程序执行出错: {ex.Message}");
                System.Console.WriteLine($"详细错误: {ex.StackTrace}");
                System.Console.WriteLine("\n按任意键退出...");
                System.Console.ReadKey();
            }
        }
    }
}