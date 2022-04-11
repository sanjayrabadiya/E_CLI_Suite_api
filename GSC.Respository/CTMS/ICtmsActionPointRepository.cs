using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ICtmsActionPointRepository : IGenericRepository<CtmsActionPoint>
    {
        List<CtmsActionPointGridDto> GetActionPointList(int CtmsMonitoringId);
    }
}