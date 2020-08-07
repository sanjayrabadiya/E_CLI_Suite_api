using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Custom;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignReportSettingRepository :  IGenericRepository<ProjectDesignReportSetting>
    {
        List<CompanyDataDto> GetProjectDesignWithFliter(ReportSettingNew reportSetting);
    }
}
