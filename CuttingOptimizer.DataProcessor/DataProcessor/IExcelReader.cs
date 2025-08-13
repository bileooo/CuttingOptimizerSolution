using System.Collections.Generic;
using CuttingOptimizer.Models;

namespace CuttingOptimizer.DataProcessor
{
    public interface IExcelReader
    {
        List<Part> ReadExcelFile(string filePath);
    }
}