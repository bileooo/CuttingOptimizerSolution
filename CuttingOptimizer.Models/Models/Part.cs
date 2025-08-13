using System;

namespace CuttingOptimizer.Models
{
    /// <summary>
    /// 部件模型
    /// </summary>
    public class Part
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public double Length { get; set; }
        public string Cabinet { get; set; } = "";
        public int SerialNumber { get; set; }
        public string DiagramPosition { get; set; } = "";
        public string HighLevelCode { get; set; } = "";
        public string PositionCode { get; set; } = "";
        public string LayoutSpace { get; set; } = "";
        public string MountingBoard { get; set; } = "";
        public string FunctionDefinition { get; set; } = "";
        public string DeviceIdentifier { get; set; } = "";
        public string PartNumber { get; set; } = "";
        public double Height { get; set; }
        public double Width { get; set; }
        public double Depth { get; set; }
    }
}