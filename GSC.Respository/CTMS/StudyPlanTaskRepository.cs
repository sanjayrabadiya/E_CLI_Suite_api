using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSC.Helper;
using GSC.Common;
using GSC.Common.Common;
using GSC.Data.Dto.Audit;
using Microsoft.EntityFrameworkCore;
using GSC.Shared.Extension;
using GSC.Respository.Master;
using GSC.Respository.ProjectRight;
using GSC.Data.Dto.Master;

namespace GSC.Respository.CTMS
{
    public class StudyPlanTaskRepository : GenericRespository<StudyPlanTask>, IStudyPlanTaskRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IHolidayMasterRepository _holidayMasterRepository;
        private readonly IWeekEndMasterRepository _weekEndMasterRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectRightRepository _projectRightRepository;

        public StudyPlanTaskRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, IHolidayMasterRepository holidayMasterRepository, IWeekEndMasterRepository weekEndMasterRepository, IProjectRightRepository projectRightRepository, IProjectRepository projectRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _projectRepository = projectRepository;
            _holidayMasterRepository = holidayMasterRepository;
            _projectRightRepository = projectRightRepository;
            _weekEndMasterRepository = weekEndMasterRepository;
        }

        public StudyPlanTaskGridDto GetStudyPlanTaskList(bool isDeleted, int StudyPlanId, int ProjectId, int countryId)
        {
            var result = new StudyPlanTaskGridDto();

            var TodayDate = DateTime.Now;
            if (countryId > 0)
            {
                var projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == ProjectId
                                                          && _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                          && a.UserId == _jwtTokenAccesser.UserId
                                                          && a.RoleId == _jwtTokenAccesser.RoleId
                                                          && a.DeletedDate == null
                                                          && a.RollbackReason == null)
                                                          && x.ManageSite.City.State.CountryId == countryId
                                                          && x.DeletedDate == null).ToList();

                if (projectIds.Count == 0)
                    projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x =>
                                                         _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                        && a.UserId == _jwtTokenAccesser.UserId
                                                        && a.RoleId == _jwtTokenAccesser.RoleId
                                                        && a.DeletedDate == null
                                                         && a.RollbackReason == null)
                                                        && x.ManageSite.City.State.CountryId == countryId
                                                        && x.Id == ProjectId
                                                        && x.DeletedDate == null).ToList();

                var studyplans = _context.StudyPlan.Where(x => projectIds.Select(f => f.Id).Contains(x.ProjectId) && x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();

                foreach (var item in studyplans)
                {
                    result.StartDate = item?.StartDate;
                    result.EndDate = item?.EndDate;
                    result.EndDateDay = item?.EndDate;
                    result.StudyPlanId = StudyPlanId;
                    //var TodayDate = DateTime.Now;

                    if (studyplans != null)
                    {
                        var tasklist = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == item.Id).OrderBy(x => x.TaskOrder).
                        ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();

                        if (tasklist.Any(x => TodayDate < x.StartDate))
                            tasklist.Where(x => TodayDate < x.StartDate).Select(c => { c.Status = CtmsChartType.NotStarted.GetDescription(); return c; }).ToList();

                        if (tasklist.Any(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null))
                            tasklist.Where(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null).Select(c => { c.Status = CtmsChartType.OnGoingDate.GetDescription(); return c; }).ToList();

                        if (tasklist.Any(x => x.StartDate < TodayDate && x.ActualStartDate == null))
                            tasklist.Where(x => x.StartDate < TodayDate && x.ActualStartDate == null).Select(c => { c.Status = CtmsChartType.DueDate.GetDescription(); return c; }).ToList();

                        if (tasklist.Any(x => x.ActualStartDate != null && x.ActualEndDate != null))
                            tasklist.Where(x => x.ActualStartDate != null && x.ActualEndDate != null).Select(c => { c.Status = CtmsChartType.Completed.GetDescription(); return c; }).ToList();

                        if (tasklist.Any(x => x.EndDate < x.ActualEndDate))
                            tasklist.Where(x => x.EndDate < x.ActualEndDate).Select(c => { c.Status = CtmsChartType.DeviatedDate.GetDescription(); return c; }).ToList();

                        result.StudyPlanTask = tasklist;
                    }
                }
            }
            else
            {
                var studyplan = _context.StudyPlan.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).OrderByDescending(x => x.Id).LastOrDefault();
                result.StartDate = studyplan?.StartDate;
                result.EndDate = studyplan?.EndDate;
                result.EndDateDay = studyplan?.EndDate;
                result.StudyPlanId = StudyPlanId;

                if (studyplan != null)
                {
                    var tasklist = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == studyplan.Id).OrderBy(x => x.TaskOrder).
                    ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();

                    if (tasklist.Any(x => TodayDate < x.StartDate))
                        tasklist.Where(x => TodayDate < x.StartDate).Select(c => { c.Status = CtmsChartType.NotStarted.GetDescription(); return c; }).ToList();

                    if (tasklist.Any(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null))
                        tasklist.Where(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null).Select(c => { c.Status = CtmsChartType.OnGoingDate.GetDescription(); return c; }).ToList();

                    if (tasklist.Any(x => x.StartDate < TodayDate && x.ActualStartDate == null))
                        tasklist.Where(x => x.StartDate < TodayDate && x.ActualStartDate == null).Select(c => { c.Status = CtmsChartType.DueDate.GetDescription(); return c; }).ToList();

                    if (tasklist.Any(x => x.ActualStartDate != null && x.ActualEndDate != null))
                        tasklist.Where(x => x.ActualStartDate != null && x.ActualEndDate != null).Select(c => { c.Status = CtmsChartType.Completed.GetDescription(); return c; }).ToList();

                    if (tasklist.Any(x => x.EndDate < x.ActualEndDate))
                        tasklist.Where(x => x.EndDate < x.ActualEndDate).Select(c => { c.Status = CtmsChartType.DeviatedDate.GetDescription(); return c; }).ToList();

                    result.StudyPlanTask = tasklist;
                }
            }
            //Add by mitul task was Resource Add
            if(result.StudyPlanTask != null)
            foreach (var item in result.StudyPlanTask)
            {
                var resourcelist = _context.StudyPlanResource.Include(x => x.ResourceType).Where(s => s.DeletedDate == null && s.StudyPlanTaskId==item.Id)
               .Select(x => new ResourceTypeGridDto
               {
                   Id = x.Id,
                   TaskId = item.Id,
                   ResourceType = x.ResourceType.ResourceTypes.GetDescription(),
                   ResourceSubType = x.ResourceType.ResourceSubType.GetDescription(),
                   Role = x.ResourceType.Role.RoleName,
                   User = x.ResourceType.User.UserName,
                   Designation = x.ResourceType.Designation.NameOFDesignation,
                   YersOfExperience = x.ResourceType.Designation.YersOfExperience,
                   NameOfMaterial = x.ResourceType.NameOfMaterial,
                   CreatedDate = x.CreatedDate,
                   CreatedByUser = x.CreatedByUser.UserName
               }).ToList();
                item.TaskResource = resourcelist;
            }

            return result;
        }
        public List<StudyPlanTask> Save(StudyPlanTask taskData)
        {
            var tasklist = new List<StudyPlanTask>();
            int ParentProjectId = _context.StudyPlan.Where(x => x.Id == taskData.StudyPlanId).Select(x => x.ProjectId).FirstOrDefault();
            if (taskData.RefrenceType == RefrenceType.Study)
            {
                var data = new StudyPlanTask();
                data.ProjectId = ParentProjectId;
                tasklist.Add(data);
            }
            else if (taskData.RefrenceType == RefrenceType.Sites)
            {
                var siteslist = _context.Project.Where(x => x.ParentProjectId == ParentProjectId && x.DeletedDate == null).Select(x => x.Id).ToList();
                foreach (var sitesId in siteslist)
                {
                    var data = new StudyPlanTask();
                    data = _mapper.Map<StudyPlanTask>(taskData);
                    data.ProjectId = sitesId;
                    tasklist.Add(data);
                }
            }
            else
            {
                var data = new StudyPlanTask();
                data.ProjectId = ParentProjectId;
                tasklist.Add(data);
                var siteslist = _context.Project.Where(x => x.ParentProjectId == ParentProjectId && x.DeletedDate == null).Select(x => x.Id).ToList();
                foreach (var sitesId in siteslist)
                {
                    data = new StudyPlanTask();
                    data.ProjectId = sitesId;
                    tasklist.Add(data);
                }
                tasklist.ForEach(t =>
                {
                    t.ProjectId = t.ProjectId;
                    t.StudyPlanId = taskData.StudyPlanId;
                    t.TaskId = taskData.TaskId;
                    t.TaskName = taskData.TaskName;
                    t.ParentId = taskData.ParentId;
                    t.isMileStone = taskData.isMileStone;
                    t.Duration = taskData.Duration;
                    t.StartDate = taskData.StartDate;
                    t.EndDate = taskData.EndDate;
                    t.Progress = taskData.Progress;
                    t.TaskOrder = taskData.TaskOrder;
                    t.ActualStartDate = taskData.ActualStartDate;
                    t.ActualEndDate = taskData.ActualEndDate;
                    t.DependentTaskId = taskData.DependentTaskId;
                    t.ActivityType = taskData.ActivityType;
                    t.OffSet = taskData.OffSet;
                    t.Percentage = taskData.Percentage;
                });
            }
            _context.StudyPlanTask.AddRange(tasklist);
            _context.Save();
            return tasklist;
        }

        public int UpdateTaskOrder(StudyPlantaskParameterDto taskmasterDto)
        {
            if (taskmasterDto.Position == Position.Above)
            {
                var data = All.Where(x => x.StudyPlanId == taskmasterDto.StudyPlanId && x.TaskOrder >= taskmasterDto.TaskOrder && x.DeletedDate == null).ToList();
                foreach (var item in data)
                {
                    item.TaskOrder = ++item.TaskOrder;
                    Update(item);
                }
                return taskmasterDto.TaskOrder;
            }
            if (taskmasterDto.Position == Position.Below)
            {
                var data = All.Where(x => x.StudyPlanId == taskmasterDto.StudyPlanId && x.TaskOrder > taskmasterDto.TaskOrder && x.DeletedDate == null).ToList();
                foreach (var item in data)
                {
                    item.TaskOrder = ++item.TaskOrder;
                    Update(item);
                }
                return ++taskmasterDto.TaskOrder;
            }
            else
            {
                var count = All.Where(x => x.StudyPlanId == taskmasterDto.StudyPlanId && x.DeletedDate == null).Count();
                return ++count;
            }

        }

        public string ValidateTask(StudyPlanTask taskmasterDto)
        {
            var studyplan = _context.StudyPlan.Where(x => x.Id == taskmasterDto.StudyPlanId).FirstOrDefault();
            if (taskmasterDto.StartDate >= studyplan.StartDate && taskmasterDto.StartDate <= studyplan.EndDate
                && taskmasterDto.EndDate <= studyplan.EndDate && taskmasterDto.EndDate >= studyplan.StartDate)
            {
                if (taskmasterDto.ParentId > 0)
                {
                    var parentdate = All.Where(x => x.Id == taskmasterDto.ParentId).FirstOrDefault();
                    if (taskmasterDto.StartDate >= parentdate.StartDate && taskmasterDto.StartDate <= parentdate.EndDate
                        && taskmasterDto.EndDate <= parentdate.EndDate && taskmasterDto.EndDate >= parentdate.StartDate)
                        return "";
                    else
                        return "Child Task Add between Parent Task Start and End Date";
                }
                return "";
            }
            else
            {
                return "Plan Date Add between Plan Start and End Date";
            }
        }

        public void UpdateParentDate(int? ParentId)
        {
            var tasklist = All.Where(i => i.Id == ParentId && i.DeletedDate == null).FirstOrDefault();

            tasklist.StartDate = All.Where(x => x.ParentId == ParentId && x.DeletedDate == null).Min(i => i.StartDate);
            tasklist.EndDate = All.Where(x => x.ParentId == ParentId && x.DeletedDate == null).Max(i => i.EndDate);
            tasklist.Duration = All.Where(x => x.ParentId == ParentId && x.DeletedDate == null).Sum(i => i.Duration);
            Update(tasklist);
            _context.Save();
        }

        public void InsertDependentTask(List<DependentTaskParameterDto> dependentTasks, int StudyPlanTaskId)
        {
            UpdateDependentTaskDate(StudyPlanTaskId);
            var dependentIds = GetRelatedChainId(StudyPlanTaskId);
            for (int i = 0; i < dependentIds.Count; i++)
            {
                UpdateDependentTaskDate(dependentIds[i].Id);
            }
        }

        private void UpdateDependentTaskDate(int StudyPlanTaskId)
        {
            var dependenttask = All.Where(x => x.Id == StudyPlanTaskId).FirstOrDefault();

            var maintask = All.Where(x => x.Id == dependenttask.Id && x.DeletedDate == null).FirstOrDefault();
            if (dependenttask.ActivityType == ActivityType.FF)
            {
                var task = All.Where(x => x.Id == dependenttask.DependentTaskId).FirstOrDefault();
                maintask.EndDate = WorkingDayHelper.AddBusinessDays(task.EndDate, dependenttask.OffSet);
                maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                Update(maintask);
            }
            else if (dependenttask.ActivityType == ActivityType.FS)
            {
                var task = All.Where(x => x.Id == dependenttask.DependentTaskId).FirstOrDefault();
                maintask.StartDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.EndDate, dependenttask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextWorkingDay(task.EndDate), dependenttask.OffSet);
                maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                Update(maintask);
            }
            else if (dependenttask.ActivityType == ActivityType.SF)
            {
                var task = All.Where(x => x.Id == dependenttask.DependentTaskId).FirstOrDefault();
                maintask.EndDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.StartDate, dependenttask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextSubstarctWorkingDay(task.StartDate), dependenttask.OffSet);
                maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                Update(maintask);
            }
            else if (dependenttask.ActivityType == ActivityType.SS)
            {
                var task = All.Where(x => x.Id == dependenttask.DependentTaskId).FirstOrDefault();
                maintask.StartDate = WorkingDayHelper.AddBusinessDays(task.StartDate, dependenttask.OffSet);
                maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                Update(maintask);
            }

            _context.Save();
        }

        public void UpdateTaskOrderSequence(int StudyPlanId)
        {
            var tasklist = All.Where(x => x.StudyPlanId == StudyPlanId && x.DeletedDate == null).OrderBy(x => x.TaskOrder).ToList();
            int i = 1;
            tasklist.ForEach(t =>
            {
                t.TaskOrder = i;
                i++;
            });
            _context.StudyPlanTask.UpdateRange(tasklist);
            _context.Save();
        }

        private List<DependentTaskDto> GetRelatedChainId(int StudyPlanId)
        {
            string sqlqry = @"with temp (
                             Id, DependentTaskId
                            ) as (
                             select Id, DependentTaskId
                             from   StudyPlanTask
                             where  DependentTaskId = " + StudyPlanId + @" and DeletedDate IS NULL
                             union  all
                             select e.Id, e.DependentTaskId
                             from   temp oc
                             join   StudyPlanTask e
                             on     e.DependentTaskId = oc.Id
                             where e.DeletedDate is null
                            )
                             select Id from temp;";
            var finaldata = _context.FromSql<DependentTaskDto>(sqlqry).ToList();
            return finaldata;
        }

        public string UpdateDependentTask(int StudyPlanTaskId)
        {
            var dependentIds = GetRelatedChainId(StudyPlanTaskId);
            var refrencedata = All.Where(x => (dependentIds.Select(x => x.Id).Contains(x.Id) || x.Id == StudyPlanTaskId) && x.DeletedDate == null).ToList();
            for (int i = 0; i < dependentIds.Count; i++)
            {
                string validate = UpdateDependentTaskDate1(dependentIds[i].Id, ref refrencedata);
                if (!string.IsNullOrEmpty(validate))
                {
                    return validate;
                }
            }
            _context.Save();
            return "";
        }

        public StudyPlanTask UpdateDependentTaskDate(StudyPlanTask maintask)
        {
            int ProjectId = _context.StudyPlan.Where(x => x.Id == maintask.StudyPlanId).FirstOrDefault().ProjectId;
            var holidaylist = _holidayMasterRepository.GetHolidayList(ProjectId);
            var weekendlist = _weekEndMasterRepository.GetWorkingDayList(ProjectId);
            //var weekendlist = new List<string>();
            WorkingDayHelper.InitholidayDate(holidaylist, weekendlist);

            if (maintask.DependentTaskId > 0)
            {
                if (maintask.ActivityType == ActivityType.FF)
                {
                    var task = All.Where(x => x.Id == maintask.DependentTaskId).FirstOrDefault();
                    maintask.EndDate = WorkingDayHelper.AddBusinessDays(task.EndDate, maintask.OffSet);
                    maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                }
                else if (maintask.ActivityType == ActivityType.FS)
                {
                    var task = All.Where(x => x.Id == maintask.DependentTaskId).FirstOrDefault();
                    maintask.StartDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.EndDate, maintask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextWorkingDay(task.EndDate), maintask.OffSet);
                    maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                }
                else if (maintask.ActivityType == ActivityType.SF)
                {
                    var task = All.Where(x => x.Id == maintask.DependentTaskId).FirstOrDefault();
                    maintask.EndDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.StartDate, maintask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextSubstarctWorkingDay(task.StartDate), maintask.OffSet);
                    maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                }
                else if (maintask.ActivityType == ActivityType.SS)
                {
                    var task = All.Where(x => x.Id == maintask.DependentTaskId).FirstOrDefault();
                    maintask.StartDate = WorkingDayHelper.AddBusinessDays(task.StartDate, maintask.OffSet);
                    maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                }
                return maintask;
            }
            return null;
        }

        private string UpdateDependentTaskDate1(int StudyPlanTaskId, ref List<StudyPlanTask> reftasklist)
        {
            int studyPlanId = reftasklist.FirstOrDefault().StudyPlanId;
            int ProjectId = _context.StudyPlan.Where(x => x.Id == studyPlanId).FirstOrDefault().ProjectId;
            var holidaylist = _holidayMasterRepository.GetHolidayList(ProjectId);
            var weekendlist = _weekEndMasterRepository.GetWorkingDayList(ProjectId);

            WorkingDayHelper.InitholidayDate(holidaylist, weekendlist);
            //var maintask = All.Where(x => x.Id == dependenttask.Id && x.DeletedDate == null).SingleOrDefault();
            var maintask = reftasklist.Where(x => x.Id == StudyPlanTaskId && x.DeletedDate == null).FirstOrDefault();
            if (maintask.ActivityType == ActivityType.FF)
            {
                var task = reftasklist.Where(x => x.Id == maintask.DependentTaskId).FirstOrDefault();
                maintask.EndDate = WorkingDayHelper.AddBusinessDays(task.EndDate, maintask.OffSet);
                maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);

                Update(maintask);
            }
            else if (maintask.ActivityType == ActivityType.FS)
            {
                var task = reftasklist.Where(x => x.Id == maintask.DependentTaskId).FirstOrDefault();
                maintask.StartDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.EndDate, maintask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextWorkingDay(task.EndDate), maintask.OffSet);
                maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);

                Update(maintask);
            }
            else if (maintask.ActivityType == ActivityType.SF)
            {
                var task = reftasklist.Where(x => x.Id == maintask.DependentTaskId).FirstOrDefault();
                maintask.EndDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.StartDate, maintask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextSubstarctWorkingDay(task.StartDate), maintask.OffSet);
                maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);

                Update(maintask);
            }
            else if (maintask.ActivityType == ActivityType.SS)
            {
                var task = reftasklist.Where(x => x.Id == maintask.DependentTaskId).FirstOrDefault();
                maintask.StartDate = WorkingDayHelper.AddBusinessDays(task.StartDate, maintask.OffSet);
                maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);

                Update(maintask);
            }
            return "";
        }

        public DateTime GetNextWorkingDate(NextWorkingDateParameterDto parameterDto)
        {
            int ProjectId = _context.StudyPlan.Where(x => x.Id == parameterDto.StudyPlanId).FirstOrDefault().ProjectId;
            var holidaylist = _holidayMasterRepository.GetHolidayList(ProjectId);
            var weekendlist = _weekEndMasterRepository.GetWorkingDayList(ProjectId);
            WorkingDayHelper.InitholidayDate(holidaylist, weekendlist);
            var nextworkingdate = WorkingDayHelper.AddBusinessDays(parameterDto.StartDate, parameterDto.Duration > 0 ? parameterDto.Duration - 1 : 0);
            return nextworkingdate;
        }

        public string ValidateweekEnd(NextWorkingDateParameterDto parameterDto)
        {
            int ProjectId = _context.StudyPlan.Where(x => x.Id == parameterDto.StudyPlanId).SingleOrDefault().ProjectId;
            return string.Empty;

        }

        public List<AuditTrailDto> GetStudyPlanTaskHistory(int id)
        {
            var result = _context.AuditTrail.Where(x => x.RecordId == id && x.TableName == "studyplantask" && x.Action == "Modified")
                .Select(x => new AuditTrailDto
                {
                    Id = x.Id,
                    TableName = x.TableName,
                    RecordId = x.RecordId,
                    Action = x.Action,
                    ColumnName = x.ColumnName,
                    OldValue = x.OldValue,
                    NewValue = x.NewValue,
                    ReasonOth = x.ReasonOth,
                    UserId = x.UserId,
                    CreatedDate = x.CreatedDate,
                    ReasonName = x.Reason,
                    UserName = x.User.UserName,
                    UserRoleName = x.UserRole,
                    IpAddress = x.IpAddress,
                    TimeZone = x.TimeZone
                }).ToList();

            return result;
        }

        public StudyPlanTaskGridDto GetStudyPlanDependentTaskList(int? StudyPlanTaskId, int ProjectId)
        {
            var result = new StudyPlanTaskGridDto();

            var studyplan = _context.StudyPlan.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).OrderByDescending(x => x.Id).LastOrDefault();
            result.StudyPlanId= studyplan.Id;

            if (studyplan != null)
            {
                var tasklist = All.Where(x => x.DeletedDate == null && x.StudyPlanId == studyplan.Id && x.Id != StudyPlanTaskId).OrderBy(x => x.TaskOrder).
               ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();
                //result.StudyPlanTask = tasklist.Where(x => x.DependentTaskId != StudyPlanTaskId).ToList();
                result.StudyPlanTask = tasklist;
            }

            return result;
        }

        public StudyPlanTaskChartDto GetDocChart(int projectId)
        {
            StudyPlanTaskChartDto result = new StudyPlanTaskChartDto();

            var StudyPlanTask = All.Include(x => x.StudyPlan).Where(x => x.StudyPlan.DeletedDate == null && x.StudyPlan.ProjectId == projectId && x.DeletedDate == null).ToList();

            var TodayDate = DateTime.Now;
            result.All = StudyPlanTask.Count();

            foreach (var item in StudyPlanTask)
            {
                if (item.EndDate < item.ActualEndDate)
                {
                    result.DeviatedDate = result.DeviatedDate + 1;
                    continue;
                }

                if (item.ActualStartDate != null && item.ActualEndDate != null)
                {
                    result.Complete = result.Complete + 1;
                    continue;
                }

                if (item.StartDate < TodayDate && item.ActualStartDate == null)
                {
                    result.DueDate = result.DueDate + 1;
                    continue;
                }

                if (item.StartDate < TodayDate && item.EndDate > TodayDate && item.ActualStartDate != null && item.ActualEndDate == null)
                {
                    result.OnGoingDate = result.OnGoingDate + 1;
                    continue;
                }

                if (TodayDate < item.StartDate)
                {
                    result.NotStartedDate = result.NotStartedDate + 1;
                    continue;
                }
            }

            return result;
        }

        public List<StudyPlanTaskChartReportDto> GetChartReport(int projectId, CtmsChartType? chartType)
        {
            var result = new List<StudyPlanTaskChartReportDto>();
            var TodayDate = DateTime.Now;
            var StudyPlanTask = All.Include(x => x.StudyPlan).Where(x => x.StudyPlan.DeletedDate == null && x.StudyPlan.ProjectId == projectId && x.DeletedDate == null).ToList();

            var data = new List<StudyPlanTask>();

            foreach (var item in StudyPlanTask)
            {
                if (item.EndDate < item.ActualEndDate)
                {
                    if (chartType == CtmsChartType.DeviatedDate)
                        data.Add(item);
                    continue;
                }

                if (item.ActualStartDate != null && item.ActualEndDate != null)
                {
                    if (chartType == CtmsChartType.Completed)
                        data.Add(item);
                    continue;
                }

                if (item.StartDate < TodayDate && item.ActualStartDate == null)
                {
                    if (chartType == CtmsChartType.DueDate)
                        data.Add(item);
                    continue;
                }

                if (item.StartDate < TodayDate && item.EndDate > TodayDate && item.ActualStartDate != null && item.ActualEndDate == null)
                {
                    if (chartType == CtmsChartType.OnGoingDate)
                        data.Add(item);
                    continue;
                }

                if (TodayDate < item.StartDate)
                {
                    if (chartType == CtmsChartType.NotStarted)
                        data.Add(item);
                    continue;
                }
            }

            result = data.Select(x => new StudyPlanTaskChartReportDto
            {
                Id = x.Id,
                Duration = x.Duration,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                TaskName = x.TaskName,
                NoOfDeviatedDay = chartType == CtmsChartType.DeviatedDate ? (x.ActualEndDate - x.EndDate).Value.Days : 0,
            }).ToList();
            return result;
        }

        public List<StudyPlanTaskDto> ResourceMgmtSearch(ResourceMgmtFilterDto search)
        {
            var result = new List<StudyPlanTaskDto>();
            if (search.countryId > 0)
            {
                var projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == search.siteId
                                                          && _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                          && a.UserId == _jwtTokenAccesser.UserId
                                                          && a.RoleId == _jwtTokenAccesser.RoleId
                                                          && a.DeletedDate == null
                                                          && a.RollbackReason == null)
                                                          && x.ManageSite.City.State.CountryId == search.countryId
                                                          && x.DeletedDate == null).ToList();

                if (projectIds.Count == 0)
                    projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x =>
                                                         _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                        && a.UserId == _jwtTokenAccesser.UserId
                                                        && a.RoleId == _jwtTokenAccesser.RoleId
                                                        && a.DeletedDate == null
                                                        && a.RollbackReason == null)
                                                        && x.ManageSite.City.State.CountryId == search.countryId
                                                        && x.Id == search.siteId
                                                        && x.DeletedDate == null).ToList();

                var studyplans = _context.StudyPlan.Where(x => projectIds.Select(f => f.Id).Contains(x.ProjectId) && x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();
                foreach (var item in studyplans)
                {
                    if (studyplans != null)
                    {
                        var tasklist = All.Where(x => false ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == item.Id).OrderBy(x => x.TaskOrder).
                         ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();
                        result = tasklist;
                    }
                }
            }
            else
            {
                var studyplan = _context.StudyPlan.Where(x => x.ProjectId == search.siteId && x.DeletedDate == null).OrderByDescending(x => x.Id).LastOrDefault();
                if (studyplan != null)
                {
                    var tasklist = All.Where(x => false ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == studyplan.Id).OrderBy(x => x.TaskOrder).
                    ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();
                    result = tasklist;
                }
            }
               
            if(result !=null)
            foreach (var item in result)
            {
                var resourcelist = _context.StudyPlanResource.Include(x => x.ResourceType).Where(s => s.DeletedDate == null && s.StudyPlanTaskId == item.Id)
               .Select(x => new ResourceTypeGridDto
               {
                   Id = x.Id,
                   TaskId = item.Id,
                   ResourceType = x.ResourceType.ResourceTypes.GetDescription(),
                   ResourceSubType = x.ResourceType.ResourceSubType.GetDescription(),
                   Role = x.ResourceType.Role.RoleName,
                   User = x.ResourceType.User.UserName,
                   Designation = x.ResourceType.Designation.NameOFDesignation,
                   YersOfExperience = x.ResourceType.Designation.YersOfExperience,
                   NameOfMaterial = x.ResourceType.NameOfMaterial,
                   CreatedDate = x.CreatedDate,
                   CreatedByUser = x.CreatedByUser.UserName
               }).ToList();
                item.TaskResource = resourcelist;
            }

            //Apply Filter
            if (search.ResourceId.HasValue)
                result = result.Where(s => s.TaskResource
                .Any(x => x.ResourceType == (search.ResourceId == (int)ResourceTypeEnum.Manpower ? ResourceTypeEnum.Manpower.GetDescription() : ResourceTypeEnum.Material.GetDescription()))).ToList();

            if (search.ResourceSubId.HasValue)
                result = result.Where(s => s.TaskResource
                .Any(x => x.ResourceSubType == (
                search.ResourceSubId == (int)SubResourceType.Permanent ? SubResourceType.Permanent.GetDescription():
                search.ResourceSubId == (int)SubResourceType.Contract ? SubResourceType.Contract.GetDescription():
                search.ResourceSubId == (int)SubResourceType.Consumable ? SubResourceType.Consumable.GetDescription() : SubResourceType.NonConsumable.GetDescription()
                ))).ToList();

            if (search.RoleId.HasValue)
                 result = result.Where(s => s.TaskResource.Any(x => x.Role == _context.SecurityRole.Where(s => s.Id == search.RoleId).Select(x => x.RoleName).FirstOrDefault())).ToList();

            if (search.UserId.HasValue)
                result = result.Where(s => s.TaskResource.Any(x => x.User == _context.Users.Where(s => s.Id == search.UserId).Select(x => x.UserName).FirstOrDefault())).ToList();
            
            if (search.DesignationId.HasValue)
                result = result.Where(s => s.TaskResource.Any(x => x.Designation == _context.Designation.Where(s => s.Id == search.DesignationId).Select(x => x.NameOFDesignation).FirstOrDefault())).ToList();

            if (search.ResourceNotAdded == true)
                result = result.Where(s => s.TaskResource.Count == 0).ToList();
            
            if (search.ResourceAdded == true)
                result = result.Where(s => s.TaskResource.Count != 0).ToList();

            return result;
        }

        public List<DropDownDto> GetRollDropDown(int studyplanId)
        {
          var studyPlanTaskDada = _context.StudyPlanTask.Where(x=>x.StudyPlanId == studyplanId && x.DeletedDate == null).ToList();
          var data= _context.StudyPlanResource.Include(x=>x.StudyPlanTask).Include(d=>d.ResourceType).ThenInclude(r => r.Role).
                Where(s => studyPlanTaskDada.Select(f => f.Id).Contains(s.StudyPlanTaskId) && s.ResourceType.RoleId !=null).
                Select(x=> new DropDownDto { Id = x.ResourceType.Role.Id, Value = x.ResourceType.Role.RoleName , IsDeleted = x.ResourceType.Role.DeletedDate != null }).Distinct().ToList();

            return data;
        }
        public List<DropDownDto> GetUserDropDown(int studyplanId)
        {
            var studyPlanTaskDada = _context.StudyPlanTask.Where(x => x.StudyPlanId == studyplanId && x.DeletedDate == null).ToList();
            var data = _context.StudyPlanResource.Include(x => x.StudyPlanTask).Include(d => d.ResourceType).ThenInclude(r => r.User).
                  Where(s => studyPlanTaskDada.Select(f => f.Id).Contains(s.StudyPlanTaskId) && s.ResourceType.UserId != null).
                  Select(x => new DropDownDto { Id = x.ResourceType.User.Id, Value = x.ResourceType.User.UserName, IsDeleted = x.ResourceType.Role.DeletedDate != null }).Distinct().ToList();

            return data;
        }
        public List<DropDownDto> GetDesignationStdDropDown(int studyplanId)
        {
            var studyPlanTaskDada = _context.StudyPlanTask.Where(x => x.StudyPlanId == studyplanId && x.DeletedDate == null).ToList();
            var data = _context.StudyPlanResource.Include(x => x.StudyPlanTask).Include(d => d.ResourceType).ThenInclude(r => r.Designation).
                  Where(s => studyPlanTaskDada.Select(f => f.Id).Contains(s.StudyPlanTaskId) && s.ResourceType.DesignationId != null).
                  Select(x => new DropDownDto { Id = x.ResourceType.Designation.Id, Value = x.ResourceType.Designation.NameOFDesignation, IsDeleted = x.ResourceType.Designation.DeletedDate != null }).Distinct().ToList();

            return data;
        }
    }
}

