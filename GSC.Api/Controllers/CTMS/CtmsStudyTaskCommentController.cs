using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using System.Linq;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsStudyTaskCommentController : BaseController
    {
        private readonly ICtmsStudyPlanTaskCommentRepository _ctmsStudyPlanTaskCommentRepository;
        private readonly ICtmsWorkflowApprovalRepository _ctmsWorkflowApprovalRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public CtmsStudyTaskCommentController(ICtmsStudyPlanTaskCommentRepository ctmsStudyPlanTaskCommentRepository
            , IJwtTokenAccesser jwtTokenAccesser
            , IMapper mapper
            , IUnitOfWork uow
            , ICtmsWorkflowApprovalRepository ctmsWorkflowApprovalRepository)
        {
            _ctmsStudyPlanTaskCommentRepository = ctmsStudyPlanTaskCommentRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _uow = uow;
            _ctmsWorkflowApprovalRepository = ctmsWorkflowApprovalRepository;
        }

        [HttpPost]
        public IActionResult Post([FromBody] CtmsStudyPlanTaskCommentDto ctmsStudyPlanTaskCommentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ctmsStudyPlanTaskCommentDto.IpAddress = _jwtTokenAccesser.IpAddress;
            ctmsStudyPlanTaskCommentDto.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");

            var studyPlanTaskComment = _mapper.Map<CtmsStudyPlanTaskComment>(ctmsStudyPlanTaskCommentDto);
            var approvalComment = _ctmsWorkflowApprovalRepository.All.FirstOrDefault(x => x.DeletedDate == null
            && x.StudyPlanId == ctmsStudyPlanTaskCommentDto.StudyPlanId
            && x.ProjectId == ctmsStudyPlanTaskCommentDto.ProjectId
            && x.IsApprove == null
            && x.TriggerType == TriggerType.StudyPlanApproval
            && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId);
            if (approvalComment != null)
            {
                studyPlanTaskComment.CtmsWorkflowApprovalId = approvalComment.Id;
                _ctmsStudyPlanTaskCommentRepository.Add(studyPlanTaskComment);
                var result = _uow.Save();
                if (result <= 0)
                {
                    ModelState.AddModelError("Message", "Error to save");
                    return BadRequest(ModelState);
                }
                return Ok(studyPlanTaskComment.Id);
            }
            else
            {
                ModelState.AddModelError("Message", "Error to save");
                return BadRequest(ModelState);
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody] CtmsStudyPlanTaskCommentDto data)
        {
            if (data.Id <= 0) return BadRequest();
            var taskComment = _ctmsStudyPlanTaskCommentRepository.Find(data.Id);
            taskComment.ReplyComment = data.ReplyComment;
            taskComment.IsReply = true;
            taskComment.CtmsWorkflowApprovalId = data.CtmsWorkflowApprovalId;
            _ctmsStudyPlanTaskCommentRepository.Update(taskComment);
            _uow.Save();

            return Ok(taskComment.Id);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var taskComments = _ctmsStudyPlanTaskCommentRepository.All.Where(x => x.StudyPlanTaskId == id
            && x.CtmsWorkflowApproval.IsApprove != null && x.DeletedDate == null && x.CtmsWorkflowApproval.DeletedDate == null)
                .ProjectTo<CtmsStudyPlanTaskCommentGridDto>(_mapper.ConfigurationProvider).ToList();

            taskComments.ForEach(x =>
            {
                x.HasChild = x.ChildId != null;
            });

            return Ok(taskComments);
        }

        [HttpGet("GetCommentHistory/{id}/{studyPlanId}/{triggerType}")]
        public IActionResult GetCommentHistory(int id, int studyPlanId, TriggerType triggerType)
        {
            if (studyPlanId <= 0) return BadRequest();
            var taskComments = _ctmsStudyPlanTaskCommentRepository.GetCommentHistory(id, studyPlanId, triggerType);
            return Ok(taskComments);
        }

        [HttpGet("GetIReplyAllComment/{ctmsApprovalId}")]
        public IActionResult GetIReplyAllComment(int ctmsApprovalId)
        {
            var isReply = _ctmsStudyPlanTaskCommentRepository.CheckAllTaskComment(ctmsApprovalId);
            return Ok(isReply);
        }

        [HttpGet("GetSenderTaskComment/{id}/{userId}/{roleId}/{studyPlanId}/{triggerType}")]
        public IActionResult GetSenderTaskComment(int id, int userId, int roleId, int studyPlanId, TriggerType triggerType)
        {
            var senderComments = _ctmsStudyPlanTaskCommentRepository.GetSenderCommentHistory(id, userId, roleId, studyPlanId, triggerType);
            return Ok(senderComments);
        }
    }
}
