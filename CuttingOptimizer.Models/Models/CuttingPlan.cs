using System.Collections.Generic;

namespace CuttingOptimizer.Models
{
    public class CuttingPlan
    {
        public int PlanNumber { get; set; }
        public string MaterialId { get; set; } = "";
        public double MaterialLength { get; set; }
        public List<CutPart> CutParts { get; set; } = new List<CutPart>();
        public double UsedLength { get; set; }
        public double RemainingLength { get; set; }
        public double Utilization { get; set; }
        public double WastedLength { get; set; }
        public bool IsRemainingMaterial { get; set; }
        public HoleConfiguration HoleConfig { get; set; }
        public double ValidStartPosition { get; set; }
        public double ValidEndPosition { get; set; }
        public double HoleConstraintWaste { get; set; }
        public string GroupIdentifier { get; set; } = "";
    }
}