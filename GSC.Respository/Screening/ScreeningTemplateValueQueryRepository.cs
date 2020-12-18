using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EditCheckImpact;
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
        private readonly IScreeningTemplateValueScheduleRepository _screeningTemplateValueScheduleRepository;
        private WorkFlowLevelDto _workFlowLevelDto;
        private readonly IGSCContext _context;
        private readonly IScreeningTemplateValueAuditRepository _screeningTemplateValueAuditRepository;
        private readonly IScreeningTemplateValueChildRepository _screeningTemplateValueChildRepository;
        public ScreeningTemplateValueQueryRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IScreeningTemplateValueScheduleRepository screeningTemplateValueScheduleRepository,
            IScreeningTemplateValueAuditRepository screeningTemplateValueAuditRepository,
            IScreeningTemplateValueChildRepository screeningTemplateValueChildRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _screeningTemplateValueScheduleRepository = screeningTemplateValueScheduleRepository;
            _context = context;
            _screeningTemplateValueAuditRepository = screeningTemplateValueAuditRepository;
            _screeningTemplateValueChildRepository = screeningTemplateValueChildRepository;
        }

        public IList<ScreeningTemplateValueQueryDto> GetQueries(int screeningTemplateValueId)
        {
            var queryDtos =
                (from query in _context.ScreeningTemplateValueQuery.Where(t =>
                        t.ScreeningTemplateValueId == screeningTemplateValueId)
                 join reasonTemp in _context.AuditReason on query.ReasonId equals reasonTemp.Id into reasonDt
                 from reason in reasonDt.DefaultIfEmpty()
                 join userTemp in _context.Users on query.CreatedBy equals userTemp.Id into userDto
                 from user in userDto.DefaultIfEmpty()
                 join roleTemp in _context.SecurityRole on query.UserRoleId equals roleTemp.Id into roleDto
                 from role in roleDto.DefaultIfEmpty()
                 select new ScreeningTemplateValueQueryDto
                 {
                     Id = query.Id,
                     Value = query.Value,
                     ReasonName = reason.ReasonName,
                     ReasonOth = query.ReasonOth,
                     Note = string.IsNullOrEmpty(query.Note) ? query.ReasonOth : query.Note,
                     CreatedDate = query.CreatedDate,
                     CreatedByName = role == null || string.IsNullOrEmpty(role.RoleName)
                         ? user.UserName
                         : user.UserName + "(" + role.RoleName + ")" +
                           Convert.ToString(query.IsSystem ? " - System" : ""),
                     StatusName = query.QueryStatus.GetDescription(),
                     QueryStatus = query.QueryStatus,
                     OldValue = query.OldValue
                 }).ToList();

            return queryDtos;
        }

        public void UpdateQuery(ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto,
            ScreeningTemplateValueQuery screeningTemplateValueQuery, ScreeningTemplateValue screeningTemplateValue)
        {
            var value = string.IsNullOrEmpty(screeningTemplateValueQueryDto.ValueName)
                ? screeningTemplateValueQueryDto.Value
                : screeningTemplateValueQueryDto.ValueName;
            var updateQueryStatus = new QueryStatus();
            updateQueryStatus = screeningTemplateValueQueryDto.Value == screeningTemplateValue.Value
                ? QueryStatus.Answered
                : QueryStatus.Resolved;

            if (screeningTemplateValue.IsSystem)
            {
                updateQueryStatus = QueryStatus.Closed;
                _screeningTemplateValueScheduleRepository.CloseSystemQuery(screeningTemplateValue.ScreeningTemplateId,
                    screeningTemplateValue.ProjectDesignVariableId);
            }

            screeningTemplateValueQuery.IsSystem = false;
            screeningTemplateValueQuery.QueryStatus = updateQueryStatus;
            screeningTemplateValueQuery.Value = value;
            screeningTemplateValueQuery.OldValue = screeningTemplateValueQueryDto.OldValue;
            screeningTemplateValueQuery.UserRoleId = _jwtTokenAccesser.RoleId;

            screeningTemplateValue.QueryStatus = updateQueryStatus;
            screeningTemplateValue.Value = screeningTemplateValueQueryDto.Value;
            if (updateQueryStatus == QueryStatus.Resolved)
            {
                var screeningTemplate = _context.ScreeningTemplate.Find(screeningTemplateValue.ScreeningTemplateId);
                if (screeningTemplate.ReviewLevel == 1)
                    screeningTemplateValue.AcknowledgeLevel = screeningTemplateValue.ReviewLevel;
                else if (screeningTemplateValue.AcknowledgeLevel != null &&
                         screeningTemplateValue.AcknowledgeLevel == -1)
                    screeningTemplateValue.AcknowledgeLevel = 1;
                else
                    screeningTemplateValue.AcknowledgeLevel =
                        Convert.ToInt16(screeningTemplateValue.AcknowledgeLevel + 1);
            }
            else
            {
                screeningTemplateValue.AcknowledgeLevel = screeningTemplateValue.ReviewLevel;
            }


            QueryAudit(screeningTemplateValueQueryDto, screeningTemplateValue, updateQueryStatus.ToString(), value,
                screeningTemplateValueQuery);

            Add(screeningTemplateValueQuery);

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
            screeningTemplateValueQuery.UserRoleId = _jwtTokenAccesser.RoleId;

            screeningTemplateValue.AcknowledgeLevel = -1;
            if (workFlowLevel.IsWorkFlowBreak)
            {
                var screeningTemplate = _context.ScreeningTemplate.Find(screeningTemplateValue.ScreeningTemplateId);
                screeningTemplateValue.AcknowledgeLevel = screeningTemplate.StartLevel;
            }

            screeningTemplateValue.UserRoleId = _jwtTokenAccesser.RoleId;
            _screeningTemplateValueRepository.Update(screeningTemplateValue);

            Add(screeningTemplateValueQuery);
        }

        public void ReviewQuery(ScreeningTemplateValue screeningTemplateValue,
            ScreeningTemplateValueQuery screeningTemplateValueQuery)
        {
            var workFlowLevel = GetReviewLevel(screeningTemplateValue.ScreeningTemplateId);
            screeningTemplateValue.QueryStatus = screeningTemplateValueQuery.QueryStatus;
            screeningTemplateValueQuery.QueryLevel = workFlowLevel.LevelNo;
            screeningTemplateValueQuery.UserRoleId = _jwtTokenAccesser.RoleId;

            if (screeningTemplateValue.QueryStatus == QueryStatus.Reopened)
            {
                screeningTemplateValue.AcknowledgeLevel = -1;
                if (workFlowLevel.IsWorkFlowBreak) screeningTemplateValue.AcknowledgeLevel = workFlowLevel.StartLevel;
            }
            else
            {
                screeningTemplateValue.AcknowledgeLevel = null;
            }

            Add(screeningTemplateValueQuery);
            _screeningTemplateValueRepository.Update(screeningTemplateValue);
        }

        public void AcknowledgeQuery(ScreeningTemplateValueQuery screeningTemplateValueQuery)
        {
            var screeningTemplateValue =
                _screeningTemplateValueRepository.Find(screeningTemplateValueQuery.ScreeningTemplateValueId);
            var screeningTemplate = _context.ScreeningTemplate.Find(screeningTemplateValue.ScreeningTemplateId);


            var workFlowLevel = GetReviewLevel(screeningTemplateValue.ScreeningTemplateId);

            screeningTemplateValue.AcknowledgeLevel = Convert.ToInt16(workFlowLevel.LevelNo + 1);
            screeningTemplateValueQuery.QueryLevel = workFlowLevel.LevelNo;
            screeningTemplateValueQuery.QueryStatus = QueryStatus.Acknowledge;
            screeningTemplateValueQuery.Note = screeningTemplateValueQuery.Note + " " +
                                               QueryStatus.Acknowledge + " " +
                                               Convert.ToString(
                                                   screeningTemplateValueQuery.QueryStatus == QueryStatus.SelfCorrection
                                                       ? "Self Correction"
                                                       : " Query");

            ClosedSelfCorrection(screeningTemplateValue, (short)screeningTemplate.ReviewLevel);

            if (screeningTemplateValue.QueryStatus == QueryStatus.SelfCorrection
                && screeningTemplateValue.AcknowledgeLevel == screeningTemplateValue.ReviewLevel)
                screeningTemplateValue.AcknowledgeLevel = Convert.ToInt16(screeningTemplateValue.AcknowledgeLevel + 1);

            ClosedSelfCorrection(screeningTemplateValue, screeningTemplateValue.ReviewLevel);

            screeningTemplateValueQuery.UserRoleId = _jwtTokenAccesser.RoleId;
            Add(screeningTemplateValueQuery);
            _screeningTemplateValueRepository.Update(screeningTemplateValue);
        }

        private void ClosedSelfCorrection(ScreeningTemplateValue screeningTemplateValue, short reviewLevel)
        {
            if (screeningTemplateValue.AcknowledgeLevel == reviewLevel)
            {
                screeningTemplateValue.AcknowledgeLevel = screeningTemplateValue.ReviewLevel;
                if (screeningTemplateValue.QueryStatus == QueryStatus.SelfCorrection)
                    screeningTemplateValue.QueryStatus = QueryStatus.Closed;
            }
        }

        public void SelfGenerate(ScreeningTemplateValueQuery screeningTemplateValueQuery,
            ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto,
            ScreeningTemplateValue screeningTemplateValue,
            ScreeningTemplate screeningTemplate)
        {

            var workFlowLevel = GetReviewLevel(screeningTemplateValue.ScreeningTemplateId);
            screeningTemplateValueQuery.QueryStatus = QueryStatus.SelfCorrection;

            if (screeningTemplate.ReviewLevel > 1)
            {
                if (workFlowLevel.IsWorkFlowBreak)
                {
                    if (workFlowLevel.LevelNo != screeningTemplate.ReviewLevel)
                    {
                        screeningTemplateValue.AcknowledgeLevel = Convert.ToInt16(workFlowLevel.LevelNo + 1);
                        screeningTemplateValue.QueryStatus = QueryStatus.SelfCorrection;
                    }
                }
                else
                {
                    screeningTemplateValue.QueryStatus = QueryStatus.SelfCorrection;
                    screeningTemplateValue.ReviewLevel = workFlowLevel.LevelNo;
                    screeningTemplateValue.AcknowledgeLevel = Convert.ToInt16(workFlowLevel.LevelNo == 1 ? 2 : 1);
                }

                if (screeningTemplateValue.AcknowledgeLevel == screeningTemplate.ReviewLevel)
                    screeningTemplateValue.QueryStatus = QueryStatus.Closed;
            }
            screeningTemplateValue.IsSystem = false;
            screeningTemplateValue.Value = screeningTemplateValueQueryDto.Value;
            screeningTemplateValue.IsNa = screeningTemplateValueQueryDto.IsNa;
            screeningTemplateValue.UserRoleId = _jwtTokenAccesser.RoleId;
            QueryAudit(screeningTemplateValueQueryDto, screeningTemplateValue, QueryStatus.SelfCorrection.ToString(),
                screeningTemplateValueQuery.Value, screeningTemplateValueQuery);

            _screeningTemplateValueChildRepository.Save(screeningTemplateValue);

            _screeningTemplateValueRepository.Update(screeningTemplateValue);

            screeningTemplateValueQuery.QueryLevel = workFlowLevel.LevelNo;
            screeningTemplateValueQuery.UserRoleId = _jwtTokenAccesser.RoleId;
            Add(screeningTemplateValueQuery);
        }


        public IList<QueryManagementDto> GetQueryEntries(QuerySearchDto filters)
        {
            var ParentProject = _context.Project.FirstOrDefault(x => x.Id == filters.ProjectId).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == filters.ProjectId).ToList().Select(x => x.Id).ToList();

            var queryDtos = (from screening in _context.ScreeningEntry.Where(t =>
                                (ParentProject != null ? t.ProjectId == filters.ProjectId : sites.Contains(t.ProjectId))
                                && t.ProjectDesignPeriod.DeletedDate == null
                                && (filters.PeriodIds == null || filters.PeriodIds.Contains(t.ProjectDesignPeriodId))
                                && (filters.SubjectIds == null || filters.SubjectIds.Contains(t.Id)))
                             join template in _context.ScreeningTemplate.Where(u =>
                                                (filters.TemplateIds == null || filters.TemplateIds.Contains(u.ProjectDesignTemplateId))
                                                && (filters.VisitIds == null || filters.VisitIds.Contains(u.ProjectDesignTemplate.ProjectDesignVisitId))
                                                && u.ProjectDesignTemplate.DeletedDate == null)
                             on screening.Id equals template.ScreeningVisit.ScreeningEntryId
                             join value in _context.ScreeningTemplateValue.Where(val =>
                                          (filters.DataEntryBy == null || val.CreatedBy == filters.DataEntryBy)
                                          && val.ProjectDesignVariable.DeletedDate == null)
                             on template.Id equals value.ScreeningTemplateId
                             join query in _context.ScreeningTemplateValueQuery on value.Id equals query.ScreeningTemplateValueId
                             join reasonTemp in _context.AuditReason on query.ReasonId equals reasonTemp.Id into reasonDt
                             from reason in reasonDt.DefaultIfEmpty()
                             join userTemp in _context.Users on query.CreatedBy equals userTemp.Id into userDto
                             from user in userDto.DefaultIfEmpty()
                             join roleTemp in _context.SecurityRole on query.UserRoleId equals roleTemp.Id into roleDto
                             from role in roleDto.DefaultIfEmpty()
                             join randomizationTemp in _context.Randomization on screening.RandomizationId equals randomizationTemp.Id into randomizationDto
                             from randomization in randomizationDto.DefaultIfEmpty()
                             select new QueryManagementDto
                             {
                                 Id = query.Id,
                                 ProjectCode = screening.Project.ProjectCode,
                                 ReasonName = reason.ReasonName,
                                 StatusName = query.QueryStatus.GetDescription(),
                                 Value = query.Value,
                                 QueryDescription = query.Note,
                                 OldValue = query.OldValue,
                                 QueryStatus = query.QueryStatus,
                                 CreatedById = user.Id,
                                 CreatedByName = role == null || string.IsNullOrEmpty(role.RoleName)
                                     ? user.UserName
                                     : user.UserName + " (" + role.RoleName + ")" +
                                       Convert.ToString(query.IsSystem ? " - System" : ""),
                                 ScreeningTemplateValue = template.ProjectDesignTemplate.TemplateName,
                                 Visit = template.ScreeningVisit.ProjectDesignVisit.DisplayName +
                                         Convert.ToString(template.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + template.ScreeningVisit.RepeatedVisitNumber),
                                 FieldName = value.ProjectDesignVariable.VariableName,
                                 VolunteerName = screening.RandomizationId != null ? randomization.Initial : screening.Attendance.Volunteer.AliasName,
                                 SubjectNo = screening.RandomizationId != null ? randomization.ScreeningNumber : screening.Attendance.Volunteer.VolunteerNo,
                                 RandomizationNumber = screening.RandomizationId != null ? randomization.RandomizationNumber : "",
                                 ScreeningTemplateValueId = query.ScreeningTemplateValueId
                             }).OrderBy(x => x.Id).ToList();

            var groupbytemp = queryDtos.GroupBy(x => x.ScreeningTemplateValueId).Select(y => new QueryManagementDto
            {
                Id = y.Last().Id,
                ProjectCode = y.Last().ProjectCode,
                ReasonName = y.Last().ReasonName,
                StatusName = y.Last().StatusName,
                QueryDescription = y.Last().QueryDescription,
                QueryStatus = y.Last().QueryStatus,
                OldValue = y.Last().OldValue,
                ScreeningTemplateValue = y.Last().ScreeningTemplateValue,
                CreatedById = y.Where(a => a.QueryStatus == QueryStatus.Open).LastOrDefault()?.CreatedById,
                Visit = y.Last().Visit,
                FieldName = y.Last().FieldName,
                VolunteerName = y.Last().VolunteerName,
                SubjectNo = y.Last().SubjectNo,
                RandomizationNumber = y.Last().RandomizationNumber,
                ClosedByName = y.Where(a => a.QueryStatus == QueryStatus.Closed).LastOrDefault()?.CreatedByName,
                Value = y.Last().Value,
                CreatedByName = y.Where(a => a.QueryStatus == QueryStatus.Open).LastOrDefault()?.CreatedByName,
                ModifieedByName = y.Where(a => a.QueryStatus == QueryStatus.Resolved).LastOrDefault()?.CreatedByName,
            }).Where(q =>
                (filters.Status == null || q.QueryStatus.GetDescription() == ((QueryStatus)filters.Status).GetDescription())
                && (filters.QueryGenerateBy == null || filters.QueryGenerateBy.Contains(q.CreatedById))
              ).ToList();

            return groupbytemp;
        }

        public IList<QueryManagementDto> GetGenerateQueryBy(int projectId)
        {
            var ParentProject = _context.Project.FirstOrDefault(x => x.Id == projectId).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == projectId).ToList().Select(x => x.Id).ToList();

            var queryData = (from screening in _context.ScreeningEntry.Where(t => ParentProject != null ? t.ProjectId == projectId : sites.Contains(t.ProjectId))
                             join template in _context.ScreeningTemplate on screening.Id equals template.ScreeningVisit.ScreeningEntryId
                             join value in _context.ScreeningTemplateValue on template.Id equals value.ScreeningTemplateId
                             join query in _context.ScreeningTemplateValueQuery.Where(q => q.QueryStatus == QueryStatus.Open) on
                                 value.Id equals query.ScreeningTemplateValueId
                             join userTemp in _context.Users on query.CreatedBy equals userTemp.Id into userDto
                             from user in userDto.DefaultIfEmpty()
                             join roleTemp in _context.SecurityRole on query.UserRoleId equals roleTemp.Id into roleDto
                             from role in roleDto.DefaultIfEmpty()
                             select new QueryManagementDto
                             {
                                 Id = user.Id,
                                 Value = role == null || string.IsNullOrEmpty(role.RoleName)
                                     ? user.UserName
                                     : user.UserName + " (" + role.RoleName + ")" +
                                       Convert.ToString(query.IsSystem ? " - System" : "")
                             }).ToList();

            var queryGeneratedBy = queryData.GroupBy(item => new { item.Value, item.Id })
                .Select(z => new QueryManagementDto { Id = z.Key.Id, Value = z.Key.Value }).ToList();
            return queryGeneratedBy;
        }

        public IList<DropDownDto> GetDataEntryBy(int projectId)
        {
            var ParentProject = _context.Project.FirstOrDefault(x => x.Id == projectId).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == projectId).ToList().Select(x => x.Id).ToList();

            var dataEntryData = (from screening in _context.ScreeningEntry.Where(t => ParentProject != null ? t.ProjectId == projectId : sites.Contains(t.ProjectId))
                                 join template in _context.ScreeningTemplate on screening.Id equals template.ScreeningVisit.ScreeningEntryId
                                 join value in _context.ScreeningTemplateValue on template.Id equals value.ScreeningTemplateId
                                 //join audit in _context.ScreeningTemplateValueAudit on value.Id equals audit.ScreeningTemplateValueId
                                 //join auditTemp in _context.ScreeningTemplateValueAudit on value.Id equals auditTemp.ScreeningTemplateValueId into auditDt
                                 //from audit in auditDt.DefaultIfEmpty()
                                 join userTemp in _context.Users on value.CreatedBy equals userTemp.Id into userDto
                                 from user in userDto.DefaultIfEmpty()
                                 join roleTemp in _context.SecurityRole on value.UserRoleId equals roleTemp.Id into roleDto
                                 from role in roleDto.DefaultIfEmpty()
                                 select new QueryManagementDto
                                 {
                                     Id = user.Id,
                                     Value = role == null || string.IsNullOrEmpty(role.RoleName)
                                         ? user.UserName
                                         : user.UserName + "(" + role.RoleName + ")"
                                 }).ToList();

            var dataEntryBy = dataEntryData.GroupBy(item => new { item.Value, item.Id })
                .Select(z => new DropDownDto { Id = z.Key.Id, Value = z.Key.Value }).ToList();
            return dataEntryBy;
        }

        public WorkFlowLevelDto GetReviewLevel(int screeningTemplateId)
        {
            if (_workFlowLevelDto != null) return _workFlowLevelDto;

            var templateData = _context.ScreeningTemplate.Where(x => x.Id == screeningTemplateId).Select(r => new
            {
                r.ScreeningVisit.ScreeningEntryId,
                r.ScreeningVisit.ScreeningEntry.ProjectDesignId,
                r.StartLevel
            }).FirstOrDefault();

            if (templateData == null) return new WorkFlowLevelDto { IsWorkFlowBreak = false, LevelNo = -1 };

            var workFlowLevel = _projectWorkflowRepository.GetProjectWorkLevel(templateData.ProjectDesignId);

            if (templateData.StartLevel != null)
                workFlowLevel.StartLevel = (short)templateData.StartLevel;

            _workFlowLevelDto = workFlowLevel;

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
            }
            else
            {
                queryOldValue = screeningTemplateValueQueryDto.OldValue;
                queryValue = value;
            }

            screeningTemplateValueQuery.Value = queryValue;
            screeningTemplateValueQuery.OldValue = queryOldValue;
                    
            var audit = new ScreeningTemplateValueAudit
            {
                ScreeningTemplateValueId= screeningTemplateValue.Id,
                OldValue = queryOldValue,
                Value = queryValue,
                Note = screeningTemplateValueQueryDto.Note + " " + status,
                ReasonId = screeningTemplateValueQueryDto.ReasonId,
                ReasonOth = screeningTemplateValueQueryDto.ReasonOth
            };
            _screeningTemplateValueAuditRepository.Save(audit);
        }

        public List<DashboardQueryStatusDto> GetDashboardQueryStatusByVisit(int projectId)
        {
            var queryStatus = (from stvq in _context.ScreeningTemplateValueQuery
                               join stv in _context.ScreeningTemplateValue on stvq.ScreeningTemplateValueId equals stv.Id into
                                   templatevalue
                               from stv in templatevalue.DefaultIfEmpty()
                               join st in _context.ScreeningTemplate on stv.ScreeningTemplateId equals st.Id into stemplate
                               from st in stemplate.DefaultIfEmpty()
                               join se in _context.ScreeningEntry on st.ScreeningVisit.ScreeningEntryId equals se.Id into entry
                               from sEntry in entry.DefaultIfEmpty()
                               join p in _context.Project on sEntry.ProjectId equals p.Id into project
                               from p in project.DefaultIfEmpty()
                               where p.Id == projectId || p.ParentProjectId == projectId
                               group new { stvq } by new { stvq.QueryStatus }
                    into g
                               select new DashboardQueryStatusDto
                               {
                                   DisplayName = g.Key.QueryStatus.GetDescription(),
                                   Total = g.Count()
                               }
                ).ToList();
            return queryStatus;
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
                                   Open = g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 1).Count(),
                                   Answered = g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 2).Count(),
                                   Resolved = g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 3).Count(),
                                   ReOpened = g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 4).Count(),
                                   Closed = g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 5).Count(),
                                   SelfCorrection = g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 6).Count(),
                                   Acknowledge = g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 7).Count(),
                                   Total = g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 1).Count() +
                                           g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 2).Count() +
                                           g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 3).Count()
                                           + g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 4).Count() +
                                           g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 5).Count() +
                                           g.Where(x => (int)x.stemplatevaluequery.QueryStatus == 6).Count()
                               }).ToList();
            return queryStatus;
        }

        public List<DashboardQueryStatusDto> GetDashboardQueryStatusByRolewise(int projectId)
        {
            var queryStatus = (from stvq in _context.ScreeningTemplateValueQuery
                               join stv in _context.ScreeningTemplateValue on stvq.ScreeningTemplateValueId equals stv.Id into
                                   templatevalue
                               from stv in templatevalue.DefaultIfEmpty()
                               join st in _context.ScreeningTemplate on stv.ScreeningTemplateId equals st.Id into stemplate
                               from st in stemplate.DefaultIfEmpty()
                               join se in _context.ScreeningEntry on st.ScreeningVisit.ScreeningEntryId equals se.Id into entry
                               from sEntry in entry.DefaultIfEmpty()
                               join p in _context.Project on sEntry.ProjectId equals p.Id into project
                               from p in project.DefaultIfEmpty()
                               join sr in _context.SecurityRole on stvq.UserRoleId equals sr.Id into role
                               from sr in role.DefaultIfEmpty()
                               where p.Id == projectId || p.ParentProjectId == projectId
                               group new { stvq } by new { stvq.UserRoleId, sr.RoleShortName, stvq.QueryStatus }
                    into g
                               select new DashboardQueryStatusDto
                               {
                                   DisplayName = g.Key.RoleShortName,
                                   QueryStatus = g.Key.QueryStatus.GetDescription(),
                                   Total = g.Count()
                               }
                ).ToList();
            return queryStatus;
        }

        public List<DashboardQueryStatusDto> GetDashboardQueryStatusByVisitwise(int projectId)
        {
            var queryStatus = (from stvq in _context.ScreeningTemplateValueQuery
                               join stv in _context.ScreeningTemplateValue on stvq.ScreeningTemplateValueId equals stv.Id into
                                   templatevalue
                               from stv in templatevalue.DefaultIfEmpty()
                               join st in _context.ScreeningTemplate on stv.ScreeningTemplateId equals st.Id into stemplate
                               from st in stemplate.DefaultIfEmpty()
                               join pdv in _context.ProjectDesignVisit on st.ScreeningVisit.ProjectDesignVisitId equals pdv.Id into design
                               from pdesign in design.DefaultIfEmpty()
                               join se in _context.ScreeningEntry on st.ScreeningVisit.ScreeningEntryId equals se.Id into entry
                               from sEntry in entry.DefaultIfEmpty()
                               join p in _context.Project on sEntry.ProjectId equals p.Id into project
                               from p in project.DefaultIfEmpty()
                               where p.Id == projectId || p.ParentProjectId == projectId
                               orderby pdesign.Id
                               group new { stvq, pdesign, st } by new { pdesign.Description, st.ScreeningVisit.ProjectDesignVisitId }
                into g
                               select new DashboardQueryStatusDto
                               {
                                   DisplayName = g.Key.Description,
                                   Open = g.Where(x => (int)x.stvq.QueryStatus == 1).Count(),
                                   Answered = g.Where(x => (int)x.stvq.QueryStatus == 2).Count(),
                                   Resolved = g.Where(x => (int)x.stvq.QueryStatus == 3).Count(),
                                   ReOpened = g.Where(x => (int)x.stvq.QueryStatus == 4).Count(),
                                   Closed = g.Where(x => (int)x.stvq.QueryStatus == 5).Count(),
                                   SelfCorrection = g.Where(x => (int)x.stvq.QueryStatus == 6).Count(),
                                   Acknowledge = g.Where(x => (int)x.stvq.QueryStatus == 7).Count(),
                                   Total = g.Where(x => (int)x.stvq.QueryStatus == 1).Count() +
                                           g.Where(x => (int)x.stvq.QueryStatus == 2).Count() +
                                           g.Where(x => (int)x.stvq.QueryStatus == 3).Count()
                                           + g.Where(x => (int)x.stvq.QueryStatus == 4).Count() +
                                           g.Where(x => (int)x.stvq.QueryStatus == 5).Count() +
                                           g.Where(x => (int)x.stvq.QueryStatus == 6).Count()
                               }).ToList();
            return queryStatus;
        }

    }
}