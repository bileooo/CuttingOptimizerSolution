using System.Collections.Generic;
using CuttingOptimizer.Models;

namespace CuttingOptimizer.DataProcessor
{
    public interface IResultFormatter
    {
        void SaveResults(List<CuttingResult> allResults, double materialLength, double minJointLength);
        string FormatResultsAsJson(List<CuttingResult> allResults, double materialLength, double minJointLength);
    }
}