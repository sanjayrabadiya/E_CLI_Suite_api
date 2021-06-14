using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Report
{
    public interface IVolunteerSummaryReport
    {
        FileStreamResult GetVolunteerSummaryDesign(int VoluteerID);
    }
}
