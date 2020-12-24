using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Workflow;
using GSC.Respository.Screening;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IUnitOfWork _uow;

        public ScreeningTemplateController(IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningEntryRepository screeningEntryRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IUnitOfWork uow, IMapper mapper,
            IScreeningTemplateReviewRepository screeningTemplateReviewRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IProjectSubjectRepository projectSubjectRepository,
            IScreeningVisitRepository screeningVisitRepository,
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
            _screeningVisitRepository = screeningVisitRepository;
        }






        [HttpPost("Repeat/{screeningTemplateId}")]
        public IActionResult Repeat(int screeningTemplateId)
        {
            if (screeningTemplateId <= 0) return BadRequest();

            var screeningTemplate = _screeningTemplateRepository.TemplateRepeat(screeningTemplateId);
            _uow.Save();
            return Ok(screeningTemplate.Id);
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

        [HttpGet]
        [AllowAnonymous]
        [Route("GetProjectDesignTemplateList/{projectDesignVisitId}")]
        public IActionResult GetProjectDesignTemplateList([FromRoute] int projectDesignVisitId)
        {
            var projectdesignTemplates = _projectDesignTemplateRepository.FindByInclude(x => x.ProjectDesignVisitId == projectDesignVisitId && x.IsParticipantView == true).ToList();
            return Ok(projectdesignTemplates);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetProjectDesignTemplate/{projectDesignTemplateId}")]
        public IActionResult GetProjectDesignTemplates([FromRoute] int projectDesignTemplateId)
        {
            var designTemplate = _projectDesignTemplateRepository.GetTemplate(projectDesignTemplateId);
            return Ok(designTemplate);
        }

        [HttpGet]
        [Route("GetProjectDesignVariableList/{id}/{projectDesignTemplateId}")]
        public IActionResult GetProjectDesignVariableList([FromRoute] int id, int projectDesignTemplateId)
        {
            var designTemplate = _projectDesignTemplateRepository.GetTemplate(projectDesignTemplateId);
            return Ok(_screeningTemplateRepository.GetScreeningTemplate(designTemplate, id));
        }

        [HttpGet]
        [Route("GetTemplate/{id}/{projectDesignTemplateId}")]
        public IActionResult GetTemplate([FromRoute] int id, int projectDesignTemplateId)
        {
            var designTemplate = _projectDesignTemplateRepository.GetTemplate(projectDesignTemplateId);

            return Ok(_screeningTemplateRepository.GetScreeningTemplate(designTemplate, id));
        }

        [HttpPut]
        [Route("SubmitTemplate/{id}")]
        [TransactionRequired]
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

            if (_uow.Save() <= 0) throw new Exception("SubmitTemplate Failed!");

            _screeningTemplateRepository.SubmitReviewTemplate(id, false);

            _uow.Save();

            var result = _screeningVisitRepository.AutomaticStatusUpdate(id);

            _uow.Save();

            return Ok(result);
        }

        private void SubmittedTemplate(int id)
        {
            if (_screeningTemplateReviewRepository.All.Any(x => x.ScreeningTemplateId == id
                                                                && x.Status == ScreeningTemplateStatus.Submitted && !x.IsRepeat))
            {
                ModelState.AddModelError("Message", "Template already submitted!");
                BadRequest(ModelState);
                return;
            }

            var screeningTemplate = _screeningTemplateRepository.Find(id);
            var projectDesignId = _screeningTemplateRepository.GetProjectDesignId(id);
            screeningTemplate.Status = ScreeningTemplateStatus.Submitted;
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
            screeningTemplate.IsDisable = false;

            _screeningTemplateReviewRepository.Save(screeningTemplate.Id, screeningTemplate.Status, 0);

            _screeningTemplateValueRepository.UpdateVariableOnSubmit(screeningTemplate.ProjectDesignTemplateId,
                screeningTemplate.Id, null);

            _screeningTemplateRepository.Update(screeningTemplate);


            Ok();
        }

        [HttpPut]
        [Route("ReviewedTemplate/{id}")]
        [TransactionRequired]
        public IActionResult ReviewedTemplate(int id)
        {
            _screeningTemplateRepository.SubmitReviewTemplate(id, false);

            _uow.Save();

            var screeningTemplate = _screeningTemplateRepository.Find(id);

            var validateMsg = _screeningTemplateValueRepository.CheckCloseQueries(id);

            if (!string.IsNullOrEmpty(validateMsg))
            {
                ModelState.AddModelError("Message", validateMsg);
                return BadRequest(ModelState);
            }

            if (_screeningTemplateReviewRepository.All.Any(x => x.ScreeningTemplateId == id
                                                                && x.CreatedBy == _jwtTokenAccesser.UserId &&
                                                                x.Status == ScreeningTemplateStatus.Reviewed
                                                                && x.RoleId == _jwtTokenAccesser.RoleId && !x.IsRepeat))
            {
                ModelState.AddModelError("Message", "Template already review!");
                return BadRequest(ModelState);
            }

            screeningTemplate.Status = ScreeningTemplateStatus.Reviewed;

            _screeningTemplateReviewRepository.Save(screeningTemplate.Id, screeningTemplate.Status, (short)screeningTemplate.ReviewLevel);

            screeningTemplate.ReviewLevel = Convert.ToInt16(screeningTemplate.ReviewLevel + 1);

            var projectDesignId = _screeningTemplateRepository.GetProjectDesignId(screeningTemplate.Id);

            if (screeningTemplate.ReviewLevel > _projectWorkflowRepository.GetMaxWorkFlowLevel(projectDesignId))
            {
                screeningTemplate.IsCompleteReview = true;
                screeningTemplate.Status = ScreeningTemplateStatus.Completed;
            }

            _screeningTemplateRepository.Update(screeningTemplate);

            _uow.Save();



            return Ok(id);
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



        [HttpPut]
        [Route("SubmitAttendanceTemplate/{id}")]
        [TransactionRequired]
        public IActionResult SubmitAttendanceTemplate(int id)
        {
            var attendanceId = _screeningTemplateRepository.All.Where(x => x.Id == id).Select(r => r.ScreeningVisit.ScreeningEntry.AttendanceId).FirstOrDefault();

            SubmittedTemplate(id);

            _projectSubjectRepository.SaveSubjectForVolunteer((int)attendanceId, id);

            if (_uow.Save() <= 0) throw new Exception("Submit Attendance Template failed.");

            _screeningTemplateRepository.SubmitReviewTemplate(id, false);

            _uow.Save();

            return Ok();
        }

        [HttpPut]
        [Route("SubmitDiscontinueTemplate/{id}")]
        public IActionResult SubmitDiscontinueTemplate(int id)
        {
            var attendanceId = _screeningTemplateRepository.All.Where(x => x.Id == id).Select(r => r.ScreeningVisit.ScreeningEntry.AttendanceId).FirstOrDefault();
            SubmitTemplate(id);

            _projectSubjectRepository.DiscontinueProjectSubject((int)attendanceId, id);

            if (_uow.Save() <= 0) throw new Exception("Submit Discontinue Template failed.");

            return Ok();
        }

        [HttpGet]
        [Route("GetTemplateByLockedDropDown")]
        public IActionResult GetTemplateByLockedDropDown([FromQuery] LockUnlockDDDto lockUnlockDDDto)
        {
            return Ok(_screeningTemplateRepository.GetTemplateByLockedDropDown(lockUnlockDDDto));
        }

        [HttpPost]
        [Route("GetvisitdeviationReport")]
        public IActionResult GetvisitdeviationReport([FromBody] VisitDeviationReportSearchDto filters)
        {
            if (filters.ProjectId <= 0) return BadRequest();

            var auditsDto = _screeningTemplateRepository.GetVisitDeviationReport(filters);

            return Ok(auditsDto);
        }
    }
}