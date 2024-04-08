using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsApprovalWorkFlowController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ICtmsApprovalWorkFlowRepository _ctmsApprovalWorkFlowRepository;
        private readonly IGSCContext _context;
        private readonly IUnitOfWork _uow;


        public CtmsApprovalWorkFlowController(ICtmsApprovalWorkFlowRepository ctmsApprovalWorkFlowRepository,
             IUnitOfWork uow, IMapper mapper,
             IJwtTokenAccesser jwtTokenAccesser, IGSCContext context)
        {
            _ctmsApprovalWorkFlowRepository = ctmsApprovalWorkFlowRepository;
            _context = context;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
        [HttpGet("GetCtmsApprovalWorkFlowList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetCtmsApprovalWorkFlowList(int projectId, bool isDeleted)
        {
            var productType = _ctmsApprovalWorkFlowRepository.GetCtmsApprovalWorkFlowList(projectId, isDeleted);
            return Ok(productType);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _ctmsApprovalWorkFlowRepository.Find(id);
            var centralDepoDto = _mapper.Map<CtmsApprovalWorkFlowDto>(centralDepo);
            centralDepoDto.UserIds = _context.CtmsApprovalWorkFlowDetail.Where(s => s.CtmsApprovalWorkFlowId == centralDepo.Id && s.DeletedDate == null).Select(s => s.UserId).ToList();

            return Ok(centralDepoDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CtmsApprovalWorkFlowDto ctmsApprovalWorkFlowDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ctmsApprovalWorkFlowDto.Id = 0;
            var CtmsApprovalWorkFlow = _mapper.Map<CtmsApprovalWorkFlow>(ctmsApprovalWorkFlowDto);
            var validate = _ctmsApprovalWorkFlowRepository.Duplicate(ctmsApprovalWorkFlowDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            CtmsApprovalWorkFlow.IpAddress = _jwtTokenAccesser.IpAddress;
            CtmsApprovalWorkFlow.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _ctmsApprovalWorkFlowRepository.Add(CtmsApprovalWorkFlow);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating shipment apporval failed on save."));

            _ctmsApprovalWorkFlowRepository.ChildUserApprovalAdd(ctmsApprovalWorkFlowDto, CtmsApprovalWorkFlow.Id);
            return Ok(CtmsApprovalWorkFlow.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CtmsApprovalWorkFlowDto ctmsApprovalWorkFlowDto)
        {
            if (ctmsApprovalWorkFlowDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ctmsApprovalWorkFlow = _mapper.Map<CtmsApprovalWorkFlow>(ctmsApprovalWorkFlowDto);
            var validate = _ctmsApprovalWorkFlowRepository.Duplicate(ctmsApprovalWorkFlowDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            ctmsApprovalWorkFlow.IpAddress = _jwtTokenAccesser.IpAddress;
            ctmsApprovalWorkFlow.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _ctmsApprovalWorkFlowRepository.Update(ctmsApprovalWorkFlow);

            if (_uow.Save() <= 0) return Ok(new Exception("Updating shipment apporval study product type failed on save."));
            _ctmsApprovalWorkFlowRepository.DelectChildWorkflowEmailUser(ctmsApprovalWorkFlowDto, ctmsApprovalWorkFlow.Id);
            _ctmsApprovalWorkFlowRepository.ChildUserApprovalAdd(ctmsApprovalWorkFlowDto, ctmsApprovalWorkFlow.Id);
            return Ok(ctmsApprovalWorkFlow.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _ctmsApprovalWorkFlowRepository.Find(id);

            if (record == null)
                return NotFound();

            var ctmsApprovalWorkFlowDetail = _context.CtmsApprovalWorkFlowDetail.Where(x => x.CtmsApprovalWorkFlowId == id).ToList();
            foreach (var item in ctmsApprovalWorkFlowDetail)
            {
                item.DeletedDate = DateTime.Now;
                item.DeletedBy = _jwtTokenAccesser.UserId;
                _context.CtmsApprovalWorkFlowDetail.Update(item);
            }

            _ctmsApprovalWorkFlowRepository.Update(record);
            _uow.Save();

            _ctmsApprovalWorkFlowRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _ctmsApprovalWorkFlowRepository.Find(id);

            if (record == null)
                return NotFound();

            var ctmsApprovalWorkFlowDetail = _context.CtmsApprovalWorkFlowDetail.Where(x => x.CtmsApprovalWorkFlowId == id).ToList();
            foreach (var item in ctmsApprovalWorkFlowDetail)
            {
                item.DeletedDate = null;
                item.DeletedBy = null;
                _context.CtmsApprovalWorkFlowDetail.Update(item);
            }

            _ctmsApprovalWorkFlowRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetRoleCtmsRights/{projectId}")]
        public IActionResult GetProjectRightsRoleShipmentApproval(int projectId)
        {
            var rights = _ctmsApprovalWorkFlowRepository.GetRoleCtmsRights(projectId);
            return Ok(rights);
        }

        [HttpGet("GetUserCtmsRights/{roleId}/{projectId}")]
        public IActionResult GetRoleUserShipmentApproval(int roleId, int projectId)
        {
            var rights = _ctmsApprovalWorkFlowRepository.GetUserCtmsRights(roleId, projectId);
            return Ok(rights);
        }
    }
}
