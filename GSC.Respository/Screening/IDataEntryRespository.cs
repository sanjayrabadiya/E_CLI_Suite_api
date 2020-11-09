using System.Collections.Generic;
using System.Threading.Tasks;
using GSC.Data.Dto.Attendance;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IDataEntryRespository
    {

        Task<DataCaptureGridDto> GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int parentProjectId, int projectId);
        List<DataEntryTemplateCountDisplayDto> GetTemplateForVisit(int screeningVisitId, ScreeningTemplateStatus templateStatus);

    }
}