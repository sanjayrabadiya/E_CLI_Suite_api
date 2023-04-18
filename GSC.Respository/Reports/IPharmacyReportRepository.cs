using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Report;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Respository.Reports
{
    public interface IPharmacyReportRepository : IGenericRepository<JobMonitoring>
    {
        FileStreamResult GetRandomizationKitReport(RandomizationIWRSReport randomizationIWRSReport);

        FileStreamResult GetProductAccountabilityCentralReport(ProductAccountabilityCentralReportSearch randomizationIWRSReport);
    }
}