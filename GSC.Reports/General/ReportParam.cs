using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Reports.General
{
    public class ReportParam
    {
        public Reports Report { get; set; }
        public int RecordId { get; set; }
    }

    public interface IReport
    {
        bool HasData { get; set; }
    }
}
