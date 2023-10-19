using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
   public interface IWorkingDayRepository : IGenericRepository<WorkingDay>
    {
        List<WorkingDayListDto> GetWorkingDayList(bool isDeleted);
        void AddSiteType(WorkingDayDto workingDayListDto);
    }
}
