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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    public class SupplyManagementApprovalController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementApprovalRepository _supplyManagementApprovalRepository;
        private readonly IGSCContext _context;
        private readonly IUnitOfWork _uow;

        public SupplyManagementApprovalController(ISupplyManagementApprovalRepository supplyManagementApprovalRepository,

            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, IGSCContext context)
        {
            _supplyManagementApprovalRepository = supplyManagementApprovalRepository;
            _context = context;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("GetSupplyManagementApprovalList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetSupplyManagementApprovalList(int projectId, bool isDeleted)
        {
            var productType = _supplyManagementApprovalRepository.GetSupplyManagementApprovalList(projectId, isDeleted);
            return Ok(productType);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _supplyManagementApprovalRepository.Find(id);
            var centralDepoDto = _mapper.Map<SupplyManagementApprovalDto>(centralDepo);
            centralDepoDto.UserIds = _context.SupplyManagementApprovalDetails.Where(s => s.SupplyManagementApprovalId == centralDepo.Id && s.DeletedDate == null).Select(s => s.UserId).ToList();

            return Ok(centralDepoDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementApprovalDto supplyManagementApprovalDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            supplyManagementApprovalDto.Id = 0;
            var supplyManagementEmailConfiguration = _mapper.Map<SupplyManagementApproval>(supplyManagementApprovalDto);
            var validate = _supplyManagementApprovalRepository.Duplicate(supplyManagementApprovalDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _supplyManagementApprovalRepository.Add(supplyManagementEmailConfiguration);
            if (_uow.Save() <= 0) throw new Exception("Creating shipment apporval failed on save.");

            _supplyManagementApprovalRepository.ChildUserApprovalAdd(supplyManagementApprovalDto, supplyManagementEmailConfiguration.Id);
            return Ok(supplyManagementEmailConfiguration.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SupplyManagementApprovalDto supplyManagementEmailConfigurationDto)
        {
            if (supplyManagementEmailConfigurationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var supplyManagementEmailConfiguration = _mapper.Map<SupplyManagementApproval>(supplyManagementEmailConfigurationDto);
            var validate = _supplyManagementApprovalRepository.Duplicate(supplyManagementEmailConfigurationDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            if (_jwtTokenAccesser.GetHeader("audit-reason-oth") != null && _jwtTokenAccesser.GetHeader("audit-reason-oth") != "")
                supplyManagementEmailConfiguration.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            if (_jwtTokenAccesser.GetHeader("audit-reason-id") != null && _jwtTokenAccesser.GetHeader("audit-reason-id") != "")
                supplyManagementEmailConfiguration.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _supplyManagementApprovalRepository.Update(supplyManagementEmailConfiguration);

            if (_uow.Save() <= 0) throw new Exception("Updating shipment apporval study product type failed on save.");
            _supplyManagementApprovalRepository.DelectChildWorkflowEmailUser(supplyManagementEmailConfigurationDto, supplyManagementEmailConfiguration.Id);
            _supplyManagementApprovalRepository.ChildUserApprovalAdd(supplyManagementEmailConfigurationDto, supplyManagementEmailConfiguration.Id);
            return Ok(supplyManagementEmailConfiguration.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementApprovalRepository.Find(id);

            if (record == null)
                return NotFound();

            var supplyManagementEmailConfigurationdetail = _context.SupplyManagementApprovalDetails.Where(x => x.SupplyManagementApprovalId == id).ToList();
            foreach (var item in supplyManagementEmailConfigurationdetail)
            {
                item.DeletedDate = DateTime.Now;
                item.DeletedBy = _jwtTokenAccesser.UserId;
                _context.SupplyManagementApprovalDetails.Update(item);
            }
            if (_jwtTokenAccesser.GetHeader("audit-reason-oth") != null && _jwtTokenAccesser.GetHeader("audit-reason-oth") != "")
                record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            if (_jwtTokenAccesser.GetHeader("audit-reason-id") != null && _jwtTokenAccesser.GetHeader("audit-reason-id") != "")
                record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _supplyManagementApprovalRepository.Update(record);
            _uow.Save();

            _supplyManagementApprovalRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementApprovalRepository.Find(id);

            if (record == null)
                return NotFound();

            var supplyManagementEmailConfigurationdetail = _context.SupplyManagementApprovalDetails.Where(x => x.SupplyManagementApprovalId == id).ToList();
            foreach (var item in supplyManagementEmailConfigurationdetail)
            {
                item.DeletedDate = null;
                item.DeletedBy = null;
                _context.SupplyManagementApprovalDetails.Update(item);
            }

            _supplyManagementApprovalRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetProjectRightsRoleShipmentApproval/{projectId}")]
        public IActionResult GetProjectRightsRoleShipmentApproval(int projectId)
        {
            var rights = _supplyManagementApprovalRepository.GetProjectRightsRoleShipmentApproval(projectId);
            return Ok(rights);
        }

        [HttpGet("GetRoleUserShipmentApproval/{roleId}/{projectId}")]
        public IActionResult GetRoleUserShipmentApproval(int roleId, int projectId)
        {
            var rights = _supplyManagementApprovalRepository.GetRoleUserShipmentApproval(roleId, projectId);
            return Ok(rights);
        }

        [HttpPost]
        [Route("ShipmentApprovalStatus")]
        public IActionResult ShipmentApprovalStatus([FromBody] SupplyManagementShipmentApprovalDto supplyManagementShipmentApprovalDto)
        {

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var supplyManagementShipmentApproval = _mapper.Map<SupplyManagementShipmentApproval>(supplyManagementShipmentApprovalDto);
            supplyManagementShipmentApproval.UserId = _jwtTokenAccesser.UserId;
            supplyManagementShipmentApproval.RoleId = _jwtTokenAccesser.RoleId;
            _context.SupplyManagementShipmentApproval.Add(supplyManagementShipmentApproval);
            _context.Save();
            if (supplyManagementShipmentApprovalDto.Status == Helper.SupplyManagementApprovalStatus.Approved)
                _supplyManagementApprovalRepository.SendShipmentWorkflowApprovalEmail(supplyManagementShipmentApproval);
            return Ok(supplyManagementShipmentApproval.Id);

        }
    }
}
