using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateRepository : GenericRespository<ScreeningTemplate, GscContext>,
        IScreeningTemplateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IScreeningTemplateValueEditCheckRepository _screeningTemplateValueEditCheckRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateValueChildRepository _screeningTemplateValueChildRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IEditCheckImpactRepository _editCheckImpactRepository;
        private readonly IMapper _mapper;
        private readonly IScheduleRuleRespository _scheduleRuleRespository;
        public ScreeningTemplateRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IUploadSettingRepository uploadSettingRepository, IMapper mapper,
            IProjectWorkflowRepository projectWorkflowRepository,
            IEditCheckImpactRepository editCheckImpactRepository,
            IScheduleRuleRespository scheduleRuleRespository,
            IScreeningTemplateValueChildRepository screeningTemplateValueChildRepository,
            IScreeningTemplateValueEditCheckRepository screeningTemplateValueEditCheckRepository)
            : base(uow, jwtTokenAccesser)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _scheduleRuleRespository = scheduleRuleRespository;
            _screeningTemplateValueChildRepository = screeningTemplateValueChildRepository;
            _editCheckImpactRepository = editCheckImpactRepository;
            _screeningTemplateValueEditCheckRepository = screeningTemplateValueEditCheckRepository;
        }

        private ScreeningTemplateBasic GetScreeningTemplateBasic(int screeningTemplateId)
        {
            return All.AsNoTracking().Where(r => r.Id == screeningTemplateId).Select(
               c => new ScreeningTemplateBasic
               {
                   Id = c.Id,
                   ScreeningEntryId = c.ScreeningVisit.ScreeningEntryId,
                   ProjectDesignId = c.ScreeningVisit.ScreeningEntry.ProjectDesignId,
                   ProjectDesignTemplateId = c.ProjectDesignTemplateId,
                   Status = c.Status,
                   DomainId = c.ProjectDesignTemplate.DomainId,
                   ReviewLevel = c.ReviewLevel,
                   RepeatedVisit = c.ScreeningVisit.RepeatedVisitNumber,
                   IsLocked = c.IsLocked,
                   IsDisable = c.IsDisable,
                   ParentId = c.ParentId
               }).FirstOrDefault();
        }

        private List<Data.Dto.Screening.ScreeningTemplateValueBasic> GetScreeningValues(int screeningTemplateId)
        {
            return _screeningTemplateValueRepository.All.AsNoTracking().Where(t => t.ScreeningTemplateId == screeningTemplateId)
                    .ProjectTo<Data.Dto.Screening.ScreeningTemplateValueBasic>(_mapper.ConfigurationProvider).ToList();
        }

        public Data.Dto.Project.Design.DesignScreeningTemplateDto GetScreeningTemplate(DesignScreeningTemplateDto designTemplateDto,
            int screeningTemplateId)
        {

            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();

            var screeningTemplateBasic = GetScreeningTemplateBasic(screeningTemplateId);

            var statusId = (int)screeningTemplateBasic.Status;

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(screeningTemplateBasic.ProjectDesignId);

            designTemplateDto.ScreeningTemplateId = screeningTemplateId;
            designTemplateDto.IsSubmittedButton = statusId < 3 && workflowlevel.IsStartTemplate;
            if (workflowlevel.LevelNo >= 0 && designTemplateDto.IsRepeated)
                designTemplateDto.IsRepeated = workflowlevel.IsStartTemplate;
            if (screeningTemplateBasic.ParentId != null)
                designTemplateDto.IsRepeated = false;

            designTemplateDto.MyReview = workflowlevel.LevelNo == screeningTemplateBasic.ReviewLevel;
            designTemplateDto.ScreeningTemplateId = screeningTemplateBasic.Id;
            designTemplateDto.IsLocked = screeningTemplateBasic.IsLocked;
            designTemplateDto.Status = screeningTemplateBasic.Status;
            designTemplateDto.StatusName = GetStatusName(screeningTemplateBasic,
                workflowlevel.LevelNo == screeningTemplateBasic.ReviewLevel, workflowlevel);

            var values = GetScreeningValues(screeningTemplateBasic.Id);

            values.ForEach(t =>
            {
                var variable = designTemplateDto.Variables.FirstOrDefault(v => v.Id == t.ProjectDesignVariableId);
                if (variable != null)
                {
                    variable.ScreeningValue = t.Value;
                    variable.ScreeningValueOld = t.Value;
                    variable.ScreeningTemplateValueId = t.Id;
                    variable.ScheduleDate = t.ScheduleDate;
                    variable.QueryStatus = t.QueryStatus;
                    variable.HasComments = t.IsComment;
                    variable.HasQueries = t.QueryStatus != null ? true : false;
                    variable.IsNaValue = t.IsNa;
                    variable.IsSystem = t.IsSystem;
                    variable.WorkFlowButton =
                        SetWorkFlowButton(t, workflowlevel, designTemplateDto, screeningTemplateBasic);

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

                    variable.IsSaved = variable.IsValid;
                }
            });

            if (!screeningTemplateBasic.IsLocked)
            {
                EditCheckProcess(designTemplateDto, values, screeningTemplateBasic);
                CheckSchedule(designTemplateDto, values, screeningTemplateBasic);
            }

            if (screeningTemplateBasic.IsDisable && string.IsNullOrEmpty(designTemplateDto.EditCheckMessage))
            {
                var template = Find(screeningTemplateBasic.Id);
                template.IsDisable = false;
                Update(template);
                Context.SaveChanges(_jwtTokenAccesser);
            }

            return designTemplateDto;
        }

        void EditCheckProcess(DesignScreeningTemplateDto projectDesignTemplateDto, List<Data.Dto.Screening.ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic)
        {
            var result = _editCheckImpactRepository.CheckValidation(values, screeningTemplateBasic, false);


            result.Where(t => t.IsTarget && t.ProjectDesignTemplateId == projectDesignTemplateDto.ProjectDesignTemplateId &&
            (t.CheckBy == EditCheckRuleBy.ByTemplate || t.CheckBy == EditCheckRuleBy.ByTemplateAnnotation)).ToList().ForEach(r =>
            {
                if (r.Operator == Operator.Warning && r.ValidateType != EditCheckValidateType.RuleValidated)
                {
                    projectDesignTemplateDto.IsWarning = true;
                    projectDesignTemplateDto.EditCheckMessage = $"{r.AutoNumber} {r.Message}";
                }
                else if (r.Operator == Operator.Enable && r.ValidateType != EditCheckValidateType.RuleValidated)
                {
                    projectDesignTemplateDto.IsRepeated = false;
                    projectDesignTemplateDto.IsSubmittedButton = false;
                    projectDesignTemplateDto.IsLocked = true;
                    projectDesignTemplateDto.EditCheckMessage = $"{r.AutoNumber} {r.Message}";
                }
            });

            var variableTargetResult = _editCheckImpactRepository.UpdateVariale(result.Where(x => x.IsTarget).ToList(), false, false);
            projectDesignTemplateDto.Variables.ForEach(r =>
            {
                var singleResult = variableTargetResult.Where(x => x.ProjectDesignVariableId == r.ProjectDesignVariableId).FirstOrDefault();
                if (singleResult != null)
                {
                    r.EditCheckValidation = new EditCheckTargetValidation();

                    if (singleResult.IsValueSet || (singleResult.IsSoftFetch && (string.IsNullOrEmpty(r.ScreeningValue))))
                    {
                        if (r.ScreeningValue != singleResult.Value)
                        {
                            _editCheckImpactRepository.InsertScreeningValue(projectDesignTemplateDto.ScreeningTemplateId,
                                                          (int)r.ProjectDesignVariableId, singleResult.Value, singleResult.Note, singleResult.IsSoftFetch, r.CollectionSource);
                        }

                        r.ScreeningValue = singleResult.Value;
                        r.ScreeningValueOld = singleResult.Value;
                    }

                    if (singleResult.OriginalValidationType != null)
                        r.ValidationType = (ValidationType)singleResult.OriginalValidationType;

                    r.EditCheckValidation.EditCheckMsg = singleResult.EditCheckMsg;
                    r.EditCheckValidation.isInfo = singleResult.isInfo;
                    r.EditCheckValidation.EditCheckDisable = singleResult.EditCheckDisable;
                }
                r.editCheckIds = GetEditCheckIds(result, (int)r.ProjectDesignVariableId);
            });


        }

        void CheckSchedule(DesignScreeningTemplateDto projectDesignTemplateDto, List<Data.Dto.Screening.ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic)
        {
            if (screeningTemplateBasic.ParentId == null)
            {
                var scheduleResult = _scheduleRuleRespository.ValidateByTemplate(values, screeningTemplateBasic, false);
                if (scheduleResult != null && scheduleResult.Count > 0)
                {
                    projectDesignTemplateDto.Variables.ForEach(r =>
                    {
                        var scheduleVariable = scheduleResult.Where(x => x.ProjectDesignVariableId == r.ProjectDesignVariableId).ToList();
                        if (scheduleVariable != null && scheduleVariable.Count > 0)
                        {
                            if (r.EditCheckValidation == null)
                            {
                                r.EditCheckValidation = new EditCheckTargetValidation();
                                r.EditCheckValidation.isInfo = true;
                            }

                            if (scheduleVariable.Any(x => x.ValidateType == EditCheckValidateType.Failed))
                                r.EditCheckValidation.isInfo = false;

                            if (scheduleVariable.Any(x => x.HasQueries))
                                r.EditCheckValidation.HasQueries = true;

                            var schMessage = scheduleResult.Select(t => new EditCheckMessage
                            {
                                AutoNumber = t.AutoNumber,
                                Message = t.Message,
                                ValidateType = t.ValidateType.GetDescription()
                            }).ToList();

                            r.EditCheckValidation.EditCheckMsg.AddRange(schMessage);
                        }
                    });
                }
            }
        }

        List<EditCheckIds> GetEditCheckIds(List<EditCheckValidateDto> editCheckValidateDtos, int projectDesignVariableId)
        {
            var editCheckIds = editCheckValidateDtos.
                Where(x => x.ProjectDesignVariableId == projectDesignVariableId).
                GroupBy(t => t.EditCheckId).Select(r => r.Key).ToList();

            var projectDesignVariableIds = editCheckValidateDtos.
                Where(x => editCheckIds.Contains(x.EditCheckId)).
               Select(t => t.ProjectDesignVariableId).Distinct().ToList();

            projectDesignVariableIds.ForEach(r =>
            {
                editCheckIds.AddRange(editCheckValidateDtos.
                Where(x => x.ProjectDesignVariableId == r).
                GroupBy(t => t.EditCheckId).Select(c => c.Key).ToList());
            });

            return editCheckIds.Distinct().
               Select(t => new EditCheckIds
               {
                   EditCheckId = t
               }).ToList();
        }
        public void SubmitReviewTemplate(int screeningTemplateId, bool isLockUnLock)
        {
            Context.DetectionAll();
            var screeningTemplateBasic = GetScreeningTemplateBasic(screeningTemplateId);
            var values = GetScreeningValues(screeningTemplateBasic.Id);
            var result = _editCheckImpactRepository.CheckValidation(values, screeningTemplateBasic, true);
            _editCheckImpactRepository.UpdateVariale(result.Where(x => x.IsTarget).ToList(), true, true);
            if (!isLockUnLock)
                _scheduleRuleRespository.ValidateByTemplate(values, screeningTemplateBasic, true);
        }

        public int GetProjectDesignId(int screeningTemplateId)
        {
            return All.Where(x => x.Id == screeningTemplateId).Select(r => r.ScreeningVisit.ScreeningEntry.ProjectDesignId).FirstOrDefault();
        }

        public int GeScreeningEntryId(int screeningTemplateId)
        {
            return All.Where(x => x.Id == screeningTemplateId).Select(r => r.ScreeningVisit.ScreeningEntryId).FirstOrDefault();
        }

        public List<ScreeningTemplateDto> GetTemplateTree(int screeningEntryId, List<Data.Dto.Screening.ScreeningTemplateValueBasic> templateValues, WorkFlowLevelDto workFlowLevel)
        {
            var result = All.Where(s =>
                    s.ScreeningVisit.ScreeningEntryId == screeningEntryId && s.DeletedDate == null).Select(s =>
                    new ScreeningTemplateDto
                    {
                        Id = s.Id,
                        ScreeningEntryId = s.ScreeningVisit.ScreeningEntryId,
                        ProjectDesignTemplateId = s.ProjectDesignTemplateId,
                        Status = s.Status,
                        ProjectDesignVisitId = s.ProjectDesignTemplate.ProjectDesignVisitId,
                        ProjectDesignTemplateName = s.ProjectDesignTemplate.TemplateName,
                        DesignOrder = s.RepeatSeqNo == null ? s.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(s.ProjectDesignTemplate.DesignOrder.ToString() + "." + s.RepeatSeqNo.Value.ToString()),
                        IsVisitRepeated = s.ScreeningVisit.RepeatedVisitNumber != null ? false :
                            workFlowLevel.LevelNo >= 0 && s.ProjectDesignTemplate.IsRepeated ? workFlowLevel.IsStartTemplate :
                            s.ProjectDesignTemplate.IsRepeated,
                        ProjectDesignVisitName = s.ScreeningVisit.ProjectDesignVisit.DisplayName +
                                                 Convert.ToString(s.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + s.ScreeningVisit.RepeatedVisitNumber),
                        Progress = s.Progress ?? 0,
                        ReviewLevel = s.ReviewLevel,
                        IsLocked = s.IsLocked,
                        MyReview = workFlowLevel.LevelNo == s.ReviewLevel,
                        ParentId = s.ParentId
                    }).ToList().OrderBy(o => o.ProjectDesignVisitId).ThenBy(t => t.DesignOrder).ToList();


            result.ForEach(s =>
            {
                s.StatusName = GetStatusName(new ScreeningTemplateBasic { ReviewLevel = s.ReviewLevel, Status = s.Status }, workFlowLevel.LevelNo == s.ReviewLevel, workFlowLevel);
                s.TemplateQueryStatus = _screeningTemplateValueRepository.GetQueryStatusByModel(templateValues, s.Id);
            });

            return result;
        }

        public List<MyReviewDto> GetScreeningTemplateReview()
        {
            var result = All.Where(x => x.DeletedDate == null
                                        && x.ReviewLevel != null && x.ReviewLevel > 0
                                        && (Context.ProjectWorkflowIndependent.Any(r => r.DeletedDate == null &&
                                                                                        r.ProjectWorkflow
                                                                                            .ProjectDesignId ==
                                                                                        x.ScreeningVisit.ScreeningEntry.ProjectDesignId
                                                                                        && r.SecurityRoleId ==
                                                                                        _jwtTokenAccesser.RoleId) ||
                                            Context.ProjectWorkflowLevel.Any(r => r.DeletedDate == null &&
                                                                                  r.ProjectWorkflow.ProjectDesignId ==
                                                                                  x.ScreeningVisit.ScreeningEntry.ProjectDesignId
                                                                                  && r.SecurityRoleId ==
                                                                                  _jwtTokenAccesser.RoleId
                                                                                  && r.LevelNo == x.ReviewLevel))
            ).Select(a => new MyReviewDto
            {
                ScreeningEntryId = a.ScreeningVisit.ScreeningEntryId,
                ProjectDesignTemplateId = a.ProjectDesignTemplateId,
                ScreeningDate = a.ScreeningVisit.ScreeningEntry.ScreeningDate,
                ScreeningNo = a.ScreeningVisit.ScreeningEntry.ScreeningNo,
                ProjectName = a.ScreeningVisit.ScreeningEntry.Project.ProjectName,
                VolunteerName = a.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                    ? a.ScreeningVisit.ScreeningEntry.Attendance.NoneRegister.Initial
                    : a.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,
                TemplateName = a.ProjectDesignTemplate.TemplateName,
                VistName = a.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                ReviewedLevel = a.ReviewLevel.ToString(),
                SubmittedDate = a.ScreeningTemplateReview.FirstOrDefault(c => c.Status == ScreeningTemplateStatus.Submitted)
                    .CreatedDate,
                SubmittedBy = a.ScreeningTemplateReview.FirstOrDefault(c => c.Status == ScreeningTemplateStatus.Submitted)
                    .CreatedByUser.UserName,
                LastReviewedDate = a.ScreeningTemplateReview
                    .FirstOrDefault(c => c.Status == ScreeningTemplateStatus.Reviewed && c.ReviewLevel == a.ReviewLevel - 1)
                    .CreatedDate,
                LastReviewedBy = a.ScreeningTemplateReview
                    .FirstOrDefault(c => c.Status == ScreeningTemplateStatus.Reviewed && c.ReviewLevel == a.ReviewLevel - 1)
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
            screeningTemplate.RepeatSeqNo = All.Count(x => x.ParentId == originalTemplate.Id) + 1;
            screeningTemplate.ScreeningVisitId = originalTemplate.ScreeningVisitId;
            screeningTemplate.IsDisable = originalTemplate.IsDisable;
            screeningTemplate.ProjectDesignTemplateId = originalTemplate.ProjectDesignTemplateId;
            screeningTemplate.Status = ScreeningTemplateStatus.Pending;
            screeningTemplate.Children = null;
            screeningTemplate.IsDisable = false;
            Add(screeningTemplate);

            return screeningTemplate;
        }


        public List<ScreeningTemplateLockUnlockDto> GetTemplatesLockUnlock(
            ScreeningTemplateLockUnlockParams lockUnlockParams)
        {
            var query = All.Where(t => t.DeletedDate == null);

            if (lockUnlockParams.ProjectDesignVisitId > 0)
                query = query.Where(x =>
                    x.ScreeningVisit.ProjectDesignVisitId == lockUnlockParams.ProjectDesignVisitId);

            if (lockUnlockParams.ProjectDesingId > 0)
                query = query.Where(x =>
                    x.ScreeningVisit.ProjectDesignVisit.ProjectDesignPeriodId ==
                    lockUnlockParams.ProjectDesingId);

            if (lockUnlockParams.ProjectId > 0)
                query = query.Where(x => x.ScreeningVisit.ScreeningEntry.ProjectId == lockUnlockParams.ProjectId);

            if (lockUnlockParams.VolunteerId > 0)
                query = query.Where(x => x.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId == lockUnlockParams.VolunteerId);

            if (lockUnlockParams.IsLock)
                query = query.Where(t => t.Status == ScreeningTemplateStatus.Pending || t.IsCompleteReview);
            else
                query = query.Where(t => t.Status == ScreeningTemplateStatus.Completed);

            query = query.Where(x => Context.ProjectWorkflowLevel.Any(t => t.DeletedDate == null && t.IsLock
                                                                                                 && t.ProjectWorkflow
                                                                                                     .ProjectDesignId ==
                                                                                                 x.ScreeningVisit.ScreeningEntry
                                                                                                     .ProjectDesignId
                                                                                                 && t.SecurityRoleId ==
                                                                                                 _jwtTokenAccesser
                                                                                                     .RoleId));

            var result = query.Select(t => new ScreeningTemplateLockUnlockDto
            {
                Id = t.Id,
                ProjectName = t.ScreeningVisit.ScreeningEntry.Project.ProjectName,
                Status = t.Status,
                ScreeningNo = t.ScreeningVisit.ScreeningEntry.ScreeningNo,
                StatusName = t.Status.GetDescription(),
                TemplateName = t.ProjectDesignTemplate.DesignOrder + " " + t.ProjectDesignTemplate.TemplateName,
                VisitName = t.ScreeningVisit.ProjectDesignVisit.DisplayName,
                VolunteerName = t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                    ? t.ScreeningVisit.ScreeningEntry.Attendance.NoneRegister.Initial
                    : t.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.FullName
            }).OrderByDescending(c => c.Id).ToList();

            return result;
        }

        public void VisitRepeat(int projectDesignVisitId, int screeningEntryId)
        {
            //var repeatedCount = 0;
            //var projectVisit = All.Where(x => x.ScreeningVisitId == projectDesignVisitId
            //                                  && x.ScreeningVisit.ScreeningEntryId == screeningEntryId).ToList();
            //if (projectVisit.Count > 0)
            //    repeatedCount = projectVisit.Max(x => x.RepeatedVisit ?? 0);
            //var templates = Context.ProjectDesignTemplate
            //    .Where(t => t.DeletedDate == null && t.ProjectDesignVisitId == projectDesignVisitId).ToList();
            //templates.ForEach(t =>
            //{
            //    var oldTemplate = Context.ScreeningTemplate.FirstOrDefault(r =>
            //        r.ScreeningVisit.ScreeningEntryId == screeningEntryId &&
            //        r.ScreeningVisitId == projectDesignVisitId && r.ProjectDesignTemplateId == t.Id);
            //    Add(new ScreeningTemplate
            //    {
            //        ScreeningEntryId = screeningEntryId,
            //        ProjectDesignTemplateId = t.Id,
            //        EditCheckDetailId = oldTemplate != null ? oldTemplate.EditCheckDetailId : null,
            //        RepeatedVisit = repeatedCount + 1,
            //        ScreeningVisitId = t.ProjectDesignVisitId,
            //        IsEditChecked = false,
            //        IsDisable = false,
            //        Status = ScreeningStatus.Pending
            //    });
            //});
        }

        public List<DashboardStudyStatusDto> GetDashboardStudyStatusByVisit(int projectId)
        {
            var projectDesign = Context.ProjectDesign.Where(x => x.ProjectId == projectId).FirstOrDefault();
            var workFlowLevel = new WorkFlowLevelDto();
            if (projectDesign != null) workFlowLevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesign.Id);

            var queryStatus = (from st in Context.ScreeningTemplate
                               join pdv in Context.ProjectDesignVisit on st.ScreeningVisitId equals pdv.Id into design
                               from pdesign in design.DefaultIfEmpty()
                               join se in Context.ScreeningEntry on st.ScreeningVisit.ScreeningEntryId equals se.Id into entry
                               from sEntry in entry.DefaultIfEmpty()
                               join p in Context.Project on sEntry.ProjectId equals p.Id into project
                               from p in project.DefaultIfEmpty()
                               where p.Id == projectId || p.ParentProjectId == projectId
                               group new { st, pdesign } by new { st.ScreeningVisitId, pdesign.DisplayName }
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
                               join st in Context.ScreeningTemplate on sEntry.Id equals st.ScreeningVisit.ScreeningEntryId into sTemplate
                               from template in sTemplate.DefaultIfEmpty()
                               join pdv in Context.ProjectDesignVisit on template.ScreeningVisitId equals pdv.Id into design
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
            var parentId = Context.Project.Where(x => x.Id == filters.ProjectId).FirstOrDefault().ParentProjectId;
            var parentIds = new List<int>();
            if (parentId == null)
            {
                parentIds = Context.Project.Where(x => x.ParentProjectId == filters.ProjectId).Select(y => y.Id).ToList();
            }
            else
            {
                parentIds.Add(filters.ProjectId);
            }

            var queryDtos = (from screening in Context.ScreeningEntry.Where(t =>
                        parentIds.Contains(t.ProjectId) && t.DeletedDate == null &&
                        (filters.PeriodIds == null || filters.PeriodIds.Contains(t.ProjectDesignPeriodId)) && (filters.SubjectIds == null || filters.SubjectIds.Contains(t.AttendanceId)))
                             join template in Context.ScreeningTemplate.Where(u =>
                                     (filters.TemplateIds == null || filters.TemplateIds.Contains(u.ProjectDesignTemplateId))
                                     && (filters.VisitIds == null ||
                                         filters.VisitIds.Contains(u.ScreeningVisit.ProjectDesignVisitId)) &&
                                     (filters.StatusIds == null || filters.StatusIds.Contains((int)u.Status)) && u.DeletedDate == null && u.ScreeningVisit.DeletedDate == null
                                     && u.ScreeningVisit.ProjectDesignVisit.DeletedDate == null) on screening.Id
                                 equals template.ScreeningVisit.ScreeningEntryId
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
                                 Visit = template.ScreeningVisit.ProjectDesignVisit.DisplayName +
                                         Convert.ToString(template.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + template.ScreeningVisit.RepeatedVisitNumber),
                                 VolunteerDelete = volunteer.DeletedDate,
                                 VolunteerName = volunteer.FullName == null ? nonregister.Initial : volunteer.AliasName,
                                 SubjectNo = volunteer.FullName == null ? nonregister.ScreeningNumber : volunteer.VolunteerNo,
                                 RandomizationNumber = volunteer.FullName == null
                                     ? nonregister.RandomizationNumber
                                     : projectsubject.Number,
                                 //ReviewLevelName = "Reviewed " + review.ReviewLevel,
                                 ReviewLevelName = review.ReviewLevel == 0
                                     ? ""
                                     : workflow.Levels.Where(x => x.LevelNo == review.ReviewLevel && x.DeletedDate == null).FirstOrDefault().SecurityRole
                                         .RoleShortName
                             }).OrderBy(x => x.Id).ToList();

            if (filters.ReviewStatus != null) queryDtos = queryDtos.Where(x => x.ReviewLevelName != null).ToList();

            var groupByTemp = queryDtos.GroupBy(x => x.Id).Select(y => new ReviewDto
            {
                Id = y.LastOrDefault().Id,
                SiteName = y.LastOrDefault().SiteName,
                ScreeningEntryId = y.LastOrDefault().ScreeningEntryId,
                ScreeningNo = y.LastOrDefault().ScreeningNo,
                StatusName = y.LastOrDefault().StatusName,
                ScreeningTemplateId = y.LastOrDefault().ScreeningTemplateId,
                ProjectCode = y.LastOrDefault().ProjectCode,
                ScreeningTemplateValue = y.LastOrDefault().ScreeningTemplateValue,
                Visit = y.LastOrDefault().Visit,
                VolunteerDelete = y.LastOrDefault().VolunteerDelete,
                VolunteerName = y.LastOrDefault().VolunteerName,
                SubjectNo = y.LastOrDefault().SubjectNo,
                RandomizationNumber = y.LastOrDefault().RandomizationNumber,
                //ReviewLevelName = "Reviewed " + review.ReviewLevel,
                ReviewLevelName = y.LastOrDefault().ReviewLevelName,
            }).Where(x => x.VolunteerDelete == null).ToList();

            return groupByTemp;
        }

        private WorkFlowButton SetWorkFlowButton(Data.Dto.Screening.ScreeningTemplateValueBasic screeningValue,
            WorkFlowLevelDto workflowlevel, DesignScreeningTemplateDto designTemplateDto,
            ScreeningTemplateBasic templateBasic)
        {
            var workFlowButton = new WorkFlowButton();
            var statusId = (int)templateBasic.Status;

            if (templateBasic.Status == ScreeningTemplateStatus.Completed)
            {
                designTemplateDto.MyReview = false;
                designTemplateDto.IsSubmittedButton = false;
                return workFlowButton;
            }

            if (statusId > 2)
            {
                if (workflowlevel.LevelNo <= templateBasic.ReviewLevel || workflowlevel.LevelNo == 0)
                    workFlowButton.SelfCorrection = workflowlevel.SelfCorrection &&
                                                    screeningValue.QueryStatus != QueryStatus.SelfCorrection;


                if (workflowlevel.LevelNo == screeningValue.AcknowledgeLevel ||
                    workflowlevel.LevelNo == 0 && workflowlevel.IsStartTemplate)
                    workFlowButton.Update = screeningValue.QueryStatus == QueryStatus.Open ||
                                            screeningValue.QueryStatus == QueryStatus.Reopened;

                if (workflowlevel.IsGenerateQuery && (designTemplateDto.MyReview || workflowlevel.LevelNo == 0))
                    workFlowButton.Generate = screeningValue.QueryStatus == null ||
                                              screeningValue.QueryStatus == QueryStatus.Closed;

                if (workflowlevel.LevelNo == screeningValue.ReviewLevel &&
                    screeningValue.ReviewLevel == screeningValue.AcknowledgeLevel)
                {
                    workFlowButton.DeleteQuery = screeningValue.QueryStatus == QueryStatus.Open;
                    workFlowButton.Review = screeningValue.QueryStatus == QueryStatus.Answered ||
                                            screeningValue.QueryStatus == QueryStatus.Resolved;
                }

                if (workflowlevel.LevelNo == 0 && workFlowButton.Review)
                    workFlowButton.Review = screeningValue.UserRoleId == _jwtTokenAccesser.RoleId;

                if (screeningValue.IsSystem && screeningValue.QueryStatus == QueryStatus.Open &&
                    workflowlevel.IsStartTemplate)
                {
                    workFlowButton.Update = screeningValue.QueryStatus == QueryStatus.Open;
                    workFlowButton.DeleteQuery = false;
                }

                if (!designTemplateDto.MyReview && workflowlevel.LevelNo == screeningValue.AcknowledgeLevel &&
                  screeningValue.AcknowledgeLevel != screeningValue.ReviewLevel)
                    workFlowButton.Acknowledge = screeningValue.QueryStatus == QueryStatus.Resolved ||
                                                 screeningValue.QueryStatus == QueryStatus.SelfCorrection;


            }

            workFlowButton.Clear = designTemplateDto.IsSubmittedButton;

            return workFlowButton;
        }

        private string GetStatusName(ScreeningTemplateBasic basicDetail, bool myReview, WorkFlowLevelDto workFlowLevel)
        {
            if (myReview) return "My Review";

            if (basicDetail.Status != ScreeningTemplateStatus.Completed && basicDetail.ReviewLevel != null &&
                basicDetail.ReviewLevel > 0)
            {
                if (workFlowLevel.WorkFlowText != null
                    && workFlowLevel.WorkFlowText.Any(x => x.LevelNo == basicDetail.ReviewLevel))
                    return workFlowLevel.WorkFlowText.FirstOrDefault(x => x.LevelNo == basicDetail.ReviewLevel)
                        ?.RoleName;
                return "Completed";
            }

            return basicDetail.Status.GetDescription();
        }

        public List<LockUnlockListDto> GetLockUnlockList(LockUnlockSearchDto lockUnlockParams)
        {
            var ProjectCode = Context.Project.Find(lockUnlockParams.ParentProjectId).ProjectCode;

            var ProjectDesignId = Context.ProjectDesign.Where(x => x.ProjectId == lockUnlockParams.ParentProjectId).Select(r => r.Id).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(ProjectDesignId);

            var screeningEntry = lockUnlockParams.Status == false ? Context.ScreeningEntry.Where(r => r.ProjectId == lockUnlockParams.ProjectId)
                : Context.ScreeningEntry.Where(r => r.ProjectDesignId == ProjectDesignId);

            if (lockUnlockParams.ParentProjectId != lockUnlockParams.ProjectId)
                screeningEntry = screeningEntry.Where(r => r.ProjectId == lockUnlockParams.ProjectId);

            if (lockUnlockParams.SubjectIds != null && lockUnlockParams.SubjectIds.Length > 0)
                screeningEntry = screeningEntry.Where(r => lockUnlockParams.SubjectIds.Contains(r.AttendanceId) && r.EntryType != AttendanceType.Screening);

            if (lockUnlockParams.PeriodIds != null && lockUnlockParams.PeriodIds.Length > 0)
                screeningEntry = screeningEntry.Where(r => lockUnlockParams.PeriodIds.Contains(r.ProjectDesignPeriodId));

            var grpresult = screeningEntry.Select(x => new LockUnlockListDto
            {
                Id = x.Id,
                screeningEntryId = x.Id,
                ProjectId = x.ProjectId,
                ProjectDesignId = x.ProjectDesignId,
                AttendanceId = x.AttendanceId,
                ParentProjectId = x.Project.ParentProjectId,
                ProjectCode = ProjectCode,
                Status = lockUnlockParams.Status,
                ProjectName = x.Project.ProjectCode,
                PeriodName = x.ProjectDesignPeriod.DisplayName,
                ScreeningNo = x.ScreeningNo,
                Initial = x.Attendance.Volunteer == null ? x.Attendance.NoneRegister.Initial : x.Attendance.Volunteer.AliasName,
                SubjectNo = x.Attendance.Volunteer == null ? x.Attendance.NoneRegister.ScreeningNumber : x.Attendance.Volunteer.VolunteerNo,
                RandomizationNumber = x.Attendance.Volunteer == null ? x.Attendance.NoneRegister.RandomizationNumber : x.Attendance.Volunteer.VolunteerNo,
                IsElectronicSignature = workflowlevel.IsElectricSignature,
                PeriodCount = All.Where(g => g.ScreeningVisit.ScreeningEntryId == x.Id && (lockUnlockParams.VisitIds == null || lockUnlockParams.VisitIds.Contains(g.ProjectDesignTemplate.ProjectDesignVisitId))
                                && (lockUnlockParams.TemplateIds == null || lockUnlockParams.TemplateIds.Contains(g.ProjectDesignTemplateId))
                                && g.DeletedDate == null && g.IsLocked != lockUnlockParams.Status
                                && (lockUnlockParams.DataEntryStatus != null && lockUnlockParams.DataEntryReviewStatus != null ? lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel)
                                  || lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status) : (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel))
                                  && (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status))))
                                    .Select(a => a.ScreeningVisit.ProjectDesignVisit.ProjectDesignPeriodId).Distinct().Count(),
                TemplateCount = All.Where(g => g.ScreeningVisit.ScreeningEntryId == x.Id && (lockUnlockParams.VisitIds == null || lockUnlockParams.VisitIds.Contains(g.ProjectDesignTemplate.ProjectDesignVisitId))
                                && (lockUnlockParams.TemplateIds == null || lockUnlockParams.TemplateIds.Contains(g.ProjectDesignTemplateId))
                                && g.DeletedDate == null && g.IsLocked != lockUnlockParams.Status
                                && (lockUnlockParams.DataEntryStatus != null && lockUnlockParams.DataEntryReviewStatus != null ? lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel)
                                  || lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status) : (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel))
                                  && (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status)))).Count(),
                VisitCount = All.Where(g => g.ScreeningVisit.ScreeningEntryId == x.Id && (lockUnlockParams.VisitIds == null || lockUnlockParams.VisitIds.Contains(g.ProjectDesignTemplate.ProjectDesignVisitId))
                                && (lockUnlockParams.TemplateIds == null || lockUnlockParams.TemplateIds.Contains(g.ProjectDesignTemplateId))
                                && g.DeletedDate == null && g.IsLocked != lockUnlockParams.Status
                                && (lockUnlockParams.DataEntryStatus != null && lockUnlockParams.DataEntryReviewStatus != null ? lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel)
                                  || lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status) : (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel))
                                  && (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status))))
                                    .Select(a => a.ScreeningVisit.Id).Distinct().Count(),
                lstTemplate = All.Where(g => g.ScreeningVisit.ScreeningEntryId == x.Id && (lockUnlockParams.VisitIds == null || lockUnlockParams.VisitIds.Contains(g.ProjectDesignTemplate.ProjectDesignVisitId))
                                && (lockUnlockParams.TemplateIds == null || lockUnlockParams.TemplateIds.Contains(g.ProjectDesignTemplateId))
                                && g.DeletedDate == null && g.IsLocked != lockUnlockParams.Status
                                && (lockUnlockParams.DataEntryStatus != null && lockUnlockParams.DataEntryReviewStatus != null ? lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel)
                                  || lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status) : (lockUnlockParams.DataEntryStatus == null || lockUnlockParams.DataEntryStatus.Contains(g.ReviewLevel))
                                  && (lockUnlockParams.DataEntryReviewStatus == null || lockUnlockParams.DataEntryReviewStatus.Contains((int)g.Status))))
                    .Select(t => new LockUnlockListDto
                    {
                        TemplateId = t.ProjectDesignTemplateId,
                        ScreeningTemplateId = t.Id,
                        screeningEntryId = t.ScreeningVisit.ScreeningEntryId,
                        ProjectCode = ProjectCode,
                        ProjectName = t.ScreeningVisit.ScreeningEntry.Project.ParentProjectId != null ? t.ScreeningVisit.ScreeningEntry.Project.ProjectCode : "",
                        PeriodName = t.ScreeningVisit.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                        ParentProjectId = t.ScreeningVisit.ScreeningEntry.Project.ParentProjectId,
                        VisitId = t.ScreeningVisit.ProjectDesignVisitId,
                        VisitName = t.ScreeningVisit.ProjectDesignVisit.DisplayName + Convert.ToString(t.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + t.ScreeningVisit.RepeatedVisitNumber),
                        ScreeningTemplateParentId = t.ParentId,
                        TemplateName = t.RepeatSeqNo == null && t.ParentId == null ? t.ProjectDesignTemplate.DesignOrder + " " + t.ProjectDesignTemplate.TemplateName
                                        : t.ProjectDesignTemplate.DesignOrder + "." + t.RepeatSeqNo + " " + t.ProjectDesignTemplate.TemplateName,
                        DesignOrder = t.ProjectDesignTemplate.DesignOrder.ToString(),
                        SeqNo = t.ProjectDesignTemplate.DesignOrder
                    }).OrderBy(b => b.VisitId).ThenBy(a => a.SeqNo).ThenBy(a => a.ScreeningTemplateId).ToList()
            }).OrderBy(x => x.ProjectId).ToList();

            return grpresult.Where(x => x.lstTemplate.Count > 0).ToList();
        }


        public ScreeningTemplateValueSaveBasics ValidateVariableValue(ScreeningTemplateValue screeningTemplateValue, List<EditCheckIds> EditCheckIds, CollectionSources? collectionSource)
        {
            var result = new ScreeningTemplateValueSaveBasics();
            result.Children = screeningTemplateValue.Children.Select(r => new ScreeningTemplateValueChildBasic { Id = r.Id }).ToList();
            result.Id = screeningTemplateValue.Id;

            if ((EditCheckIds != null && EditCheckIds.Count() > 0) || collectionSource == CollectionSources.Date
                || collectionSource == CollectionSources.DateTime || collectionSource == CollectionSources.Time)
            {
                var value = screeningTemplateValue.IsNa ? "NA" : screeningTemplateValue.Value;

                if (screeningTemplateValue.Children != null && screeningTemplateValue.Children.Count > 0)
                    value = string.Join(",", _screeningTemplateValueChildRepository.All.AsNoTracking().Where(x => x.ScreeningTemplateValueId == screeningTemplateValue.Id && x.Value == "true").Select(t => t.ProjectDesignVariableValueId));

                var screeningTemplate = All.AsNoTracking().Where(x => x.Id == screeningTemplateValue.ScreeningTemplateId).FirstOrDefault();

                var editResult = _editCheckImpactRepository.VariableValidateProcess(screeningTemplate.ScreeningVisit.ScreeningEntryId, screeningTemplateValue.ScreeningTemplateId,
                    screeningTemplateValue.IsNa ? "NA" : screeningTemplateValue.Value, screeningTemplate.ProjectDesignTemplateId,
                    screeningTemplateValue.ProjectDesignVariableId, EditCheckIds, false, screeningTemplate.ScreeningVisit.RepeatedVisitNumber);

                var scheduleResult = _scheduleRuleRespository.ValidateByVariable(screeningTemplate.ScreeningVisit.ScreeningEntryId, screeningTemplate.Id,
                 screeningTemplateValue.Value, screeningTemplate.ProjectDesignTemplateId,
                 screeningTemplateValue.ProjectDesignVariableId, true);

                result.EditCheckResult = _scheduleRuleRespository.VariableResultProcess(editResult, scheduleResult);
            }

            return result;
        }
    }
}

