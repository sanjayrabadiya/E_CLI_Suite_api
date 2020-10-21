using System.Collections.Generic;
using GSC.Data.Dto.Attendance;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IDataEntryRespository
    {

        DataCaptureGridDto GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int parentProjectId, int projectId);
        List<DataEntryVisitTemplateDto> GetTemplateForVisit(int screeningEntryId, int projectDesignVisitId, int screeningStatus, bool isQuery);

    }
}