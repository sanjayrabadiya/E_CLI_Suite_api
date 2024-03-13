using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.Reports;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class JobMonitoringController : BaseController
    {
        private readonly IJobMonitoringRepository _jobMonitoringRepository;
        
        public JobMonitoringController(IJobMonitoringRepository jobMonitoringRepository)
        {
            _jobMonitoringRepository = jobMonitoringRepository;
         
        }

        
        [HttpGet]        
        public IActionResult Get()
        {
            var JobMonitoring = _jobMonitoringRepository.JobMonitoringList();
            return Ok(JobMonitoring);
        }              
          
    }
}