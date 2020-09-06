using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.Project.Workflow;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueQueryRepository : GenericRespository<ScreeningTemplateValueQuery, GscContext>,
        IScreeningTemplateValueQueryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IScreeningTemplateValueEditCheckRepository _screeningTemplateValueEditCheckRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateValueScheduleRepository _screeningTemplateValueScheduleRepository;
        private WorkFlowLevelDto _workFlowLevelDto;
        public ScreeningTemplateValueQueryRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IScreeningTemplateValueEditCheckRepository screeningTemplateValueEditCheckRepository,
            IScreeningTemplateValueScheduleRepository screeningTemplateValueScheduleRepository)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _screeningTemplateValueEditCheckRepository = screeningTemplateValueEditCheckRepository;
            _screeningTemplateValueScheduleRepository = screeningTemplateValueScheduleRepository;
        }

        public IList<ScreeningTemplateValueQueryDto> GetQueries(int screeningTemplateValueId)
        {
            var queryDtos =
                (from query in Context.ScreeningTemplateValueQuery.Where(t =>
                        t.ScreeningTemplateValueId == screeningTemplateValueId)
                 join reasonTemp in Context.AuditReason on query.ReasonId equals reasonTemp.Id into reasonDt
                 from reason in reasonDt.DefaultIfEmpty()
                 join userTemp in Context.Users on query.CreatedBy equals userTemp.Id into userDto
                 from user in userDto.DefaultIfEmpty()
                 join roleTemp in Context.SecurityRole on query.UserRoleId equals roleTemp.Id into roleDto
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
                _screeningTemplateValueEditCheckRepository.CloseSystemQuery(screeningTemplateValue.ScreeningTemplateId,
                    screeningTemplateValue.ProjectDesignVariableId);
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
                var screeningTemplate = Context.ScreeningTemplate.Find(screeningTemplateValue.ScreeningTemplateId);
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
                var screeningTemplate = Context.ScreeningTemplate.Find(screeningTemplateValue.ScreeningTemplateId);
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
            var screeningTemplate = Context.ScreeningTemplate.Find(screeningTemplateValue.ScreeningTemplateId);


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
            _screeningTemplateValueRepository.Update(screeningTemplateValue);

            screeningTemplateValueQuery.QueryLevel = workFlowLevel.LevelNo;
            screeningTemplateValueQuery.UserRoleId = _jwtTokenAccesser.RoleId;
            Add(screeningTemplateValueQuery);
        }

        public List<ScreeningQueryReviewDto> GetScreeningQueryReview()
        {
            var result = All.Where(x => x.DeletedDate == null && x.QueryStatus != QueryStatus.Closed
            ).Select(a => new ScreeningQueryReviewDto
            {
                ScreeningEntryId = a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Id,
                ScreeningDate = a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ScreeningDate,
                ProjectName = a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ProjectName,
                VolunteerName = a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                    ? a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.NoneRegister.Initial
                    : a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.FullName,
                TemplateName = a.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplate.TemplateName,
                VistName = a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ProjectDesignVisit
                    .DisplayName,
                VariableName = a.ScreeningTemplateValue.ProjectDesignVariable.VariableName,
                ScreeningTemplateId = a.ScreeningTemplateValue.ScreeningTemplateId,
                QueryLevel = a.QueryLevel,
                QueryStaus = a.QueryStatus.GetDescription(),
                Reason = a.Reason.ReasonName,
                Comments = a.Note,
                SubmittedDate = a.CreatedDate,
                SubmittedBy = a.CreatedByUser.UserName,
                ScreeningNo = a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ScreeningNo +
                              " |Project Name: " + a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project
                                  .ProjectName
                              + " |Volunteer Name: " +
                              a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer == null
                    ? a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.NoneRegister.Initial
                    : a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.FullName +
                      " |Screening Date: " + a.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ScreeningDate
            }).ToList();

            return result;
        }


        public List<DashboardQueryStatusDto> GetDashboardQueryStatusByVisit(int projectId)
        {
            var queryStatus = (from stvq in Context.ScreeningTemplateValueQuery
                               join stv in Context.ScreeningTemplateValue on stvq.ScreeningTemplateValueId equals stv.Id into
                                   templatevalue
                               from stv in templatevalue.DefaultIfEmpty()
                               join st in Context.ScreeningTemplate on stv.ScreeningTemplateId equals st.Id into stemplate
                               from st in stemplate.DefaultIfEmpty()
                               join se in Context.ScreeningEntry on st.ScreeningVisit.ScreeningEntryId equals se.Id into entry
                               from sEntry in entry.DefaultIfEmpty()
                               join p in Context.Project on sEntry.ProjectId equals p.Id into project
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
            var queryStatus = (from p in Context.Project
                               join se in Context.ScreeningEntry on p.Id equals se.ProjectId into entry
                               from sEntry in entry.DefaultIfEmpty()
                               join st in Context.ScreeningTemplate on sEntry.Id equals st.ScreeningVisit.ScreeningEntryId into stemplate
                               from st in stemplate.DefaultIfEmpty()
                               join pdv in Context.ProjectDesignVisit on st.ScreeningVisitId equals pdv.Id into design
                               from pdesign in design.DefaultIfEmpty()
                               join stv in Context.ScreeningTemplateValue on st.Id equals stv.ScreeningTemplateId into templatevalue
                               from stv in templatevalue.DefaultIfEmpty()
                               join stvq in Context.ScreeningTemplateValueQuery on stv.Id equals stvq.ScreeningTemplateValueId into
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
            var queryStatus = (from stvq in Context.ScreeningTemplateValueQuery
                               join stv in Context.ScreeningTemplateValue on stvq.ScreeningTemplateValueId equals stv.Id into
                                   templatevalue
                               from stv in templatevalue.DefaultIfEmpty()
                               join st in Context.ScreeningTemplate on stv.ScreeningTemplateId equals st.Id into stemplate
                               from st in stemplate.DefaultIfEmpty()
                               join se in Context.ScreeningEntry on st.ScreeningVisit.ScreeningEntryId equals se.Id into entry
                               from sEntry in entry.DefaultIfEmpty()
                               join p in Context.Project on sEntry.ProjectId equals p.Id into project
                               from p in project.DefaultIfEmpty()
                               join sr in Context.SecurityRole on stvq.UserRoleId equals sr.Id into role
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
            var queryStatus = (from stvq in Context.ScreeningTemplateValueQuery
                               join stv in Context.ScreeningTemplateValue on stvq.ScreeningTemplateValueId equals stv.Id into
                                   templatevalue
                               from stv in templatevalue.DefaultIfEmpty()
                               join st in Context.ScreeningTemplate on stv.ScreeningTemplateId equals st.Id into stemplate
                               from st in stemplate.DefaultIfEmpty()
                               join pdv in Context.ProjectDesignVisit on st.ScreeningVisitId equals pdv.Id into design
                               from pdesign in design.DefaultIfEmpty()
                               join se in Context.ScreeningEntry on st.ScreeningVisit.ScreeningEntryId equals se.Id into entry
                               from sEntry in entry.DefaultIfEmpty()
                               join p in Context.Project on sEntry.ProjectId equals p.Id into project
                               from p in project.DefaultIfEmpty()
                               where p.Id == projectId || p.ParentProjectId == projectId
                               orderby pdesign.Id
                               group new { stvq, pdesign, st } by new { pdesign.Description, st.ScreeningVisitId }
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

        public IList<QueryManagementDto> GetQueryEntries(QuerySearchDto filters)
        {
            var queryDtos = (from screening in Context.ScreeningEntry.Where(t =>
                    t.ProjectId == filters.ProjectId &&
                    (filters.PeriodIds == null || filters.PeriodIds.Contains(t.ProjectDesignPeriodId)) && (filters.SubjectIds == null || filters.SubjectIds.Contains(t.AttendanceId)))
                             join template in Context.ScreeningTemplate.Where(u =>
                                     (filters.TemplateIds == null || filters.TemplateIds.Contains(u.ProjectDesignTemplateId))
                                     && (filters.VisitIds == null ||
                                         filters.VisitIds.Contains(u.ScreeningVisit.ProjectDesignVisitId))) on screening.Id
                                 equals
                                 template.ScreeningVisit.ScreeningEntryId
                             join value in Context.ScreeningTemplateValue.Where(val =>
                                 filters.DataEntryBy == null || val.CreatedBy == filters.DataEntryBy) on template.Id equals value
                                 .ScreeningTemplateId
                             join query in Context.ScreeningTemplateValueQuery on value.Id equals query.ScreeningTemplateValueId
                             join reasonTemp in Context.AuditReason on query.ReasonId equals reasonTemp.Id into reasonDt
                             from reason in reasonDt.DefaultIfEmpty()
                             join userTemp in Context.Users on query.CreatedBy equals userTemp.Id into userDto
                             from user in userDto.DefaultIfEmpty()
                             join roleTemp in Context.SecurityRole on query.UserRoleId equals roleTemp.Id into roleDto
                             from role in roleDto.DefaultIfEmpty()
                             join usermodifiedTemp in Context.Users on query.ModifiedBy equals usermodifiedTemp.Id into
                                 userModifiedDto
                             from userModified in userDto.DefaultIfEmpty()
                             join roleModifiedTemp in Context.SecurityRole on query.UserRoleId equals roleModifiedTemp.Id into
                                 roleModifiedDto
                             from roleModified in roleDto.DefaultIfEmpty()
                             join attendance in Context.Attendance.Where(t =>
                                 t.DeletedDate == null) on screening
                                 .AttendanceId equals attendance.Id
                             join volunteerTemp in Context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into
                                 volunteerDto
                             from volunteer in volunteerDto.DefaultIfEmpty()
                             join noneregisterTemp in Context.NoneRegister on attendance.Id equals noneregisterTemp.AttendanceId into
                                 noneregisterDto
                             from nonregister in noneregisterDto.DefaultIfEmpty()
                             join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals
                                 projectSubjectTemp.Id into projectsubjectDto
                             from projectsubject in projectsubjectDto.DefaultIfEmpty()
                             select new QueryManagementDto
                             {
                                 Id = query.Id,
                                 ScreeningEntryId = screening.Id,
                                 ScreeningTemplateId = template.Id,
                                 ProjectCode = screening.Project.ProjectCode,
                                 Value = query.Value,
                                 ProjectDesignVariableId = value.ProjectDesignVariableId,
                                 ReasonName = reason.ReasonName,
                                 ReasonOth = query.ReasonOth,
                                 Note = string.IsNullOrEmpty(query.Note) ? query.ReasonOth : query.Note,
                                 CreatedDate = query.CreatedDate,
                                 CreatedById = user.Id,
                                 CreatedByName = role == null || string.IsNullOrEmpty(role.RoleName)
                                     ? user.UserName
                                     : user.UserName + " (" + role.RoleName + ")" +
                                       Convert.ToString(query.IsSystem ? " - System" : ""),
                                 ModifiedDate = query.ModifiedDate,
                                 ModifieedByName = roleModified == null || string.IsNullOrEmpty(roleModified.RoleName)
                                     ? userModified.UserName
                                     : userModified.UserName + " (" + roleModified.RoleName + ")",
                                 StatusName = query.QueryStatus.GetDescription(),
                                 QueryStatus = query.QueryStatus,
                                 QueryDescription = query.Note,
                                 OldValue = query.OldValue,
                                 CollectionSource = (int)value.ProjectDesignVariable.CollectionSource,
                                 ScreeningTemplateValue = template.ProjectDesignTemplate.TemplateName,
                                 Visit = template.ScreeningVisit.ProjectDesignVisit.DisplayName +
                                         Convert.ToString(template.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + template.ScreeningVisit.RepeatedVisitNumber),
                                 AcknowledgementStatus =
                                     query.QueryStatus.GetDescription() == QueryStatus.Acknowledge.GetDescription()
                                         ? "Acknowledge"
                                         : "",
                                 FieldName = value.ProjectDesignVariable.VariableName,
                                 VolunteerName = volunteer.FullName == null ? nonregister.Initial : volunteer.AliasName,
                                 SubjectNo = volunteer.FullName == null ? nonregister.ScreeningNumber : volunteer.VolunteerNo,
                                 RandomizationNumber = volunteer.FullName == null
                                     ? nonregister.RandomizationNumber
                                     : projectsubject.Number,
                                 //AuditId = audit.Id
                                 ScreeningTemplateValueId = query.ScreeningTemplateValueId
                             }).OrderBy(x => x.Id).ToList();

            var groupbytemp = queryDtos.GroupBy(x => x.ScreeningTemplateValueId).Select(y => new QueryManagementDto
            {
                Id = y.Last().Id,
                ScreeningEntryId = y.Last().ScreeningEntryId,
                ScreeningTemplateId = y.Last().ScreeningTemplateId,
                ProjectDesignVariableId = y.Last().ProjectDesignVariableId,
                ProjectCode = y.Last().ProjectCode,
                Value = y.Last().Value,
                ReasonName = y.Last().ReasonName,
                ReasonOth = y.Last().ReasonOth,
                Note = y.Last().Note,
                CollectionSource = y.Last().CollectionSource,
                CreatedById = y.Where(a => a.QueryStatus == QueryStatus.Open).LastOrDefault()?.CreatedById,
                CreatedByName = y.Where(a => a.QueryStatus == QueryStatus.Open).LastOrDefault()?.CreatedByName,
                ModifieedByName = y.Where(a => a.QueryStatus == QueryStatus.Resolved).LastOrDefault()?.CreatedByName,
                ClosedByName = y.Where(a => a.QueryStatus == QueryStatus.Closed).LastOrDefault()?.CreatedByName,
                StatusName = y.Last().StatusName,
                QueryStatus = y.Last().QueryStatus,
                QueryDescription = y.Last().QueryDescription,
                OldValue = y.Last().OldValue,
                ScreeningTemplateValue = y.Last().ScreeningTemplateValue,
                Visit = y.Last().Visit,
                AcknowledgementStatus = y.Last().AcknowledgementStatus,
                FieldName = y.Last().FieldName,
                VolunteerName = y.Last().VolunteerName,
                SubjectNo = y.Last().SubjectNo,
                RandomizationNumber = y.Last().RandomizationNumber
            }).Where(q =>
                (filters.Status == null ||
                 q.QueryStatus.GetDescription() == ((QueryStatus)filters.Status).GetDescription()) &&
                (filters.QueryGenerateBy == null || filters.QueryGenerateBy.Contains(q.CreatedById))).ToList();

            return groupbytemp;
        }

        public IList<QueryManagementDto> GetGenerateQueryBy(int projectId)
        {
            var queryData = (from screening in Context.ScreeningEntry.Where(t => t.ProjectId == projectId)
                             join template in Context.ScreeningTemplate on screening.Id equals template.ScreeningVisit.ScreeningEntryId
                             join value in Context.ScreeningTemplateValue on template.Id equals value.ScreeningTemplateId
                             join query in Context.ScreeningTemplateValueQuery.Where(q => q.QueryStatus == QueryStatus.Open) on
                                 value.Id equals query.ScreeningTemplateValueId
                             join reasonTemp in Context.AuditReason on query.ReasonId equals reasonTemp.Id into reasonDt
                             from reason in reasonDt.DefaultIfEmpty()
                             join userTemp in Context.Users on query.CreatedBy equals userTemp.Id into userDto
                             from user in userDto.DefaultIfEmpty()
                             join roleTemp in Context.SecurityRole on query.UserRoleId equals roleTemp.Id into roleDto
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

        public IList<QueryManagementDto> GetDataEntryBy(int projectId)
        {
            var dataEntryData = (from screening in Context.ScreeningEntry.Where(t => t.ProjectId == projectId)
                                 join template in Context.ScreeningTemplate on screening.Id equals template.ScreeningVisit.ScreeningEntryId
                                 join value in Context.ScreeningTemplateValue on template.Id equals value.ScreeningTemplateId
                                 join audit in Context.ScreeningTemplateValueAudit on value.Id equals audit.ScreeningTemplateValueId
                                 join reasonTemp in Context.AuditReason on audit.ReasonId equals reasonTemp.Id into reasonDt
                                 from reason in reasonDt.DefaultIfEmpty()
                                 join userTemp in Context.Users on audit.UserId equals userTemp.Id into userDto
                                 from user in userDto.DefaultIfEmpty()
                                 join roleTemp in Context.SecurityRole on audit.UserRoleId equals roleTemp.Id into roleDto
                                 from role in roleDto.DefaultIfEmpty()
                                 select new QueryManagementDto
                                 {
                                     Id = user.Id,
                                     Value = role == null || string.IsNullOrEmpty(role.RoleName)
                                         ? user.UserName
                                         : user.UserName + "(" + role.RoleName + ")"
                                 }).ToList();

            var dataEntryBy = dataEntryData.GroupBy(item => new { item.Value, item.Id })
                .Select(z => new QueryManagementDto { Id = z.Key.Id, Value = z.Key.Value }).ToList();
            return dataEntryBy;
        }

        public WorkFlowLevelDto GetReviewLevel(int screeningTemplateId)
        {
            if (_workFlowLevelDto != null) return _workFlowLevelDto;

            var templateData = Context.ScreeningTemplate.Where(x => x.Id == screeningTemplateId).Select(r => new
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
                var oldProjectDesignVariableValueIds = Context.ScreeningTemplateValueChild.AsNoTracking().Where(t =>
                        screeningTemplateValueQueryDto.Children.Select(s => s.Id).Contains(t.Id) && t.Value == "true")
                    .Select(t => t.ProjectDesignVariableValueId).ToList();

                queryOldValue = string.Join(", ",
                    Context.ProjectDesignVariableValue.Where(t => oldProjectDesignVariableValueIds.Contains(t.Id))
                        .Select(t => t.ValueName).ToList());

                var newProjectDesignVariableValueIds = screeningTemplateValueQueryDto.Children
                    .Where(t => t.Value == "true").Select(t => t.ProjectDesignVariableValueId).ToList();

                queryValue = string.Join(", ",
                    Context.ProjectDesignVariableValue.Where(t => newProjectDesignVariableValueIds.Contains(t.Id))
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
            screeningTemplateValue.Audits = new List<ScreeningTemplateValueAudit>
            {
                new ScreeningTemplateValueAudit
                {
                    OldValue = queryOldValue,
                    Value = queryValue,
                    Note = screeningTemplateValueQueryDto.Note + " " + status,
                    UserId = _jwtTokenAccesser.UserId,
                    UserRoleId = _jwtTokenAccesser.RoleId,
                    ReasonId = screeningTemplateValueQueryDto.ReasonId
                }
            };
        }
    }
}