using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.EditCheck;
using GSC.Respository.Project.Schedule;
using GSC.Respository.Project.Workflow;
using GSC.Respository.Screening;
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningTemplateLockUnlockController : BaseController
    {
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateLockUnlockRepository _screeningTemplateLockUnlockRepository;
        private readonly IScreeningEntryRepository _screeningEntryRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateReviewRepository _screeningTemplateReviewRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IProjectSubjectRepository _projectSubjectRepository;
        public ScreeningTemplateLockUnlockController(IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningTemplateLockUnlockRepository screeningTemplateLockUnlockRepository,
            IScreeningEntryRepository screeningEntryRepository,
            IAttendanceRepository attendanceRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IUnitOfWork uow, IMapper mapper,
            IScreeningTemplateReviewRepository screeningTemplateReviewRepository,
             IProjectWorkflowRepository projectWorkflowRepository,
             IProjectSubjectRepository projectSubjectRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _screeningTemplateRepository = screeningTemplateRepository;
            _screeningTemplateLockUnlockRepository = screeningTemplateLockUnlockRepository;
            _screeningEntryRepository = screeningEntryRepository;
            _attendanceRepository = attendanceRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _uow = uow;
            _mapper = mapper;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateReviewRepository = screeningTemplateReviewRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _projectSubjectRepository = projectSubjectRepository;
        }

        [HttpGet]
        [Route("GetLockUnlockList")]
        public IActionResult GetLockUnlockList([FromQuery] LockUnlockSearchDto lockUnlockParams)
        {
            if (lockUnlockParams.ProjectId <= 0)
            {
                return BadRequest();
            }

            var lockUnlockTemplates = _screeningTemplateRepository.GetLockUnlockList(lockUnlockParams);

            return Ok(lockUnlockTemplates);
        }

        [HttpPut]
        [Route("LockUnlockTemplateList/{status}")]
        [TransactionRequired]
        public IActionResult LockUnlockTemplateList([FromBody] ScreeningTemplateLockUnlockAuditDto item, ScreeningTemplateStatus status)
        {
            var screeningTemplate = _screeningTemplateRepository.FindByInclude(x => x.Id == item.ScreeningTemplateId && x.DeletedDate == null).FirstOrDefault();

            if (item.IsLocked)
                CheckEditCheck(item.ScreeningTemplateId);

            string validateMsg = _screeningTemplateValueRepository.CheckCloseQueries(item.ScreeningTemplateId);

            if (!string.IsNullOrEmpty(validateMsg))
            {
                ModelState.AddModelError("Message", validateMsg);
                return BadRequest(ModelState);
            }

            var screeningTemplateLockUnlock = _mapper.Map<ScreeningTemplateLockUnlockAudit>(item);
            _screeningTemplateLockUnlockRepository.Insert(screeningTemplateLockUnlock);
            screeningTemplate.IsLocked = item.IsLocked;
            _screeningTemplateRepository.Update(screeningTemplate);
            if (_uow.Save() <= 0)
            {
                throw new Exception($"Failed Lock Unlock Template");
            }

            if (!item.IsLocked)
                CheckEditCheck(item.ScreeningTemplateId);
            return Ok();
        }

        void CheckEditCheck(int id)
        {
            _screeningTemplateRepository.SubmitReviewTemplate(id, true);
            _uow.Save();
        }

        [HttpGet]
        [Route("GetLockUnlockHistoryDetails/{projectId}/{parentProjectId}")]
        public IActionResult GetLockUnlockHistoryDetails(int projectId, int parentProjectId)
        {
            if (projectId <= 0) return BadRequest();
            return Ok(_screeningTemplateLockUnlockRepository.ProjectLockUnLockHistory(projectId, parentProjectId));
        }
    }
}
