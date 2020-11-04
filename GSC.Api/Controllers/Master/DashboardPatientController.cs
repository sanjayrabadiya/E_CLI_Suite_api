using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Respository.Attendance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardPatientController : ControllerBase
    {
        private readonly IRandomizationRepository _randomizationRepository;
        public DashboardPatientController(IRandomizationRepository randomizationRepository)
        {
            _randomizationRepository = randomizationRepository;
        }

        [HttpGet("GetDashboardPatientDetail")]
        public IActionResult GetDashboardPatientDetail()
        {
            return Ok(_randomizationRepository.GetDashboardPatientDetail());
        }
    }
}
