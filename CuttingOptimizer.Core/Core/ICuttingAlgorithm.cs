using System.Collections.Generic;
using CuttingOptimizer.Models;

namespace CuttingOptimizer.Core
{
    /// <summary>
    /// 切割算法接口
    /// </summary>
    public interface ICuttingAlgorithm
    {
        CuttingResult OptimizeCutting(List<Part> parts, double materialLength, double minJointLength, HoleConfiguration holeConfig = null);
    }
}