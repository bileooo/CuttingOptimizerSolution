using System;
using OfficeOpenXml;

namespace CuttingOptimizer.IOHelper
{
    public static class LicenseHelper
    {
        public static void SetEPPlusLicense()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                Console.WriteLine("EPPlus 许可证设置成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EPPlus 许可证设置失败: {ex.Message}");
            }
        }
    }
}