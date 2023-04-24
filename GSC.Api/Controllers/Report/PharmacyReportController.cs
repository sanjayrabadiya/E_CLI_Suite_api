using AutoMapper;
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
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == search.ProjectId).FirstOrDefault();
            if (setting == null)
            {
                ModelState.AddModelError("Message", "Please set kit number setting");
                return BadRequest(ModelState);
            }
            return _pharmacyReportRepository.GetRandomizationKitReport(search);
        }

        [HttpPost]
        [Route("GetProductAccountabilityCentralReport")]
        public IActionResult GetProductAccountabilityCentralReport([FromBody] ProductAccountabilityCentralReportSearch search)
        {
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == search.ProjectId).FirstOrDefault();
            if (setting == null)
            {
                ModelState.AddModelError("Message", "Please set kit number setting");
                return BadRequest(ModelState);
            }
            return _pharmacyReportRepository.GetProductAccountabilityCentralReport(search);
        }
        [HttpPost]
        [Route("GetProductAccountabilitySiteReport")]
        public IActionResult GetProductAccountabilitySiteReport([FromBody] ProductAccountabilityCentralReportSearch search)
        {
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == search.ProjectId).FirstOrDefault();
            if (setting == null)
            {
                ModelState.AddModelError("Message", "Please set kit number setting");
                return BadRequest(ModelState);
            }
            return _pharmacyReportRepository.GetProductAccountabilitySiteReport(search);
        }
        [HttpPost]
        [Route("GetProductShipmentReport")]
        public IActionResult GetProductShipmentReport([FromBody] ProductAccountabilityCentralReportSearch search)
        {
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == search.ProjectId).FirstOrDefault();
            if (setting == null)
            {
                ModelState.AddModelError("Message", "Please set kit number setting");
                return BadRequest(ModelState);
            }
            return _pharmacyReportRepository.GetProductShipmentReport(search);
        }
    }
}