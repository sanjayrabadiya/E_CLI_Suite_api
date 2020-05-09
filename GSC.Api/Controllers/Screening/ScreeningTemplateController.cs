using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.EditCheck;
using GSC.Respository.Project.Workflow;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningTemplateController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectSubjectRepository _projectSubjectRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IScreeningEntryRepository _screeningEntryRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateReviewRepository _screeningTemplateReviewRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IUnitOfWork<GscContext> _uow;

        public ScreeningTemplateController(IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningEntryRepository screeningEntryRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IScreeningTemplateReviewRepository screeningTemplateReviewRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IProjectSubjectRepository projectSubjectRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _screeningTemplateRepository = screeningTemplateRepository;
            _screeningEntryRepository = screeningEntryRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _uow = uow;
            _mapper = mapper;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateReviewRepository = screeningTemplateReviewRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _projectSubjectRepository = projectSubjectRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var screeningEntries = _screeningTemplateRepository.FindBy(x => x.IsDeleted == isDeleted).ToList();
            var screeningTemplateiesDto = _mapper.Map<IEnumerable<ScreeningTemplateDto>>(screeningEntries);
            return Ok(screeningTemplateiesDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var screeningTemplate = _screeningTemplateRepository.Find(id);
            var screeningTemplateDto = _mapper.Map<ScreeningTemplateDto>(screeningTemplate);
            return Ok(screeningTemplateDto);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ScreeningTemplateDto screeningTemplateDto)
        {
            if (screeningTemplateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var screeningTemplate = _mapper.Map<ScreeningTemplate>(screeningTemplateDto);

            _screeningEntryRepository.Find(screeningTemplate.ScreeningEntryId);

            _screeningTemplateRepository.Update(screeningTemplate);
            if (_uow.Save() <= 0) throw new Exception("Updating Screening Template failed on save.");
            return Ok(screeningTemplate.Id);
        }

        [HttpPost("Repeat/{screeningTemplateId}")]
        public IActionResult Repeat(int screeningTemplateId)
        {
            if (screeningTemplateId <= 0) return BadRequest();

            var screeningTemplate = _screeningTemplateRepository.TemplateRepeat(screeningTemplateId);
            if (_uow.Save() <= 0) throw new Exception("Repeat Template failed on save.");
            return Ok(screeningTemplate.Id);
        }

        [HttpPost("VisitRepeat/{projectDesignVisitId}/{screeningEntryId}")]
        public IActionResult VisitRepeat(int projectDesignVisitId, int screeningEntryId)
        {
            _screeningTemplateRepository.VisitRepeat(projectDesignVisitId, screeningEntryId);
            if (_uow.Save() <= 0) throw new Exception("Visit Repeat failed on save.");
            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _screeningTemplateRepository.Find(id);

            if (record == null)
                return NotFound();

            _screeningTemplateRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _screeningTemplateRepository.Find(id);

            if (record == null)
                return NotFound();
            _screeningTemplateRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpPost("GetTemplate")]
        public IActionResult GetTemplate([FromBody] ScreeningTemplateDto screeningTemplate)
        {
            if (screeningTemplate.Id <= 0) return BadRequest();

            if (screeningTemplate.ProjectDesignTemplateId <= 0) return BadRequest();
            var designTemplate =
                _projectDesignTemplateRepository.GetTemplate(screeningTemplate.ProjectDesignTemplateId);
            var designTemplateDto = _mapper.Map<ProjectDesignTemplateDto>(designTemplate);
            designTemplateDto.ProjectDesignVisitName = designTemplate.ProjectDesignVisit.DisplayName;

            return Ok(_screeningTemplateRepository.GetScreeningTemplate(designTemplateDto, screeningTemplate));
        }

        [HttpPut]
        [Route("SubmitTemplate/{id}")]
        public IActionResult SubmitTemplate(int id)
        {
            if (_projectDesignTemplateRepository.All.Any(x =>
                x.Id == id && x.ProjectDesignVisit.ProjectDesignPeriod.DiscontinuedTemplateId == id))
                SubmitDiscontinueTemplate(id);
            else if (_projectDesignTemplateRepository.All.Any(x =>
                x.Id == id && x.ProjectDesignVisit.ProjectDesignPeriod.AttendanceTemplateId == id))
                SubmitAttendanceTemplate(id);
            else
                SubmittedTemplate(id);

            if (_uow.Save() <= 0) throw new Exception("Creating Project Design Period failed on clone period.");

            return Ok();
        }

        private void SubmittedTemplate(int id)
        {
            if (_screeningTemplateReviewRepository.All.Any(x => x.ScreeningTemplateId == id
                                                                && x.Status == ScreeningStatus.Submitted))
            {
                ModelState.AddModelError("Message", "Template already submitted!");
                BadRequest(ModelState);
                return;
            }

            var screeningTemplate = _screeningTemplateRepository.Find(id);
            var projectDesignId = _screeningEntryRepository.Find(screeningTemplate.ScreeningEntryId).ProjectDesignId;
            screeningTemplate.Status = ScreeningStatus.Submitted;
            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesignId);
            if (workflowlevel.IsWorkFlowBreak)
            {
                screeningTemplate.ReviewLevel = Convert.ToInt16(workflowlevel.LevelNo + 1);
                screeningTemplate.StartLevel = workflowlevel.LevelNo;
            }
            else
            {
                screeningTemplate.ReviewLevel = 1;
                screeningTemplate.StartLevel = -1;
            }

            screeningTemplate.ScreeningTemplateReview = new List<ScreeningTemplateReview>();
            screeningTemplate.ScreeningTemplateReview.Add(new ScreeningTemplateReview
            {
                Status = ScreeningStatus.Submitted,
                ReviewLevel = 0,
                RoleId = _jwtTokenAccesser.RoleId
            });

            _screeningTemplateValueRepository.UpdateVariableOnSubmit(screeningTemplate.ProjectDesignTemplateId,
                screeningTemplate.Id, null);

            _screeningTemplateRepository.Update(screeningTemplate);

            if (_screeningTemplateRepository.All.Any(x =>
                x.DeletedDate == null && x.ScreeningEntryId == screeningTemplate.ScreeningEntryId && x.Id != id &&
                x.Status > ScreeningStatus.InProcess))
            {
                var screeningEntry = _screeningEntryRepository.Find(screeningTemplate.ScreeningEntryId);
                screeningEntry.Status = ScreeningStatus.Submitted;
            }

            Ok();
        }

        [HttpPut]
        [Route("ReviewedTemplate/{id}")]
        public IActionResult ReviewedTemplate(int id)
        {
            var screeningTemplate = _screeningTemplateRepository.Find(id);

            var screeningTemplateValue = _screeningTemplateValueRepository.FindByInclude(x =>
                x.ScreeningTemplateId == id && x.DeletedDate == null, x => x.ProjectDesignVariable).ToList();

            var validateMsg = _screeningTemplateValueRepository.CheckCloseQueries(screeningTemplateValue);

            if (!string.IsNullOrEmpty(validateMsg))
            {
                ModelState.AddModelError("Message", validateMsg);
                return BadRequest(ModelState);
            }

            if (_screeningTemplateReviewRepository.All.Any(x => x.ScreeningTemplateId == id
                                                                && x.CreatedBy == _jwtTokenAccesser.UserId &&
                                                                x.Status == ScreeningStatus.Reviewed
                                                                && x.RoleId == _jwtTokenAccesser.RoleId && !x.IsRepeat))
            {
                ModelState.AddModelError("Message", "Template already review!");
                return BadRequest(ModelState);
            }

            screeningTemplate.Status = ScreeningStatus.Reviewed;

            screeningTemplate.ScreeningTemplateReview = new List<ScreeningTemplateReview>();
            screeningTemplate.ScreeningTemplateReview.Add(new ScreeningTemplateReview
            {
                Status = ScreeningStatus.Reviewed,
                ReviewLevel = Convert.ToInt16(screeningTemplate.ReviewLevel),
                RoleId = _jwtTokenAccesser.RoleId
            });
            screeningTemplate.ReviewLevel = Convert.ToInt16(screeningTemplate.ReviewLevel + 1);
            var projectDesignId = _screeningEntryRepository.Find(screeningTemplate.ScreeningEntryId).ProjectDesignId;
            if (screeningTemplate.ReviewLevel > _projectWorkflowRepository.GetMaxWorkFlowLevel(projectDesignId))
                screeningTemplate.IsCompleteReview = true;

            _screeningTemplateRepository.Update(screeningTemplate);

            if (_uow.Save() <= 0) throw new Exception("Failed Template Review");

            return Ok();
        }
               

        [HttpGet]
        [Route("GetScreeningTemplateReview")]
        public IActionResult GetScreeningTemplateReview()
        {
            return Ok(_screeningTemplateRepository.GetScreeningTemplateReview());
        }

        [HttpGet]
        [Route("GetTemplateReviewHistory/{id}")]
        public IActionResult GetTemplateReviewHistory(int id)
        {
            return Ok(_screeningTemplateReviewRepository.GetTemplateReviewHistory(id));
        }

        [HttpPost("GetTemplatesLockUnlock")]
        public IActionResult GetTemplatesLockUnlock([FromBody] ScreeningTemplateLockUnlockParams lockUnlockParams)
        {
            if (lockUnlockParams.ProjectId <= 0 || lockUnlockParams.VolunteerId <= 0) return BadRequest();

            var lockUnlockTemplates = _screeningTemplateRepository.GetTemplatesLockUnlock(lockUnlockParams);

            return Ok(lockUnlockTemplates);
        }

        [HttpPut]
        [Route("LockUnlockTemplate/{id}/{status}")]
        public IActionResult LockUnlockTemplate(int id, ScreeningStatus status)
        {
            var screeningTemplate = _screeningTemplateRepository.Find(id);

            if (status == ScreeningStatus.Completed && _screeningTemplateValueRepository.All.Any(x =>
                    x.DeletedDate == null && x.ScreeningTemplateId == id && !x.ScreeningTemplate.IsCompleteReview))
            {
                ModelState.AddModelError("Message", "This template under review!");
                return BadRequest(ModelState);
            }

            var screeningTemplateValue = _screeningTemplateValueRepository.FindByInclude(x =>
                x.ScreeningTemplateId == id && x.DeletedDate == null, x => x.ProjectDesignVariable).ToList();
            var validateMsg = _screeningTemplateValueRepository.CheckCloseQueries(screeningTemplateValue);

            if (!string.IsNullOrEmpty(validateMsg))
            {
                ModelState.AddModelError("Message", validateMsg);
                return BadRequest(ModelState);
            }

            screeningTemplate.Status = status;

            _screeningTemplateRepository.Update(screeningTemplate);

            if (_uow.Save() <= 0) throw new Exception("Failed Lock Unlock Template");

            return Ok();
        }

        [HttpPut]
        [Route("SubmitAttendanceTemplate/{id}")]
        public IActionResult SubmitAttendanceTemplate(int id)
        {
            var screeningTemplate = _screeningTemplateRepository.Find(id);

            SubmittedTemplate(id);

            var screeningEntry = _screeningEntryRepository.Find(screeningTemplate.ScreeningEntryId);
            _projectSubjectRepository.SaveSubjectForVolunteer(screeningEntry.AttendanceId, id);

            if (_uow.Save() <= 0) throw new Exception("Submit Attendance Template failed.");

            return Ok();
        }

        [HttpPut]
        [Route("SubmitDiscontinueTemplate/{id}")]
        public IActionResult SubmitDiscontinueTemplate(int id)
        {
            var screeningTemplate = _screeningTemplateRepository.Find(id);
            SubmitTemplate(id);

            var screeningEntry = _screeningEntryRepository.Find(screeningTemplate.ScreeningEntryId);
            _projectSubjectRepository.DiscontinueProjectSubject(screeningEntry.AttendanceId, id);

            if (_uow.Save() <= 0) throw new Exception("Submit Discontinue Template failed.");

            return Ok();
        }


        [HttpGet]
        [Route("GetDashboardStudyStatusByVisit/{projectId}")]
        public IActionResult GetDashboardStudyStatusByVisit(int projectId)
        {
            return Ok(_screeningTemplateRepository.GetDashboardStudyStatusByVisit(projectId));
        }

        [HttpGet]
        [Route("GetDashboardStudyStatusBySite/{projectId}")]
        public IActionResult GetDashboardStudyStatusBySite(int projectId)
        {
            return Ok(_screeningTemplateRepository.GetDashboardStudyStatusBySite(projectId));
        }
    }
}