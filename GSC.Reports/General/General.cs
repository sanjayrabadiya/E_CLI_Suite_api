using GSC.Reports.Reports.ProjectDesign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Reports.General
{
    public static class General
    {
        public static string GetReportAssemblyName(Reports report)
        {
            switch (report)
            {
                case Reports.ProjectDesign:
                    return typeof(ProjectDesign).AssemblyQualifiedName;
                default:
                    return "";
            }
        }
    }

    public enum Reports
    {
        ProjectDesign = 1
    }
}
