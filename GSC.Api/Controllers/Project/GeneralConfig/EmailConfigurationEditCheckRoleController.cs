using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.Project.GeneralConfig;
using GSC.Respository.Project.StudyLevelFormSetup;
using GSC.Respository.SupplyManagement;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailConfigurationEditCheckRoleController : BaseController
    {
 
        private readonly IEmailConfigurationEditCheckRoleRepository _emailConfigurationEditCheckRoleRepository;
        private readonly IEmailConfigurationEditCheckRepository _emailConfigurationEditCheckRepository;

        public EmailConfigurationEditCheckRoleController(
       
         IEmailConfigurationEditCheckRoleRepository emailConfigurationEditCheckRoleRepository,
         IEmailConfigurationEditCheckRepository emailConfigurationEditCheckRepository
        )
        {

           
            _emailConfigurationEditCheckRoleRepository = emailConfigurationEditCheckRoleRepository;
            _emailConfigurationEditCheckRepository = emailConfigurationEditCheckRepository;
        }

        [HttpGet("GetEmailVariableConfigureTemplateDetail/{id}")]
        public IActionResult GetDetailList(int id)
        {
            EmailConfigurationEditCheckRoleDto obj = new EmailConfigurationEditCheckRoleDto();
            var data = _emailConfigurationEditCheckRepository.Find(id);
            if (data != null)
            {
                obj.Subject = data.Subject;
                obj.EmailConfigurationEditCheckId = id;
                obj.EmailBody = data.EmailBody;
                obj.IsSMS = data.IsSMS;
                obj.RoleId = _emailConfigurationEditCheckRoleRepository.All.Where(s => s.EmailConfigurationEditCheckId == id && s.DeletedDate == null).Select(s => s.RoleId).ToList();
            }

            return Ok(obj);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EmailConfigurationEditCheckRoleDto emailConfigurationEditCheckRoleDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _emailConfigurationEditCheckRoleRepository.AddChileRecord(emailConfigurationEditCheckRoleDto);
            
            return Ok(emailConfigurationEditCheckRoleDto.EmailConfigurationEditCheckId);
        }

        [HttpGet("GetProjectRightsRoleEmailTemplate/{projectId}")]
        public IActionResult GetProjectRightsRoleShipmentApproval(int projectId)
        {
            var rights = _emailConfigurationEditCheckRoleRepository.GetProjectRightsRoleEmailTemplate(projectId);
            return Ok(rights);
        }

    }
}
