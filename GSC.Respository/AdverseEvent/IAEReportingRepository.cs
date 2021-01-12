using GSC.Common.GenericRespository;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Entities.AdverseEvent;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.AdverseEvent
{
    public interface IAEReportingRepository : IGenericRepository<AEReporting>
    {
        List<AEReportingDto> GetAEReportingList();
    }
}
