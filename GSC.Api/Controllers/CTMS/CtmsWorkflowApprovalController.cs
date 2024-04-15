using AutoMapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsWorkflowApprovalController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ICtmsWorkflowApprovalRepository _ctmsWorkflowApprovalRepository;

        public CtmsWorkflowApprovalController(IUnitOfWork uow, IMapper mapper, ICtmsWorkflowApprovalRepository ctmsWorkflowApprovalRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _ctmsWorkflowApprovalRepository = ctmsWorkflowApprovalRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var approval = _ctmsWorkflowApprovalRepository.Find(id);
            var approvalDto = _mapper.Map<CtmsWorkflowApprovalDto>(approval);
            return Ok(approvalDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] IList<CtmsWorkflowApprovalDto> approversDto)
        {
            if (approversDto.Count <= 0)
            {
                ModelState.AddModelError("Message", "List is empty");
                return BadRequest(ModelState);
            }
            foreach (var approverDto in approversDto)
            {
                approverDto.Id = 0;
                approverDto.SendDate = DateTime.Now;
                var approver = _mapper.Map<CtmsWorkflowApproval>(approverDto);
                _ctmsWorkflowApprovalRepository.Add(approver);
                _uow.Save();
            }

            return Ok(approversDto.Count);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CtmsWorkflowApprovalDto approverDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            approverDto.Id = 0;
            approverDto.SendDate = DateTime.Now;
            var approver = _mapper.Map<CtmsWorkflowApproval>(approverDto);
            _ctmsWorkflowApprovalRepository.Add(approver);
            var result = _uow.Save();
            if (result > 0)
            {
                ModelState.AddModelError("Message", "Error to save");
                return BadRequest(ModelState);
            }
            return Ok(result);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CtmsWorkflowApprovalDto approverDto)
        {
            if (approverDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            approverDto.ActionDate = DateTime.Now;
            var approver = _mapper.Map<CtmsWorkflowApproval>(approverDto);
            _ctmsWorkflowApprovalRepository.Update(approver);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Ctms Workflow Approval failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(approver.Id);
        }

        [HttpGet("GetApprovalHistoryBySender/{studyPlanId}/{projectId}")]
        public IActionResult GetApprovalHistoryBySender(int studyPlanId, int projectId)
        {
            var history = _ctmsWorkflowApprovalRepository.GetApprovalBySender(studyPlanId, projectId);
            return Ok(history);
        }

        [HttpGet("GetApprovalHistoryByApprover/{studyPlanId}/{projectId}")]
        public IActionResult GetApprovalHistoryByApprover(int studyPlanId, int projectId)
        {
            var history = _ctmsWorkflowApprovalRepository.GetApprovalByApprover(studyPlanId, projectId);
            return Ok(history);
        }

        [HttpGet("GetApprovalStatus/{studyPlanId}/{projectId}")]
        public IActionResult GetApprovalStatus(int studyPlanId, int projectId)
        {
            var status = _ctmsWorkflowApprovalRepository.GetApprovalStatus(studyPlanId, projectId);
            return Ok(status);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _ctmsWorkflowApprovalRepository.Find(id);

            if (record == null)
                return NotFound();

            _ctmsWorkflowApprovalRepository.Delete(record);
            _uow.Save();

            return Ok();
        }
    }
}
