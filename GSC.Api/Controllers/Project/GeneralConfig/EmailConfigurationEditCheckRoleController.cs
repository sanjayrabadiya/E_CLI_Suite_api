using GSC.Api.Controllers.Common;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Respository.Project.GeneralConfig;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


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
