using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    public class SupplyManagementEmailConfigurationController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementEmailConfigurationRepository _supplyManagementEmailConfigurationRepository;
        private readonly IGSCContext _context;
        private readonly IUnitOfWork _uow;

        public SupplyManagementEmailConfigurationController(ISupplyManagementEmailConfigurationRepository supplyManagementEmailConfigurationRepository,

            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, IGSCContext context)
        {
            _supplyManagementEmailConfigurationRepository = supplyManagementEmailConfigurationRepository;
            _context = context;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("GetSupplyManagementEmailConfigurationList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetSupplyManagementEmailConfigurationList(int projectId, bool isDeleted)
        {
            var productType = _supplyManagementEmailConfigurationRepository.GetSupplyManagementEmailConfigurationList(projectId, isDeleted);
            return Ok(productType);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _supplyManagementEmailConfigurationRepository.Find(id);
            var centralDepoDto = _mapper.Map<SupplyManagementEmailConfigurationDto>(centralDepo);

            return Ok(centralDepoDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementEmailConfigurationDto supplyManagementEmailConfigurationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            supplyManagementEmailConfigurationDto.Id = 0;
            var supplyManagementEmailConfiguration = _mapper.Map<SupplyManagementEmailConfiguration>(supplyManagementEmailConfigurationDto);
            var validate = _supplyManagementEmailConfigurationRepository.Duplicate(supplyManagementEmailConfiguration);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            supplyManagementEmailConfiguration.IpAddress = _jwtTokenAccesser.IpAddress;
            supplyManagementEmailConfiguration.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _supplyManagementEmailConfigurationRepository.Add(supplyManagementEmailConfiguration);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating email configuration failed on save."));

            
            return Ok(supplyManagementEmailConfiguration.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SupplyManagementEmailConfigurationDto supplyManagementEmailConfigurationDto)
        {
            if (supplyManagementEmailConfigurationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);



            var supplyManagementEmailConfiguration = _mapper.Map<SupplyManagementEmailConfiguration>(supplyManagementEmailConfigurationDto);
            var validate = _supplyManagementEmailConfigurationRepository.Duplicate(supplyManagementEmailConfiguration);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            if (_jwtTokenAccesser.GetHeader("audit-reason-oth") != null && _jwtTokenAccesser.GetHeader("audit-reason-oth") != "")
                supplyManagementEmailConfiguration.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            if (_jwtTokenAccesser.GetHeader("audit-reason-id") != null && _jwtTokenAccesser.GetHeader("audit-reason-id") != "")
                supplyManagementEmailConfiguration.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            supplyManagementEmailConfiguration.IpAddress = _jwtTokenAccesser.IpAddress;
            supplyManagementEmailConfiguration.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _supplyManagementEmailConfigurationRepository.Update(supplyManagementEmailConfiguration);

            if (_uow.Save() <= 0) return Ok(new Exception("Updating email configuration study product type failed on save."));
            
            return Ok(supplyManagementEmailConfiguration.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementEmailConfigurationRepository.Find(id);

            if (record == null)
                return NotFound();

            var supplyManagementEmailConfigurationdetail = _context.SupplyManagementEmailConfigurationDetail.Where(x => x.SupplyManagementEmailConfigurationId == id).ToList();
            foreach (var item in supplyManagementEmailConfigurationdetail)
            {
                item.DeletedDate = DateTime.Now;
                item.DeletedBy = _jwtTokenAccesser.UserId;
                _context.SupplyManagementEmailConfigurationDetail.Update(item);
            }
            if (_jwtTokenAccesser.GetHeader("audit-reason-oth") != null && _jwtTokenAccesser.GetHeader("audit-reason-oth") != "")
                record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            if (_jwtTokenAccesser.GetHeader("audit-reason-id") != null && _jwtTokenAccesser.GetHeader("audit-reason-id") != "")
                record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _supplyManagementEmailConfigurationRepository.Update(record);
            _uow.Save();

            _supplyManagementEmailConfigurationRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementEmailConfigurationRepository.Find(id);

            if (record == null)
                return NotFound();

            var supplyManagementEmailConfigurationdetail = _context.SupplyManagementEmailConfigurationDetail.Where(x => x.SupplyManagementEmailConfigurationId == id).ToList();
            foreach (var item in supplyManagementEmailConfigurationdetail)
            {
                item.DeletedDate = null;
                item.DeletedBy = null;
                _context.SupplyManagementEmailConfigurationDetail.Update(item);
            }

            _supplyManagementEmailConfigurationRepository.Active(record);
            _uow.Save();

            return Ok();
        }
        [HttpGet("GetProjectRightsIWRS/{projectId}")]
        public IActionResult GetProjectRightsIWRS(int projectId)
        {
            var rights = _supplyManagementEmailConfigurationRepository.GetProjectRightsIWRS(projectId);
            return Ok(rights);
        }

        [HttpPost]
        [Route("SaveDetails")]
        public IActionResult SaveDetails([FromBody] SupplyManagementEmailConfigurationDto supplyManagementEmailConfigurationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            _supplyManagementEmailConfigurationRepository.ChildEmailUserAdd(supplyManagementEmailConfigurationDto, supplyManagementEmailConfigurationDto.Id);
            return Ok(supplyManagementEmailConfigurationDto.Id);
        }

        [HttpGet("GetSupplyManagementEmailConfigurationDetailList/{id}")]
        public IActionResult GetSupplyManagementEmailConfigurationDetailList(int id)
        {
            var productType = _supplyManagementEmailConfigurationRepository.GetSupplyManagementEmailConfigurationDetailList(id);
            return Ok(productType);
        }
        [HttpDelete("DeleteUserEmail/{id}")]
        public ActionResult DeleteUserEmail(int id)
        {
            var record = _context.SupplyManagementEmailConfigurationDetail.Where(x => x.Id == id).FirstOrDefault();

            if (record == null)
                return NotFound();
            if (!string.IsNullOrEmpty(_jwtTokenAccesser.GetHeader("audit-reason-oth")))
                record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            if (!string.IsNullOrEmpty(_jwtTokenAccesser.GetHeader("audit-reason-id")))
                record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));

            record.DeletedDate = DateTime.Now;
            record.DeletedBy = _jwtTokenAccesser.UserId;

            _context.SupplyManagementEmailConfigurationDetail.Update(record);
            _uow.Save();

            return Ok();
        }
        [HttpPatch("ActiveUserEmail/{id}")]
        public ActionResult ActiveUserEmail(int id)
        {
            var record = _context.SupplyManagementEmailConfigurationDetail.Where(x => x.Id == id).FirstOrDefault();

            if (record == null)
                return NotFound();

            record.DeletedDate = null;
            record.DeletedBy = null;

            _context.SupplyManagementEmailConfigurationDetail.Update(record);
            _uow.Save();

            return Ok();
        }
        [HttpGet("GetEmailHistory/{id}")]
        public IActionResult GetEmailHistory(int id)
        {
            var productType = _supplyManagementEmailConfigurationRepository.GetEmailHistory(id);
            return Ok(productType);
        }

    }
}
