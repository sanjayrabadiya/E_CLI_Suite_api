using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.CTMS
{
    public interface IWeekEndMasterRepository : IGenericRepository<WeekEndMaster>
    {
        List<WeekEndGridDto> GetWeekendList(bool isDeleted);
        List<string> GetworkingDayList(int ProjectId);
        List<string> GetweekEndDay(int ProjectId);
    }
}
