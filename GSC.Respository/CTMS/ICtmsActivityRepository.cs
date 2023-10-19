using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ICtmsActivityRepository : IGenericRepository<CtmsActivity>
    {
        List<DropDownDto> GetCtmsActivityList();
    }
}