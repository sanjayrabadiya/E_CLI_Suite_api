using System.Collections.Generic;
using GSC.Data.Dto.Attendance;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IDataEntryRespository
    {

        IList<DataCaptureGridDto> GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int projectId);

    }
}