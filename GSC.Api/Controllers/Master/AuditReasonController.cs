using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class AuditReasonController : BaseController
    {
        private readonly IAuditReasonRepository _auditReasonRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;

        public AuditReasonController(IAuditReasonRepository auditReasonRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _auditReasonRepository = auditReasonRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var auditReasons = _auditReasonRepository
                .All.Where(x =>x.IsDeleted == isDeleted
                ).OrderByDescending(x => x.Id).ToList();
            var auditReasonsDto = _mapper.Map<IEnumerable<AuditReasonDto>>(auditReasons);
            auditReasonsDto.ForEach(t => t.ModuleName = t.ModuleId.GetDescription());
            auditReasonsDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(auditReasonsDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var auditReason = _auditReasonRepository.Find(id);
            var auditReasonDto = _mapper.Map<AuditReasonDto>(auditReason);
            return Ok(auditReasonDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] AuditReasonDto auditReasonDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            auditReasonDto.Id = 0;
            var auditReason = _mapper.Map<AuditReason>(auditReasonDto);
            var validate = _auditReasonRepository.Duplicate(auditReason);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _auditReasonRepository.Add(auditReason);
            if (_uow.Save() <= 0) throw new Exception("Creating Audit Reason failed on save.");
            return Ok(auditReason.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] AuditReasonDto auditReasonDto)
        {
            if (auditReasonDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var auditReason = _mapper.Map<AuditReason>(auditReasonDto);
            var validate = _auditReasonRepository.Duplicate(auditReason);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _auditReasonRepository.AddOrUpdate(auditReason);

            if (_uow.Save() <= 0) throw new Exception("Updating Audit Reason failed on save.");
            return Ok(auditReason.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _auditReasonRepository.Find(id);

            if (record == null)
                return NotFound();

            _auditReasonRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _auditReasonRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _auditReasonRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _auditReasonRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetAuditReasonDropDown/{auditModule}")]
        public IActionResult GetAuditReasonDropDown(AuditModule auditModule)
        {
            return Ok(_auditReasonRepository.GetAuditReasonDropDown(auditModule));
        }

        [HttpGet]
        [Route("GetAuditReasonByModuleDropDown/{modulelId}")]
        public IActionResult GetAuditReasonByModuleDropDown(int modulelId)
        {
            var auditReasons = _auditReasonRepository
                .All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.IsDeleted == false &&
                    ((int) x.ModuleId == modulelId || (int) x.ModuleId == 2)
                ).OrderBy(o => o.ReasonName).ToList();
            var auditReasonsDto = _mapper.Map<IEnumerable<AuditReasonDto>>(auditReasons);
            auditReasonsDto.ForEach(t => t.ModuleName = t.ModuleId.GetDescription());
            return Ok(auditReasonsDto);
        }
    }
}