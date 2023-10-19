﻿using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Report;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Report;
using GSC.Respository.Project.Design;
using GSC.Respository.Reports;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class PharmacyReportController : BaseController
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IPharmacyReportRepository _pharmacyReportRepository;
        private readonly IMapper _mapper;
        public PharmacyReportController(
             IUnitOfWork uow, IJwtTokenAccesser jwtTokenAccesser, IPharmacyReportRepository pharmacyReportRepository,
            IMapper mapper, IGSCContext context)
        {
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _pharmacyReportRepository = pharmacyReportRepository;
        }
        [HttpPost]
        [Route("GetRandomizationKitReport")]
        public IActionResult GetRandomizationKitReport([FromBody] RandomizationIWRSReport search)
        {
            return _pharmacyReportRepository.GetRandomizationKitReport(search);
        }
        [HttpPost]
        [Route("GetRandomizationKitReportData")]
        public IActionResult GetRandomizationKitReportData([FromBody] RandomizationIWRSReport search)
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