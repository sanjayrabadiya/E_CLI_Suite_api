using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsApprovalRolesController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ICtmsApprovalRolesRepository _ctmsApprovalRolesRepository;
        private readonly IGSCContext _context;
        private readonly IUnitOfWork _uow;


        public CtmsApprovalRolesController(ICtmsApprovalRolesRepository ctmsApprovalRolesRepository,
             IUnitOfWork uow, IMapper mapper,
             IJwtTokenAccesser jwtTokenAccesser, IGSCContext context)
        {
            _ctmsApprovalRolesRepository = ctmsApprovalRolesRepository;
            _context = context;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
        [HttpGet("GetCtmsApprovalWorkFlowList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetCtmsApprovalWorkFlowList(int projectId, bool isDeleted)
        {
            var CtmsApprovalWorkFlows = _ctmsApprovalRolesRepository.GetCtmsApprovalWorkFlowList(projectId, isDeleted);
            return Ok(CtmsApprovalWorkFlows);
        }
        [HttpGet("GetCtmsApprovalWorkFlowDetailsList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetCtmsApprovalWorkFlowDetailsList(int projectId, bool isDeleted)
        {
            var CtmsApprovalWorkFlowDetails = _ctmsApprovalRolesRepository.GetCtmsApprovalWorkFlowDetailsList(projectId, isDeleted);
            return Ok(CtmsApprovalWorkFlowDetails);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _ctmsApprovalRolesRepository.Find(id);
            var centralDepoDto = _mapper.Map<CtmsApprovalRolesDto>(centralDepo);
            centralDepoDto.UserIds = _context.CtmsApprovalUsers.Where(s => s.CtmsApprovalRolesId == centralDepo.Id && s.DeletedDate == null)
                .Select(s => s.UserId).ToList();
            return Ok(centralDepoDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CtmsApprovalRolesDto ctmsApprovalWorkFlowDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ctmsApprovalWorkFlowDto.Id = 0;
            var CtmsApprovalWorkFlow = _mapper.Map<CtmsApprovalRoles>(ctmsApprovalWorkFlowDto);
            var validate = _ctmsApprovalRolesRepository.Duplicate(ctmsApprovalWorkFlowDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            CtmsApprovalWorkFlow.IpAddress = _jwtTokenAccesser.IpAddress;
            CtmsApprovalWorkFlow.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _ctmsApprovalRolesRepository.Add(CtmsApprovalWorkFlow);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating shipment apporval failed on save."));

            _ctmsApprovalRolesRepository.ChildUserApprovalAdd(ctmsApprovalWorkFlowDto, CtmsApprovalWorkFlow.Id);
            return Ok(CtmsApprovalWorkFlow.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CtmsApprovalRolesDto ctmsApprovalWorkFlowDto)
        {
            if (ctmsApprovalWorkFlowDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ctmsApprovalWorkFlow = _mapper.Map<CtmsApprovalRoles>(ctmsApprovalWorkFlowDto);
            //var validate = _ctmsApprovalRolesRepository.Duplicate(ctmsApprovalWorkFlowDto);
            //if (!string.IsNullOrEmpty(validate))
            //{
            //    ModelState.AddModelError("Message", validate);
            //    return BadRequest(ModelState);
            //}

            ctmsApprovalWorkFlow.IpAddress = _jwtTokenAccesser.IpAddress;
            ctmsApprovalWorkFlow.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _ctmsApprovalRolesRepository.Update(ctmsApprovalWorkFlow);

            if (_uow.Save() <= 0) return Ok(new Exception("Updating shipment apporval study product type failed on save."));
            _ctmsApprovalRolesRepository.DeleteChildWorkflowEmailUser(ctmsApprovalWorkFlowDto, ctmsApprovalWorkFlow.Id);
            _ctmsApprovalRolesRepository.ChildUserApprovalAdd(ctmsApprovalWorkFlowDto, ctmsApprovalWorkFlow.Id);
            return Ok(ctmsApprovalWorkFlow.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _ctmsApprovalRolesRepository.Find(id);

            if (record == null)
                return NotFound();

            var ctmsApprovalWorkFlowDetail = _context.CtmsApprovalUsers.Where(x => x.CtmsApprovalRolesId == id).ToList();
            foreach (var item in ctmsApprovalWorkFlowDetail)
            {
                item.DeletedDate = DateTime.Now;
                item.DeletedBy = _jwtTokenAccesser.UserId;
                _context.CtmsApprovalUsers.Update(item);
            }

            _ctmsApprovalRolesRepository.Update(record);
            _uow.Save();

            _ctmsApprovalRolesRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _ctmsApprovalRolesRepository.Find(id);

            if (record == null)
                return NotFound();

            var ctmsApprovalWorkFlowDetail = _context.CtmsApprovalUsers.Where(x => x.CtmsApprovalRolesId == id).ToList();
            foreach (var item in ctmsApprovalWorkFlowDetail)
            {
                item.DeletedDate = null;
                item.DeletedBy = null;
                _context.CtmsApprovalUsers.Update(item);
            }

            _ctmsApprovalRolesRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetRoleCtmsRights/{projectId}")]
        public IActionResult GetProjectRightsRoleShipmentApproval(int projectId)
        {
            var rights = _ctmsApprovalRolesRepository.GetRoleCtmsRights(projectId);
            return Ok(rights);
        }

        [HttpGet("GetUserCtmsRights/{roleId}/{projectId}")]
        public IActionResult GetRoleUserShipmentApproval(int roleId, int projectId)
        {
            var rights = _ctmsApprovalRolesRepository.GetUserCtmsRights(roleId, projectId);
            return Ok(rights);
        }
    }
}
