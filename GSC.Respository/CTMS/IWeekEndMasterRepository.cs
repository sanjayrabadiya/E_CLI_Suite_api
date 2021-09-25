using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
using System.Text;
using static GSC.Common.WorkingDayHelper;

namespace GSC.Respository.CTMS
{
    public interface IWeekEndMasterRepository : IGenericRepository<WeekEndMaster>
    {
        List<WeekEndGridDto> GetWeekendList(bool isDeleted);
        List<WeekendData> GetWorkingDayList(int ProjectId);
        List<string> GetWeekEndDay(int ProjectId);
    }
}
