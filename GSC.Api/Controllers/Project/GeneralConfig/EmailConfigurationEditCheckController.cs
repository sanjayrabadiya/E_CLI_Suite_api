using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Respository.Project.GeneralConfig;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.Project.GeneralConfig
{
    [Route("api/[controller]")]
    public class EmailConfigurationEditCheckController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IEmailConfigurationEditCheckRepository _emailConfigurationEditCheckRepository;
        private readonly IEmailConfigurationEditCheckDetailRepository _emailConfigurationEditCheckDetailRepository;
        private readonly IEmailConfigurationEditCheckRoleRepository _emailConfigurationEditCheckRoleRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
       
        public EmailConfigurationEditCheckController(
            IUnitOfWork uow, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser, IEmailConfigurationEditCheckRepository emailConfigurationEditCheckRepository,
            IEmailConfigurationEditCheckDetailRepository emailConfigurationEditCheckDetailRepository, IEmailConfigurationEditCheckRoleRepository emailConfigurationEditCheckRoleRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _emailConfigurationEditCheckRepository = emailConfigurationEditCheckRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _emailConfigurationEditCheckDetailRepository = emailConfigurationEditCheckDetailRepository;
            _emailConfigurationEditCheckRoleRepository = emailConfigurationEditCheckRoleRepository;
        }

       
        [HttpGet("GetEmailEditCheckList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetEmailEditCheckList(int projectId, bool isDeleted)
        {
            var productVerification = _emailConfigurationEditCheckRepository.GetEmailEditCheckList(projectId, isDeleted);
            return Ok(productVerification);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EmailConfigurationEditCheckDto emailConfigurationEditCheckDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            
            emailConfigurationEditCheckDto.Id = 0;
            var emailConfigurationEditCheck = _mapper.Map<EmailConfigurationEditCheck>(emailConfigurationEditCheckDto);

            _emailConfigurationEditCheckRepository.Add(emailConfigurationEditCheck);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating fector failed on save."));
            return Ok(emailConfigurationEditCheck.Id);
        }
        [HttpPut]
        public IActionResult Put([FromBody] EmailConfigurationEditCheckDto emailConfigurationEditCheckDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var supplyManagementFector = _mapper.Map<EmailConfigurationEditCheck>(emailConfigurationEditCheckDto);
            _emailConfigurationEditCheckRepository.DeleteEmailConfigEditCheckChild(emailConfigurationEditCheckDto.Id);
            _emailConfigurationEditCheckRepository.Update(supplyManagementFector);
            if (_uow.Save() <= 0) return Ok(new Exception("Updating fector failed on save."));

            return Ok(supplyManagementFector.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _emailConfigurationEditCheckRepository.Find(id);
            if (record == null)
                return NotFound();

            _emailConfigurationEditCheckRepository.Delete(record);

            var verifyRecord = _emailConfigurationEditCheckDetailRepository.All.Where(x => x.EmailConfigurationEditCheckId == record.Id).ToList();

            if (verifyRecord.Any())
            {
                foreach (var item in verifyRecord)
                {
                    _emailConfigurationEditCheckDetailRepository.Delete(item);
                }
            }

            var emailrole = _emailConfigurationEditCheckRoleRepository.All.Where(x => x.EmailConfigurationEditCheckId == record.Id).ToList();

            if (emailrole.Any())
            {
                foreach (var item in emailrole)
                {
                    _emailConfigurationEditCheckRoleRepository.Delete(item);
                }
            }
            record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _emailConfigurationEditCheckRepository.Update(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetEmailConfigurationEditCheckSendMailHistory/{id}")]
        public IActionResult GetEmailConfigurationEditCheckSendMailHistory(int id)
        {
            var productVerification = _emailConfigurationEditCheckRepository.GetEmailConfigurationEditCheckSendMailHistory(id);
            return Ok(productVerification);
        }
    }
}
