namespace CuttingOptimizer.Models
{
    public class CutPart
    {
        public int Id { get; set; }
        public string PartId { get; set; } = "";
        public string PartName { get; set; } = "";
        public double Length { get; set; }
        public double StartPosition { get; set; }
        public double EndPosition { get; set; }
        public string DiagramPosition { get; set; } = "";
        public string LayoutSpace { get; set; } = "";
        public string MountingBoard { get; set; } = "";
        public string DeviceIdentifier { get; set; } = "";
        public string PartNumber { get; set; } = "";
        public string FunctionDefinition { get; set; } = "";
    }
}