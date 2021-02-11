using System.Collections.Generic;
using System.Threading.Tasks;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.ProjectRight;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IDataEntryRespository
    {

        Task<DataCaptureGridDto> GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int parentProjectId, int projectId);
        List<DataEntryTemplateCountDisplayTree> GetTemplateForVisit(int screeningVisitId, ScreeningTemplateStatus templateStatus);
        List<DataEntryTemplateCountDisplayTree> GetTemplateVisitQuery(int screeningVisitId, QueryStatus queryStatus);
        List<DataEntryTemplateCountDisplayTree> GetTemplateVisitMyQuery(int screeningVisitId, int parentProjectId);
        List<DataEntryTemplateCountDisplayTree> GetTemplateVisitWorkFlow(int screeningVisitId, short reviewLevel);
        // Dashboard chart for data entry status
        List<DashboardQueryStatusDto> GetDataEntriesStatus(int projectId);

    }
}