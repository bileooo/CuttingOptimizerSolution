using System.Collections.Generic;

namespace CuttingOptimizer.Models
{
    public class CuttingResult
    {
        public List<CuttingPlan> CuttingPlans { get; set; } = new List<CuttingPlan>();
        public List<OversizedPart> OversizedParts { get; set; } = new List<OversizedPart>();
        public int MaterialsUsed { get; set; }
        public double TotalUsedLength { get; set; }
        public double MaterialUtilization { get; set; }
        public double TotalWaste { get; set; }
        public double TotalHoleWaste { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Suggestions { get; set; } = new List<string>();
        public string FunctionType { get; set; } = "";
    }
}