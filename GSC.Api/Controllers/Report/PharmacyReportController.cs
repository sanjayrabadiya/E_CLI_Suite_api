using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Configuration;
using GSC.Respository.Reports;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class PharmacyReportController : BaseController
    {

       
        private readonly IPharmacyReportRepository _pharmacyReportRepository;
      
        public PharmacyReportController(IPharmacyReportRepository pharmacyReportRepository)
        {
          
            _pharmacyReportRepository = pharmacyReportRepository;
        }
        [HttpPost]
        [Route("GetRandomizationKitReport")]
        public IActionResult GetRandomizationKitReport([FromBody] RandomizationIwrsReport search)
        {
            return _pharmacyReportRepository.GetRandomizationKitReport(search);
        }
        [HttpPost]
        [Route("GetRandomizationKitReportData")]
        public IActionResult GetRandomizationKitReportData([FromBody] RandomizationIwrsReport search)
        {
            return Ok(_pharmacyReportRepository.GetRandomizationKitReportData(search));
        }

        [HttpPost]
        [Route("GetProductAccountabilityCentralReport")]
        public IActionResult GetProductAccountabilityCentralReport([FromBody] ProductAccountabilityCentralReportSearch search)
        {
            return _pharmacyReportRepository.GetProductAccountabilityCentralReport(search);
        }
        [HttpPost]
        [Route("GetProductAccountabilitySiteReport")]
        public IActionResult GetProductAccountabilitySiteReport([FromBody] ProductAccountabilityCentralReportSearch search)
        {
            return _pharmacyReportRepository.GetProductAccountabilitySiteReport(search);
        }
        [HttpPost]
        [Route("GetProductShipmentReport")]
        public IActionResult GetProductShipmentReport([FromBody] ProductAccountabilityCentralReportSearch search)
        {
            return _pharmacyReportRepository.GetProductShipmentReport(search);
        }
        [HttpPost]
        [Route("GetProductShipmentReportData")]
        public IActionResult GetProductShipmentReportData([FromBody] ProductAccountabilityCentralReportSearch search)
        {
            return Ok(_pharmacyReportRepository.GetProductShipmentReportData(search));
        }

        [HttpGet]
        [Route("GetPharmacyStudyProductTypeDropDownPharmacyReport/{projectId}")]
        public IActionResult GetPharmacyStudyProductTypeDropDownPharmacyReport(int projectId)
        {
            return Ok(_pharmacyReportRepository.GetPharmacyStudyProductTypeDropDownPharmacyReport(projectId));
        }

        [HttpGet]
        [Route("GetPatientforKitHistoryReport/{projectId}")]
        public IActionResult GetPatientforKitHistoryReport(int projectId)
        {
            return Ok(_pharmacyReportRepository.GetPatientforKitHistoryReport(projectId));
        }

        [HttpGet]
        [Route("GetKitlistforReport/{projectId}")]
        public IActionResult GetKitlistforReport(int projectId)
        {
            return Ok(_pharmacyReportRepository.GetKitlistforReport(projectId));
        }

        [HttpPost]
        [Route("GetKitHistoryReport")]
        public IActionResult GetKitHistoryReport([FromBody] KitHistoryReportSearchModel search)
        {
            return Ok(_pharmacyReportRepository.GetKitHistoryReport(search));
        }
        [HttpPost]
        [Route("GetKitHistoryReportExcelToExcel")]
        public IActionResult GetKitHistoryReportExcelToExcel([FromBody] KitHistoryReportSearchModel search)
        {
            return _pharmacyReportRepository.GetKitHistoryReportExcelToExcel(search);
        }

    }
}