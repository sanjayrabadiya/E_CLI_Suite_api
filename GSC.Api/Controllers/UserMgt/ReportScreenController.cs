using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportScreenController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IReportScreenRepository _reportScreenRepository;

        public ReportScreenController(IReportScreenRepository reportScreenRepository,
            IUnitOfWork uow)
        {
            _reportScreenRepository = reportScreenRepository;
            _uow = uow;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            var Screens = _reportScreenRepository.GetReportScreen();

            return Ok(Screens);
        }
    }
}
