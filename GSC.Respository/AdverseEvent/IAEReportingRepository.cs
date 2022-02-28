using GSC.Common.GenericRespository;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.AdverseEvent;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.AdverseEvent
{
    public interface IAEReportingRepository : IGenericRepository<AEReporting>
    {
        List<AEReportingDto> GetAEReportingList();
        List<AEReportingGridDto> GetAEReportingGridData(int projectId);
        DesignScreeningTemplateDto GetAEReportingForm();
        AEReportingDto GetAEReportingFilledForm(int id);
        ScreeningDetailsforAE GetScreeningDetailsforAE(int id);
        List<DashboardDto> GetAEReportingMyTaskList(int ProjectId);
    }
}
