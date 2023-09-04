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
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public DashboardCompanyController(IDashboardCompanyRepository dashboardCompanyRepository,
            IUnitOfWork uow, IMapper mapper,
            ILocationRepository locationRepository,
            IUploadSettingRepository uploadSettingRepository)
        {
            _dashboardCompanyRepository = dashboardCompanyRepository;
            _uow = uow;
            _mapper = mapper;
            _locationRepository = locationRepository;
            _uploadSettingRepository = uploadSettingRepository;
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