using System.Collections.Generic;
using GSC.Data.Dto.Attendance;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IDataEntryRespository
    {

        DataCaptureGridDto GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int parentProjectId, int projectId);

    }
}