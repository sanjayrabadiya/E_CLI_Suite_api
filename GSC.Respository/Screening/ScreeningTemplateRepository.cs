using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.Project.Workflow;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateRepository : GenericRespository<ScreeningTemplate>,
        IScreeningTemplateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateValueChildRepository _screeningTemplateValueChildRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IEditCheckImpactRepository _editCheckImpactRepository;
        private readonly IMapper _mapper;
        private readonly IScheduleRuleRespository _scheduleRuleRespository;
        private readonly IGSCContext _context;
        public ScreeningTemplateRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IUploadSettingRepository uploadSettingRepository, IMapper mapper,
            IProjectWorkflowRepository projectWorkflowRepository,
            IEditCheckImpactRepository editCheckImpactRepository,
            IScheduleRuleRespository scheduleRuleRespository,
            IScreeningTemplateValueChildRepository screeningTemplateValueChildRepository)
            : base(context)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _scheduleRuleRespository = scheduleRuleRespository;
            _screeningTemplateValueChildRepository = screeningTemplateValueChildRepository;
            _editCheckImpactRepository = editCheckImpactRepository;
            _context = context;
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
                   ScreeningVisitId = c.ScreeningVisitId,
                   ProjectDesignVisitId = c.ScreeningVisit.ProjectDesignVisitId,
                   IsNoCRF = c.ScreeningVisit.ProjectDesignVisit.IsNonCRF,
                   IsLocked = c.IsLocked,
                   IsDisable = c.IsDisable,
                   PatientStatus = c.ScreeningVisit.ScreeningEntry.Randomization.PatientStatusId,
                   VisitStatus = c.ScreeningVisit.Status,
                   ParentId = c.ParentId
               }).FirstOrDefault();
        }

        private List<Data.Dto.Screening.ScreeningTemplateValueBasic> GetScreeningValues(int screeningTemplateId)
        {
            return _screeningTemplateValueRepository.All.AsNoTracking().Where(t => t.ScreeningTemplateId == screeningTemplateId)
                    .ProjectTo<Data.Dto.Screening.ScreeningTemplateValueBasic>(_mapper.ConfigurationProvider).ToList();
        }

        public DesignScreeningTemplateDto GetScreeningTemplate(DesignScreeningTemplateDto designTemplateDto, int screeningTemplateId)
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
                var variable = designTemplateDto.Variables.FirstOrDefault(v => v.ProjectDesignVariableId == t.ProjectDesignVariableId);
                if (variable != null)
                {
                    variable.ScreeningValue = t.Value;
                    variable.ScreeningValueOld = t.IsNa ? "N/A" : t.Value;
                    variable.ScreeningTemplateValueId = t.Id;
                    variable.ScheduleDate = t.ScheduleDate;
                    variable.QueryStatus = t.QueryStatus;
                    variable.HasComments = t.IsComment;
                    variable.HasQueries = t.QueryStatus != null ? true : false;
                    variable.IsNaValue = t.IsNa;
                    variable.IsSystem = t.QueryStatus == QueryStatus.Closed ? false : t.IsSystem;
                    variable.WorkFlowButton = SetWorkFlowButton(t, workflowlevel, designTemplateDto, screeningTemplateBasic);

                    variable.DocPath = t.DocPath != null ? t.DocPath : null;
                    variable.DocFullPath = t.DocPath != null ? documentUrl + t.DocPath : null;
                    if (!string.IsNullOrWhiteSpace(variable.ScreeningValue) || variable.IsNaValue)
                        variable.IsValid = true;

                    if (variable.Values != null)
                        variable.Values.ToList().ForEach(val =>
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

            if (designTemplateDto.Status == ScreeningTemplateStatus.Pending && designTemplateDto.IsSchedule &&
                (screeningTemplateBasic.PatientStatus == ScreeningPatientStatus.ScreeningFailure ||
               screeningTemplateBasic.VisitStatus == ScreeningVisitStatus.Withdrawal ||
               screeningTemplateBasic.VisitStatus == ScreeningVisitStatus.Missed ||
               screeningTemplateBasic.VisitStatus == ScreeningVisitStatus.OnHold))
                designTemplateDto.IsSubmittedButton = false;

            return designTemplateDto;
        }

        public bool IsRepated(int screeningTemplateId)
        {
            return All.Any(x => x.DeletedDate == null &&
                                x.Id == screeningTemplateId && (x.ParentId != null || x.ScreeningVisit.ParentId != null));
        }

        void EditCheckProcess(DesignScreeningTemplateDto projectDesignTemplateDto, List<Data.Dto.Screening.ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic)
        {
            var result = _editCheckImpactRepository.CheckValidation(projectDesignTemplateDto, values, screeningTemplateBasic, false);


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
            projectDesignTemplateDto.Variables.ToList().ForEach(r =>
            {
                var singleResult = variableTargetResult.Where(x => x.ProjectDesignVariableId == r.ProjectDesignVariableId).FirstOrDefault();
                if (singleResult != null)
                {
                    r.EditCheckValidation = new EditCheckTargetValidation();

                    if (singleResult.IsValueSet || (singleResult.IsSoftFetch && (string.IsNullOrEmpty(r.ScreeningValue))))
                    {
                        if (Convert.ToString(r.ScreeningValue ?? "") != Convert.ToString(singleResult.Value ?? ""))
                        {
                            _editCheckImpactRepository.InsertScreeningValue(projectDesignTemplateDto.ScreeningTemplateId,
                                                          (int)r.ProjectDesignVariableId, singleResult.Value, singleResult.Note, singleResult.IsSoftFetch, r.CollectionSource, singleResult.EditCheckDisable);
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
                    projectDesignTemplateDto.Variables.ToList().ForEach(r =>
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
            _context.DetachAllEntities();
            var screeningTemplateBasic = GetScreeningTemplateBasic(screeningTemplateId);
            var values = GetScreeningValues(screeningTemplateBasic.Id);
            var result = _editCheckImpactRepository.CheckValidation(null, values, screeningTemplateBasic, true);
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



        public List<ScreeningTemplateTree> GetTemplateTree(int screeningEntryId, List<Data.Dto.Screening.ScreeningTemplateValueBasic> templateValues, WorkFlowLevelDto workFlowLevel)
        {

            var result = All.Where(s => s.ScreeningVisit.ScreeningEntryId == screeningEntryId && s.DeletedDate == null
            && s.ScreeningVisit.Status >= ScreeningVisitStatus.Open).Select(t => new ScreeningTemplateTree
            {
                Id = t.Id,
                ScreeningVisitId = t.ScreeningVisitId,
                ProjectDesignTemplateId = t.ProjectDesignTemplateId,
                Status = t.Status,
                ProjectDesignTemplateName = t.ProjectDesignTemplate.TemplateName,
                DesignOrder = t.RepeatSeqNo == null ? t.ProjectDesignTemplate.DesignOrder : Convert.ToDecimal(t.ProjectDesignTemplate.DesignOrder.ToString() + "." + t.RepeatSeqNo.Value.ToString()),
                Progress = t.Progress ?? 0,
                ReviewLevel = t.ReviewLevel,
                IsLocked = t.IsLocked,
                MyReview = workFlowLevel.LevelNo == t.ReviewLevel,
                ParentId = t.ParentId,
            }).ToList();

            result.ForEach(a =>
            {
                a.StatusName = GetStatusName(new ScreeningTemplateBasic { ReviewLevel = a.ReviewLevel, Status = a.Status }, workFlowLevel.LevelNo == a.ReviewLevel, workFlowLevel);
                a.TemplateQueryStatus = _screeningTemplateValueRepository.GetQueryStatusByModel(templateValues, a.Id);
            });

            return result;
        }

        public List<MyReviewDto> GetScreeningTemplateReview()
        {
            var result = All.Where(x => x.DeletedDate == null
                                        && x.ReviewLevel != null && x.ReviewLevel > 0
                                        && (_context.ProjectWorkflowIndependent.Any(r => r.DeletedDate == null &&
                                                                                        r.ProjectWorkflow
                                                                                            .ProjectDesignId ==
                                                                                        x.ScreeningVisit.ScreeningEntry.ProjectDesignId
                                                                                        && r.SecurityRoleId ==
                                                                                        _jwtTokenAccesser.RoleId) ||
                                            _context.ProjectWorkflowLevel.Any(r => r.DeletedDate == null &&
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
                VolunteerName = a.ScreeningVisit.ScreeningEntry.RandomizationId != null
                    ? a.ScreeningVisit.ScreeningEntry.Randomization.Initial
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
            screeningTemplate.ProjectDesignTemplateId = originalTemplate.ProjectDesignTemplateId;
            screeningTemplate.Status = ScreeningTemplateStatus.Pending;
            screeningTemplate.Children = null;
            screeningTemplate.IsDisable = false;
            Add(screeningTemplate);

            return screeningTemplate;
        }

        public IList<ReviewDto> GetReviewReportList(ReviewSearchDto filters)
        {
            var parentId = _context.Project.Where(x => x.Id == filters.ProjectId).FirstOrDefault().ParentProjectId;
            var parentIds = new List<int>();
            if (parentId == null)
            {
                parentIds = _context.Project.Where(x => x.ParentProjectId == filters.ProjectId).Select(y => y.Id).ToList();
            }
            else
            {
                parentIds.Add(filters.ProjectId);
            }

            var result = All.Where(x => x.DeletedDate == null);
            if (filters.ReviewStatus != null)
            {
                result = result.Where(y => y.Status != ScreeningTemplateStatus.Pending && y.Status != ScreeningTemplateStatus.InProcess);
            }

            if (filters.ReviewStatus != null) result = result.Where(x => x.ReviewLevel != null);
            if (parentIds != null) result = result.Where(x => parentIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectId));
            if (filters.PeriodIds != null) result = result.Where(x => filters.PeriodIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectDesignPeriodId));
            if (filters.VisitIds != null) result = result.Where(x => filters.VisitIds.Contains(x.ScreeningVisit.ProjectDesignVisitId));
            if (filters.SubjectIds != null) result = result.Where(x => filters.SubjectIds.Contains(x.ScreeningVisit.ScreeningEntry.Id));
            if (filters.TemplateIds != null) result = result.Where(x => filters.TemplateIds.Contains(x.ProjectDesignTemplateId));
            if (filters.StatusIds != null) result = result.Where(x => filters.StatusIds.Contains((int)x.Status));
            if (filters.ReviewStatus != null) result = result.Where(x => filters.ReviewStatus.Contains(x.ReviewLevel));
            result = result.Where(x => x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.DeletedDate == null);

            return result.Select(r => new ReviewDto
            {
                Id = r.Id,
                SiteName = string.IsNullOrEmpty(r.ScreeningVisit.ScreeningEntry.Project.SiteName) ? r.ScreeningVisit.ScreeningEntry.Project.ProjectName : r.ScreeningVisit.ScreeningEntry.Project.SiteName,
                ScreeningEntryId = r.ScreeningVisit.ScreeningEntryId,
                ScreeningNo = r.ScreeningVisit.ScreeningEntry.ScreeningNo,
                ReviewLevel = r.ReviewLevel,
                StatusName = r.Status.GetDescription(),
                ScreeningTemplateId = r.Id,
                ProjectCode = r.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                ScreeningTemplateValue = r.ProjectDesignTemplate.TemplateName,
                Visit = r.ScreeningVisit.ProjectDesignVisit.DisplayName,
                VolunteerName = r.ScreeningVisit.ScreeningEntry.AttendanceId != null ? r.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName : r.ScreeningVisit.ScreeningEntry.Randomization.Initial,
                SubjectNo = r.ScreeningVisit.ScreeningEntry.AttendanceId != null ? r.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.VolunteerNo : r.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber,
                RandomizationNumber = r.ScreeningVisit.ScreeningEntry.AttendanceId != null ? r.ScreeningVisit.ScreeningEntry.Attendance.ProjectSubject.Number : r.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber,
                ReviewLevelName = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflow.ProjectDesignId == r.ScreeningVisit.ScreeningEntry.ProjectDesignId
                && x.LevelNo == r.ReviewLevel && x.DeletedDate == null).Select(t => t.SecurityRole.RoleShortName).FirstOrDefault()

            }).ToList();
        }

        private WorkFlowButton SetWorkFlowButton(Data.Dto.Screening.ScreeningTemplateValueBasic screeningValue,
            WorkFlowLevelDto workflowlevel, DesignScreeningTemplateDto designTemplateDto,
            ScreeningTemplateBasic templateBasic)
        {
            var workFlowButton = new WorkFlowButton();
            var statusId = (int)templateBasic.Status;

            if (templateBasic.IsLocked == true)
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

                if (templateBasic.Status != ScreeningTemplateStatus.Completed && workflowlevel.IsGenerateQuery && (designTemplateDto.MyReview || workflowlevel.LevelNo == 0))
                    workFlowButton.Generate = screeningValue.QueryStatus == null ||
                                              screeningValue.QueryStatus == QueryStatus.Closed;

                if (workflowlevel.LevelNo == screeningValue.ReviewLevel &&
                    screeningValue.ReviewLevel == screeningValue.AcknowledgeLevel)
                    workFlowButton.Review = screeningValue.QueryStatus == QueryStatus.Answered || screeningValue.QueryStatus == QueryStatus.Resolved;

                if (workflowlevel.LevelNo == 0 && workFlowButton.Review)
                    workFlowButton.Review = screeningValue.UserRoleId == _jwtTokenAccesser.RoleId;

                if (screeningValue.IsSystem && screeningValue.QueryStatus == QueryStatus.Open && workflowlevel.IsStartTemplate)
                    workFlowButton.Update = screeningValue.QueryStatus == QueryStatus.Open;


                if (!screeningValue.IsSystem && screeningValue.QueryStatus == QueryStatus.Open && screeningValue.UserRoleId == _jwtTokenAccesser.RoleId)
                {
                    workFlowButton.Update = false;
                    workFlowButton.DeleteQuery = true;
                }

                if (!designTemplateDto.MyReview && workflowlevel.LevelNo == screeningValue.AcknowledgeLevel &&
                  screeningValue.AcknowledgeLevel != screeningValue.ReviewLevel)
                    workFlowButton.Acknowledge = screeningValue.QueryStatus == QueryStatus.Resolved ||
                                                 screeningValue.QueryStatus == QueryStatus.SelfCorrection;


                if (workflowlevel.LevelNo > 0 && templateBasic.IsNoCRF && !workflowlevel.IsNoCRF)
                {
                    workFlowButton.Generate = false;
                    workFlowButton.SelfCorrection = false;
                    workFlowButton.Update = false;
                    workFlowButton.Acknowledge = false;
                }
            }

            workFlowButton.Clear = designTemplateDto.IsSubmittedButton;

            return workFlowButton;
        }

        public string GetStatusName(ScreeningTemplateBasic basicDetail, bool myReview, WorkFlowLevelDto workFlowLevel)
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
            var ProjectCode = _context.Project.Find(lockUnlockParams.ParentProjectId).ProjectCode;

            var ProjectDesignId = _context.ProjectDesign.Where(x => x.ProjectId == lockUnlockParams.ParentProjectId).Select(r => r.Id).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(ProjectDesignId);

            //var screeningEntry = lockUnlockParams.Status == false ? _context.ScreeningEntry.Where(r => r.ProjectId == lockUnlockParams.ProjectId)
            //    : _context.ScreeningEntry.Where(r => r.ProjectDesignId == ProjectDesignId);

            var screeningEntry = _context.ScreeningEntry.Where(r => r.ProjectDesignId == ProjectDesignId);

            if (lockUnlockParams.ParentProjectId != lockUnlockParams.ProjectId)
                screeningEntry = screeningEntry.Where(r => r.ProjectId == lockUnlockParams.ProjectId);

            if (lockUnlockParams.SubjectIds != null && lockUnlockParams.SubjectIds.Length > 0)
                screeningEntry = screeningEntry.Where(r => lockUnlockParams.SubjectIds.Contains(r.Id) && r.EntryType != DataEntryType.Screening);

            if (lockUnlockParams.PeriodIds != null && lockUnlockParams.PeriodIds.Length > 0)
                screeningEntry = screeningEntry.Where(r => lockUnlockParams.PeriodIds.Contains(r.ProjectDesignPeriodId));

            var grpresult = screeningEntry.Select(x => new LockUnlockListDto
            {
                Id = x.Id,
                screeningEntryId = x.Id,
                ProjectId = x.ProjectId,
                ProjectDesignId = x.ProjectDesignId,
                ParentProjectId = x.Project.ParentProjectId,
                ProjectCode = ProjectCode,
                Status = lockUnlockParams.Status,
                ProjectName = x.Project.ProjectCode,
                PeriodName = x.ProjectDesignPeriod.DisplayName,
                ScreeningNo = x.ScreeningNo,
                Initial = x.RandomizationId != null ? x.Randomization.Initial : x.Attendance.Volunteer.AliasName,
                SubjectNo = x.RandomizationId != null ? x.Randomization.ScreeningNumber : x.Attendance.Volunteer.VolunteerNo,
                RandomizationNumber = x.RandomizationId != null ? x.Randomization.RandomizationNumber : x.Attendance.Volunteer.VolunteerNo,
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

                var screeningTemplate = All.AsNoTracking().Where(x => x.Id == screeningTemplateValue.ScreeningTemplateId).
                    Select(r => new { r.Id, r.ScreeningVisitId, r.ParentId, VisitParent = r.ScreeningVisit.ParentId, r.ProjectDesignTemplateId, r.ScreeningVisit.ScreeningEntryId, r.ScreeningVisit.ProjectDesignVisitId }).FirstOrDefault();

                var editResult = _editCheckImpactRepository.VariableValidateProcess(screeningTemplate.ScreeningEntryId, screeningTemplateValue.ScreeningTemplateId,
                    screeningTemplateValue.IsNa ? "NA" : screeningTemplateValue.Value, screeningTemplate.ProjectDesignTemplateId,
                    screeningTemplateValue.ProjectDesignVariableId, EditCheckIds, false, screeningTemplate.ScreeningVisitId, screeningTemplate.ProjectDesignVisitId, screeningTemplateValue.IsNa);

                if (screeningTemplate.ParentId == null && screeningTemplate.VisitParent == null)
                {
                    var scheduleResult = _scheduleRuleRespository.ValidateByVariable(screeningTemplate.ScreeningEntryId, screeningTemplate.ScreeningVisitId,
                                       screeningTemplateValue.Value, screeningTemplate.ProjectDesignTemplateId,
                                       screeningTemplateValue.ProjectDesignVariableId, true);

                    result.EditCheckResult = _scheduleRuleRespository.VariableResultProcess(editResult, scheduleResult);
                }
                else
                    result.EditCheckResult = _scheduleRuleRespository.VariableResultProcess(editResult, null);
            }

            return result;
        }

        public IList<DropDownDto> GetTemplateByLockedDropDown(LockUnlockDDDto lockUnlockDDDto)
        {
            var ParentProject = _context.Project.FirstOrDefault(x => x.Id == lockUnlockDDDto.ChildProjectId).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == lockUnlockDDDto.ChildProjectId).ToList().Select(x => x.Id).ToList();

            var Templates = All.Include(a => a.ProjectDesignTemplate).Include(a => a.ScreeningVisit)
                .ThenInclude(a => a.ScreeningEntry)
                .Where(a => a.DeletedDate == null && ParentProject != null ? a.ScreeningVisit.ScreeningEntry.ProjectId == lockUnlockDDDto.ChildProjectId : sites.Contains(a.ScreeningVisit.ScreeningEntry.ProjectId)
                ).ToList();

            if (lockUnlockDDDto.SubjectIds != null)
                Templates = Templates.Where(a => lockUnlockDDDto.SubjectIds.Contains(a.ScreeningVisit.ScreeningEntryId)).ToList();

            if (lockUnlockDDDto.Id != null)
                Templates = Templates.Where(a => lockUnlockDDDto.Id.Contains(a.ScreeningVisit.ProjectDesignVisitId)).ToList();

            Templates = Templates.Where(a => a.IsLocked == !lockUnlockDDDto.IsLock).ToList();

            return Templates.Select(x => new DropDownDto
            {
                Id = x.ProjectDesignTemplateId,
                Value = x.ProjectDesignTemplate.TemplateName
            }).Distinct().ToList();
        }

        public IList<VisitDeviationReport> GetVisitDeviationReport(VisitDeviationReportSearchDto filters)
        {
            int? parentprojectid = 0;
            var parentId = _context.Project.Where(x => x.Id == filters.ProjectId).FirstOrDefault().ParentProjectId;
            var parentIds = new List<int>();
            if (parentId == null)
            {
                parentIds = _context.Project.Where(x => x.ParentProjectId == filters.ProjectId).Select(y => y.Id).ToList();
                parentprojectid = filters.ProjectId;
            }
            else
            {
                parentIds.Add(filters.ProjectId);
                parentprojectid = parentId;
            }
            var studycode = _context.Project.Where(x => x.Id == parentprojectid).FirstOrDefault().ProjectCode;

            //    string sqlqry = @";with cts as(
            //                        select *,ROW_NUMBER() OVER (ORDER BY SiteCode,Initial) Id,'" + studycode + @"' StudyCode,'' RefValueExcel,'' TargetValueExcel,'Date' Unit,
            //                                case when TargetValue < DATEADD(day,(NoOfDay - NegativeDeviation) , RefValue) then DATEDIFF(DAY,DATEADD(day,(NoOfDay - NegativeDeviation) , RefValue), TargetValue) else
            //                          case when TargetValue > DATEADD(day,(NoOfDay + PositiveDeviation) , RefValue) then DATEDIFF(DAY, DATEADD(day,(NoOfDay + PositiveDeviation) , RefValue), TargetValue) end end Deviation
            //                        from
            //                        (
            //                         select NoneRegister.Initial,NoneRegister.ScreeningNumber ScreeningNo,ScreeningEntry.ScreeningDate,ProjectDesignVisit.DisplayName RefVisit,
            //                         ProjectDesignTemplate.TemplateName RefTemplate,ProjectDesignVariable.VariableName RefVariable,
            //                            convert(varchar,convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,ScreeningTemplateValue.Value)))) RefValue,
            //                         convert(varchar,convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,tvalue.Value)))) TargetValue,
            //                            tVisit.DisplayName TargetVisit,tDesignTemplate.TemplateName TargetTemplate,tvariable.VariableName TargetVariable,
            //                         ProjectScheduleTemplate.NoOfDay,ProjectScheduleTemplate.PositiveDeviation,ProjectScheduleTemplate.NegativeDeviation,Project.ProjectCode SiteCode,
            //                            CASE WHEN convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,tvalue.Value))) >= DATEADD(day,(NoOfDay - NegativeDeviation) , convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,ScreeningTemplateValue.Value)))) 
            //                            AND  convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,tvalue.Value))) <= DATEADD(day,(NoOfDay + PositiveDeviation) , convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,ScreeningTemplateValue.Value)))) THEN 1 ELSE 0 END flag,
            //                            ScreeningEntry.AttendanceId,ScreeningTemplateValue.ProjectDesignVariableId,ScreeningTemplate.ProjectDesignTemplateId,ScreeningTemplate.ProjectDesignVisitId,
            //                            ProjectDesignVisit.ProjectDesignPeriodId,Project.Id ProjectId

            //                         from
            //                         ScreeningTemplateValue
            //                         inner join
            //                         ScreeningTemplate
            //                         on
            //                         ScreeningTemplateValue.ScreeningTemplateId = ScreeningTemplate.Id and
            //                         ScreeningTemplate.DeletedDate is null and
            //                         ScreeningTemplateValue.value is not null and
            //                         isdate(ScreeningTemplateValue.value) = 1
            //                         inner join
            //                         ScreeningEntry
            //                         on
            //                         ScreeningTemplate.ScreeningEntryId = ScreeningEntry.Id
            //                         inner join
            //                         Attendance
            //                         on
            //                         ScreeningEntry.AttendanceId = Attendance.Id
            //                         inner join 
            //                         NoneRegister
            //                         on
            //                         Attendance.Id = NoneRegister.AttendanceId
            //                         inner join
            //                         ProjectDesignVariable
            //                         on
            //                         ScreeningTemplateValue.ProjectDesignVariableId = ProjectDesignVariable.id and
            //                         ProjectDesignVariable.deleteddate is null
            //                         inner join
            //                         ProjectDesignTemplate
            //                         on
            //                         ProjectDesignVariable.projectdesigntemplateid = ProjectDesignTemplate.id and
            //                         ProjectDesignTemplate.deleteddate is null
            //                         inner join
            //                         projectdesignvisit
            //                         on
            //                         ProjectDesignTemplate.projectdesignvisitid = projectdesignvisit.id and
            //                         projectdesignvisit.deleteddate is null
            //                         inner join
            //                         projectschedule
            //                         on
            //                         projectschedule.projectdesignvariableid = ProjectDesignVariable.Id and
            //                         projectschedule.projectdesigntemplateid = ProjectDesignTemplate.Id and
            //                         projectschedule.deleteddate is null
            //                         inner join
            //                         ProjectScheduletemplate
            //                         on
            //                         projectscheduletemplate.projectscheduleid = ProjectSchedule.id and
            //                         ProjectSchedule.deleteddate is null and
            //                         projectscheduletemplate.operator = 2
            //                         inner join
            //                         ScreeningTemplateValue tvalue
            //                         on
            //                         tvalue.projectdesignvariableid = ProjectScheduletemplate.projectdesignvariableid and
            //                         tvalue.DeletedDate is null and
            //                         tvalue.Value is not null and
            //                         ISDATE(tvalue.Value) = 1
            //                         inner join
            //                         ScreeningTemplate tTemplate
            //                         on
            //                         tvalue.ScreeningTemplateId = tTemplate.Id and
            //                         tTemplate.ScreeningEntryId = ScreeningEntry.Id and
            //                         tTemplate.ProjectDesignTemplateId = ProjectScheduletemplate.ProjectDesignTemplateId
            //                         inner join
            //                         ProjectDesignVariable tvariable
            //                         on
            //                         tvalue.ProjectDesignVariableId = tvariable.Id and
            //                         tvariable.DeletedDate is null 
            //                         inner join
            //                         ProjectDesignTemplate tDesignTemplate
            //                         on
            //                         tvariable.ProjectDesignTemplateId = tDesignTemplate.Id and
            //                         tDesignTemplate.DeletedDate is null 
            //                         inner join
            //                         ProjectDesignVisit tVisit
            //                         on
            //                         tDesignTemplate.ProjectDesignVisitId = tVisit.Id and
            //                         tVisit.DeletedDate is null
            //                         inner join
            //                         Project 
            //                         on
            //                         Attendance.ProjectId = Project.Id
            //                        ) a
            //                        )
            //select * from cts where flag=0
            //                        ";
            string sqlqry = @";with cts as(
                                select *,ROW_NUMBER() OVER (ORDER BY SiteCode,Initial) Id,'" + studycode + @"' StudyCode,'' RefValueExcel,'' TargetValueExcel,'Date' Unit,
                                        case when TargetValue < DATEADD(day,(NoOfDay - NegativeDeviation) , RefValue) then DATEDIFF(DAY,DATEADD(day,(NoOfDay - NegativeDeviation) , RefValue), TargetValue) else
		                                case when TargetValue > DATEADD(day,(NoOfDay + PositiveDeviation) , RefValue) then DATEDIFF(DAY, DATEADD(day,(NoOfDay + PositiveDeviation) , RefValue), TargetValue) end end Deviation
                                from
                                (
	                                select Randomization.Initial,Randomization.ScreeningNumber ScreeningNo,isnull(Randomization.RandomizationNumber,'') RandomizationNumber,
									ScreeningEntry.ScreeningDate,ProjectDesignVisit.DisplayName RefVisit,
	                                ProjectDesignTemplate.TemplateName RefTemplate,ProjectDesignVariable.VariableName RefVariable,
                                    convert(varchar,convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,ScreeningTemplateValue.Value)))) RefValue,
	                                convert(varchar,convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,tvalue.Value)))) TargetValue,
                                    tVisit.DisplayName TargetVisit,tDesignTemplate.TemplateName TargetTemplate,tvariable.VariableName TargetVariable,
	                                ProjectScheduleTemplate.NoOfDay,ProjectScheduleTemplate.PositiveDeviation,ProjectScheduleTemplate.NegativeDeviation,Project.ProjectCode SiteCode,
                                    CASE WHEN convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,tvalue.Value))) >= DATEADD(day,(NoOfDay - NegativeDeviation) , convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,ScreeningTemplateValue.Value)))) 
                                    AND  convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,tvalue.Value))) <= DATEADD(day,(NoOfDay + PositiveDeviation) , convert(date,dateadd(HOUR,5,dateadd(MINUTE,30,ScreeningTemplateValue.Value)))) THEN 1 ELSE 0 END flag,
                                    ScreeningEntry.RandomizationId,ScreeningTemplateValue.ProjectDesignVariableId,ScreeningTemplate.ProjectDesignTemplateId,ScreeningVisit.ProjectDesignVisitId,
                                    ProjectDesignVisit.ProjectDesignPeriodId,Project.Id ProjectId
	                                from
	                                ScreeningTemplateValue
	                                inner join
	                                ScreeningTemplate
	                                on
	                                ScreeningTemplateValue.ScreeningTemplateId = ScreeningTemplate.Id and
	                                ScreeningTemplate.DeletedDate is null and
	                                ScreeningTemplateValue.value is not null and
	                                isdate(ScreeningTemplateValue.value) = 1
									inner join
									ScreeningVisit 
									on
									ScreeningTemplate.ScreeningVisitId = ScreeningVisit.Id
	                                inner join
	                                ScreeningEntry
	                                on
	                                ScreeningVisit.ScreeningEntryId = ScreeningEntry.Id
	                                inner join
	                                Randomization
	                                on
	                                ScreeningEntry.RandomizationId = Randomization.Id
	                                inner join
	                                ProjectDesignVariable
	                                on
	                                ScreeningTemplateValue.ProjectDesignVariableId = ProjectDesignVariable.id and
	                                ProjectDesignVariable.deleteddate is null
	                                inner join
	                                ProjectDesignTemplate
	                                on
	                                ProjectDesignVariable.projectdesigntemplateid = ProjectDesignTemplate.id and
	                                ProjectDesignTemplate.deleteddate is null
	                                inner join
	                                projectdesignvisit
	                                on
	                                ProjectDesignTemplate.projectdesignvisitid = projectdesignvisit.id and
	                                projectdesignvisit.deleteddate is null
	                                inner join
	                                projectschedule
	                                on
	                                projectschedule.projectdesignvariableid = ProjectDesignVariable.Id and
	                                projectschedule.projectdesigntemplateid = ProjectDesignTemplate.Id and
	                                projectschedule.deleteddate is null
	                                inner join
	                                ProjectScheduletemplate
	                                on
	                                projectscheduletemplate.projectscheduleid = ProjectSchedule.id and
	                                ProjectSchedule.deleteddate is null and
	                                projectscheduletemplate.operator = 2
	                                inner join
	                                ScreeningTemplateValue tvalue
	                                on
	                                tvalue.projectdesignvariableid = ProjectScheduletemplate.projectdesignvariableid and
	                                tvalue.DeletedDate is null and
	                                tvalue.Value is not null and
	                                ISDATE(tvalue.Value) = 1
	                                inner join
	                                ScreeningTemplate tTemplate
	                                on
	                                tvalue.ScreeningTemplateId = tTemplate.Id and
	                                --tTemplate.ScreeningEntryId = ScreeningEntry.Id and
	                                tTemplate.ProjectDesignTemplateId = ProjectScheduletemplate.ProjectDesignTemplateId
									inner join
									ScreeningVisit tScreeningvisit
									on
									tTemplate.ScreeningVisitId = tScreeningvisit.Id and
									tScreeningvisit.ScreeningEntryId = ScreeningEntry.Id
	                                inner join
	                                ProjectDesignVariable tvariable
	                                on
	                                tvalue.ProjectDesignVariableId = tvariable.Id and
	                                tvariable.DeletedDate is null 
	                                inner join
	                                ProjectDesignTemplate tDesignTemplate
	                                on
	                                tvariable.ProjectDesignTemplateId = tDesignTemplate.Id and
	                                tDesignTemplate.DeletedDate is null 
	                                inner join
	                                ProjectDesignVisit tVisit
	                                on
	                                tDesignTemplate.ProjectDesignVisitId = tVisit.Id and
	                                tVisit.DeletedDate is null
	                                inner join
	                                Project 
	                                on
	                                Randomization.ProjectId = Project.Id
                                ) a
                                )
								select * from cts where flag=0";
            var finaldata = _context.FromSql<VisitDeviationReport>(sqlqry).ToList();

            finaldata = finaldata.Where(x => parentIds.Contains(x.ProjectId)).ToList();
            if (filters.PeriodIds != null && filters.PeriodIds.Length > 0) finaldata = finaldata.Where(x => filters.PeriodIds.Contains(x.ProjectDesignPeriodId)).ToList();
            if (filters.VisitIds != null && filters.VisitIds.Length > 0) finaldata = finaldata.Where(x => filters.VisitIds.Contains(x.ProjectDesignVisitId)).ToList();
            if (filters.SubjectIds != null && filters.SubjectIds.Length > 0) finaldata = finaldata.Where(x => filters.SubjectIds.Contains(x.RandomizationId)).ToList();
            if (filters.TemplateIds != null && filters.TemplateIds.Length > 0) finaldata = finaldata.Where(x => filters.TemplateIds.Contains(x.ProjectDesignTemplateId)).ToList();
            if (filters.VariableIds != null && filters.VariableIds.Length > 0) finaldata = finaldata.Where(x => filters.VariableIds.Contains(x.ProjectDesignVariableId)).ToList();
            var dateformat = _context.AppSetting.Where(x => x.KeyName == "GeneralSettingsDto.DateFormat").ToList().FirstOrDefault().KeyValue;
            for (int i = 0; i < finaldata.Count; i++)
            {
                finaldata[i].RefValueExcel = DateTime.Parse(finaldata[i].RefValue).ToString(dateformat);
                finaldata[i].TargetValueExcel = DateTime.Parse(finaldata[i].TargetValue).ToString(dateformat);
            }
            return finaldata;
        }

        public bool CheckLockedProject(int ProjectId)
        {
            var ParentProject = _context.Project.FirstOrDefault(x => x.Id == ProjectId).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == ProjectId).ToList().Select(x => x.Id).ToList();

            var ScreeningTemplates = All.Where(y => y.DeletedDate == null && ParentProject != null ? y.ScreeningVisit.ScreeningEntry.ProjectId == ProjectId
            : sites.Contains(y.ScreeningVisit.ScreeningEntry.ProjectId)).ToList();

            var IsLocked = ScreeningTemplates.Count() <= 0 || ScreeningTemplates.Any(y => y.IsLocked == false) ? false : true;
            return IsLocked;
        }
    }
}

