using System.Collections.Generic;
using System.Threading.Tasks;
using GSC.Data.Dto.Attendance;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IDataEntryRespository
    {

        Task<DataCaptureGridDto> GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int parentProjectId, int projectId);
        List<DataEntryTemplateCountDisplayTree> GetTemplateForVisit(int screeningVisitId, ScreeningTemplateStatus templateStatus);
        List<DataEntryTemplateCountDisplayDto> GetTemplateVisitQuery(int screeningVisitId, QueryStatus queryStatus);
        List<DataEntryTemplateCountDisplayDto> GetTemplateVisitMyQuery(int screeningVisitId, short myLevel);
        List<DataEntryTemplateCountDisplayDto> GetTemplateVisitWorkFlow(int screeningVisitId, short reviewLevel);
    }
}