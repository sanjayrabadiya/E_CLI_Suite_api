using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Helper;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using GSC.Respository.EmailSender;
using Microsoft.EntityFrameworkCore;


namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsWorkflowApprovalController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ICtmsWorkflowApprovalRepository _ctmsWorkflowApprovalRepository;
        private readonly ICtmsStudyPlanTaskCommentRepository _ctmsStudyPlanTaskCommentRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IStudyPlanRepository _studyPlanRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;

        public CtmsWorkflowApprovalController(IUnitOfWork uow, IMapper mapper,
            ICtmsWorkflowApprovalRepository ctmsWorkflowApprovalRepository, IJwtTokenAccesser
            jwtTokenAccesser, IStudyPlanRepository studyPlanRepository,
            ICtmsStudyPlanTaskCommentRepository ctmsStudyPlanTaskCommentRepository,
            IEmailSenderRespository emailSenderRespository)
        {
            _uow = uow;
            _mapper = mapper;
            _ctmsWorkflowApprovalRepository = ctmsWorkflowApprovalRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _studyPlanRepository = studyPlanRepository;
            _ctmsStudyPlanTaskCommentRepository = ctmsStudyPlanTaskCommentRepository;
            _emailSenderRespository = emailSenderRespository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var approval = _ctmsWorkflowApprovalRepository.Find(id);
            var approvalDto = _mapper.Map<CtmsWorkflowApprovalDto>(approval);
            return Ok(approvalDto);
        }

        [HttpGet]
        [Route("GetRolUserTree/{projectId}/{triggerType}")]
        public IActionResult GetRolUserTree(int projectId, TriggerType triggerType)
        {
            if (projectId <= 0) return BadRequest();
            return Ok(_ctmsWorkflowApprovalRepository.GetProjectRightByProjectId(projectId, triggerType));
        }

        [HttpPost("SaveApprovalRequest")]
        public IActionResult SaveApprovalRequest([FromBody] IList<CtmsWorkflowApprovalDto> approversDto)
        {
            if (approversDto.Count <= 0)
            {
                ModelState.AddModelError("Message", "List is empty");
                return BadRequest(ModelState);
            }
            foreach (var approverDto in approversDto)
            {
                approverDto.Id = 0;
                approverDto.SenderId = _jwtTokenAccesser.UserId;
                approverDto.SendDate = DateTime.Now;
                var approver = _mapper.Map<CtmsWorkflowApproval>(approverDto);
                _ctmsWorkflowApprovalRepository.Add(approver);
            }

            _uow.Save();

            var userIds = approversDto.Select(x => x.UserId).ToList();

            var approverDetails = _ctmsWorkflowApprovalRepository.All.Where(x => x.StudyPlanId == approversDto.First().StudyPlanId
            && x.ProjectId == approversDto.First().ProjectId && x.DeletedDate == null
            && x.TriggerType == approversDto.First().TriggerType && userIds.Contains(x.UserId)).Include(i => i.User).Include(i => i.Project).ToList();
            _emailSenderRespository.SendCtmsApprovalEmail(approverDetails);

            return Ok(approversDto.Count);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CtmsWorkflowApprovalDto approverDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            approverDto.Id = 0;
            approverDto.SendDate = DateTime.Now;
            approverDto.ApproverComment = "";
            approverDto.IsApprove = null;
            approverDto.ActionDate = null;
            approverDto.SenderId = _jwtTokenAccesser.UserId;
            var lastRecord = _ctmsWorkflowApprovalRepository.Find(approverDto.CtmsWorkflowApprovalId ?? 0);
            approverDto.CtmsWorkflowApprovalId = null;
            var approver = _mapper.Map<CtmsWorkflowApproval>(approverDto);
            _ctmsWorkflowApprovalRepository.Add(approver);
            var result = _uow.Save();
            lastRecord.CtmsWorkflowApprovalId = approver.Id;
            _ctmsWorkflowApprovalRepository.Update(lastRecord);

            if (approverDto.TriggerType == TriggerType.StudyPlanApproval)
            {
                var taskComments = _ctmsStudyPlanTaskCommentRepository.All.Where(x => x.CtmsWorkflowApprovalId == lastRecord.Id && x.DeletedDate == null && !x.FinalReply).ToList();
                taskComments.ForEach(l =>
                {
                    l.FinalReply = true;
                    _ctmsStudyPlanTaskCommentRepository.Update(l);
                });
            }

            _uow.Save();
            if (result <= 0)
            {
                ModelState.AddModelError("Message", "Error to save");
                return BadRequest(ModelState);
            }

            var approverDetails = _ctmsWorkflowApprovalRepository.All.Where(x => x.Id == approver.Id
            && x.StudyPlanId == approver.StudyPlanId && x.ProjectId == approver.ProjectId).Include(i => i.User).Include(i => i.Project).ToList();

            if (approverDetails.Any())
                _emailSenderRespository.SendCtmsApprovalEmail(approverDetails);

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

            var allApprove = _ctmsWorkflowApprovalRepository.GetApprovalStatus(approverDto.StudyPlanId, approverDto.ProjectId, approverDto.TriggerType);
            if (allApprove)
            {
                var studyPlan = _studyPlanRepository.Find(approverDto.StudyPlanId);
                if (approverDto.TriggerType == TriggerType.BudgetManagementApproved)
                {
                    studyPlan.IsBudgetApproval = true;
                }

                if (approverDto.TriggerType == TriggerType.StudyPlanApproval)
                {
                    studyPlan.IsPlanApproval = true;
                }

                _studyPlanRepository.Update(studyPlan);
                _uow.Save();


            }



            var approverDetails = _ctmsWorkflowApprovalRepository.All.Where(x => x.Id == approver.Id
          && x.StudyPlanId == approver.StudyPlanId && x.ProjectId == approver.ProjectId).Include(i => i.User).Include(i => i.Sender)
          .Include(i => i.Project).ToList();


            if (approverDetails.Any())
            {
                if (approver.IsApprove == false)
                    _emailSenderRespository.SendCtmsReciverEmail(approverDetails);
                else
                    _emailSenderRespository.SendCtmsApprovedEmail(approverDetails);
            }

            return Ok(approver.Id);
        }

        [HttpGet("GetApprovalHistoryBySender/{studyPlanId}/{projectId}/{triggerType}")]
        public IActionResult GetApprovalHistoryBySender(int studyPlanId, int projectId, TriggerType triggerType)
        {
            var history = _ctmsWorkflowApprovalRepository.GetApprovalBySender(studyPlanId, projectId, triggerType);
            return Ok(history);
        }

        [HttpGet("GetApprovalHistoryByApprover/{studyPlanId}/{projectId}/{triggerType}")]
        public IActionResult GetApprovalHistoryByApprover(int studyPlanId, int projectId, TriggerType triggerType)
        {
            var history = _ctmsWorkflowApprovalRepository.GetApprovalByApprover(studyPlanId, projectId, triggerType);
            return Ok(history);
        }

        [HttpGet("CheckIsSender/{studyPlanId}/{projectId}/{triggerType}")]
        public IActionResult CheckIsSender(int studyPlanId, int projectId, TriggerType triggerType)
        {
            var status = _ctmsWorkflowApprovalRepository.CheckSender(studyPlanId, projectId, triggerType);
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

        [HttpGet("GetApprovalUsers/{studyPlanId}")]
        public IActionResult GetApprovalUsers(int studyPlanId)
        {
            var result = _ctmsWorkflowApprovalRepository.GetApprovalUsers(studyPlanId);
            return Ok(result);
        }

        [HttpGet("CheckNewComment/{studyPlanId}/{projectId}/{triggerType}")]
        public IActionResult CheckNewComment(int studyPlanId, int projectId, TriggerType triggerType)
        {
            var result = _ctmsWorkflowApprovalRepository.IsNewComment(studyPlanId, projectId, triggerType);
            return Ok(result);
        }

        [HttpGet("CheckCommentReply/{studyPlanId}/{projectId}/{userId}/{roleId}/{triggerType}")]
        public IActionResult CheckCommentReply(int studyPlanId, int projectId, int userId, int roleId, TriggerType triggerType)
        {
            var result = _ctmsWorkflowApprovalRepository.IsCommentReply(studyPlanId, projectId, userId, roleId, triggerType);
            return Ok(result);
        }
    }
}
