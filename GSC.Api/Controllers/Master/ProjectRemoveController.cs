using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Common;
using GSC.Respository.Configuration;
using GSC.Respository.LogReport;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.UserMgt;
using GSC.Shared.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ProjectRemoveController : BaseController
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserLoginReportRespository _userLoginReportRepository;
        private readonly IProjectDataRemoveService _projectDataRemoveService;
        public ProjectRemoveController(IProjectRepository projectRepository,
            IUserLoginReportRespository userLoginReportRepository,
            IProjectDataRemoveService projectDataRemoveService
            )
        {
            _projectRepository = projectRepository;
            _userLoginReportRepository = userLoginReportRepository;
            _projectDataRemoveService = projectDataRemoveService;
        }


        [HttpPost]
        [Route("GetStudyListByCompanyid")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> GetStudyListByCompanyid([FromBody] ProjectRemoveDataDto obj)
        {
            _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
            var data = _projectRepository.All.Where(x => x.DeletedDate == null && x.ParentProjectId == null).Select(x => new DropDownStudyDto
            {
                Id = x.Id,
                Value = x.ProjectCode
            }).ToList();
            return Ok(data);
        }
        [HttpPost]
        [Route("AdverseEventDataRemove")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> AdverseEventDataRemove([FromBody] ProjectRemoveDataDto obj)
        {
            _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
            var finaldata = await _projectDataRemoveService.AdverseEventRemove(obj);
            return Ok(finaldata);
        }

        [HttpPost]
        [Route("InformConsentDataRemove")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> InformConsentDataRemove([FromBody] ProjectRemoveDataDto obj)
        {
            _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
            var finaldata = await _projectDataRemoveService.InformConsentRemove(obj);
            return Ok(finaldata);
        }

        [HttpPost]
        [Route("AttedenceDataRemove")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> AttedenceDataRemove([FromBody] ProjectRemoveDataDto obj)
        {
            _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
            var finaldata = await _projectDataRemoveService.AttendenceDataRemove(obj);
            return Ok(finaldata);
        }

        [HttpPost]
        [Route("CTMSDataRemove")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> CTMSDataRemove([FromBody] ProjectRemoveDataDto obj)
        {
            _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
            var finaldata = await _projectDataRemoveService.CTMSDataRemove(obj);
            return Ok(finaldata);
        }

        [HttpPost]
        [Route("LabManagementDataRemove")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> LabManagementDataRemove([FromBody] ProjectRemoveDataDto obj)
        {
            _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
            var finaldata = await _projectDataRemoveService.LabManagementDataRemove(obj);
            return Ok(finaldata);
        }

        [HttpPost]
        [Route("MedraDataRemove")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> MedraDataRemove([FromBody] ProjectRemoveDataDto obj)
        {
            _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
            var finaldata = await _projectDataRemoveService.MedraDataRemove(obj);
            return Ok(finaldata);
        }

        [HttpPost]
        [Route("ETMFDataRemove")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> ETMFDataRemove([FromBody] ProjectRemoveDataDto obj)
        {
            _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
            var finaldata = await _projectDataRemoveService.ETMFDataRemove(obj);
            return Ok(finaldata);
        }

        [HttpPost]
        [Route("ScreeningDataRemove")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> ScreeningDataRemove([FromBody] ProjectRemoveDataDto obj)
        {
            _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
            var finaldata = await _projectDataRemoveService.ScreeningDataRemove(obj);
            return Ok(finaldata);
        }

        [HttpPost]
        [Route("StudyDesignDataRemove")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> StudyDesignDataRemove([FromBody] ProjectRemoveDataDto obj)
        {
            _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
            var finaldata = await _projectDataRemoveService.DesignDataRemove(obj);
            return Ok(finaldata);
        }
    }
}