﻿using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Workflow;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueQueryRepository : GenericRespository<ScreeningTemplateValueQuery>,
        IScreeningTemplateValueQueryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private WorkFlowLevelDto _workFlowLevelDto;
        private readonly IGSCContext _context;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IScreeningTemplateValueAuditRepository _screeningTemplateValueAuditRepository;
        private readonly IScreeningTemplateValueChildRepository _screeningTemplateValueChildRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        public ScreeningTemplateValueQueryRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IScreeningTemplateValueAuditRepository screeningTemplateValueAuditRepository,
            IScreeningTemplateValueChildRepository screeningTemplateValueChildRepository,
            IProjectDesignRepository projectDesignRepository,
            IAppSettingRepository appSettingRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _context = context;
            _screeningTemplateValueAuditRepository = screeningTemplateValueAuditRepository;
            _screeningTemplateValueChildRepository = screeningTemplateValueChildRepository;
            _projectDesignRepository = projectDesignRepository;
            _appSettingRepository = appSettingRepository;
        }

        public IList<ScreeningTemplateValueQueryGridDto> GetQueries(int screeningTemplateValueId)
        {
            return All.Where(x => x.ScreeningTemplateValueId == screeningTemplateValueId).Select(t => new ScreeningTemplateValueQueryGridDto
            {
                Id = t.Id,
                Value = t.Value,
                ReasonName = t.Reason.ReasonName,
                ReasonOth = t.ReasonOth,
                Note = string.IsNullOrEmpty(t.Note) ? t.ReasonOth : t.Note,
                CreatedDate = t.CreatedDate,
                CreatedByName = t.UserName + "(" + t.UserRole + ")" +
                            Convert.ToString(t.IsSystem ? " - System" : ""),
                StatusName = t.QueryStatus.GetDescription(),
                QueryStatus = t.QueryStatus,
                OldValue = t.OldValue,
                CollectionSource = t.ScreeningTemplateValue.ProjectDesignVariable.CollectionSource,
                QueryResponseTime = t.QueryParentId > 0 ? $"{(t.CreatedDate - t.QueryParent.CreatedDate).Value.Days} : {(t.CreatedDate - t.QueryParent.CreatedDate).Value.Hours} : {(t.CreatedDate - t.QueryParent.CreatedDate).Value.Minutes}" : ""
            }).OrderByDescending(a => a.Id).ToList();
        }

        public void Save(ScreeningTemplateValueQuery screeningTemplateValueQuery)
        {
            screeningTemplateValueQuery.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");

            if (screeningTemplateValueQuery.IsSystem)
            {
                screeningTemplateValueQuery.UserName = "System";
            }
            else
            {
                screeningTemplateValueQuery.UserName = _jwtTokenAccesser.UserName;
                screeningTemplateValueQuery.UserRole = _jwtTokenAccesser.RoleName;
            }


            if (screeningTemplateValueQuery.QueryStatus != QueryStatus.Open && screeningTemplateValueQuery.QueryStatus != QueryStatus.SelfCorrection)
            {
                var lastQuery = All.Where(x => x.ScreeningTemplateValueId == screeningTemplateValueQuery.ScreeningTemplateValueId).OrderByDescending(t => t.Id).FirstOrDefault();
                if (lastQuery != null)
                {
                    screeningTemplateValueQuery.QueryParentId = lastQuery.Id;
                    screeningTemplateValueQuery.PreviousQueryDate = lastQuery.CreatedDate;
                }
            }

            screeningTemplateValueQuery.CreatedDate = _jwtTokenAccesser.GetClientDate();
            Add(screeningTemplateValueQuery);
        }

        public void UpdateQuery(ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto,
            ScreeningTemplateValueQuery screeningTemplateValueQuery, ScreeningTemplateValue screeningTemplateValue)
        {
            var value = string.IsNullOrEmpty(screeningTemplateValueQueryDto.ValueName)
                ? screeningTemplateValueQueryDto.Value
                : screeningTemplateValueQueryDto.ValueName;

            var updateQueryStatus = value != screeningTemplateValueQueryDto.OldValue
                ? QueryStatus.Resolved
                : QueryStatus.Answered;


            screeningTemplateValueQuery.IsSystem = false;

            screeningTemplateValue.Value = screeningTemplateValueQueryDto.Value;
            screeningTemplateValue.IsNa = screeningTemplateValueQueryDto.IsNa;

            var screeningTemplate = _context.ScreeningTemplate.Find(screeningTemplateValue.ScreeningTemplateId);

            QueryAudit(screeningTemplateValueQueryDto, screeningTemplateValue, updateQueryStatus.ToString(), value, screeningTemplateValueQuery);

            if (screeningTemplateValueQueryDto.Children != null)
            {
                screeningTemplateValueQueryDto.OldValue = screeningTemplateValueQuery.OldValue;
                value = screeningTemplateValueQuery.Value;
                updateQueryStatus = screeningTemplateValueQuery.Value != screeningTemplateValueQuery.OldValue ? QueryStatus.Resolved : QueryStatus.Answered;
            }

            if (screeningTemplateValue.IsSystem)
            {
                screeningTemplateValue.AcknowledgeLevel = null;
                updateQueryStatus = QueryStatus.Closed;
            }

            screeningTemplateValueQuery.QueryStatus = updateQueryStatus;
            screeningTemplateValueQuery.Value = value;
            screeningTemplateValueQuery.OldValue = screeningTemplateValueQueryDto.OldValue;


            screeningTemplateValue.QueryStatus = updateQueryStatus;

            if (updateQueryStatus == QueryStatus.Answered || screeningTemplate.ReviewLevel == 1)
            {
                screeningTemplateValue.AcknowledgeLevel = screeningTemplateValue.ReviewLevel;
            }
            else if (updateQueryStatus == QueryStatus.Resolved)
            {
                var workFlowLevel = GetReviewLevel(screeningTemplateValue.ScreeningTemplateId);
                var templateLevel = _projectWorkflowRepository.GetTemplateWorkFlow(screeningTemplate.ProjectDesignTemplateId, workFlowLevel.ProjectDesignId, workFlowLevel.LevelNo);
                if (templateLevel > 0)
                    screeningTemplateValue.AcknowledgeLevel = templateLevel;
                else if (workFlowLevel.IsNoCRF)
                    screeningTemplateValue.AcknowledgeLevel = _projectWorkflowRepository.GetNoCRFLevel(workFlowLevel.ProjectDesignId, workFlowLevel.LevelNo);
                else if (workFlowLevel.IsVisitBase)
                    screeningTemplateValue.AcknowledgeLevel = _projectWorkflowRepository.GetVisitLevel(workFlowLevel.ProjectDesignVisitId, workFlowLevel.ProjectDesignId, workFlowLevel.LevelNo);
                else if (workFlowLevel.IsWorkFlowBreak)
                    screeningTemplateValue.AcknowledgeLevel = _projectWorkflowRepository.GetMaxLevelWorkBreak(workFlowLevel.ProjectDesignId);
                else
                    screeningTemplateValue.AcknowledgeLevel = 1;

                if (screeningTemplateValue.AcknowledgeLevel >= screeningTemplate.ReviewLevel)
                    screeningTemplateValue.AcknowledgeLevel = screeningTemplateValue.ReviewLevel;
            }





            Save(screeningTemplateValueQuery);

            _screeningTemplateValueChildRepository.Save(screeningTemplateValue);

            _screeningTemplateValueRepository.Update(screeningTemplateValue);
        }

        public void GenerateQuery(ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto,
            ScreeningTemplateValueQuery screeningTemplateValueQuery, ScreeningTemplateValue screeningTemplateValue)
        {
            var value = string.IsNullOrEmpty(screeningTemplateValueQueryDto.ValueName)
                ? screeningTemplateValueQueryDto.Value
                : screeningTemplateValueQueryDto.ValueName;

            var workFlowLevel = GetReviewLevel(screeningTemplateValue.ScreeningTemplateId);
            screeningTemplateValue.ReviewLevel = workFlowLevel.LevelNo;
            screeningTemplateValue.QueryStatus = QueryStatus.Open;

            screeningTemplateValueQuery.Value = value;
            screeningTemplateValueQuery.QueryLevel = screeningTemplateValue.ReviewLevel;
            screeningTemplateValueQuery.QueryStatus = QueryStatus.Open;
            screeningTemplateValueQuery.OldValue = screeningTemplateValueQueryDto.OldValue;

            if (screeningTemplateValueQueryDto.CollectionSource == CollectionSources.Table)
            {
                screeningTemplateValueQuery.Value = "";
                screeningTemplateValueQuery.OldValue = "";
            }

            screeningTemplateValue.AcknowledgeLevel = -1;
            screeningTemplateValue.UserRoleId = _jwtTokenAccesser.RoleId;
            _screeningTemplateValueRepository.Update(screeningTemplateValue);

            Save(screeningTemplateValueQuery);
        }

        public void ReviewQuery(ScreeningTemplateValue screeningTemplateValue,
            ScreeningTemplateValueQuery screeningTemplateValueQuery)
        {
            var workFlowLevel = GetReviewLevel(screeningTemplateValue.ScreeningTemplateId);
            screeningTemplateValue.QueryStatus = screeningTemplateValueQuery.QueryStatus;
            screeningTemplateValueQuery.QueryLevel = workFlowLevel.LevelNo;

            if (screeningTemplateValue.QueryStatus == QueryStatus.Reopened)
                screeningTemplateValue.AcknowledgeLevel = -1;
            else
                screeningTemplateValue.AcknowledgeLevel = null;

            Save(screeningTemplateValueQuery);
            _screeningTemplateValueRepository.Update(screeningTemplateValue);
        }


        public void ReviewAllQuery(UpdateAllQueryStatus updateAllQueryStatus)
        {
            var screeningTemplateValues = _screeningTemplateValueRepository.All.Where(x => updateAllQueryStatus.ScreeningTemplateValueIds.Contains(x.Id) && x.DeletedDate == null).ToList();
            screeningTemplateValues.ForEach(x =>
            {
                if (updateAllQueryStatus.QueryStatus == QueryStatus.Reopened)
                    x.AcknowledgeLevel = -1;
                else
                    x.AcknowledgeLevel = null;

                x.QueryStatus = updateAllQueryStatus.QueryStatus;
                _screeningTemplateValueRepository.Update(x);

                var screeningTemplateValueQuery = new ScreeningTemplateValueQuery();
                screeningTemplateValueQuery.QueryLevel = x.ReviewLevel;
                screeningTemplateValueQuery.ScreeningTemplateValueId = x.Id;
                screeningTemplateValueQuery.ReasonId = updateAllQueryStatus.ReasonId;
                screeningTemplateValueQuery.ReasonOth = updateAllQueryStatus.ReasonOth;
                screeningTemplateValueQuery.Note = updateAllQueryStatus.Note;
                Save(screeningTemplateValueQuery);
            });
        }

        public void AcknowledgeAllQuery(UpdateAllQueryStatus updateAllQueryStatus)
        {
            var ids = updateAllQueryStatus.ScreeningTemplateValueIds.Distinct().ToList();
            ids.ForEach(x =>
            {
                var screeningTemplateValueQuery = new ScreeningTemplateValueQuery();
                screeningTemplateValueQuery.ScreeningTemplateValueId = x;
                screeningTemplateValueQuery.ReasonId = updateAllQueryStatus.ReasonId;
                screeningTemplateValueQuery.ReasonOth = updateAllQueryStatus.ReasonOth;
                screeningTemplateValueQuery.Note = updateAllQueryStatus.Note;
                AcknowledgeQuery(screeningTemplateValueQuery);
            });
        }

        public void AcknowledgeQuery(ScreeningTemplateValueQuery screeningTemplateValueQuery)
        {
            var screeningTemplateValue = _screeningTemplateValueRepository.Find(screeningTemplateValueQuery.ScreeningTemplateValueId);
            var screeningTemplate = _context.ScreeningTemplate.Find(screeningTemplateValue.ScreeningTemplateId);

            var workFlowLevel = GetReviewLevel(screeningTemplateValue.ScreeningTemplateId);
            var templateLevel = _projectWorkflowRepository.GetTemplateWorkFlow(screeningTemplate.ProjectDesignTemplateId, workFlowLevel.ProjectDesignId, workFlowLevel.LevelNo);
            if (templateLevel > 0)
                screeningTemplateValue.AcknowledgeLevel = templateLevel;
            else if (workFlowLevel.IsNoCRF)
                screeningTemplateValue.AcknowledgeLevel = _projectWorkflowRepository.GetNoCRFLevel(workFlowLevel.ProjectDesignId, workFlowLevel.LevelNo);
            else if (workFlowLevel.IsVisitBase)
                screeningTemplateValue.AcknowledgeLevel = _projectWorkflowRepository.GetVisitLevel(workFlowLevel.ProjectDesignVisitId, workFlowLevel.ProjectDesignId, workFlowLevel.LevelNo);
            else
                screeningTemplateValue.AcknowledgeLevel = (short)(workFlowLevel.LevelNo + 1);

            screeningTemplateValueQuery.QueryLevel = workFlowLevel.LevelNo;
            screeningTemplateValueQuery.QueryStatus = QueryStatus.Acknowledge;
            screeningTemplateValueQuery.Note = screeningTemplateValueQuery.Note + " " +
                                               QueryStatus.Acknowledge + " " +
                                               Convert.ToString(
                                                   screeningTemplateValueQuery.QueryStatus == QueryStatus.SelfCorrection
                                                       ? "Self Correction"
                                                       : " Query");

            ClosedSelfCorrection(screeningTemplateValue, screeningTemplateValue.ReviewLevel);

            ClosedSelfCorrection(screeningTemplateValue, (short)screeningTemplate.ReviewLevel);

            if (screeningTemplateValue.QueryStatus != QueryStatus.Closed && screeningTemplateValue.AcknowledgeLevel >= screeningTemplate.ReviewLevel)
                screeningTemplateValue.AcknowledgeLevel = screeningTemplateValue.ReviewLevel;

            Save(screeningTemplateValueQuery);
            _screeningTemplateValueRepository.Update(screeningTemplateValue);
        }

        private void ClosedSelfCorrection(ScreeningTemplateValue screeningTemplateValue, short reviewLevel)
        {
            if (screeningTemplateValue.AcknowledgeLevel == reviewLevel && screeningTemplateValue.QueryStatus == QueryStatus.SelfCorrection)
            {
                screeningTemplateValue.QueryStatus = QueryStatus.Closed;
                screeningTemplateValue.AcknowledgeLevel = null;
            }

        }

        public void SelfGenerate(ScreeningTemplateValueQuery screeningTemplateValueQuery, ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto,
            ScreeningTemplateValue screeningTemplateValue, ScreeningTemplate screeningTemplate)
        {

            var workFlowLevel = GetReviewLevel(screeningTemplateValue.ScreeningTemplateId);

            screeningTemplateValueQuery.QueryStatus = QueryStatus.SelfCorrection;

            if (screeningTemplate.ReviewLevel > 1)
            {
                screeningTemplateValue.QueryStatus = QueryStatus.SelfCorrection;
                screeningTemplateValue.ReviewLevel = workFlowLevel.LevelNo;

                var templateLevel = _projectWorkflowRepository.GetTemplateWorkFlow(screeningTemplate.ProjectDesignTemplateId, workFlowLevel.ProjectDesignId, Convert.ToInt16(workFlowLevel.LevelNo == 1 ? 1 : 0));
                if (templateLevel > 0)
                    screeningTemplateValue.AcknowledgeLevel = templateLevel;
                else if (workFlowLevel.IsNoCRF)
                {
                    screeningTemplateValue.AcknowledgeLevel = _projectWorkflowRepository.GetNoCRFLevel(workFlowLevel.ProjectDesignId, Convert.ToInt16(workFlowLevel.LevelNo == 1 ? 1 : 0));
                    if (workFlowLevel.LevelNo > 0 && screeningTemplateValue.AcknowledgeLevel < screeningTemplate.ReviewLevel)
                        screeningTemplateValue.AcknowledgeLevel += 1;
                }
                else if (workFlowLevel.IsVisitBase)
                    screeningTemplateValue.AcknowledgeLevel = _projectWorkflowRepository.GetVisitLevel(workFlowLevel.ProjectDesignVisitId, workFlowLevel.ProjectDesignId, Convert.ToInt16(workFlowLevel.LevelNo == 1 ? 1 : 0));
                else if (workFlowLevel.IsWorkFlowBreak)
                {
                    screeningTemplateValue.AcknowledgeLevel = _projectWorkflowRepository.GetMaxLevelWorkBreak(workFlowLevel.ProjectDesignId);
                    if (workFlowLevel.LevelNo > 0 && screeningTemplateValue.AcknowledgeLevel < screeningTemplate.ReviewLevel)
                        screeningTemplateValue.AcknowledgeLevel += 1;
                }

                else
                    screeningTemplateValue.AcknowledgeLevel = Convert.ToInt16(workFlowLevel.LevelNo == 1 ? 2 : 1);

                if (screeningTemplateValue.AcknowledgeLevel == screeningTemplate.ReviewLevel || screeningTemplateValue.AcknowledgeLevel > screeningTemplate.ReviewLevel)
                {
                    screeningTemplateValue.QueryStatus = QueryStatus.Closed;
                    screeningTemplateValue.AcknowledgeLevel = null;
                }
            }


            screeningTemplateValue.IsSystem = false;
            screeningTemplateValue.Value = screeningTemplateValueQueryDto.Value;
            screeningTemplateValue.IsNa = screeningTemplateValueQueryDto.IsNa;
            screeningTemplateValue.UserRoleId = _jwtTokenAccesser.RoleId;

            QueryAudit(screeningTemplateValueQueryDto, screeningTemplateValue, QueryStatus.SelfCorrection.ToString(), screeningTemplateValueQuery.Value, screeningTemplateValueQuery);

            _screeningTemplateValueChildRepository.Save(screeningTemplateValue);

            _screeningTemplateValueRepository.Update(screeningTemplateValue);

            screeningTemplateValueQuery.QueryLevel = workFlowLevel.LevelNo;

            Save(screeningTemplateValueQuery);
        }


        public IList<QueryManagementDto> GetQueryEntries(QuerySearchDto filters)
        {
            var sites = new List<int>();
            if (filters.SiteId != null)
            {
                sites = _context.Project.Where(x => x.Id == filters.SiteId).ToList().Select(x => x.Id).ToList();
            }
            else
            {
                sites = _context.Project.Where(x => x.ParentProjectId == filters.ProjectId && !x.IsTestSite).ToList().Select(x => x.Id).ToList();
            }

            var query = All.Where(x => (filters.SiteId != null ? x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == filters.SiteId : sites.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId))
             && (filters.SubjectIds == null || filters.SubjectIds.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Id))
             && (filters.PeriodIds == null || filters.PeriodIds.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectDesignPeriodId))
             && (filters.VisitIds == null || filters.VisitIds.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplate.ProjectDesignVisitId))
             && (filters.TemplateIds == null || filters.TemplateIds.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplateId))
             ).Select(t => new QueryManagementDto
             {
                 Id = t.Id,
                 ScreeningTemplateValueId = t.ScreeningTemplateValueId,
                 ProjectCode = t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                 Value = t.Value,
                 ReasonName = t.Reason.ReasonName,
                 ReasonOth = t.ReasonOth,
                 Note = string.IsNullOrEmpty(t.Note) ? t.ReasonOth : t.Note,
                 CreatedDate = t.CreatedDate,
                 CreatedByName = t.UserName + "(" + t.UserRole + ")" +
                           Convert.ToString(t.IsSystem ? " - System" : ""),
                 StatusName = t.QueryStatus.GetDescription(),
                 QueryStatus = t.QueryStatus,
                 OldValue = t.OldValue,
                 QueryDescription = t.Note,
                 // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                 ScreeningTemplateValue = t.ScreeningTemplateValue.ScreeningTemplate.RepeatSeqNo == null && t.ScreeningTemplateValue.ScreeningTemplate.ParentId == null ? t.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplate.DesignOrder + ". " + t.ScreeningTemplateValue.ScreeningTemplate.ScreeningTemplateName
                                            : t.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplate.DesignOrder + "." + t.ScreeningTemplateValue.ScreeningTemplate.RepeatSeqNo + " " + t.ScreeningTemplateValue.ScreeningTemplate.ScreeningTemplateName,
                 // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                 Visit = t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningVisitName +
                                         Convert.ToString(t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber),
                 FieldName = t.ScreeningTemplateValue.ProjectDesignVariable.VariableName,
                 CollectionSource = t.ScreeningTemplateValue.ProjectDesignVariable.CollectionSource,
                 VolunteerName = t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ? t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.Initial : t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,
                 SubjectNo = t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ? t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber : t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.VolunteerNo,
                 RandomizationNumber = t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ? t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber : "",
                 QueryResponseTime = t.QueryParentId > 0 ? $"{(t.CreatedDate - t.QueryParent.CreatedDate).Value.Days} : {(t.CreatedDate - t.QueryParent.CreatedDate).Value.Hours} : {(t.CreatedDate - t.QueryParent.CreatedDate).Value.Minutes}" : "",
                 QueryParentId = t.QueryParentId,
                 DataEntryByName = t.ScreeningTemplateValue.SecurityRole == null || string.IsNullOrEmpty(t.ScreeningTemplateValue.SecurityRole.RoleName)
                                         ? t.ScreeningTemplateValue.CreatedByUser.UserName
                                         : t.ScreeningTemplateValue.CreatedByUser.UserName + "(" + t.ScreeningTemplateValue.SecurityRole.RoleShortName + ")",
             }).OrderByDescending(a => a.Id).ToList();

            var result = (from generate in query.Where(c => c.QueryStatus == QueryStatus.Open)
                          join answeredTemp in _context.ScreeningTemplateValueQuery.Where(x => x.QueryStatus == QueryStatus.Answered || x.QueryStatus == QueryStatus.Resolved) on generate.Id equals answeredTemp.QueryParentId
                          into answeredDto
                          from answered in answeredDto.DefaultIfEmpty()
                          select new QueryManagementDto
                          {
                              Id = generate.Id,
                              ScreeningTemplateValueId = generate.ScreeningTemplateValueId,
                              ProjectCode = generate.ProjectCode,
                              Value = answered == null ? generate.Value : answered.Value,
                              OldValue = answered == null ? generate.OldValue : answered.OldValue,
                              ReasonName = generate.ReasonName,
                              ReasonOth = generate.ReasonOth,
                              Note = generate.Note,
                              StatusName = generate.StatusName,
                              QueryStatus = generate.QueryStatus,
                              QueryDescription = generate.Note,
                              ScreeningTemplateValue = generate.ScreeningTemplateValue,
                              Visit = generate.Visit,
                              FieldName = generate.FieldName,
                              CollectionSource = generate.CollectionSource,
                              VolunteerName = generate.VolunteerName,
                              SubjectNo = generate.SubjectNo,
                              RandomizationNumber = generate.RandomizationNumber,
                              QueryParentId = generate.QueryParentId,
                              CreatedByName = generate.CreatedByName,
                              CreatedDate = generate.CreatedDate,
                              ModifieedByName = answered == null ? null : answered.UserName + "(" + answered.UserRole + ")",
                              ModifiedDate = answered?.CreatedDate,
                              DataEntryByName = generate.DataEntryByName,
                              GenerateToAns = answered == null ? null : $"{(answered.CreatedDate - generate.CreatedDate).Value.Days} : {(answered.CreatedDate - generate.CreatedDate).Value.Hours} : {(answered.CreatedDate - generate.CreatedDate).Value.Minutes}",
                          }).Where(t => (filters.DataEntryBy == null || filters.DataEntryBy.Contains(t.DataEntryByName))
                            && (filters.QueryGenerateBy == null || filters.QueryGenerateBy.Contains(t.CreatedByName)))
                          .OrderBy(x => x.Id).ThenBy(v => v.SubjectNo).ToList();

            result = result.Select(x => { return GetCloseData(query, x); }).ToList();

            return result.Where(b => filters.Status == null || b.QueryStatus.GetDescription() == ((QueryStatus)filters.Status).GetDescription()).ToList();
        }

        public IList<QueryManagementDto> GetScreeningQueryEntries(ScreeningQuerySearchDto filters)
        {
            var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);

            var query = All.Where(x => (x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == filters.ProjectId)
                 && (x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.StudyId == filters.StudyId)
                 && (filters.VolunteerId == null || x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId == filters.VolunteerId)
                 && (filters.ScreeningDate == null || x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ScreeningDate.Date == Convert.ToDateTime(filters.ScreeningDate).Date))
                .Select(t => new QueryManagementDto
                {
                    Id = t.Id,
                    ScreeningTemplateValueId = t.ScreeningTemplateValueId,
                    ProjectCode = t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                    Value = t.Value,
                    ReasonName = t.Reason.ReasonName,
                    ReasonOth = t.ReasonOth,
                    Note = string.IsNullOrEmpty(t.Note) ? t.ReasonOth : t.Note,
                    CreatedDate = t.CreatedDate,
                    CreatedByName = t.UserName + "(" + t.UserRole + ")" +
                             Convert.ToString(t.IsSystem ? " - System" : ""),
                    StatusName = t.QueryStatus.GetDescription(),
                    QueryStatus = t.QueryStatus,
                    OldValue = t.OldValue,
                    QueryDescription = t.Note,
                    // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                    ScreeningTemplateValue = t.ScreeningTemplateValue.ScreeningTemplate.RepeatSeqNo == null && t.ScreeningTemplateValue.ScreeningTemplate.ParentId == null ? t.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplate.DesignOrder + ". " + t.ScreeningTemplateValue.ScreeningTemplate.ScreeningTemplateName
                                              : t.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplate.DesignOrder + "." + t.ScreeningTemplateValue.ScreeningTemplate.RepeatSeqNo + " " + t.ScreeningTemplateValue.ScreeningTemplate.ScreeningTemplateName,
                    // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                    Visit = t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningVisitName +
                                           Convert.ToString(t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber),
                    FieldName = t.ScreeningTemplateValue.ProjectDesignVariable.VariableName,
                    CollectionSource = t.ScreeningTemplateValue.ProjectDesignVariable.CollectionSource,
                    VolunteerName = t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ? t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.Initial : t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,
                    AttendanceDate = DateTime.Parse(t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.AttendanceDate.ToString()).ToString(GeneralSettings.DateFormat),
                    ScreeningDate = DateTime.Parse(t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ScreeningDate.ToString()).ToString(GeneralSettings.DateFormat),
                    SubjectNo = t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ? t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber : t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.VolunteerNo,
                    RandomizationNumber = t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ? t.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber : "",
                    QueryResponseTime = t.QueryParentId > 0 ? $"{(t.CreatedDate - t.QueryParent.CreatedDate).Value.Days} : {(t.CreatedDate - t.QueryParent.CreatedDate).Value.Hours} : {(t.CreatedDate - t.QueryParent.CreatedDate).Value.Minutes}" : "",
                    QueryParentId = t.QueryParentId,
                    DataEntryByName = t.ScreeningTemplateValue.SecurityRole == null || string.IsNullOrEmpty(t.ScreeningTemplateValue.SecurityRole.RoleName)
                                           ? t.ScreeningTemplateValue.CreatedByUser.UserName
                                           : t.ScreeningTemplateValue.CreatedByUser.UserName + "(" + t.ScreeningTemplateValue.SecurityRole.RoleShortName + ")",
                }).OrderByDescending(a => a.Id).ToList();

            var result = (from generate in query.Where(c => c.QueryStatus == QueryStatus.Open)
                          join answeredTemp in _context.ScreeningTemplateValueQuery.Where(x => x.QueryStatus == QueryStatus.Answered || x.QueryStatus == QueryStatus.Resolved) on generate.Id equals answeredTemp.QueryParentId
                          into answeredDto
                          from answered in answeredDto.DefaultIfEmpty()
                          select new QueryManagementDto
                          {
                              Id = generate.Id,
                              ScreeningTemplateValueId = generate.ScreeningTemplateValueId,
                              ProjectCode = generate.ProjectCode,
                              Value = answered == null ? generate.Value : answered.Value,
                              OldValue = answered == null ? generate.OldValue : answered.OldValue,
                              ReasonName = generate.ReasonName,
                              ReasonOth = generate.ReasonOth,
                              Note = generate.Note,
                              StatusName = generate.StatusName,
                              QueryStatus = generate.QueryStatus,
                              QueryDescription = generate.Note,
                              ScreeningTemplateValue = generate.ScreeningTemplateValue,
                              Visit = generate.Visit,
                              FieldName = generate.FieldName,
                              CollectionSource = generate.CollectionSource,
                              VolunteerName = generate.VolunteerName,
                              AttendanceDate = generate.AttendanceDate,
                              ScreeningDate = generate.ScreeningDate,
                              SubjectNo = generate.SubjectNo,
                              RandomizationNumber = generate.RandomizationNumber,
                              QueryParentId = generate.QueryParentId,
                              CreatedByName = generate.CreatedByName,
                              CreatedDate = generate.CreatedDate,
                              ModifieedByName = answered == null ? null : answered.UserName + "(" + answered.UserRole + ")",
                              ModifiedDate = answered?.CreatedDate,
                              DataEntryByName = generate.DataEntryByName,
                              GenerateToAns = answered == null ? null : $"{(answered.CreatedDate - generate.CreatedDate).Value.Days} : {(answered.CreatedDate - generate.CreatedDate).Value.Hours} : {(answered.CreatedDate - generate.CreatedDate).Value.Minutes}",
                          }).OrderBy(x => x.Id).ThenBy(v => v.SubjectNo).ToList();

            result = result.Select(x => { return GetCloseData(query, x); }).ToList();

            return result;
        }

        public QueryManagementDto GetCloseData(List<QueryManagementDto> queryData, QueryManagementDto ParentData)
        {
            var QueryParentId = ParentData.Id;
            var AnsweredData = new QueryManagementDto();

            queryData = queryData.Where(x => x.ScreeningTemplateValueId == ParentData.ScreeningTemplateValueId).ToList();
            for (int i = 0; i < queryData.Count; i++)
            {
                var child = queryData.Where(x => x.QueryParentId == QueryParentId).FirstOrDefault();
                if (child != null)
                {
                    QueryParentId = child.Id;
                    ParentData.QueryStatus = child.QueryStatus;
                    ParentData.StatusName = child.QueryStatus.GetDescription();
                    if (child.QueryStatus == QueryStatus.Closed)
                    {
                        ParentData.ClosedDate = child.CreatedDate;
                        ParentData.ClosedByName = child.CreatedByName;
                        ParentData.GenerateToClose = $"{(child.CreatedDate - ParentData.CreatedDate).Value.Days} : {(child.CreatedDate - ParentData.CreatedDate).Value.Hours} : {(child.CreatedDate - ParentData.CreatedDate).Value.Minutes}";
                        ParentData.AnsToClose = AnsweredData.CreatedDate == null ? null : $"{(child.CreatedDate - AnsweredData.CreatedDate).Value.Days} : {(child.CreatedDate - AnsweredData.CreatedDate).Value.Hours} : {(child.CreatedDate - AnsweredData.CreatedDate).Value.Minutes}";
                        return ParentData;
                    }
                    else if (child.QueryStatus == QueryStatus.Answered || child.QueryStatus == QueryStatus.Resolved)
                    {
                        AnsweredData = child;
                    }
                }
            }
            return ParentData;
        }

        public IList<QueryManagementDto> GetGenerateQueryBy(int projectId)
        {
            var ParentProject = _context.Project.Where(x => x.Id == projectId).Select(s => s.ParentProjectId).FirstOrDefault();
            var sites = _context.Project.Where(x => x.ParentProjectId == projectId).Select(x => x.Id).ToList();

            var queryData = (from query in _context.ScreeningTemplateValueQuery.Include(x => x.ScreeningTemplateValue).ThenInclude(x => x.ScreeningTemplate)
                             .ThenInclude(x => x.ScreeningVisit).ThenInclude(x => x.ScreeningEntry)
                             .Where(q => q.QueryStatus == QueryStatus.Open && ParentProject != null ?
                             q.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId : sites.Contains(q.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId))
                             select new QueryManagementDto
                             {
                                 Id = query.Id,
                                 Value = query.UserRole == null || string.IsNullOrEmpty(query.UserRole)
                                     ? query.UserName
                                     : query.UserName + "(" + query.UserRole + ")" +
                                       Convert.ToString(query.IsSystem ? " - System" : "")
                             }).ToList();

            var queryGeneratedBy = queryData.GroupBy(item => new { item.Value })
                .Select(z => new QueryManagementDto { Id = z.FirstOrDefault().Id, Value = z.Key.Value }).ToList();
            return queryGeneratedBy;
        }

        public IList<DropDownDto> GetDataEntryBy(int projectId)
        {
            var ParentProject = _context.Project.Where(x => x.Id == projectId).Select(s => s.ParentProjectId).FirstOrDefault();
            var sites = _context.Project.Where(x => x.ParentProjectId == projectId).Select(x => x.Id).ToList();

            var dataEntryBy = _context.ScreeningTemplateValue.Include(x => x.ScreeningTemplate).ThenInclude(x => x.ScreeningVisit)
                              .ThenInclude(x => x.ScreeningEntry).Where(t => ParentProject != null ? t.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId
                                 : sites.Contains(t.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId))
                              .Select(c => new DropDownDto
                              {
                                  Id = (int)c.CreatedBy,
                                  Value = c.UserRoleId == null ? c.CreatedByUser.UserName
                                        : c.CreatedByUser.UserName + "(" + c.SecurityRole.RoleShortName + ")"
                              }).ToList();

            return dataEntryBy.GroupBy(c => c.Value).Select(z => new DropDownDto { Id = z.FirstOrDefault().Id, Value = z.Key }).ToList();
        }

        public WorkFlowLevelDto GetReviewLevel(int screeningTemplateId)
        {
            if (_workFlowLevelDto != null) return _workFlowLevelDto;

            var templateData = _context.ScreeningTemplate.Where(x => x.Id == screeningTemplateId).Select(r => new
            {
                r.ScreeningVisit.ScreeningEntryId,
                r.ScreeningVisit.ScreeningEntry.ProjectDesignId,
                r.ScreeningVisit.ProjectDesignVisit.IsNonCRF,
                r.StartLevel,
                r.ScreeningVisit.ProjectDesignVisitId
            }).FirstOrDefault();

            if (templateData == null) return new WorkFlowLevelDto { IsWorkFlowBreak = false, LevelNo = -1 };

            var workFlowLevel = _projectWorkflowRepository.GetProjectWorkLevel(templateData.ProjectDesignId);

            if (templateData.StartLevel != null)
                workFlowLevel.StartLevel = (short)templateData.StartLevel;

            _workFlowLevelDto = workFlowLevel;

            _workFlowLevelDto.IsNoCRF = templateData.IsNonCRF;
            _workFlowLevelDto.ProjectDesignId = templateData.ProjectDesignId;
            _workFlowLevelDto.ProjectDesignVisitId = templateData.ProjectDesignVisitId;
            return _workFlowLevelDto;
        }

        private void QueryAudit(ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto,
            ScreeningTemplateValue screeningTemplateValue, string status, string value,
            ScreeningTemplateValueQuery screeningTemplateValueQuery)
        {
            var queryOldValue = "";
            var queryValue = "";
            if (screeningTemplateValueQueryDto.Children?.Count > 0)
            {
                var oldProjectDesignVariableValueIds = _context.ScreeningTemplateValueChild.AsNoTracking().Where(t =>
                        screeningTemplateValueQueryDto.Children.Select(s => s.Id).Contains(t.Id) && t.Value == "true")
                    .Select(t => t.ProjectDesignVariableValueId).ToList();

                queryOldValue = string.Join(", ",
                    _context.ProjectDesignVariableValue.Where(t => oldProjectDesignVariableValueIds.Contains(t.Id))
                        .Select(t => t.ValueName).ToList());

                var newProjectDesignVariableValueIds = screeningTemplateValueQueryDto.Children
                    .Where(t => t.Value == "true").Select(t => t.ProjectDesignVariableValueId).ToList();

                queryValue = string.Join(", ",
                    _context.ProjectDesignVariableValue.Where(t => newProjectDesignVariableValueIds.Contains(t.Id))
                        .Select(t => t.ValueName).ToList());

                _screeningTemplateValueRepository.UpdateChild(screeningTemplateValueQueryDto.Children.ToList());

                if (QueryStatus.Answered.GetDescription() == status && queryValue != queryOldValue)
                    status = QueryStatus.Resolved.GetDescription();

            }
            else
            {
                queryOldValue = screeningTemplateValueQueryDto.OldValue;
                queryValue = screeningTemplateValueQueryDto.IsNa ? "N/A" : value;
            }

            screeningTemplateValueQuery.Value = queryValue;
            screeningTemplateValueQuery.OldValue = queryOldValue;

            var audit = new ScreeningTemplateValueAudit
            {
                ScreeningTemplateValueId = screeningTemplateValue.Id,
                OldValue = queryOldValue,
                Value = queryValue,
                Note = screeningTemplateValueQueryDto.Note + " " + status,
                ReasonId = screeningTemplateValueQueryDto.ReasonId,
                ReasonOth = screeningTemplateValueQueryDto.ReasonOth
            };
            _screeningTemplateValueAuditRepository.Save(audit);
        }


        // Total query chart
        public List<DashboardQueryStatusDto> GetDashboardTotalQueryStatus(int projectId)
        {
            var queries = _screeningTemplateValueRepository.All.Where(r =>
             (r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId || r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ParentProjectId == projectId) &&
             (!r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) &&
             r.ProjectDesignVariable.DeletedDate == null && r.DeletedDate == null).
                  GroupBy(c => new
                  {
                      c.QueryStatus
                  }).Select(t => new DashboardQueryStatusDto
                  {
                      Status = t.Key.QueryStatus,
                      DisplayName = t.Key.QueryStatus.GetDescription(),
                      Total = t.Count()
                  }).ToList().OrderBy(x => x.Status).ToList();

            var closeQueries = All.Count(r =>
            (r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId || r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ParentProjectId == projectId) &&
            r.QueryStatus == QueryStatus.Closed &&
            r.ScreeningTemplateValue.ProjectDesignVariable.DeletedDate == null && r.ScreeningTemplateValue.DeletedDate == null);

            queries.Where(x => x.DisplayName == QueryStatus.Closed.GetDescription()).OrderBy(x => x.Status).ToList().ForEach(x => x.Total = closeQueries);


            if (!queries.Any(x => x.DisplayName == QueryStatus.Closed.GetDescription()))
                queries.Add(new DashboardQueryStatusDto
                {
                    DisplayName = QueryStatus.Closed.GetDescription(),
                    Total = closeQueries
                });

            return queries;
        }

        public List<DashboardQueryStatusDto> GetDashboardQueryStatusBySite(int projectId)
        {
            var queryStatus = (from p in _context.Project
                               join se in _context.ScreeningEntry on p.Id equals se.ProjectId into entry
                               from sEntry in entry.DefaultIfEmpty()
                               join st in _context.ScreeningTemplate on sEntry.Id equals st.ScreeningVisit.ScreeningEntryId into stemplate
                               from st in stemplate.DefaultIfEmpty()
                               join pdv in _context.ProjectDesignVisit on st.ScreeningVisit.ProjectDesignVisitId equals pdv.Id into design
                               from pdesign in design.DefaultIfEmpty()
                               join stv in _context.ScreeningTemplateValue on st.Id equals stv.ScreeningTemplateId into templatevalue
                               from stv in templatevalue.DefaultIfEmpty()
                               join stvq in _context.ScreeningTemplateValueQuery on stv.Id equals stvq.ScreeningTemplateValueId into
                                   templatevaluequery
                               from stemplatevaluequery in templatevaluequery.DefaultIfEmpty()
                               where p.Id == projectId || p.ParentProjectId == projectId
                               group new { stemplatevaluequery, p } by new { stemplatevaluequery.QueryStatus, p.ProjectCode }
                into g
                               select new DashboardQueryStatusDto
                               {
                                   DisplayName = g.Key.ProjectCode,
                                   Open = g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 1),
                                   Answered = g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 2),
                                   Resolved = g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 3),
                                   ReOpened = g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 4),
                                   Closed = g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 5),
                                   SelfCorrection = g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 6),
                                   Acknowledge = g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 7),
                                   Total = g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 1) +
                                           g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 2) +
                                           g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 3)
                                           + g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 4) +
                                           g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 5) +
                                           g.Count(x => (int)x.stemplatevaluequery.QueryStatus == 6)
                               }).ToList();
            return queryStatus;
        }

        // Role wise chart
        public List<DashboardQueryStatusDto> GetDashboardQueryStatusByRolewise(int projectId)
        {
            var result = All.Where(x => (x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId ||
           x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ParentProjectId == projectId) && (!x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite)
           && (x.QueryStatus == QueryStatus.Open || x.QueryStatus == QueryStatus.SelfCorrection
           || x.QueryStatus == QueryStatus.Acknowledge) && x.UserRole != null).GroupBy(
               t => new { t.UserRole, t.QueryStatus }).Select(g => new DashboardQueryStatusDto
               {
                   DisplayName = g.Key.UserRole,
                   QueryStatus = g.Key.QueryStatus.GetDescription(),
                   Total = g.Count()
               }).ToList();

            return result;
        }

        // Visit wise chart
        public List<DashboardQueryStatusDto> GetDashboardQueryStatusByVisitwise(int projectId)
        {
            var queries = _screeningTemplateValueRepository.All.Where(r =>
          r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId && !r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite &&
          r.ProjectDesignVariable.DeletedDate == null && r.DeletedDate == null && r.QueryStatus != QueryStatus.Closed).
               GroupBy(c => new
               {
                   c.ScreeningTemplate.ScreeningVisit.ProjectDesignVisitId,
                   c.AcknowledgeLevel,
                   c.UserRoleId,
                   c.ReviewLevel,
                   c.QueryStatus
               }).Select(t => new
               {
                   t.Key.AcknowledgeLevel,
                   t.Key.ProjectDesignVisitId,
                   t.Key.ReviewLevel,
                   t.Key.UserRoleId,
                   t.Key.QueryStatus,
                   TotalQuery = t.Count()
               }).ToList();

            var closeQueries = All.Where(r =>
            r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId &&
             !r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite &&
            r.ScreeningTemplateValue.ProjectDesignVariable.DeletedDate == null
            && r.ScreeningTemplateValue.DeletedDate == null && r.QueryStatus == QueryStatus.Closed).
                 GroupBy(c => new
                 {
                     c.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ProjectDesignVisitId,

                 }).Select(t => new
                 {
                     t.Key.ProjectDesignVisitId,
                     TotalQuery = t.Count()
                 }).ToList();

            var visitData = _context.ScreeningVisit.Where(r => r.ScreeningEntry.ProjectId == projectId && !r.ScreeningEntry.Project.IsTestSite
            && r.DeletedDate == null && r.Status > ScreeningVisitStatus.ReSchedule).Select(t => new
            {
                ProjectDesignVisitId = t.ProjectDesignVisitId,
                // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                VisitName = t.ScreeningVisitName
            }).Distinct().ToList();

            var result = visitData.Select(r => new DashboardQueryStatusDto
            {
                DisplayName = r.VisitName,

                Answered = queries.Where(x => x.ProjectDesignVisitId == r.ProjectDesignVisitId && x.QueryStatus == QueryStatus.Answered).Sum(t => t.TotalQuery),
                Resolved = queries.Where(x => x.ProjectDesignVisitId == r.ProjectDesignVisitId && x.QueryStatus == QueryStatus.Resolved).Sum(t => t.TotalQuery),
                ReOpened = queries.Where(x => x.ProjectDesignVisitId == r.ProjectDesignVisitId && x.QueryStatus == QueryStatus.Reopened).Sum(t => t.TotalQuery),
                Closed = closeQueries.Where(x => x.ProjectDesignVisitId == r.ProjectDesignVisitId).Sum(t => t.TotalQuery),
                SelfCorrection = queries.Where(x => x.ProjectDesignVisitId == r.ProjectDesignVisitId && x.QueryStatus == QueryStatus.SelfCorrection).Sum(t => t.TotalQuery),
                Acknowledge = queries.Where(x => x.ProjectDesignVisitId == r.ProjectDesignVisitId && x.AcknowledgeLevel != x.ReviewLevel && (x.QueryStatus == QueryStatus.Resolved || x.QueryStatus == QueryStatus.SelfCorrection)).Sum(t => t.TotalQuery),
            }).ToList();

            result.ForEach(x => x.Total = (x.Answered + x.Resolved + x.ReOpened + x.Closed + x.SelfCorrection + x.Acknowledge));

            return result;
        }

        // Site wise open query chart
        public List<DashboardQueryStatusDto> GetDashboardOpenQuerySitewise(int projectId)
        {
            var queries = _screeningTemplateValueRepository.All.Where(r =>
            (r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId || r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ParentProjectId == projectId) &&
            (!r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) &&
            r.ProjectDesignVariable.DeletedDate == null && r.DeletedDate == null && r.QueryStatus == QueryStatus.Open).
                 GroupBy(c => new
                 {
                     c.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ProjectCode
                 }).Select(t => new DashboardQueryStatusDto
                 {
                     DisplayName = t.Key.ProjectCode,
                     Total = t.Count()
                 }).ToList();

            return queries;
        }


        public IList<ScreeningQueryDto> GetScreeningQuery(int parentProjectId, int projectId)
        {
            var projectDesignId = _projectDesignRepository.All.Where(r => r.ProjectId == parentProjectId).Select(t => t.Id).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesignId);

            var result = (from screeningValue in _context.ScreeningTemplateValue.Where(t =>
             t.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == projectId && t.QueryStatus != null && t.QueryStatus != QueryStatus.Closed
                                 && t.ProjectDesignVariable.DeletedDate == null)
                          let lastQuery = screeningValue.ScreeningTemplateValueQuerys.Where(b => b.QueryStatus == QueryStatus.Open).OrderByDescending(c => c.Id).Select(a => new
                          {
                              a.Note,
                              a.UserName,
                              a.CreatedDate,
                              a.UserRole
                          }).FirstOrDefault()
                          let lastAduit = screeningValue.ScreeningTemplateValueAudits.OrderByDescending(c => c.Id).Select(a => new
                          {
                              a.ReasonOth,
                              a.AuditReason.ReasonName,
                              a.OldValue,
                              a.Value,
                              a.UserName,
                              a.CreatedDate,
                              a.UserRole
                          }).FirstOrDefault()
                          select new ScreeningQueryDto
                          {
                              ProjectCode = screeningValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                              ReasonName = lastAduit.ReasonName,
                              ReasonOth = lastAduit.ReasonOth,
                              StatusName = screeningValue.QueryStatus.GetDescription(),
                              Value = screeningValue.Value,
                              QueryDescription = lastQuery.Note,
                              OldValue = lastAduit.OldValue,
                              QueryStatus = screeningValue.QueryStatus,
                              LastUpdateBy = string.IsNullOrEmpty(lastAduit.UserRole) ? lastAduit.UserName : $"{lastAduit.UserName} ({lastAduit.UserRole})",
                              LastUpdateDate = lastAduit.CreatedDate,
                              LastQueryBy = string.IsNullOrEmpty(lastQuery.UserRole) ? lastQuery.UserName : $"{lastQuery.UserName} ({lastQuery.UserRole})",
                              AcknowledgeLevel = screeningValue.AcknowledgeLevel,
                              ReviewLevel = screeningValue.ReviewLevel,
                              UserRoleId = screeningValue.UserRoleId,
                              LastQueryDate = lastQuery.CreatedDate,
                              ScreeningEntryId = screeningValue.ScreeningTemplate.ScreeningVisit.ScreeningEntryId,
                              IsSystem = screeningValue.IsSystem,
                              DesignOrder = screeningValue.ScreeningTemplate.ProjectDesignTemplate.DesignOrder,
                              // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                              ProjectDesignTemplateName = screeningValue.ScreeningTemplate.RepeatSeqNo == null && screeningValue.ScreeningTemplate.ParentId == null ?
                                 screeningValue.ScreeningTemplate.ProjectDesignTemplate.DesignOrder + ". " + screeningValue.ScreeningTemplate.ScreeningTemplateName
                                            : screeningValue.ScreeningTemplate.ProjectDesignTemplate.DesignOrder + "." + screeningValue.ScreeningTemplate.RepeatSeqNo + " " + screeningValue.ScreeningTemplate.ScreeningTemplateName,
                              // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                              Visit = screeningValue.ScreeningTemplate.ScreeningVisit.ScreeningVisitName +
                                         Convert.ToString(screeningValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber == null ? "" : "-" + screeningValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber),
                              FieldName = screeningValue.ProjectDesignVariable.VariableName,
                              ProjectDesignVariableId = screeningValue.ProjectDesignVariableId,
                              VolunteerName = screeningValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ?
                                 screeningValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.Initial : screeningValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,

                              SubjectNo = screeningValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ?
                                 screeningValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber : screeningValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.VolunteerNo,
                              RandomizationNumber = screeningValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ?
                                 screeningValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber : "",
                              ScreeningTemplateValueId = screeningValue.Id,
                              CollectionSource = (int)screeningValue.ProjectDesignVariable.CollectionSource,

                          }).OrderBy(x => x.ScreeningTemplateValueId).ToList();

            result = result.Where(x => (x.AcknowledgeLevel == workflowlevel.LevelNo ||
                                    ((x.QueryStatus == QueryStatus.Open || x.QueryStatus == QueryStatus.Reopened) && workflowlevel.LevelNo == 0 && workflowlevel.IsStartTemplate) ||
                                    ((x.QueryStatus == QueryStatus.Resolved || x.QueryStatus == QueryStatus.Answered) && workflowlevel.LevelNo == 0 && x.UserRoleId == _jwtTokenAccesser.RoleId)
                                    )).ToList();
            result.ForEach(x =>
            {
                if (x.AcknowledgeLevel != x.ReviewLevel && (x.QueryStatus == QueryStatus.Resolved || x.QueryStatus == QueryStatus.SelfCorrection))
                    x.QueryStatus = QueryStatus.Acknowledge;
                else if (x.QueryStatus == QueryStatus.Open || x.QueryStatus == QueryStatus.Reopened)
                    x.QueryStatus = QueryStatus.Open;
                else if (x.QueryStatus == QueryStatus.Answered || x.QueryStatus == QueryStatus.Resolved)
                    x.QueryStatus = QueryStatus.Answered;
            });

            return result;



        }

        public string GetLatestValue(int screeningTemplateValueId)
        {
            return All.Where(x => x.ScreeningTemplateValueId == screeningTemplateValueId && x.QueryStatus != QueryStatus.Open).Select(c => new
            {
                c.Id,
                c.Value
            }).OrderByDescending(a => a.Id).FirstOrDefault()?.Value;
        }


    }
}