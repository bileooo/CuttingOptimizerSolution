using System.Collections.Generic;

namespace CuttingOptimizer.Models
{
    // PartSegment 类已在 OversizedPart.cs 中定义，此处移除重复定义

    // 如果需要其他优化相关的模型类，可以在这里添加
    public class RemainingMaterial
    {
        public double Length { get; set; }
        public string MaterialId { get; set; } = "";
        public bool IsUsable { get; set; }
    }

    public class OptimizationState
    {
        public List<Part> RemainingParts { get; set; } = new List<Part>();
        public List<CuttingPlan> CompletedPlans { get; set; } = new List<CuttingPlan>();
        public double TotalWaste { get; set; }
        public double TotalUtilization { get; set; }
    }
}