﻿using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportScreenController : BaseController
    {
        private readonly IReportScreenRepository _reportScreenRepository;

        public ReportScreenController(IReportScreenRepository reportScreenRepository,
            IUnitOfWork uow)
        {
            _reportScreenRepository = reportScreenRepository;
        }

        [HttpGet]       
        public IActionResult Get()
        {
            var Screens = _reportScreenRepository.GetReportScreen();
            return Ok(Screens);
        }
    }
}
