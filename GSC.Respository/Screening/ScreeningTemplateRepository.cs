using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.Project.EditCheck;
using GSC.Respository.Project.Workflow;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateRepository : GenericRespository<ScreeningTemplate, GscContext>,
        IScreeningTemplateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IScreeningTemplateValueEditCheckRepository _screeningTemplateValueEditCheckRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IEditCheckImpactRepository _editCheckImpactRepository;
        public ScreeningTemplateRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IUploadSettingRepository uploadSettingRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IEditCheckImpactRepository editCheckImpactRepository,
            IScreeningTemplateValueEditCheckRepository screeningTemplateValueEditCheckRepository)
            : base(uow, jwtTokenAccesser)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _editCheckImpactRepository = editCheckImpactRepository;
            _screeningTemplateValueEditCheckRepository = screeningTemplateValueEditCheckRepository;
        }

        public ProjectDesignTemplateDto GetScreeningTemplate(ProjectDesignTemplateDto designTemplateDto,
            ScreeningTemplateDto screeningTemplate)
        {
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var screeningTemplateObject = Find(screeningTemplate.Id);

            var projectDesignId = Context.ScreeningEntry.Find(screeningTemplateObject.ScreeningEntryId).ProjectDesignId;

            var statusId = (int)screeningTemplateObject.Status;
            
            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesignId);

            designTemplateDto.IsSubmittedButton = statusId < 3 && workflowlevel.IsStartTemplate;
            if (workflowlevel.LevelNo >= 0 && designTemplateDto.IsRepeated)
                designTemplateDto.IsRepeated = workflowlevel.IsStartTemplate;
            if (screeningTemplateObject.ParentId != null)
                designTemplateDto.IsRepeated = false;

            designTemplateDto.MyReview = workflowlevel.LevelNo == screeningTemplateObject.ReviewLevel;
            designTemplateDto.ScreeningTemplateId = screeningTemplateObject.Id;
            designTemplateDto.IsLocked = screeningTemplate.IsLocked;
            designTemplateDto.Status = screeningTemplateObject.Status;
            designTemplateDto.StatusName = GetStatusName(screeningTemplateObject,
                workflowlevel.LevelNo == screeningTemplateObject.ReviewLevel, workflowlevel);

            var values = _screeningTemplateValueRepository
                .FindByInclude(t => t.ScreeningTemplateId == screeningTemplate.Id, t => t.Comments, t => t.Queries,
                    t => t.Children).ToList();

            _editCheckImpactRepository.CheckValidation(screeningTemplateObject, values, projectDesignId, projectDesignId);

            values.ForEach(t =>
            {
                var variable = designTemplateDto.Variables.FirstOrDefault(v => v.Id == t.ProjectDesignVariableId);
                if (variable != null)
                {
                    variable.ScreeningValue = t.Value;
                    variable.ScreeningValueOld = t.Value;
                    variable.ScreeningTemplateValueId = t.Id;
                    variable.QueryStatus = t.QueryStatus;
                    variable.HasComments = t.Comments.Any();
                    variable.HasQueries = t.Queries.Any();
                    variable.IsNaValue = t.IsNa;
                    variable.IsSystem = t.IsSystem;
                    variable.WorkFlowButton =
                        SetWorkFlowButton(t, workflowlevel, designTemplateDto, screeningTemplateObject);

                    variable.DocPath = t.DocPath != null ? documentUrl + t.DocPath : null;
                    if (!string.IsNullOrWhiteSpace(variable.ScreeningValue) || variable.IsNaValue)
                        variable.IsValid = true;

                    if (variable.Values != null)
                        variable.Values.ForEach(val =>
                        {
                            var childValue = t.Children.FirstOrDefault(v => v.ProjectDesignVariableValueId == val.Id);
                            if (childValue != null)
                            {
                                variable.IsValid = true;
                                val.ScreeningValue = childValue.Value;
                                val.ScreeningValueOld = childValue.Value;
                                val.ScreeningTemplateValueChildId = childValue.Id;
                            }
                        });
                }
            });

            var editChecks =
                _screeningTemplateValueEditCheckRepository.EditCheckSet(designTemplateDto.ScreeningTemplateId, false);
            designTemplateDto.Variables.ForEach(t =>
            {
                if (designTemplateDto.VariableTemplate != null)
                {
                    t.Note = designTemplateDto.VariableTemplate
                        .VariableTemplateDetails.FirstOrDefault(x => x.VariableId == t.VariableId)?.Note;

                    if (!string.IsNullOrEmpty(t.Note))
                        t.Note = "[" + t.Note + "]";
                }

                if (t.ScreeningValue == null && !t.IsNaValue) t.ScreeningValue = t.DefaultValue;

                t.OriginalValidationType = t.ValidationType;
                t.EditCheckValidation = editChecks.FirstOrDefault(r => r.ProjectDesignVariableId == t.Id) ??
                                        new EditCheckTargetValidation();
                if (t.EditCheckValidation.ProjectDesignVariableId > 0 &&
                    t.EditCheckValidation.OriginalValidationType != null)
                    t.ValidationType = (ValidationType)t.EditCheckValidation.OriginalValidationType;
            });

            if (screeningTemplateObject.EditCheckDetailId.HasValue && screeningTemplateObject.IsDisable)
            {
                var editCheck = Context.EditCheckDetail.Where(x => x.Id == screeningTemplateObject.EditCheckDetailId)
                    .Select(r => new { r.EditCheck.AutoNumber, r.Message, r.Operator }).FirstOrDefault();

                if (editCheck != null)
                {
                    //designTemplateDto.IsSubmittedButton = editCheck.Operator == Operator.Warning;
                    //designTemplateDto.RuleOperator = editCheck.Operator;
                    //designTemplateDto.EditCheckMessage = editCheck.AutoNumber + " - " + editCheck.Message;
                }

            }

            return designTemplateDto;
        }

  
        public List<ScreeningTemplateDto> GetTemplateTree(int screeningEntryId, int? parentId,
            List<ScreeningTemplateValue> templateValues, WorkFlowLevelDto workFlowLevel)
        {            
            return All.Where(s =>
                   s.ScreeningEntryId == screeningEntryId && s.ParentId == parentId && s.DeletedDate == null).Select(s =>
                   new ScreeningTemplateDto
                   {
                       Id = s.Id,
                       ScreeningEntryId = s.ScreeningEntryId,
                       ProjectDesignTemplateId = s.ProjectDesignTemplateId,
                       Status = s.Status,
                       ProjectDesignVisitId = s.ProjectDesignTemplate.ProjectDesignVisitId,
                       ProjectDesignTemplateName = s.ProjectDesignTemplate.TemplateName,
                       DesignOrder = s.ProjectDesignTemplate.DesignOrder,
                       IsVisitRepeated = s.RepeatedVisit != null ? false :
                           workFlowLevel.LevelNo >= 0 && s.ProjectDesignVisit.IsRepeated ? workFlowLevel.IsStartTemplate :
                           s.ProjectDesignVisit.IsRepeated,
                       ProjectDesignVisitName = s.ProjectDesignTemplate.ProjectDesignVisit.DisplayName +
                                                Convert.ToString(s.RepeatedVisit == null ? "" : "_" + s.RepeatedVisit),
                       StatusName = GetStatusName(s, workFlowLevel.LevelNo == s.ReviewLevel, workFlowLevel),
                       Progress = s.Progress ?? 0,
                       ReviewLevel = s.ReviewLevel,
                       MyReview = workFlowLevel.LevelNo == s.ReviewLevel,
                       TemplateQueryStatus = _screeningTemplateValueRepository.GetQueryStatusByModel(templateValues, s.Id),
                       IsParent = s.Children.Any(),
                       IsLocked = Context.ScreeningTemplateLockUnlockAudit.Any(x => x.ProjectDesignTemplateId == s.ProjectDesignTemplateId && x.ScreeningEntryId == screeningEntryId && x.ProjectId == s.ScreeningEntry.ProjectId
                       ) ? Context.ScreeningTemplateLockUnlockAudit.Where(x => x.ProjectDesignTemplateId == s.ProjectDesignTemplateId && x.ScreeningEntryId == screeningEntryId && x.ProjectId == s.ScreeningEntry.ProjectId
                        ).OrderByDescending(b => b.Id).FirstOrDefault().IsLocked : false                           
                   }).ToList().OrderBy(o => o.ProjectDesignVisitId).ThenBy(t => t.DesignOrder).ToList();           
        }

        public List<MyReviewDto> GetScreeningTemplateReview()
        {
            var result = All.Where(x => x.DeletedDate == null
                                        && x.ReviewLevel != null && x.ReviewLevel > 0
                                        && (Context.ProjectWorkflowIndependent.Any(r => r.DeletedDate == null &&
                                                                                        r.ProjectWorkflow
                                                                                            .ProjectDesignId ==
                                                                                        x.ScreeningEntry.ProjectDesignId
                                                                                        && r.SecurityRoleId ==
                                                                                        _jwtTokenAccesser.RoleId) ||
                                            Context.ProjectWorkflowLevel.Any(r => r.DeletedDate == null &&
                                                                                  r.ProjectWorkflow.ProjectDesignId ==
                                                                                  x.ScreeningEntry.ProjectDesignId
                                                                                  && r.SecurityRoleId ==
                                                                                  _jwtTokenAccesser.RoleId
                                                                                  && r.LevelNo == x.ReviewLevel))
            ).Select(a => new MyReviewDto
            {
                ScreeningEntryId = a.ScreeningEntryId,
                ProjectDesignTemplateId = a.ProjectDesignTemplateId,
                ScreeningDate = a.ScreeningEntry.ScreeningDate,
                ScreeningNo = a.ScreeningEntry.ScreeningNo,
                ProjectName = a.ScreeningEntry.Project.ProjectName,
                VolunteerName = a.ScreeningEntry.Attendance.Volunteer == null
                    ? a.ScreeningEntry.Attendance.NoneRegister.Initial
                    : a.ScreeningEntry.Attendance.Volunteer.AliasName,
                TemplateName = a.ProjectDesignTemplate.TemplateName,
                VistName = a.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                ReviewedLevel = a.ReviewLevel.ToString(),
                SubmittedDate = a.ScreeningTemplateReview.FirstOrDefault(c => c.Status == ScreeningStatus.Submitted)
                    .CreatedDate,
                SubmittedBy = a.ScreeningTemplateReview.FirstOrDefault(c => c.Status == ScreeningStatus.Submitted)
                    .CreatedByUser.UserName,
                LastReviewedDate = a.ScreeningTemplateReview
                    .FirstOrDefault(c => c.Status == ScreeningStatus.Reviewed && c.ReviewLevel == a.ReviewLevel - 1)
                    .CreatedDate,
                LastReviewedBy = a.ScreeningTemplateReview
                    .FirstOrDefault(c => c.Status == ScreeningStatus.Reviewed && c.ReviewLevel == a.ReviewLevel - 1)
                    .CreatedByUser.UserName
            }).ToList();

            return result;
        }

        public ScreeningTemplate TemplateRepeat(int id)
        {
            var screeningTemplate = new ScreeningTemplate();
            var originalTemplate = Find(id);
            screeningTemplate.ParentId = originalTemplate.Id;
            screeningTemplate.Id = 0;
            screeningTemplate.ProjectDesignVisitId = originalTemplate.ProjectDesignVisitId;
            screeningTemplate.ScreeningEntryId = originalTemplate.ScreeningEntryId;
            screeningTemplate.EditCheckDetailId = originalTemplate.EditCheckDetailId;
            screeningTemplate.IsDisable = originalTemplate.IsDisable;
            screeningTemplate.ProjectDesignTemplateId = originalTemplate.ProjectDesignTemplateId;
            screeningTemplate.IsEditChecked = false;
            screeningTemplate.Status = ScreeningStatus.Pending;
            screeningTemplate.Children = null;

            Add(screeningTemplate);

            return screeningTemplate;
        }


        public List<ScreeningTemplateLockUnlockDto> GetTemplatesLockUnlock(
            ScreeningTemplateLockUnlockParams lockUnlockParams)
        {
            var query = All.Where(t => t.DeletedDate == null);

            if (lockUnlockParams.ProjectDesignVisitId > 0)
                query = query.Where(x =>
                    x.ProjectDesignTemplate.ProjectDesignVisitId == lockUnlockParams.ProjectDesignVisitId);

            if (lockUnlockParams.ProjectDesingId > 0)
                query = query.Where(x =>
                    x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId ==
                    lockUnlockParams.ProjectDesingId);

            if (lockUnlockParams.ProjectId > 0)
                query = query.Where(x => x.ScreeningEntry.ProjectId == lockUnlockParams.ProjectId);

            if (lockUnlockParams.VolunteerId > 0)
                query = query.Where(x => x.ScreeningEntry.Attendance.VolunteerId == lockUnlockParams.VolunteerId);

            if (lockUnlockParams.IsLock)
                query = query.Where(t => t.Status == ScreeningStatus.Pending || t.IsCompleteReview);
            else
                query = query.Where(t => t.Status == ScreeningStatus.Completed);

            query = query.Where(x => Context.ProjectWorkflowLevel.Any(t => t.DeletedDate == null && t.IsLock
                                                                                                 && t.ProjectWorkflow
                                                                                                     .ProjectDesignId ==
                                                                                                 x.ScreeningEntry
                                                                                                     .ProjectDesignId
                                                                                                 && t.SecurityRoleId ==
                                                                                                 _jwtTokenAccesser
                                                                                                     .RoleId));

            var result = query.Select(t => new ScreeningTemplateLockUnlockDto
            {
                Id = t.Id,
                ProjectName = t.ScreeningEntry.Project.ProjectName,
                Status = t.Status,
                ScreeningNo = t.ScreeningEntry.ScreeningNo,
                StatusName = t.Status.GetDescription(),
                TemplateName = t.ProjectDesignTemplate.DesignOrder + " " + t.ProjectDesignTemplate.TemplateName,
                VisitName = t.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                VolunteerName = t.ScreeningEntry.Attendance.Volunteer == null
                    ? t.ScreeningEntry.Attendance.NoneRegister.Initial
                    : t.ScreeningEntry.Attendance.Volunteer.FullName
            }).OrderByDescending(c => c.Id).ToList();

            return result;
        }

        public void VisitRepeat(int projectDesignVisitId, int screeningEntryId)
        {
            var repeatedCount = 0;
            var projectVisit = All.Where(x => x.ProjectDesignVisitId == projectDesignVisitId
                                              && x.ScreeningEntryId == screeningEntryId).ToList();
            if (projectVisit.Count > 0)
                repeatedCount = projectVisit.Max(x => x.RepeatedVisit ?? 0);
            var templates = Context.ProjectDesignTemplate
                .Where(t => t.DeletedDate == null && t.ProjectDesignVisitId == projectDesignVisitId).ToList();
            templates.ForEach(t =>
            {
                var oldTemplate = Context.ScreeningTemplate.FirstOrDefault(r =>
                    r.ScreeningEntryId == screeningEntryId &&
                    r.ProjectDesignVisitId == projectDesignVisitId && r.ProjectDesignTemplateId == t.Id);
                Add(new ScreeningTemplate
                {
                    ScreeningEntryId = screeningEntryId,
                    ProjectDesignTemplateId = t.Id,
                    EditCheckDetailId = oldTemplate != null ? oldTemplate.EditCheckDetailId : null,
                    IsDisable = oldTemplate != null ? oldTemplate.IsDisable : false,
                    RepeatedVisit = repeatedCount + 1,
                    ProjectDesignVisitId = t.ProjectDesignVisitId,
                    IsEditChecked = false,
                    Status = ScreeningStatus.Pending
                });
            });
        }

        public List<DashboardStudyStatusDto> GetDashboardStudyStatusByVisit(int projectId)
        {
            var projectDesign = Context.ProjectDesign.Where(x => x.ProjectId == projectId).FirstOrDefault();
            var workFlowLevel = new WorkFlowLevelDto();
            if (projectDesign != null) workFlowLevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesign.Id);

            var queryStatus = (from st in Context.ScreeningTemplate
                               join pdv in Context.ProjectDesignVisit on st.ProjectDesignVisitId equals pdv.Id into design
                               from pdesign in design.DefaultIfEmpty()
                               join se in Context.ScreeningEntry on st.ScreeningEntryId equals se.Id into entry
                               from sEntry in entry.DefaultIfEmpty()
                               join p in Context.Project on sEntry.ProjectId equals p.Id into project
                               from p in project.DefaultIfEmpty()
                               where p.Id == projectId || p.ParentProjectId == projectId
                               group new { st, pdesign } by new { st.ProjectDesignVisitId, pdesign.DisplayName }
                into g
                               select new DashboardStudyStatusDto
                               {
                                   WorkflowDetail = workFlowLevel,
                                   DisplayName = g.Key.DisplayName,
                                   NotStarted = g.Where(x => (int?)x.st.Status == 1).Count(),
                                   InProcess = g.Where(x => (int?)x.st.Status == 2).Count(),
                                   Submitted = g.Where(x => (int?)x.st.Status == 3).Count(),
                                   Reviewed = g.Where(x => (int?)x.st.Status == 4).Count(),
                                   Completed = g.Where(x => (int?)x.st.Status == 5).Count(),
                                   Review1 = g.Where(x => (int?)x.st.ReviewLevel == 1).Count(),
                                   Review2 = g.Where(x => (int?)x.st.ReviewLevel == 2).Count(),
                                   Review3 = g.Where(x => (int?)x.st.ReviewLevel == 3).Count(),
                                   Review4 = g.Where(x => (int?)x.st.ReviewLevel == 4).Count(),
                                   Review5 = g.Where(x => (int?)x.st.ReviewLevel == 5).Count()
                               }).ToList();
            return queryStatus;
        }

        public List<DashboardStudyStatusDto> GetDashboardStudyStatusBySite(int projectId)
        {
            var queryStatus = (from p in Context.Project
                               join se in Context.ScreeningEntry on p.Id equals se.ProjectId into entry
                               from sEntry in entry.DefaultIfEmpty()
                               join st in Context.ScreeningTemplate on sEntry.Id equals st.ScreeningEntryId into sTemplate
                               from template in sTemplate.DefaultIfEmpty()
                               join pdv in Context.ProjectDesignVisit on template.ProjectDesignVisitId equals pdv.Id into design
                               from pDesign in design.DefaultIfEmpty()
                               where p.Id == projectId || p.ParentProjectId == projectId
                               group new { p, template } by new { template.Status, p.ProjectCode }
                into g
                               select new DashboardStudyStatusDto
                               {
                                   DisplayName = g.Key.ProjectCode,
                                   NotStarted = g.Where(x => (int?)x.template.Status == 1).Count(),
                                   InProcess = g.Where(x => (int?)x.template.Status == 2).Count(),
                                   Submitted = g.Where(x => (int?)x.template.Status == 3).Count(),
                                   Reviewed = g.Where(x => (int?)x.template.Status == 4).Count(),
                                   Completed = g.Where(x => (int?)x.template.Status == 5).Count(),
                                   Review1 = g.Where(x => (int?)x.template.ReviewLevel == 1).Count(),
                                   Review2 = g.Where(x => (int?)x.template.ReviewLevel == 2).Count(),
                                   Review3 = g.Where(x => (int?)x.template.ReviewLevel == 3).Count(),
                                   Review4 = g.Where(x => (int?)x.template.ReviewLevel == 4).Count(),
                                   Review5 = g.Where(x => (int?)x.template.ReviewLevel == 5).Count()
                               }).ToList();
            return queryStatus;
        }

        public IList<ReviewDto> GetReviewReportList(ReviewSearchDto filters)
        {
            var queryDtos = (from screening in Context.ScreeningEntry.Where(t =>
                        t.ProjectId == filters.ProjectId && t.DeletedDate == null &&
                        (filters.PeriodIds == null || filters.PeriodIds.Contains(t.ProjectDesignPeriodId)) && (filters.SubjectIds == null || filters.SubjectIds.Contains(t.AttendanceId)))
                             join template in Context.ScreeningTemplate.Where(u =>
                                     (filters.TemplateIds == null || filters.TemplateIds.Contains(u.ProjectDesignTemplateId))
                                     && (filters.VisitIds == null ||
                                         filters.VisitIds.Contains(u.ProjectDesignTemplate.ProjectDesignVisitId)) &&
                                     (filters.StatusIds == null || filters.StatusIds.Contains((int)u.Status)) && u.DeletedDate == null && u.ProjectDesignTemplate.DeletedDate == null
                                     && u.ProjectDesignTemplate.ProjectDesignVisit.DeletedDate == null) on screening.Id
                                 equals template.ScreeningEntryId
                             join reviewTemp in Context.ScreeningTemplateReview.Where(review =>
                                     (filters.ReviewStatus == null || filters.ReviewStatus.Contains(review.ReviewLevel)) && review.DeletedDate == null)
                                 on template.Id equals reviewTemp.ScreeningTemplateId into reviewDto
                             from review in reviewDto.DefaultIfEmpty()
                             join workflow in Context.ProjectWorkflow.Where(x => x.DeletedDate == null) on screening.ProjectDesignId equals workflow
                                 .ProjectDesignId
                             join project in Context.Project on screening.ProjectId equals project.Id
                             join attendance in Context.Attendance.Where(t =>
                                     t.DeletedDate == null)
                                 on screening.AttendanceId equals attendance.Id
                             join volunteerTemp in Context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into
                                 volunteerDto
                             from volunteer in volunteerDto.DefaultIfEmpty()
                             join noneregisterTemp in Context.NoneRegister on attendance.Id equals noneregisterTemp.AttendanceId
                                 into noneregisterDto
                             from nonregister in noneregisterDto.DefaultIfEmpty()
                             join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals
                                 projectSubjectTemp.Id into projectsubjectDto
                             from projectsubject in projectsubjectDto.DefaultIfEmpty()
                             select new ReviewDto
                             {
                                 Id = template.Id,
                                 AttendanceId = screening.AttendanceId,
                                 SiteName = string.IsNullOrEmpty(project.SiteName) ? project.ProjectName : project.SiteName,
                                 ScreeningEntryId = screening.Id,
                                 ScreeningNo = screening.ScreeningNo,
                                 StatusName = template.Status.GetDescription(),
                                 ScreeningTemplateId = template.Id,
                                 ProjectCode = screening.Project.ProjectCode,
                                 ScreeningTemplateValue = template.ProjectDesignTemplate.TemplateName,
                                 Visit = template.ProjectDesignVisit.DisplayName +
                                         Convert.ToString(template.RepeatedVisit == null ? "" : "_" + template.RepeatedVisit),
                                 VolunteerDelete = volunteer.DeletedDate,
                                 VolunteerName = volunteer.FullName == null ? nonregister.Initial : volunteer.AliasName,
                                 SubjectNo = volunteer.FullName == null ? nonregister.ScreeningNumber : volunteer.VolunteerNo,
                                 RandomizationNumber = volunteer.FullName == null
                                     ? nonregister.RandomizationNumber
                                     : projectsubject.Number,
                                 //ReviewLevelName = "Reviewed " + review.ReviewLevel,
                                 ReviewLevelName = review.ReviewLevel == 0
                                     ? ""
                                     : workflow.Levels.Where(x => x.LevelNo == review.ReviewLevel).FirstOrDefault().SecurityRole
                                         .RoleShortName
                             }).OrderBy(x => x.Id).ToList();

            if (filters.ReviewStatus != null) queryDtos = queryDtos.Where(x => x.ReviewLevelName != null).ToList();

            return queryDtos.Where(x => x.VolunteerDelete == null).ToList();
        }

        private WorkFlowButton SetWorkFlowButton(ScreeningTemplateValue screningTemplateValue,
            WorkFlowLevelDto workflowlevel, ProjectDesignTemplateDto designTemplateDto,
            ScreeningTemplate screeningTemplate)
        {
            var workFlowButton = new WorkFlowButton();
            var statusId = (int)screeningTemplate.Status;

            if (screeningTemplate.Status == ScreeningStatus.Completed)
            {
                designTemplateDto.MyReview = false;
                designTemplateDto.IsSubmittedButton = false;
                return workFlowButton;
            }

            if (statusId>2)
            {
                if (workflowlevel.LevelNo <= screeningTemplate.ReviewLevel || workflowlevel.LevelNo == 0)
                    workFlowButton.SelfCorrection = workflowlevel.SelfCorrection &&
                                                    screningTemplateValue.QueryStatus != QueryStatus.SelfCorrection;


                if (workflowlevel.LevelNo == screningTemplateValue.AcknowledgeLevel ||
                    workflowlevel.LevelNo == 0 && workflowlevel.IsStartTemplate)
                    workFlowButton.Update = screningTemplateValue.QueryStatus == QueryStatus.Open ||
                                            screningTemplateValue.QueryStatus == QueryStatus.Reopened;

                if (workflowlevel.IsGenerateQuery && (designTemplateDto.MyReview || workflowlevel.LevelNo == 0))
                    workFlowButton.Generate = screningTemplateValue.QueryStatus == null ||
                                              screningTemplateValue.QueryStatus == QueryStatus.Closed;

                if (workflowlevel.LevelNo == screningTemplateValue.ReviewLevel &&
                    screningTemplateValue.ReviewLevel == screningTemplateValue.AcknowledgeLevel)
                {
                    workFlowButton.DeleteQuery = screningTemplateValue.QueryStatus == QueryStatus.Open;
                    workFlowButton.Review = screningTemplateValue.QueryStatus == QueryStatus.Answered ||
                                            screningTemplateValue.QueryStatus == QueryStatus.Resolved;
                }

                if (workflowlevel.LevelNo == 0 && workFlowButton.Review)
                    workFlowButton.Review = screningTemplateValue.UserRoleId == _jwtTokenAccesser.RoleId;

                if (screningTemplateValue.IsSystem && screningTemplateValue.QueryStatus == QueryStatus.Open &&
                    workflowlevel.IsStartTemplate)
                {
                    workFlowButton.Update = screningTemplateValue.QueryStatus == QueryStatus.Open;
                    workFlowButton.DeleteQuery = false;
                }

                if (!designTemplateDto.MyReview && workflowlevel.LevelNo == screningTemplateValue.AcknowledgeLevel &&
                  screningTemplateValue.AcknowledgeLevel != screningTemplateValue.ReviewLevel)
                    workFlowButton.Acknowledge = screningTemplateValue.QueryStatus == QueryStatus.Resolved ||
                                                 screningTemplateValue.QueryStatus == QueryStatus.SelfCorrection;


            }

            workFlowButton.Clear = designTemplateDto.IsSubmittedButton;
            
            return workFlowButton;
        }

        private string GetStatusName(ScreeningTemplate screeningTemplate, bool myReview, WorkFlowLevelDto workFlowLevel)
        {
            if (myReview) return "My Review";

            if (screeningTemplate.Status != ScreeningStatus.Completed && screeningTemplate.ReviewLevel != null &&
                screeningTemplate.ReviewLevel > 0)
            {
                if (workFlowLevel.WorkFlowText != null
                    && workFlowLevel.WorkFlowText.Any(x => x.LevelNo == screeningTemplate.ReviewLevel))
                    return workFlowLevel.WorkFlowText.FirstOrDefault(x => x.LevelNo == screeningTemplate.ReviewLevel)
                        ?.RoleName;
                return "Completed";
            }

            return screeningTemplate.Status.GetDescription();
        }

        public List<LockUnlockListDto> GetLockUnlockList(LockUnlockSearchDto lockUnlockParams)
        {
            var ProjectCode = Context.Project.Find(lockUnlockParams.ParentProjectId).ProjectCode;

            var ParentProjectId = Context.Project.Where(x => x.Id == lockUnlockParams.ProjectId).FirstOrDefault().ParentProjectId;
            var ProjectDesignId = ParentProjectId == null ? Context.ProjectDesign.Where(x => x.ProjectId == lockUnlockParams.ProjectId).FirstOrDefault().Id :
                 Context.ProjectDesign.Where(x => x.ProjectId == ParentProjectId).FirstOrDefault().Id;

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(ProjectDesignId);
            var grpresult = new List<LockUnlockListDto>();

            var lockedin = Context.ScreeningTemplateLockUnlockAudit.Where(x => x.ProjectId == lockUnlockParams.ProjectId).ToList();

            var grplockedIn = lockedin.GroupBy(x => new { x.ScreeningEntryId, x.ProjectDesignId, x.ProjectDesignTemplateId })
                      .Select(y => new LockUnlockListDto()
                      {
                          Id = y.Key.ScreeningEntryId,
                          ProjectDesignId = y.Key.ProjectDesignId,
                          TemplateId = y.Key.ProjectDesignTemplateId,
                          IsLocked = y.LastOrDefault().IsLocked,
                          ProjectId = y.LastOrDefault().ProjectId
                      }).ToList();

            if (lockUnlockParams.Status)
            {
                var result = (from screening in Context.ScreeningEntry.Where(t => t.ProjectId == lockUnlockParams.ProjectId && (lockUnlockParams.PeriodIds == null || lockUnlockParams.PeriodIds.Contains(t.ProjectDesignPeriodId)) && t.DeletedDate == null
                              && (lockUnlockParams.SubjectIds == null || lockUnlockParams.SubjectIds.Contains(t.AttendanceId)) && t.EntryType != AttendanceType.Screening)
                              join template in Context.ScreeningTemplate.Where(u => (lockUnlockParams.TemplateIds == null || lockUnlockParams.TemplateIds.Contains(u.ProjectDesignTemplateId))
                              && (lockUnlockParams.VisitIds == null || lockUnlockParams.VisitIds.Contains(u.ProjectDesignTemplate.ProjectDesignVisitId)) && u.DeletedDate == null
                              && (lockUnlockParams.DataEntryStatus != null && lockUnlockParams.DataEntryReviewStatus != null ? (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(u.ReviewLevel))
                              || (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)u.Status)) : (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(u.ReviewLevel))
                              && (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)u.Status))))
                              on screening.Id equals template.ScreeningEntryId
                              join volunteerTemp in Context.Volunteer.Where(x => x.DeletedDate == null) on screening.Attendance.VolunteerId equals volunteerTemp.Id into volunteerDto
                              from volunteer in volunteerDto.DefaultIfEmpty()
                              join noneregisterTemp in Context.NoneRegister.Where(x => x.DeletedDate == null) on screening.Attendance.Id equals noneregisterTemp.AttendanceId into noneregisterDto
                              from nonregister in noneregisterDto.DefaultIfEmpty()
                              join projectSubjectTemp in Context.ProjectSubject.Where(x => x.DeletedDate == null) on screening.Attendance.ProjectSubjectId equals projectSubjectTemp.Id into projectsubjectDto
                              from projectsubject in projectsubjectDto.DefaultIfEmpty()
                              select new LockUnlockListDto
                              {
                                  Id = screening.Id,
                                  ProjectId = screening.ProjectId,
                                  ProjectDesignId = screening.ProjectDesignId,
                                  AttendanceId = screening.AttendanceId,
                                  ScreeningTemplateId = template.Id,
                                  ScreeningTemplateParentId = template.ParentId,
                                  ParentProjectId = screening.Project.ParentProjectId,
                                  ProjectCode = ProjectCode,
                                  ProjectName = screening.Project.ProjectCode,
                                  PeriodName = screening.ProjectDesignPeriod.DisplayName,
                                  ScreeningNo = screening.ScreeningNo,
                                  TemplateId = template.ProjectDesignTemplateId,
                                  TemplateName = template.ProjectDesignTemplate.TemplateName,
                                  VisitId = template.ProjectDesignTemplate.ProjectDesignVisitId,
                                  VisitName = template.ProjectDesignTemplate.ProjectDesignVisit.DisplayName +
                                            Convert.ToString(template.RepeatedVisit == null ? "" : "_" + template.RepeatedVisit),
                                  Initial = volunteer.FullName == null ? nonregister.Initial : volunteer.AliasName,
                                  SubjectNo = volunteer.FullName == null ? nonregister.ScreeningNumber : volunteer.VolunteerNo,
                                  RandomizationNumber = volunteer.FullName == null ? nonregister.RandomizationNumber : projectsubject.Number,
                                  IsElectronicSignature = workflowlevel.IsElectricSignature,
                                  ScreeningStatusNo = template.Status,
                                  ReviewLevel = template.ReviewLevel,
                              }).OrderBy(x => x.Id).ToList();

                result.RemoveAll(r => grplockedIn.Any(a => a.TemplateId == r.TemplateId && a.Id == r.Id && a.ProjectDesignId == r.ProjectDesignId && a.ProjectId == r.ProjectId && a.IsLocked));

                grpresult = result.GroupBy(x => x.SubjectNo).Select(s => new LockUnlockListDto
                {
                    SubjectNo = s.Key,
                    AttendanceId = s.FirstOrDefault().AttendanceId,
                    Id = s.FirstOrDefault().Id,
                    ProjectId = s.FirstOrDefault().ProjectId,
                    ParentProjectId = s.FirstOrDefault().ParentProjectId,
                    ProjectCode = s.FirstOrDefault().ProjectCode,
                    ProjectDesignId = s.FirstOrDefault().ProjectDesignId,
                    ProjectName = s.FirstOrDefault().ProjectName,
                    Status = lockUnlockParams.Status,
                    Initial = s.FirstOrDefault().Initial,
                    TemplateName = s.FirstOrDefault().TemplateName,
                    VisitId = s.FirstOrDefault().VisitId,
                    VisitName = s.FirstOrDefault().VisitName,
                    RandomizationNumber = s.FirstOrDefault().RandomizationNumber,
                    PeriodCount = s.GroupBy(p => p.PeriodName).Count(),
                    TemplateCount = s.GroupBy(t => new { t.VisitName, t.TemplateId }).Count(),
                    VisitCount = s.GroupBy(v => v.VisitName).Count(),
                    IsElectronicSignature = s.FirstOrDefault().IsElectronicSignature,
                    lstTemplate = s.GroupBy(t => new { t.TemplateName, t.VisitName }).Select(t => new LockUnlockListDto
                    {
                        TemplateId = t.FirstOrDefault().TemplateId,
                        ProjectCode = s.FirstOrDefault().ProjectCode,
                        ProjectName = s.FirstOrDefault().ProjectName,
                        PeriodName = s.FirstOrDefault().PeriodName,
                        ParentProjectId = s.FirstOrDefault().ParentProjectId,
                        VisitId = t.FirstOrDefault().VisitId,
                        VisitName = t.FirstOrDefault().VisitName,
                        TemplateName = t.FirstOrDefault().TemplateName
                    }).OrderBy(x => x.VisitId).ToList()
                }).ToList();
            }
            else
            {
                var result = (from screening in Context.ScreeningEntry.Where(t => t.ProjectId == lockUnlockParams.ProjectId && (lockUnlockParams.PeriodIds == null || lockUnlockParams.PeriodIds.Contains(t.ProjectDesignPeriodId)) && t.DeletedDate == null
                              && (lockUnlockParams.SubjectIds == null || lockUnlockParams.SubjectIds.Contains(t.AttendanceId)) && t.EntryType != AttendanceType.Screening)
                              join template in Context.ScreeningTemplate.Where(u => (lockUnlockParams.TemplateIds == null || lockUnlockParams.TemplateIds.Contains(u.ProjectDesignTemplateId))
                              && (lockUnlockParams.VisitIds == null || lockUnlockParams.VisitIds.Contains(u.ProjectDesignTemplate.ProjectDesignVisitId)) && u.DeletedDate == null
                              && (lockUnlockParams.DataEntryStatus != null && lockUnlockParams.DataEntryReviewStatus != null ? (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(u.ReviewLevel))
                              || (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)u.Status)) : (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(u.ReviewLevel))
                              && (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)u.Status))))
                              on screening.Id equals template.ScreeningEntryId
                              join locktemplate in Context.ScreeningTemplateLockUnlockAudit.Where(x => x.IsLocked) on new { x = screening.Attendance.Id, y = template.ProjectDesignTemplateId } equals new { x = locktemplate.ScreeningEntry.AttendanceId, y = locktemplate.ProjectDesignTemplateId }
                              join volunteerTemp in Context.Volunteer.Where(x => x.DeletedDate == null) on screening.Attendance.VolunteerId equals volunteerTemp.Id into volunteerDto
                              from volunteer in volunteerDto.DefaultIfEmpty()
                              join noneregisterTemp in Context.NoneRegister.Where(x => x.DeletedDate == null) on screening.Attendance.Id equals noneregisterTemp.AttendanceId into noneregisterDto
                              from nonregister in noneregisterDto.DefaultIfEmpty()
                              join projectSubjectTemp in Context.ProjectSubject.Where(x => x.DeletedDate == null) on screening.Attendance.ProjectSubjectId equals projectSubjectTemp.Id into projectsubjectDto
                              from projectsubject in projectsubjectDto.DefaultIfEmpty()
                              select new LockUnlockListDto
                              {
                                  Id = screening.Id,
                                  ProjectId = screening.ProjectId,
                                  ProjectDesignId = screening.ProjectDesignId,
                                  AttendanceId = screening.AttendanceId,
                                  ScreeningTemplateId = template.Id,
                                  ScreeningTemplateParentId = template.ParentId,
                                  ParentProjectId = screening.Project.ParentProjectId,
                                  ProjectCode = ProjectCode,
                                  ProjectName = screening.Project.ProjectCode,
                                  PeriodName = screening.ProjectDesignPeriod.DisplayName,
                                  ScreeningNo = screening.ScreeningNo,
                                  TemplateId = template.ProjectDesignTemplateId,
                                  TemplateName = template.ProjectDesignTemplate.TemplateName,
                                  VisitId = template.ProjectDesignTemplate.ProjectDesignVisitId,
                                  VisitName = template.ProjectDesignTemplate.ProjectDesignVisit.DisplayName +
                                            Convert.ToString(template.RepeatedVisit == null ? "" : "_" + template.RepeatedVisit),
                                  Initial = volunteer.FullName == null ? nonregister.Initial : volunteer.AliasName,
                                  SubjectNo = volunteer.FullName == null ? nonregister.ScreeningNumber : volunteer.VolunteerNo,
                                  RandomizationNumber = volunteer.FullName == null ? nonregister.RandomizationNumber : projectsubject.Number,
                                  IsElectronicSignature = workflowlevel.IsElectricSignature,
                                  ScreeningStatusNo = template.Status,
                                  ReviewLevel = template.ReviewLevel,
                              }).OrderBy(x => x.Id).ToList();

                result.RemoveAll(r => grplockedIn.Any(a => a.TemplateId == r.TemplateId && a.Id == r.Id && a.ProjectDesignId == r.ProjectDesignId && a.ProjectId == r.ProjectId && !a.IsLocked));

                grpresult = result.GroupBy(x => x.SubjectNo).Select(s => new LockUnlockListDto
                {
                    SubjectNo = s.Key,
                    AttendanceId = s.FirstOrDefault().AttendanceId,
                    Id = s.FirstOrDefault().Id,
                    ParentProjectId = s.FirstOrDefault().ParentProjectId,
                    ProjectId = s.FirstOrDefault().ProjectId,
                    ProjectCode = s.FirstOrDefault().ProjectCode,
                    ProjectDesignId = s.FirstOrDefault().ProjectDesignId,
                    ProjectName = s.FirstOrDefault().ProjectName,
                    Status = lockUnlockParams.Status,
                    Initial = s.FirstOrDefault().Initial,
                    TemplateName = s.FirstOrDefault().TemplateName,
                    VisitId = s.FirstOrDefault().VisitId,
                    VisitName = s.FirstOrDefault().VisitName,
                    RandomizationNumber = s.FirstOrDefault().RandomizationNumber,
                    PeriodCount = s.GroupBy(p => p.PeriodName).Count(),
                    TemplateCount = s.GroupBy(t => new { t.VisitName, t.TemplateId }).Count(),
                    VisitCount = s.GroupBy(v => v.VisitName).Count(),
                    IsElectronicSignature = s.FirstOrDefault().IsElectronicSignature,
                    lstTemplate = s.GroupBy(t => new { t.TemplateName, t.VisitName }).Select(t => new LockUnlockListDto
                    {
                        TemplateId = t.FirstOrDefault().TemplateId,
                        ProjectCode = s.FirstOrDefault().ProjectCode,
                        ProjectName = s.FirstOrDefault().ProjectName,
                        PeriodName = s.FirstOrDefault().PeriodName,
                        ParentProjectId = s.FirstOrDefault().ParentProjectId,
                        VisitId = t.FirstOrDefault().VisitId,
                        VisitName = t.FirstOrDefault().VisitName,
                        TemplateName = t.FirstOrDefault().TemplateName
                    }).OrderBy(x => x.VisitId).ToList()
                }).ToList();
            }

            return grpresult;
        }

    }
}

