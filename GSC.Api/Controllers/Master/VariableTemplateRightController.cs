using System.Linq;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class VariableTemplateRightController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IVariableTemplateRightRepository _variableTemplateRightRepository;

        public VariableTemplateRightController(IVariableTemplateRightRepository variableTemplateRightRepository,
            IUnitOfWork uow)
        {
            _variableTemplateRightRepository = variableTemplateRightRepository;
            _uow = uow;
        }


        [HttpGet("{securityRoleId}")]
        public IActionResult Get(int securityRoleId)
        {
            if (securityRoleId <= 0) return BadRequest();

            var variableTemplateIds = _variableTemplateRightRepository
                .FindBy(t => t.SecurityRoleId == securityRoleId && t.DeletedDate == null)
                .Select(t => t.VariableTemplateId).ToList();


            return Ok(variableTemplateIds);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VariableTemplateRightDto variableTemplateRightDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _variableTemplateRightRepository.SaveTemplateRights(variableTemplateRightDto);

            _uow.Save();

            return Ok();
        }
    }
}