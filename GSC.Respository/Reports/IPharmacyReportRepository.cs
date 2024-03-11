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
        FileStreamResult GetRandomizationKitReport(RandomizationIwrsReport randomizationIWRSReport);

        List<RandomizationIwrsReportData> GetRandomizationKitReportData(RandomizationIwrsReport randomizationIWRSReport);

        FileStreamResult GetProductAccountabilityCentralReport(ProductAccountabilityCentralReportSearch randomizationIWRSReport);

        FileStreamResult GetProductAccountabilitySiteReport(ProductAccountabilityCentralReportSearch randomizationIWRSReport);

        FileStreamResult GetProductShipmentReport(ProductAccountabilityCentralReportSearch randomizationIWRSReport);

        List<DropDownDto> GetPharmacyStudyProductTypeDropDownPharmacyReport(int ProjectId);

        List<ProductAccountabilityCentralReport> GetProductShipmentReportData(ProductAccountabilityCentralReportSearch randomizationIWRSReport);

        List<DropDownDto> GetPatientforKitHistoryReport(int projectid);

        List<DropDownDto> GetKitlistforReport(int projectid);

        List<ProductAccountabilityCentralReport> GetKitHistoryReport(KitHistoryReportSearchModel randomizationIWRSReport);

        FileStreamResult GetKitHistoryReportExcelToExcel(KitHistoryReportSearchModel randomizationIWRSReport);
    }
}