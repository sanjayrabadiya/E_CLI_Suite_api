using GSC.Data.Dto.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Report
{
    public interface IGscReport
    {
        FileStreamResult GetProjectDesign(int id);
        FileStreamResult GetProjectDesignWithFliter(ReportSettingNew reportSetting, CompanyData companyData);
    }
}