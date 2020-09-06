using System.Collections.Generic;
using GSC.Data.Dto.Attendance;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IDataEntryRespository
    {
        IList<DataEntryDto> GetDataEntriesBySubject(int projectDesignPeriodId, int projectId);

        List<DataEntryVisitSummaryDto> GetVisitForDataEntry(int attendanceId, int screeningEntryId);

        List<DataEntryVisitTemplateDto> GetTemplateForVisit(int screeningEntryId, int projectDesignVisitId,
            ScreeningTemplateStatus screeningStatus, bool isQuery);

    }
}