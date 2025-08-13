using System.Collections.Generic;
using CuttingOptimizer.Models;

namespace CuttingOptimizer.IOHelper
{
    public interface IInputHelper
    {
        double GetMaterialLength();
        double GetMinJointLength();
        HoleConfiguration GetHoleConfiguration(double materialLength);
        List<Part> ImportExcelFile();
        void DisplayResults(CuttingResult result, string functionType);
    }
}