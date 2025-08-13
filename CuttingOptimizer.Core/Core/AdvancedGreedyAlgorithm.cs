using System;
using System.Collections.Generic;
using System.Linq;
using CuttingOptimizer.Models;

namespace CuttingOptimizer.Core
{
    public class AdvancedGreedyAlgorithm : ICuttingAlgorithm
    {
        public CuttingResult OptimizeCutting(List<Part> parts, double materialLength, double minJointLength, HoleConfiguration holeConfig = null)
        {
            var result = new CuttingResult();

            if (parts == null || !parts.Any())
                return result;

            // 大数据集性能提示
            if (parts.Count > 3000)
            {
                Console.WriteLine($"检测到大数据集 ({parts.Count} 个部件)，正在使用优化算法处理...");
                Console.WriteLine("注意：大数据集可能需要更长时间处理，请耐心等待。");
            }
            
            var remainingParts = new List<Part>(parts);

            // 处理超长部件
            var oversizedParts = remainingParts.Where(p => p.Length > materialLength).ToList();
            foreach (var part in oversizedParts)
            {
                var oversized = CreateOversizedPart(part, materialLength);
                result.OversizedParts.Add(oversized);
                remainingParts.Remove(part);
            }

            // 根据功能类型和部件编号分组处理
            var groupedParts = remainingParts
                .GroupBy(p => new { p.FunctionDefinition, p.PartNumber })
                .OrderBy(g => g.Key.FunctionDefinition)
                .ThenBy(g => g.Key.PartNumber);

            foreach (var group in groupedParts)
            {
                var groupParts = group.ToList();
                var groupName = $"{group.Key.FunctionDefinition}({group.Key.PartNumber})";
                
                Console.WriteLine($"\n正在处理分组: {groupName} - 数量: {groupParts.Count}");

                List<CuttingPlan> groupPlans;

                // 导轨部件使用孔洞约束（如果有配置）
                if (group.Key.FunctionDefinition?.Contains("导轨") == true && holeConfig != null)
                {
                    groupPlans = SolveWithHoleConstraints(groupParts, materialLength, minJointLength, holeConfig);
                }
                else
                {
                    groupPlans = SolveWithOptimalStrategy(groupParts, materialLength, minJointLength);
                }

                // 为每个切割计划添加分组信息
                foreach (var plan in groupPlans)
                {
                    plan.GroupIdentifier = groupName;
                }

                result.CuttingPlans.AddRange(groupPlans);
            }

            // 计算统计信息
            CalculateStatistics(result, materialLength);

            return result;
        }

        private List<CuttingPlan> SolveWithOptimalStrategy(List<Part> parts, double materialLength, double minJointLength)
        {
            var plans = new List<CuttingPlan>();
            var remainingParts = new List<Part>(parts);
            int planNumber = 1;

            // 如果部件数量太多，使用简化算法
            bool useSimplifiedAlgorithm = parts.Count > 2000;
            if (useSimplifiedAlgorithm)
            {
                Console.WriteLine($"分组部件数量 ({parts.Count}) 较多，使用简化算法以提高性能...");
            }

            while (remainingParts.Any())
            {
                List<Part> bestCombination;
                
                if (useSimplifiedAlgorithm)
                {
                    bestCombination = FindSimplifiedCombination(remainingParts, materialLength);
                }
                else
                {
                    bestCombination = FindOptimalCombinationImproved(remainingParts, materialLength);
                }

                if (bestCombination.Any())
                {
                    var plan = CreateCuttingPlan(bestCombination, materialLength, minJointLength, planNumber);
                    plans.Add(plan);

                    foreach (var part in bestCombination)
                    {
                        remainingParts.Remove(part);
                    }

                    planNumber++;
                }
                else
                {
                    // 如果找不到组合，取最长的部件
                    var longestPart = remainingParts.OrderByDescending(p => p.Length).First();
                    var plan = CreateCuttingPlan(new List<Part> { longestPart }, materialLength, minJointLength, planNumber);
                    plans.Add(plan);
                    remainingParts.Remove(longestPart);
                    planNumber++;
                }
            }

            return plans;
        }

        private List<Part> FindOptimalCombinationImproved(List<Part> parts, double materialLength)
        {
            // 改进的最优组合算法：先大后小策略
            var selectedParts = new List<Part>();
            var availableParts = parts.OrderByDescending(p => p.Length).ToList(); // 按长度降序排列
            double remainingLength = materialLength;

            // 第一阶段：优先选择最大的部件
            var largestPart = availableParts.FirstOrDefault(p => p.Length <= remainingLength);
            if (largestPart != null)
            {
                selectedParts.Add(largestPart);
                remainingLength -= largestPart.Length;
                availableParts.Remove(largestPart);
            }

            // 第二阶段：使用动态规划优化剩余空间
            if (remainingLength > 0 && availableParts.Any())
            {
                var remainingCombination = FindBestFitCombination(availableParts, remainingLength);
                selectedParts.AddRange(remainingCombination);
            }

            return selectedParts;
        }

        private List<Part> FindBestFitCombination(List<Part> parts, double remainingLength)
        {
            // 对于大数据集，使用优化的贪心算法代替动态规划
            if (parts.Count > 1000 || remainingLength > 50000)
            {
                Console.WriteLine($"大数据集检测到 ({parts.Count} 个部件)，使用优化算法...");
                return FindBestFitCombinationOptimized(parts, remainingLength);
            }
            
            // 原有的动态规划算法（仅用于小数据集）
            return FindBestFitCombinationDP(parts, remainingLength);
        }

        private List<Part> FindBestFitCombinationOptimized(List<Part> parts, double remainingLength)
        {
            var selectedParts = new List<Part>();
            var availableParts = new List<Part>(parts);
            
            // 按长度降序排序，优先选择大部件
            availableParts.Sort((a, b) => b.Length.CompareTo(a.Length));
            
            double currentLength = 0;
            
            // 分阶段贪心选择
            while (availableParts.Any() && currentLength < remainingLength)
            {
                Part bestPart = null;
                double bestFit = 0;
                
                // 寻找最佳匹配的部件
                foreach (var part in availableParts)
                {
                    if (currentLength + part.Length <= remainingLength)
                    {
                        // 计算匹配度：越接近剩余空间越好
                        double remainingAfterThis = remainingLength - currentLength - part.Length;
                        double fitScore = part.Length / (remainingAfterThis + 1); // 避免除零
                        
                        if (fitScore > bestFit)
                        {
                            bestFit = fitScore;
                            bestPart = part;
                        }
                    }
                }
                
                if (bestPart != null)
                {
                    selectedParts.Add(bestPart);
                    currentLength += bestPart.Length;
                    availableParts.Remove(bestPart);
                }
                else
                {
                    break; // 没有合适的部件了
                }
            }
            
            // 如果还有剩余空间，尝试多部件组合
            if (currentLength < remainingLength && availableParts.Any())
            {
                var smallParts = availableParts.Where(p => p.Length <= remainingLength - currentLength).ToList();
                var combination = FindSmallPartsCombination(smallParts, remainingLength - currentLength);
                selectedParts.AddRange(combination);
            }
            
            return selectedParts;
        }

        private List<Part> FindSmallPartsCombination(List<Part> parts, double targetLength)
        {
            // 对小部件使用简化的组合算法，限制处理数量
            var selectedParts = new List<Part>();
            var availableParts = parts.Take(100).ToList(); // 限制处理100个部件以避免内存问题
            
            double currentLength = 0;
            
            // 按长度降序排序
            availableParts.Sort((a, b) => b.Length.CompareTo(a.Length));
            
            for (int i = 0; i < availableParts.Count && currentLength < targetLength; i++)
            {
                var part = availableParts[i];
                if (currentLength + part.Length <= targetLength)
                {
                    selectedParts.Add(part);
                    currentLength += part.Length;
                }
            }
            
            return selectedParts;
        }

        private List<Part> FindBestFitCombinationDP(List<Part> parts, double remainingLength)
        {
            // 原有的动态规划算法，但增加安全检查
            int n = parts.Count;
            int capacity = (int)(remainingLength * 100); // 降低精度从1000到100
            
            // 内存安全检查
            long memoryRequired = (long)(n + 1) * (capacity + 1) * 16; // 16字节per entry (double + bool)
            if (memoryRequired > 100_000_000) // 限制在100MB以内
            {
                Console.WriteLine($"内存需求过大 ({memoryRequired / 1_000_000}MB)，使用优化算法替代...");
                return FindBestFitCombinationOptimized(parts, remainingLength);
            }
            
            var dp = new double[n + 1, capacity + 1];
            var keep = new bool[n + 1, capacity + 1];
            
            // 填充DP表
            for (int i = 1; i <= n; i++)
            {
                var part = parts[i - 1];
                int weight = (int)(part.Length * 100); // 精度降低到100
                double value = part.Length;
                
                for (int w = 0; w <= capacity; w++)
                {
                    dp[i, w] = dp[i - 1, w];
                    
                    if (weight <= w)
                    {
                        double newValue = dp[i - 1, w - weight] + value;
                        if (newValue > dp[i, w])
                        {
                            dp[i, w] = newValue;
                            keep[i, w] = true;
                        }
                    }
                }
            }
            
            // 回溯找出选中的部件
            var selectedParts = new List<Part>();
            int currentCapacity = capacity;
            
            for (int i = n; i > 0 && currentCapacity > 0; i--)
            {
                if (keep[i, currentCapacity])
                {
                    selectedParts.Add(parts[i - 1]);
                    currentCapacity -= (int)(parts[i - 1].Length * 100);
                }
            }
            
            return selectedParts;
        }

        private List<CuttingPlan> SolveWithHoleConstraints(List<Part> parts, double materialLength, double minJointLength, HoleConfiguration holeConfig)
        {
            var plans = new List<CuttingPlan>();
            var remainingParts = new List<Part>(parts);
            int planNumber = 1;

            while (remainingParts.Any())
            {
                var bestCombination = FindHoleAwareCombinationImproved(remainingParts, materialLength, holeConfig);

                if (bestCombination.Any())
                {
                    var plan = CreateHoleAwareCuttingPlan(bestCombination, materialLength, minJointLength, holeConfig, planNumber);
                    plans.Add(plan);

                    foreach (var part in bestCombination)
                    {
                        remainingParts.Remove(part);
                    }

                    planNumber++;
                }
                else
                {
                    var longestPart = remainingParts.OrderByDescending(p => p.Length).First();
                    var plan = CreateHoleAwareCuttingPlan(new List<Part> { longestPart }, materialLength, minJointLength, holeConfig, planNumber);
                    plans.Add(plan);
                    remainingParts.Remove(longestPart);
                    planNumber++;
                }
            }

            return plans;
        }

        private List<Part> FindHoleAwareCombinationImproved(List<Part> parts, double materialLength, HoleConfiguration holeConfig)
        {
            // 改进的孔洞约束组合算法：先大后小策略
            var selectedParts = new List<Part>();
            var availableParts = parts.OrderByDescending(p => p.Length).ToList();
            double currentPosition = 0; // 修改：从材料开头开始，而不是从LeftMargin开始
            double maxEndPosition = materialLength - holeConfig.RightMargin;

            // 第一阶段：优先放置最长的部件（可以从开头开始）
            var firstPart = availableParts.FirstOrDefault(p => p.Length <= maxEndPosition);
            if (firstPart != null)
            {
                selectedParts.Add(firstPart);
                currentPosition = firstPart.Length; // 第一个部件直接从0开始
                availableParts.Remove(firstPart);
            }

            // 第二阶段：后续部件需要考虑孔洞约束
            foreach (var part in availableParts.ToList())
            {
                // 从第二个部件开始才需要考虑孔洞约束
                var validStart = holeConfig.CalculateNextValidStart(Math.Max(currentPosition, holeConfig.LeftMargin));

                if (validStart + part.Length <= maxEndPosition)
                {
                    selectedParts.Add(part);
                    currentPosition = validStart + part.Length;
                    availableParts.Remove(part);
                }
            }

            return selectedParts;
        }

        private CuttingPlan CreateCuttingPlan(List<Part> selectedParts, double materialLength, double minJointLength, int planNumber)
        {
            var plan = new CuttingPlan
            {
                PlanNumber = planNumber,
                MaterialId = $"M{planNumber:D3}",
                MaterialLength = materialLength
            };

            double currentPos = 0;
            int cutId = 1;

            // 重要改进：按照先大后小的顺序排列部件
            var sortedParts = selectedParts.OrderByDescending(p => p.Length).ToList();

            foreach (var part in sortedParts)
            {
                plan.CutParts.Add(new CutPart
                {
                    Id = cutId++,
                    PartId = part.Id,
                    PartName = part.Name,
                    Length = part.Length,
                    StartPosition = currentPos,
                    EndPosition = currentPos + part.Length,
                    DiagramPosition = part.DiagramPosition,
                    LayoutSpace = part.LayoutSpace,
                    MountingBoard = part.MountingBoard,
                    DeviceIdentifier = part.DeviceIdentifier,
                    PartNumber = part.PartNumber,
                    FunctionDefinition = part.FunctionDefinition
                });

                currentPos += part.Length;
            }

            plan.UsedLength = plan.CutParts.Sum(p => p.Length);
            plan.RemainingLength = materialLength - plan.UsedLength;
            plan.Utilization = plan.UsedLength / materialLength;
            plan.WastedLength = plan.RemainingLength;

            return plan;
        }

        private CuttingPlan CreateHoleAwareCuttingPlan(List<Part> selectedParts, double materialLength, double minJointLength, HoleConfiguration holeConfig, int planNumber)
        {
            var plan = new CuttingPlan
            {
                PlanNumber = planNumber,
                MaterialId = $"M{planNumber:D3}",
                MaterialLength = materialLength,
                HoleConfig = holeConfig,
                ValidStartPosition = 0, // 修改：第一个部件可以从0开始
                ValidEndPosition = materialLength - holeConfig.RightMargin
            };

            double currentPos = 0; // 修改：从0开始
            int cutId = 1;
            bool isFirstPart = true;

            // 按照先大后小的顺序排列部件
            var sortedParts = selectedParts.OrderByDescending(p => p.Length).ToList();

            foreach (var part in sortedParts)
            {
                double validStart;

                if (isFirstPart)
                {
                    // 第一个部件可以直接从材料开头开始
                    validStart = 0;
                    isFirstPart = false;
                }
                else
                {
                    // 后续部件需要考虑孔洞约束
                    validStart = holeConfig.CalculateNextValidStart(Math.Max(currentPos, holeConfig.LeftMargin));
                }

                plan.CutParts.Add(new CutPart
                {
                    Id = cutId++,
                    PartId = part.Id,
                    PartName = part.Name,
                    Length = part.Length,
                    StartPosition = validStart,
                    EndPosition = validStart + part.Length,
                    DiagramPosition = part.DiagramPosition,
                    LayoutSpace = part.LayoutSpace,
                    MountingBoard = part.MountingBoard,
                    DeviceIdentifier = part.DeviceIdentifier,
                    PartNumber = part.PartNumber,
                    FunctionDefinition = part.FunctionDefinition
                });

                currentPos = validStart + part.Length;
            }

            plan.UsedLength = plan.CutParts.Sum(p => p.Length);
            plan.RemainingLength = materialLength - (plan.CutParts.LastOrDefault()?.EndPosition ?? 0);
            plan.Utilization = plan.UsedLength / materialLength;
            plan.WastedLength = plan.RemainingLength;

            // 计算孔洞约束浪费：只计算第二个部件开始的孔洞间隙
            double holeWaste = 0;
            if (plan.CutParts.Count > 1)
            {
                // 第一个部件到第二个部件之间的间隙（如果第二个部件需要从孔洞位置开始）
                var firstPart = plan.CutParts.First();
                var secondPart = plan.CutParts.Skip(1).First();

                if (secondPart.StartPosition > firstPart.EndPosition)
                    holeWaste += secondPart.StartPosition - firstPart.EndPosition;

                // 后续部件间的孔洞间隙
                for (int i = 2; i < plan.CutParts.Count; i++)
                {
                    double gap = plan.CutParts[i].StartPosition - plan.CutParts[i - 1].EndPosition;
                    if (gap > 0)
                        holeWaste += gap;
                }
            }

            plan.HoleConstraintWaste = holeWaste;

            return plan;
        }

        private OversizedPart CreateOversizedPart(Part part, double materialLength)
        {
            var segments = new List<PartSegment>();
            double remainingLength = part.Length;
            int segmentIndex = 1;

            while (remainingLength > 0)
            {
                double segmentLength = Math.Min(remainingLength, materialLength);
                segments.Add(new PartSegment
                {
                    SegmentIndex = segmentIndex++,
                    Length = segmentLength,
                    Name = $"{part.Name}_段{segmentIndex - 1}"
                });
                remainingLength -= segmentLength;
            }

            return new OversizedPart
            {
                OriginalPart = part,
                TotalSegments = segments.Count,
                JointCount = segments.Count - 1,
                SpliceCount = segments.Count - 1,
                DeviceIdentifier = part.DeviceIdentifier,
                PartNumber = part.PartNumber,
                Segments = segments
            };
        }

        private void CalculateStatistics(CuttingResult result, double materialLength)
        {
            result.MaterialsUsed = result.CuttingPlans.Count;
            result.TotalUsedLength = result.CuttingPlans.Sum(p => p.UsedLength);

            // 修正：计算所有类型的浪费
            result.TotalWaste = result.CuttingPlans.Sum(p => p.RemainingLength); // 所有剩余长度
            result.TotalHoleWaste = result.CuttingPlans.Sum(p => p.HoleConstraintWaste); // 孔洞约束浪费

            // 验证数学一致性
            double totalMaterialLength = result.MaterialsUsed * materialLength;
            double calculatedTotal = result.TotalUsedLength + result.TotalWaste + result.TotalHoleWaste;

            // 如果不一致，记录警告
            if (Math.Abs(totalMaterialLength - calculatedTotal) > 0.1 )
                result.Warnings.Add($"统计计算不一致：材料总长度{totalMaterialLength}mm，计算总和{calculatedTotal}mm，差值{totalMaterialLength - calculatedTotal}mm");
            

            // 正确的利用率计算
            result.MaterialUtilization = totalMaterialLength > 0 ? result.TotalUsedLength / totalMaterialLength : 0;

            // 添加详细统计信息
            result.Suggestions.Add($"材料总长度: {totalMaterialLength}mm");
            result.Suggestions.Add($"实际使用: {result.TotalUsedLength}mm");
            result.Suggestions.Add($"普通浪费: {result.TotalWaste}mm");
            result.Suggestions.Add($"孔洞浪费: {result.TotalHoleWaste}mm");
            result.Suggestions.Add($"总计: {result.TotalUsedLength + result.TotalWaste + result.TotalHoleWaste}mm");
        }

        private List<Part> FindSimplifiedCombination(List<Part> parts, double materialLength)
        {
            // 简化的贪心算法，用于大数据集
            var selectedParts = new List<Part>();
            var availableParts = parts.OrderByDescending(p => p.Length).ToList();
            double currentLength = 0;

            // 简单的首次适应算法，优先选择大部件
            foreach (var part in availableParts)
            {
                if (currentLength + part.Length <= materialLength)
                {
                    selectedParts.Add(part);
                    currentLength += part.Length;
                }
            }

            return selectedParts;
        }
    }
}