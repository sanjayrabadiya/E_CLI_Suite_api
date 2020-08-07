using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Custom;
using GSC.Data.Entities.Custom;
using GSC.Data.Entities.Report;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Report
{
    public interface IGscReport
    {
        FileStreamResult GetProjectDesign(int id);
        FileStreamResult GetProjectDesignWithFliter(ReportSettingNew reportSettingNew, CompanyDataDto companyData, JobMonitoring jobMonitoring);
    }
}