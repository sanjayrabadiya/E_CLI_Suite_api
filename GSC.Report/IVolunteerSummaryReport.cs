using GSC.Data.Dto.Volunteer;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Report
{
    public interface IVolunteerSummaryReport
    {
        FileStreamResult GetVolunteerSummaryDesign(int VoluteerID);
        FileStreamResult GetVolunteerSearchDesign(IList<VolunteerGridDto> volunteerGridDto);
    }
}
