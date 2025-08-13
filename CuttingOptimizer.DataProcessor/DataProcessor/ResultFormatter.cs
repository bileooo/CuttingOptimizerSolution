using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CuttingOptimizer.Models;

namespace CuttingOptimizer.DataProcessor
{
    public class ResultFormatter : IResultFormatter
    {
        public void SaveResults(List<CuttingResult> allResults, double materialLength, double minJointLength)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"cutting_result_{timestamp}.json";
                var json = FormatResultsAsJson(allResults, materialLength, minJointLength);
                File.WriteAllText(fileName, json);
                Console.WriteLine($"\n结果已保存到: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存结果失败: {ex.Message}");
            }
        }

        public string FormatResultsAsJson(List<CuttingResult> allResults, double materialLength, double minJointLength)
        {
            var summary = new
            {
                Timestamp = DateTime.Now,
                MaterialLength = materialLength,
                MinJointLength = minJointLength,
                Results = allResults.Select(r => new
                {
                    FunctionType = r.FunctionType,
                    MaterialsUsed = r.MaterialsUsed,
                    TotalUsedLength = Math.Round(r.TotalUsedLength, 0),
                    MaterialUtilization = Math.Round(r.MaterialUtilization, 4),
                    TotalWaste = Math.Round(r.TotalWaste, 0),
                    TotalHoleWaste = Math.Round(r.TotalHoleWaste, 0),
                    CuttingPlans = r.CuttingPlans.Select(p => new
                    {
                        p.MaterialId,
                        p.MaterialLength,
                        p.GroupIdentifier,
                        UsedLength = Math.Round(p.UsedLength, 0),
                        RemainingLength = Math.Round(p.RemainingLength, 0),
                        Utilization = Math.Round(p.Utilization, 4),
                        p.IsRemainingMaterial,
                        HoleConstraintWaste = Math.Round(p.HoleConstraintWaste, 0),
                        CutParts = p.CutParts.Select(c => new
                        {
                            c.PartName,
                            Length = Math.Round(c.Length, 0),
                            c.DiagramPosition,
                            c.LayoutSpace,
                            c.MountingBoard,
                            c.DeviceIdentifier,
                            c.PartNumber,
                            c.FunctionDefinition,
                            StartPosition = Math.Round(c.StartPosition, 0),
                            EndPosition = Math.Round(c.EndPosition, 0)
                        })
                    }),
                    OversizedParts = r.OversizedParts.Select(o => new
                    {
                        OriginalPartName = o.OriginalPart.Name,
                        OriginalLength = Math.Round(o.OriginalPart.Length, 0),
                        o.TotalSegments,
                        o.SpliceCount,
                        o.DeviceIdentifier,
                        o.PartNumber,
                        Segments = o.Segments.Select(s => new
                        {
                            s.SegmentIndex,
                            Length = Math.Round(s.Length, 0),
                            s.Name
                        })
                    }),
                    r.Warnings,
                    r.Suggestions
                }),
                OverallSummary = new
                {
                    TotalMaterialsUsed = allResults.Sum(r => r.MaterialsUsed),
                    TotalUsedLength = Math.Round(allResults.Sum(r => r.TotalUsedLength), 0),
                    OverallUtilization = allResults.Sum(r => r.MaterialsUsed * materialLength) > 0 ?
                        Math.Round(allResults.Sum(r => r.TotalUsedLength) / allResults.Sum(r => r.MaterialsUsed * materialLength), 4) : 0,
                    TotalWaste = Math.Round(allResults.Sum(r => r.TotalWaste), 0),
                    TotalHoleWaste = Math.Round(allResults.Sum(r => r.TotalHoleWaste), 0)
                }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return JsonSerializer.Serialize(summary, options);
        }
    }
}