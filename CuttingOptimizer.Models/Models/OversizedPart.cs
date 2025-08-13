using System.Collections.Generic;

namespace CuttingOptimizer.Models
{
    public class OversizedPart
    {
        public Part OriginalPart { get; set; } = new Part();
        public int TotalSegments { get; set; }
        public int JointCount { get; set; }
        public int SpliceCount { get; set; }
        public string DeviceIdentifier { get; set; } = "";
        public string PartNumber { get; set; } = "";
        public List<PartSegment> Segments { get; set; } = new List<PartSegment>();
    }

    public class PartSegment
    {
        public int SegmentIndex { get; set; }
        public double Length { get; set; }
        public string Name { get; set; } = "";
    }
}