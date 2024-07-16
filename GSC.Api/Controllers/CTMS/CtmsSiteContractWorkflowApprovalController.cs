using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsSiteContractWorkflowApprovalController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ICtmsSiteContractWorkflowApprovalRepository _ctmsWorkflowApprovalRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly ISiteContractRepository _siteContractRepository;

        public CtmsSiteContractWorkflowApprovalController(
            IUnitOfWork uow,
            IMapper mapper,
            ICtmsSiteContractWorkflowApprovalRepository ctmsWorkflowApprovalRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            ISiteContractRepository siteContractRepository,
            IEmailSenderRespository emailSenderRespository)
        {
            _uow = uow;
            _mapper = mapper;
            _ctmsWorkflowApprovalRepository = ctmsWorkflowApprovalRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _emailSenderRespository = emailSenderRespository;
            _siteContractRepository = siteContractRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest("Invalid ID");

            var approval = _ctmsWorkflowApprovalRepository.Find(id);
            if (approval == null) return NotFound();

            var approvalDto = _mapper.Map<CtmsSiteContractWorkflowApprovalDto>(approval);
            return Ok(approvalDto);
        }

        [HttpGet]
        [Route("GetRolUserTree/{siteContractId}/{projectId}/{siteId}/{triggerType}")]
        public IActionResult GetRolUserTree(int siteContractId, int projectId, int siteId, TriggerType triggerType)
        {
            if (projectId <= 0) return BadRequest("Invalid project ID");

            var result = _ctmsWorkflowApprovalRepository.GetProjectRightByProjectId(siteContractId, projectId, siteId, triggerType);
            return Ok(result);
        }

        [HttpPost("SaveApprovalRequest")]
        public IActionResult SaveApprovalRequest([FromBody] IList<CtmsSiteContractWorkflowApprovalDto> approversDto)
        {
            if (approversDto == null || !approversDto.Any())
                return BadRequest("Approval list is empty");

            foreach (var approverDto in approversDto)
            {
                approverDto.Id = 0;
                approverDto.SenderId = _jwtTokenAccesser.UserId;
                approverDto.SendDate = DateTime.Now;
                var approver = _mapper.Map<CtmsSiteContractWorkflowApproval>(approverDto);
                _ctmsWorkflowApprovalRepository.Add(approver);
            }

            _uow.Save();
            NotifyApprovers(approversDto);
            return Ok(approversDto.Count);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CtmsSiteContractWorkflowApprovalDto approverDto)
        {
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

            approverDto.Id = 0;
            approverDto.SendDate = DateTime.Now;
            approverDto.SenderId = _jwtTokenAccesser.UserId;

            var approver = _mapper.Map<CtmsSiteContractWorkflowApproval>(approverDto);
            _ctmsWorkflowApprovalRepository.Add(approver);

            if (_uow.Save() <= 0) return BadRequest("Error saving approval");

            UpdatePreviousRecord(approverDto, approver);

            NotifyApprovers(new List<CtmsSiteContractWorkflowApprovalDto> { approverDto });
            return Ok(approver.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CtmsSiteContractWorkflowApprovalDto approverDto)
        {
            if (approverDto.Id <= 0 || !ModelState.IsValid)
                return BadRequest("Invalid input");

            approverDto.ActionDate = DateTime.Now;
            var approver = _mapper.Map<CtmsSiteContractWorkflowApproval>(approverDto);

            _ctmsWorkflowApprovalRepository.Update(approver);
            if (_uow.Save() <= 0)
                return BadRequest("Updating Ctms Workflow Approval failed on save.");

            if (_ctmsWorkflowApprovalRepository.GetApprovalStatus(approverDto.SiteContractId, approverDto.ProjectId, approverDto.TriggerType))
            {
                UpdateSiteContractApprovalStatus(approverDto.SiteContractId);
            }

            NotifyApprovers(new List<CtmsSiteContractWorkflowApprovalDto> { approverDto });
            return Ok(approver.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _ctmsWorkflowApprovalRepository.Find(id);
            if (record == null) return NotFound();

            _ctmsWorkflowApprovalRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetApprovalHistoryBySender/{siteContractId}/{projectId}/{triggerType}")]
        public IActionResult GetApprovalHistoryBySender(int siteContractId, int projectId, TriggerType triggerType)
        {
            var history = _ctmsWorkflowApprovalRepository.GetApprovalBySender(siteContractId, projectId, triggerType);
            return Ok(history);
        }

        [HttpGet("GetApprovalHistoryByApprover/{siteContractId}/{projectId}/{triggerType}")]
        public IActionResult GetApprovalHistoryByApprover(int siteContractId, int projectId, TriggerType triggerType)
        {
            var history = _ctmsWorkflowApprovalRepository.GetApprovalByApprover(siteContractId, projectId, triggerType);
            return Ok(history);
        }

        [HttpGet("CheckIsSender/{siteContractId}/{projectId}/{siteId}/{triggerType}")]
        public IActionResult CheckIsSender(int siteContractId, int projectId, int siteId, TriggerType triggerType)
        {
            var status = _ctmsWorkflowApprovalRepository.CheckSender(siteContractId, projectId, siteId, triggerType);
            return Ok(status);
        }

        [HttpGet("GetApproverNewComment/{triggerType}")]
        public IActionResult GetApproverNewComment(TriggerType triggerType)
        {
            var result = _ctmsWorkflowApprovalRepository.GetApproverNewComment(triggerType);
            return Ok(result);
        }

        [HttpGet("GetSenderNewComment/{triggerType}")]
        public IActionResult GetSenderNewComment(TriggerType triggerType)
        {
            var result = _ctmsWorkflowApprovalRepository.GetSenderNewComment(triggerType);
            return Ok(result);
        }

        [HttpGet("GetApprovalUsers/{siteContractId}")]
        public IActionResult GetApprovalUsers(int siteContractId)
        {
            var result = _ctmsWorkflowApprovalRepository.GetApprovalUsers(siteContractId);
            return Ok(result);
        }

        [HttpGet("CheckNewComment/{siteContractId}/{projectId}/{triggerType}")]
        public IActionResult CheckNewComment(int siteContractId, int projectId, TriggerType triggerType)
        {
            var result = _ctmsWorkflowApprovalRepository.IsNewComment(siteContractId, projectId, triggerType);
            return Ok(result);
        }

        [HttpGet("CheckCommentReply/{siteContractId}/{projectId}/{userId}/{roleId}/{triggerType}")]
        public IActionResult CheckCommentReply(int siteContractId, int projectId, int userId, int roleId, TriggerType triggerType)
        {
            var result = _ctmsWorkflowApprovalRepository.IsCommentReply(siteContractId, projectId, userId, roleId, triggerType);
            return Ok(result);
        }

        private void UpdatePreviousRecord(CtmsSiteContractWorkflowApprovalDto approverDto, CtmsSiteContractWorkflowApproval approver)
        {
            var lastRecord = _ctmsWorkflowApprovalRepository.Find(approverDto.CtmsSiteContractWorkflowApprovalId ?? 0);
            if (lastRecord != null)
            {
                lastRecord.CtmsSiteContractWorkflowApprovalId = approver.Id;
                _ctmsWorkflowApprovalRepository.Update(lastRecord);
                _uow.Save();
            }
        }

        private void NotifyApprovers(IList<CtmsSiteContractWorkflowApprovalDto> approversDto)
        {
            var userIds = approversDto.Select(x => x.UserId).ToList();

            var approverDetails = _ctmsWorkflowApprovalRepository.All
                .Where(x => x.SiteContractId == approversDto.First().SiteContractId
                    && x.ProjectId == approversDto.First().ProjectId
                    && x.DeletedDate == null
                    && x.TriggerType == approversDto.First().TriggerType
                    && userIds.Contains(x.UserId))
                .Include(i => i.User)
                .Include(i => i.Project)
                .ToList();

            _emailSenderRespository.SendCtmsSiteContractApprovalEmail(approverDetails);
        }

        private void UpdateSiteContractApprovalStatus(int siteContractId)
        {
            var siteContract = _siteContractRepository.Find(siteContractId);
            if (siteContract != null)
            {
                siteContract.IsApproved = true;
                _siteContractRepository.Update(siteContract);
                _uow.Save();
            }
        }
    }
}
