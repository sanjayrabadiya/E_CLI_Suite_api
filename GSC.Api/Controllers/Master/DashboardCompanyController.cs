using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Respository.Common;
using GSC.Respository.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    public class DashboardCompanyController : BaseController
    {
        private readonly IDashboardCompanyRepository _dashboardCompanyRepository;

        public DashboardCompanyController(IDashboardCompanyRepository dashboardCompanyRepository)
        {
            _dashboardCompanyRepository = dashboardCompanyRepository;
        }
        [HttpGet]
        [Route("GetDashboardCompanyList")]
        public IActionResult GetDashboardCompanyList()
        {
            return Ok(_dashboardCompanyRepository.GetDashboardCompanyList());
        }

        [HttpGet]
        [Route("GetDashboardProjectsStatus")]
        public IActionResult GetDashboardProjectsStatus()
        {
            var screeningVisits = _dashboardCompanyRepository.GetDashboardProjectsStatus();
            return Ok(screeningVisits);
        }
        //Add yash
        [HttpGet]
        [Route("GetDashboardManageStudy")]
        public IActionResult GetDashboardManageStudy()
        {
            var res = _dashboardCompanyRepository.GetDashboardManageStudy();
            return Ok(res);
        }

    }
}