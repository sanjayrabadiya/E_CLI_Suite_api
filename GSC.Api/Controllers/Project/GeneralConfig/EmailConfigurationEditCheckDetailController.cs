using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Respository.Project.GeneralConfig;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;


namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailConfigurationEditCheckDetailController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        
        private readonly IEmailConfigurationEditCheckRepository _emailConfigurationEditCheckRepository;
        private readonly IEmailConfigurationEditCheckDetailRepository _emailConfigurationEditCheckDetailRepository;
        public EmailConfigurationEditCheckDetailController(
        IUnitOfWork uow, IMapper mapper,
        IJwtTokenAccesser jwtTokenAccesser,
         IEmailConfigurationEditCheckRepository emailConfigurationEditCheckRepository,
         IEmailConfigurationEditCheckDetailRepository emailConfigurationEditCheckDetailRepository
        )
        {

            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _emailConfigurationEditCheckRepository = emailConfigurationEditCheckRepository;
            _emailConfigurationEditCheckDetailRepository = emailConfigurationEditCheckDetailRepository;

        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var data = _emailConfigurationEditCheckDetailRepository.GetDetail(id);
            return Ok(data);
        }
        [HttpGet("GetDetailList/{id}")]
        public IActionResult GetDetailList(int id)
        {
            var data = _emailConfigurationEditCheckDetailRepository.GetDetailList(id);
            return Ok(data);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EmailConfigurationEditCheckDetailDto emailConfigurationEditCheckDetailDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            emailConfigurationEditCheckDetailDto.CollectionValue = emailConfigurationEditCheckDetailDto.CollectionValue.ToLower();

            var supplyManagementFectorDetail = _mapper.Map<EmailConfigurationEditCheckDetail>(emailConfigurationEditCheckDetailDto);

            _emailConfigurationEditCheckDetailRepository.Add(supplyManagementFectorDetail);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating fector detail failed on save."));

            _emailConfigurationEditCheckRepository.UpdateEditCheckEmailFormula(emailConfigurationEditCheckDetailDto.EmailConfigurationEditCheckId);
            return Ok(supplyManagementFectorDetail.Id);
        }
        [HttpPut]
        public IActionResult Put([FromBody] EmailConfigurationEditCheckDetailDto emailConfigurationEditCheckDetailDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            emailConfigurationEditCheckDetailDto.CollectionValue = emailConfigurationEditCheckDetailDto.CollectionValue.ToLower();
            var supplyManagementFectorDetail = _mapper.Map<EmailConfigurationEditCheckDetail>(emailConfigurationEditCheckDetailDto);

            _emailConfigurationEditCheckDetailRepository.Update(supplyManagementFectorDetail);
            if (_uow.Save() <= 0) return Ok(new Exception("Updating fector detail failed on save."));
            _emailConfigurationEditCheckRepository.UpdateEditCheckEmailFormula(emailConfigurationEditCheckDetailDto.EmailConfigurationEditCheckId);
            return Ok(supplyManagementFectorDetail.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _emailConfigurationEditCheckDetailRepository.Find(id);
            if (record == null)
                return NotFound();

            _emailConfigurationEditCheckDetailRepository.Delete(record);
            if (!string.IsNullOrEmpty(_jwtTokenAccesser.GetHeader("audit-reason-oth")))
                record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            if (!string.IsNullOrEmpty(_jwtTokenAccesser.GetHeader("audit-reason-id")))
                record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _emailConfigurationEditCheckDetailRepository.Update(record);
            _uow.Save();
            _emailConfigurationEditCheckRepository.UpdateEditCheckEmailFormula(record.EmailConfigurationEditCheckId);
            _uow.Save();
            return Ok();
        }
    }
}
