using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.CTMS
{
    public interface IWeekEndMasterRepository : IGenericRepository<WeekEndMaster>
    {
        List<string> GetworkingDayList(int ProjectId);
        List<string> GetweekEndDay(int ProjectId);
    }
}
