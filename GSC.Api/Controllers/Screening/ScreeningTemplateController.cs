using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Workflow;
using GSC.Respository.Screening;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningTemplateController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectSubjectRepository _projectSubjectRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateReviewRepository _screeningTemplateReviewRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IUnitOfWork _uow;
        private readonly IScreeningProgress _screeningProgress;
        private readonly IScheduleTerminate _scheduleTerminate;

        public ScreeningTemplateController(IScreeningTemplateRepository screeningTemplateRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScreeningTemplateReviewRepository screeningTemplateReviewRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IProjectSubjectRepository projectSubjectRepository,
            IScreeningVisitRepository screeningVisitRepository,
            IScreeningProgress screeningProgress,
            IJwtTokenAccesser jwtTokenAccesser, IUnitOfWork uow,
           IScheduleTerminate scheduleTerminate)
        {
            _screeningTemplateRepository = screeningTemplateRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _uow = uow;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateReviewRepository = screeningTemplateReviewRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _projectSubjectRepository = projectSubjectRepository;
            _screeningVisitRepository = screeningVisitRepository;
            _screeningProgress = screeningProgress;
            _scheduleTerminate = scheduleTerminate;
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
        [Route("GetProjectDesignTemplateList/{projectDesignVisitId}")]
        public IActionResult GetProjectDesignTemplateList([FromRoute] int projectDesignVisitId)
        {
            var projectdesignTemplates = _projectDesignTemplateRepository.FindByInclude(x => x.ProjectDesignVisitId == projectDesignVisitId && x.IsParticipantView == true).ToList();
            return Ok(projectdesignTemplates);
        }

        [HttpGet]
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

            _uow.Save();

            _screeningTemplateRepository.SubmitReviewTemplate(id, false);

            _uow.Save();

            var tempValue = _screeningTemplateRepository.All.Where(x => x.Id == id).Select(t => new
            {
                t.ScreeningVisit.ScreeningEntryId,
                t.ProjectDesignTemplateId
            }).FirstOrDefault();

            _screeningProgress.GetScreeningProgress(tempValue.ScreeningEntryId, id);

            var result = _screeningVisitRepository.AutomaticStatusUpdate(id);

            _uow.Save();

            _scheduleTerminate.TerminateScheduleTemplateVisit(tempValue.ProjectDesignTemplateId, tempValue.ScreeningEntryId, false);

            return Ok(result);
        }


        [HttpPut]
        [Route("UnSubmitTemplate/{id}")]
        [TransactionRequired]
        public IActionResult UnSubmitTemplate(int id)
        {
            var validateMsg = _screeningTemplateValueRepository.CheckCloseQueries(id);

            if (!string.IsNullOrEmpty(validateMsg))
            {
                ModelState.AddModelError("Message", "Queries generated, please close it");
                return BadRequest(ModelState);
            }

            var screeningTemplate = _screeningTemplateRepository.Find(id);

            screeningTemplate.Status = ScreeningTemplateStatus.InProcess;
            screeningTemplate.ReviewLevel = null;
            screeningTemplate.StartLevel = null;

            var screeningTemplateReview = _screeningTemplateReviewRepository.All.Where(x => x.ScreeningTemplateId == id).ToList();

            screeningTemplateReview.ForEach(c =>
            {
                c.IsRepeat = true;
                _screeningTemplateReviewRepository.Update(c);
            });
            _screeningTemplateRepository.Update(screeningTemplate);
            _uow.Save();

            return Ok(screeningTemplate);
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
            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesignId);

            screeningTemplate.ReviewLevel = 1;
            screeningTemplate.StartLevel = -1;

            var isNonCRF = _screeningTemplateRepository.All.Any(x => x.Id == id && x.ScreeningVisit.ProjectDesignVisit.IsNonCRF);
            if (isNonCRF)
                screeningTemplate.ReviewLevel = _projectWorkflowRepository.GetNoCRFLevel(projectDesignId, (short)screeningTemplate.StartLevel);

            screeningTemplate.Status = ScreeningTemplateStatus.Submitted;
            screeningTemplate.IsDisable = false;
            screeningTemplate.IsHide = false;

            screeningTemplate.Progress = 100; // Add by Tinku progress 100% if form is submited

            _screeningTemplateReviewRepository.Save(screeningTemplate.Id, screeningTemplate.Status, 0);

            _screeningTemplateValueRepository.UpdateVariableOnSubmit(screeningTemplate.ProjectDesignTemplateId, screeningTemplate.Id);

            CheckCompletedStatus(screeningTemplate, projectDesignId);

            _screeningTemplateRepository.Update(screeningTemplate);

            _screeningTemplateRepository.SendEmailOnVaribleConfiguration(id);

            Ok();
        }


        [HttpPut]
        [Route("UnReviewedTemplate/{id}")]
        [TransactionRequired]
        public IActionResult UnReviewedTemplate(int id)
        {
            var validateMsg = _screeningTemplateValueRepository.CheckCloseQueries(id);

            if (!string.IsNullOrEmpty(validateMsg))
            {
                ModelState.AddModelError("Message", "Queries generated, please close it");
                return BadRequest(ModelState);
            }

            var screeningTemplate = _screeningTemplateRepository.Find(id);
            screeningTemplate.Status = ScreeningTemplateStatus.Reviewed;
            var lastReviewLevel = screeningTemplate.LastReviewLevel;
            screeningTemplate.ReviewLevel = screeningTemplate.LastReviewLevel;
            screeningTemplate.LastReviewLevel = null;
            screeningTemplate.IsCompleteReview = false;
            _screeningTemplateRepository.Update(screeningTemplate);

            var screeningTemplateReview = _screeningTemplateReviewRepository.All.Where(x => x.ScreeningTemplateId == id && x.ReviewLevel == lastReviewLevel).ToList();

            screeningTemplateReview.ForEach(c =>
            {
                c.IsRepeat = true;
                _screeningTemplateReviewRepository.Update(c);
            });

            _uow.Save();

            return Ok(screeningTemplate);
        }


        [HttpPut]
        [Route("ReviewedTemplate/{id}")]
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
            screeningTemplate.LastReviewLevel = screeningTemplate.ReviewLevel;

            _screeningTemplateReviewRepository.Save(screeningTemplate.Id, screeningTemplate.Status, (short)screeningTemplate.ReviewLevel);

            var projectDesignId = _screeningTemplateRepository.GetProjectDesignId(screeningTemplate.Id);

            var isNonCRF = _screeningTemplateRepository.All.Any(x => x.Id == id && x.ScreeningVisit.ProjectDesignVisit.IsNonCRF);
            if (isNonCRF)
                screeningTemplate.ReviewLevel = _projectWorkflowRepository.GetNoCRFLevel(projectDesignId, (short)screeningTemplate.ReviewLevel);
            else
                screeningTemplate.ReviewLevel = Convert.ToInt16(screeningTemplate.ReviewLevel + 1);

            CheckCompletedStatus(screeningTemplate, projectDesignId);
            screeningTemplate.Status = ScreeningTemplateStatus.Reviewed;
            _screeningTemplateRepository.Update(screeningTemplate);

            _uow.Save();

            return Ok(id);
        }

        void CheckCompletedStatus(ScreeningTemplate screeningTemplate, int projectDesignId)
        {
            if (screeningTemplate.ReviewLevel > _projectWorkflowRepository.GetMaxWorkFlowLevel(projectDesignId))
            {
                screeningTemplate.IsCompleteReview = true;
                screeningTemplate.Status = ScreeningTemplateStatus.Completed;
            }
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
            SubmittedTemplate(id);

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

        [HttpGet]
        [Route("CheckLockedProject/{ProjectId}")]
        public IActionResult CheckLockedProject(int ProjectId)
        {
            return Ok(_screeningTemplateRepository.CheckLockedProject(ProjectId));
        }

        [HttpPost]
        [Route("GetScheduleDueReport")]
        public IActionResult GetScheduleDueReport([FromBody] ScheduleDueReportSearchDto filters)
        {
            if (filters.ProjectId <= 0) return BadRequest();

            var auditsDto = _screeningTemplateRepository.GetScheduleDueReport(filters);

            return Ok(auditsDto);
        }

        [HttpGet]
        [Route("GetProjectDesignVariableListForGridView/{id}/{projectDesignTemplateId}")]
        public IActionResult GetProjectDesignVariableListForGridView([FromRoute] int id, int projectDesignTemplateId)
        {
            var designTemplate = _projectDesignTemplateRepository.GetTemplate(projectDesignTemplateId);
            return Ok(_screeningTemplateRepository.GetScreeningGridView(designTemplate, id));
        }

        [HttpPut]
        [Route("DeleteRepeatTemplate/{id}")]
        [TransactionRequired]
        public IActionResult DeleteRepeatTemplate(int id)
        {
            _screeningTemplateValueRepository.DeleteRepeatTemplateValue(id);
            var record = _screeningTemplateRepository.Find(id);
            _screeningTemplateRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPut]
        [Route("DeleteRepeatVisit/{id}")]
        [TransactionRequired]
        public IActionResult DeleteRepeatVisit(int id)
        {
            _screeningTemplateRepository.DeleteRepeatVisitTemplate(id);
            var visit = _screeningVisitRepository.Find(id);

            if (visit == null)
                return NotFound();

            _screeningVisitRepository.Delete(visit);
            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("GetTemplateForBarcode/{id}/{projectDesignTemplateId}/{isDosing}/{firstTime}")]
        public IActionResult GetTemplateForBarcode([FromRoute] int id, int projectDesignTemplateId, bool isDosing, bool firstTime)
        {
            var designTemplate = _projectDesignTemplateRepository.GetTemplate(projectDesignTemplateId);

            return Ok(_screeningTemplateRepository.GetTemplateForBarcode(designTemplate, id, isDosing, firstTime));
        }

    }
}