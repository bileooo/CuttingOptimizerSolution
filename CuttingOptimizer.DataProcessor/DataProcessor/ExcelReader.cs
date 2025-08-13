using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CuttingOptimizer.Models;
using OfficeOpenXml;

namespace CuttingOptimizer.DataProcessor
{
    public class ExcelReader : IExcelReader
    {
        // 列名映射字典 - 支持多种可能的列名
        private readonly Dictionary<string, string[]> columnMappings = new Dictionary<string, string[]>
        {
            ["SerialNumber"] = new[] {"序号"},
            ["DiagramPosition"] = new[] {"图例位置"},
            ["HighLevelCode"] = new[] {"高层代号"},
            ["PositionCode"] = new[] {"位置代号"},
            ["LayoutSpace"] = new[] {"布局空间"},
            ["MountingBoard"] = new[] {"安装版"},
            ["FunctionDefinition"] = new[] {"功能定义"},
            ["DeviceIdentifier"] = new[] {"设备标识符"},
            ["PartNumber"] = new[] {"部件编号"},
            ["Height"] = new[] {"高度"},
            ["Width"] = new[] {"宽度"},
            ["Depth"] = new[] {"深度"},
            ["Length"] = new[] {"长度"}
        };

        public List<Part> ReadExcelFile(string filePath)
        {
            var parts = new List<Part>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;
                var colCount = worksheet.Dimension?.Columns ?? 0;

                if (rowCount < 2)
                {
                    throw new InvalidOperationException("Excel文件至少需要包含标题行和一行数据");
                }

                // 读取表头并创建列映射
                var columnIndexes = BuildColumnIndexMapping(worksheet, colCount);

                Console.WriteLine("\n=== 表头识别结果 ===");
                foreach (var mapping in columnIndexes)
                {
                    Console.WriteLine($"{mapping.Key}: 第{mapping.Value}列");
                }

                // 检查必需的列
                if (!columnIndexes.ContainsKey("Length"))
                {
                    throw new InvalidOperationException("Excel文件必须包含长度列（长度/长/Length/L）");
                }

                if (!columnIndexes.ContainsKey("PartNumber"))
                {
                    throw new InvalidOperationException("Excel文件必须包含部件编号列（部件编号/部件号/零件号/Part/编号/型号/规格）");
                }

                // 读取数据行
                int autoSerialNumber = 1;
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        // 检查是否为空行
                        bool isEmptyRow = true;
                        for (int col = 1; col <= colCount; col++)
                        {
                            if (worksheet.Cells[row, col].Value != null &&
                                !string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Value.ToString()))
                            {
                                isEmptyRow = false;
                                break;
                            }
                        }
                        if (isEmptyRow) continue;

                        // 读取各字段值
                        var serialNumber = columnIndexes.ContainsKey("SerialNumber")
                            ? ParseInt(worksheet.Cells[row, columnIndexes["SerialNumber"]].Value)
                            : autoSerialNumber++;

                        var diagramPosition = GetCellValue(worksheet, row, columnIndexes, "DiagramPosition");
                        var highLevelCode = GetCellValue(worksheet, row, columnIndexes, "HighLevelCode");
                        var positionCode = GetCellValue(worksheet, row, columnIndexes, "PositionCode");
                        var layoutSpace = GetCellValue(worksheet, row, columnIndexes, "LayoutSpace");
                        var mountingBoard = GetCellValue(worksheet, row, columnIndexes, "MountingBoard");
                        var functionDefinition = GetCellValue(worksheet, row, columnIndexes, "FunctionDefinition");
                        var deviceIdentifier = GetCellValue(worksheet, row, columnIndexes, "DeviceIdentifier");
                        var partNumber = GetCellValue(worksheet, row, columnIndexes, "PartNumber");
                        var height = GetCellLength(worksheet, row, columnIndexes, "Height");
                        var width = GetCellLength(worksheet, row, columnIndexes, "Width");
                        var depth = GetCellLength(worksheet, row, columnIndexes, "Depth");
                        var length = GetCellLength(worksheet, row, columnIndexes, "Length");

                        // 验证必要字段
                        if (length <= 0)
                        {
                            Console.WriteLine($"第{row}行: 长度无效 ({length})，跳过此行");
                            continue;
                        }

                        if (string.IsNullOrEmpty(partNumber))
                        {
                            Console.WriteLine($"第{row}行: 部件编号为空，跳过此行");
                            continue;
                        }

                        // 自动判断功能定义
                        if (string.IsNullOrEmpty(functionDefinition))
                        {
                            if (partNumber.Contains("U") && (partNumber.Contains("787") || partNumber.Contains("166")))
                            {
                                functionDefinition = "安装导轨";
                            }
                            else
                            {
                                functionDefinition = "线槽";
                            }
                        }

                        var part = new Part
                        {
                            Id = $"P{serialNumber:D4}",
                            Name = $"{functionDefinition}({partNumber})",
                            Length = length,
                            Cabinet = $"{highLevelCode}-{positionCode}",
                            SerialNumber = serialNumber,
                            DiagramPosition = diagramPosition,
                            HighLevelCode = highLevelCode,
                            PositionCode = positionCode,
                            LayoutSpace = layoutSpace,
                            MountingBoard = mountingBoard,
                            FunctionDefinition = functionDefinition,
                            DeviceIdentifier = deviceIdentifier,
                            PartNumber = partNumber,
                            Height = height,
                            Width = width,
                            Depth = depth
                        };

                        parts.Add(part);
                        Console.WriteLine($"导入部件: {part.Name} - 长度: {part.Length}mm - 功能: {part.FunctionDefinition}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"第{row}行数据解析失败: {ex.Message}");
                    }
                }
            }

            return parts;
        }

        private Dictionary<string, int> BuildColumnIndexMapping(ExcelWorksheet worksheet, int colCount)
        {
            var columnIndexes = new Dictionary<string, int>();

            // 读取表头行
            for (int col = 1; col <= colCount; col++)
            {
                var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                if (string.IsNullOrEmpty(headerValue)) continue;

                // 尝试匹配每个字段
                foreach (var mapping in columnMappings)
                {
                    var fieldName = mapping.Key;
                    var possibleNames = mapping.Value;

                    if (possibleNames.Any(name =>
                        string.Equals(headerValue, name, StringComparison.OrdinalIgnoreCase) ||
                        headerValue.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                        name.Contains(headerValue)))
                    {
                        if (!columnIndexes.ContainsKey(fieldName))
                        {
                            columnIndexes[fieldName] = col;
                            Console.WriteLine($"识别列: {headerValue} -> {fieldName}");
                            break;
                        }
                    }
                }
            }

            return columnIndexes;
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnIndexes, string fieldName)
        {
            if (!columnIndexes.ContainsKey(fieldName))
                return "";

            return worksheet.Cells[row, columnIndexes[fieldName]].Value?.ToString()?.Trim() ?? "";
        }

        private double GetCellLength(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnIndexes, string fieldName)
        {
            if (!columnIndexes.ContainsKey(fieldName))
                return 0;

            return ParseLength(worksheet.Cells[row, columnIndexes[fieldName]].Value);
        }

        private int ParseInt(object value)
        {
            if (value == null) return 0;
            if (int.TryParse(value.ToString(), out int result)) return result;
            if (double.TryParse(value.ToString(), out double doubleResult)) return (int)doubleResult;
            return 0;
        }

        private double ParseLength(object value)
        {
            if (value == null) return 0;
            if (double.TryParse(value.ToString(), out double result)) return result;
            return 0;
        }
    }
}