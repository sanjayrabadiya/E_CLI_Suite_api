using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.CTMS
{
   public interface IHolidayMasterRepository : IGenericRepository<HolidayMaster>
    {
        List<HolidayMasterGridDto> GetHolidayList(bool isDeleted);
        List<DateTime> GetHolidayList(int projectId);
    }
}
