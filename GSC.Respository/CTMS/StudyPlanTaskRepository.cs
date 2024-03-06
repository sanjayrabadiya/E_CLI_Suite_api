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
using GSC.Helper;
using GSC.Common;
using GSC.Data.Dto.Audit;
using Microsoft.EntityFrameworkCore;
using GSC.Shared.Extension;
using GSC.Respository.Master;
using GSC.Respository.ProjectRight;
using GSC.Data.Dto.Master;
using System.Linq.Dynamic.Core;

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



                    var tasklistResource = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == item.Id).OrderBy(x => x.TaskOrder).
                     ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();

                    var tasklist = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == item.Id && x.ParentId == 0).OrderBy(x => x.TaskOrder).
                    ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();

                    if (tasklist.Exists(x => TodayDate < x.StartDate))
                        tasklist.Where(x => TodayDate < x.StartDate).Select(c => { c.Status = CtmsChartType.NotStarted.GetDescription(); return c; }).ToList();

                    if (tasklist.Exists(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null))
                        tasklist.Where(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null).Select(c => { c.Status = CtmsChartType.OnGoingDate.GetDescription(); return c; }).ToList();

                    if (tasklist.Exists(x => x.StartDate < TodayDate && x.ActualStartDate == null))
                        tasklist.Where(x => x.StartDate < TodayDate && x.ActualStartDate == null).Select(c => { c.Status = CtmsChartType.DueDate.GetDescription(); return c; }).ToList();

                    if (tasklist.Exists(x => x.ActualStartDate != null && x.ActualEndDate != null))
                        tasklist.Where(x => x.ActualStartDate != null && x.ActualEndDate != null).Select(c => { c.Status = CtmsChartType.Completed.GetDescription(); return c; }).ToList();

                    if (tasklist.Exists(x => x.EndDate < x.ActualEndDate))
                        tasklist.Where(x => x.EndDate < x.ActualEndDate).Select(c => { c.Status = CtmsChartType.DeviatedDate.GetDescription(); return c; }).ToList();

                    result.StudyPlanTask = tasklist;
                    result.StudyPlanTaskTemp = tasklistResource;

                    //sub task wise grid disply Add by mitul on 09-01-2024
                    result.StudyPlanTask.ForEach(s =>
                    {
                        var subtasklist = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == item.Id && x.ParentId == s.Id).OrderBy(x => x.TaskOrder).
                            ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();

                        if (subtasklist.Exists(x => TodayDate < x.StartDate))
                            subtasklist.Where(x => TodayDate < x.StartDate).Select(c => { c.Status = CtmsChartType.NotStarted.GetDescription(); return c; }).ToList();

                        if (subtasklist.Exists(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null))
                            subtasklist.Where(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null).Select(c => { c.Status = CtmsChartType.OnGoingDate.GetDescription(); return c; }).ToList();

                        if (subtasklist.Exists(x => x.StartDate < TodayDate && x.ActualStartDate == null))
                            subtasklist.Where(x => x.StartDate < TodayDate && x.ActualStartDate == null).Select(c => { c.Status = CtmsChartType.DueDate.GetDescription(); return c; }).ToList();

                        if (subtasklist.Exists(x => x.ActualStartDate != null && x.ActualEndDate != null))
                            subtasklist.Where(x => x.ActualStartDate != null && x.ActualEndDate != null).Select(c => { c.Status = CtmsChartType.Completed.GetDescription(); return c; }).ToList();

                        if (subtasklist.Exists(x => x.EndDate < x.ActualEndDate))
                            subtasklist.Where(x => x.EndDate < x.ActualEndDate).Select(c => { c.Status = CtmsChartType.DeviatedDate.GetDescription(); return c; }).ToList();

                        s.Subtasks = subtasklist;
                    });
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
                    var tasklistResource = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == studyplan.Id).OrderBy(x => x.TaskOrder).
                    ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();

                    var tasklist = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == studyplan.Id && x.ParentId == 0).OrderBy(x => x.TaskOrder).
                    ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();

                    if (tasklist.Exists(x => TodayDate < x.StartDate))
                        tasklist.Where(x => TodayDate < x.StartDate).Select(c => { c.Status = CtmsChartType.NotStarted.GetDescription(); return c; }).ToList();

                    if (tasklist.Exists(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null))
                        tasklist.Where(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null).Select(c => { c.Status = CtmsChartType.OnGoingDate.GetDescription(); return c; }).ToList();

                    if (tasklist.Exists(x => x.StartDate < TodayDate && x.ActualStartDate == null))
                        tasklist.Where(x => x.StartDate < TodayDate && x.ActualStartDate == null).Select(c => { c.Status = CtmsChartType.DueDate.GetDescription(); return c; }).ToList();

                    if (tasklist.Exists(x => x.ActualStartDate != null && x.ActualEndDate != null))
                        tasklist.Where(x => x.ActualStartDate != null && x.ActualEndDate != null).Select(c => { c.Status = CtmsChartType.Completed.GetDescription(); return c; }).ToList();

                    if (tasklist.Exists(x => x.EndDate < x.ActualEndDate))
                        tasklist.Where(x => x.EndDate < x.ActualEndDate).Select(c => { c.Status = CtmsChartType.DeviatedDate.GetDescription(); return c; }).ToList();

                    result.StudyPlanTask = tasklist;
                    result.StudyPlanTaskTemp = tasklistResource;

                    //sub task wise grid disply Add by mitul on 09-01-2024
                    result.StudyPlanTask.ForEach(s =>
                    {
                        var subtasklist = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == studyplan.Id && x.ParentId == s.Id).OrderBy(x => x.TaskOrder).
                            ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();

                        if (subtasklist.Exists(x => TodayDate < x.StartDate))
                            subtasklist.Where(x => TodayDate < x.StartDate).Select(c => { c.Status = CtmsChartType.NotStarted.GetDescription(); return c; }).ToList();

                        if (subtasklist.Exists(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null))
                            subtasklist.Where(x => x.StartDate < TodayDate && x.EndDate > TodayDate && x.ActualStartDate != null && x.ActualEndDate == null).Select(c => { c.Status = CtmsChartType.OnGoingDate.GetDescription(); return c; }).ToList();

                        if (subtasklist.Exists(x => x.StartDate < TodayDate && x.ActualStartDate == null))
                            subtasklist.Where(x => x.StartDate < TodayDate && x.ActualStartDate == null).Select(c => { c.Status = CtmsChartType.DueDate.GetDescription(); return c; }).ToList();

                        if (subtasklist.Exists(x => x.ActualStartDate != null && x.ActualEndDate != null))
                            subtasklist.Where(x => x.ActualStartDate != null && x.ActualEndDate != null).Select(c => { c.Status = CtmsChartType.Completed.GetDescription(); return c; }).ToList();

                        if (subtasklist.Exists(x => x.EndDate < x.ActualEndDate))
                            subtasklist.Where(x => x.EndDate < x.ActualEndDate).Select(c => { c.Status = CtmsChartType.DeviatedDate.GetDescription(); return c; }).ToList();

                        s.Subtasks = subtasklist;
                    });
                }
            }
            //Add by mitul task was Resource Add
            if (result.StudyPlanTaskTemp != null)
                foreach (var item in result.StudyPlanTaskTemp)
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
                       Unit = x.ResourceType.Unit.UnitName,
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
                return count;
            }

        }

        public string ValidateTask(StudyPlanTask taskmasterDto)
        {
            var studyplan = _context.StudyPlan.Where(x => x.Id == taskmasterDto.StudyPlanId).FirstOrDefault();
            if (studyplan != null)
            {
                if (taskmasterDto.StartDate >= studyplan.StartDate && taskmasterDto.StartDate <= studyplan.EndDate
                && taskmasterDto.EndDate <= studyplan.EndDate && taskmasterDto.EndDate >= studyplan.StartDate)
                {
                    if (taskmasterDto.ParentId > 0)
                    {
                        var parentdate = All.Where(x => x.Id == taskmasterDto.ParentId).FirstOrDefault();
                        if (parentdate != null)
                        {
                            if (taskmasterDto.StartDate >= parentdate.StartDate && taskmasterDto.StartDate <= parentdate.EndDate
                           && taskmasterDto.EndDate <= parentdate.EndDate && taskmasterDto.EndDate >= parentdate.StartDate)
                                return "";
                            else
                                return "Child Task Add between Parent Task Start and End Date";
                        }
                    }
                    return "";
                }
                else
                {
                    return "Plan Date Add between Plan Start and End Date";
                }
            }
            return "";
        }

        public void UpdateParentDate(int? ParentId)
        {
            var tasklist = All.Where(i => i.Id == ParentId && i.DeletedDate == null).FirstOrDefault();
            if (tasklist != null)
            {
                tasklist.StartDate = All.Where(x => x.ParentId == ParentId && x.DeletedDate == null).Min(i => i.StartDate);
                tasklist.EndDate = All.Where(x => x.ParentId == ParentId && x.DeletedDate == null).Max(i => i.EndDate);
                tasklist.Duration = All.Where(x => x.ParentId == ParentId && x.DeletedDate == null).Sum(i => i.Duration);
                Update(tasklist);
            }
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
            if (dependenttask != null)
            {
                var maintask = All.Where(x => x.Id == dependenttask.Id && x.DeletedDate == null).FirstOrDefault();
                if (maintask != null)
                {
                    if (dependenttask.ActivityType == ActivityType.FF)
                    {
                        var task = All.Where(x => x.Id == dependenttask.DependentTaskId).FirstOrDefault();
                        if (task != null)
                        {
                            maintask.EndDate = WorkingDayHelper.AddBusinessDays(task.EndDate, dependenttask.OffSet);
                        }
                        maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                        Update(maintask);
                    }
                    else if (dependenttask.ActivityType == ActivityType.FS)
                    {
                        var task = All.Where(x => x.Id == dependenttask.DependentTaskId).FirstOrDefault();
                        if (task != null)
                        {
                            maintask.StartDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.EndDate, dependenttask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextWorkingDay(task.EndDate), dependenttask.OffSet);
                        }
                        maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                        Update(maintask);
                    }
                    else if (dependenttask.ActivityType == ActivityType.SF)
                    {
                        var task = All.Where(x => x.Id == dependenttask.DependentTaskId).FirstOrDefault();
                        if (task != null)
                        {
                            maintask.EndDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.StartDate, dependenttask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextSubstarctWorkingDay(task.StartDate), dependenttask.OffSet);
                        }
                        maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                        Update(maintask);
                    }
                    else if (dependenttask.ActivityType == ActivityType.SS)
                    {
                        var task = All.Where(x => x.Id == dependenttask.DependentTaskId).FirstOrDefault();
                        if (task != null)
                        {
                            maintask.StartDate = WorkingDayHelper.AddBusinessDays(task.StartDate, dependenttask.OffSet);
                        }
                        maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                        Update(maintask);
                    }
                }
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

        public StudyPlanTask UpdateDependentTaskDate(StudyPlanTask StudyPlanTask)
        {
            int ProjectId = _context.StudyPlan.Where(x => x.Id == StudyPlanTask.StudyPlanId).Select(s => s.ProjectId).FirstOrDefault();
            var holidaylist = _holidayMasterRepository.GetHolidayList(ProjectId);
            var weekendlist = _weekEndMasterRepository.GetWorkingDayList(ProjectId);

            WorkingDayHelper.InitholidayDate(holidaylist, weekendlist);

            if (StudyPlanTask.DependentTaskId > 0)
            {
                if (StudyPlanTask.ActivityType == ActivityType.FF)
                {
                    var task = All.Where(x => x.Id == StudyPlanTask.DependentTaskId).FirstOrDefault();
                    if (task != null)
                    {
                        StudyPlanTask.EndDate = WorkingDayHelper.AddBusinessDays(task.EndDate, StudyPlanTask.OffSet);
                    }
                    StudyPlanTask.StartDate = WorkingDayHelper.SubtractBusinessDays(StudyPlanTask.EndDate, StudyPlanTask.Duration > 0 ? StudyPlanTask.Duration - 1 : 0);
                }
                else if (StudyPlanTask.ActivityType == ActivityType.FS)
                {
                    var task = All.Where(x => x.Id == StudyPlanTask.DependentTaskId).FirstOrDefault();
                    if (task != null)
                    {
                        StudyPlanTask.StartDate = StudyPlanTask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.EndDate, StudyPlanTask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextWorkingDay(task.EndDate), StudyPlanTask.OffSet);
                    }
                    StudyPlanTask.EndDate = WorkingDayHelper.AddBusinessDays(StudyPlanTask.StartDate, StudyPlanTask.Duration > 0 ? StudyPlanTask.Duration - 1 : 0);
                }
                else if (StudyPlanTask.ActivityType == ActivityType.SF)
                {
                    var task = All.Where(x => x.Id == StudyPlanTask.DependentTaskId).FirstOrDefault();
                    if (task != null)
                    {
                        StudyPlanTask.EndDate = StudyPlanTask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.StartDate, StudyPlanTask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextSubstarctWorkingDay(task.StartDate), StudyPlanTask.OffSet);
                    }
                    StudyPlanTask.StartDate = WorkingDayHelper.SubtractBusinessDays(StudyPlanTask.EndDate, StudyPlanTask.Duration > 0 ? StudyPlanTask.Duration - 1 : 0);
                }
                else if (StudyPlanTask.ActivityType == ActivityType.SS)
                {
                    var task = All.Where(x => x.Id == StudyPlanTask.DependentTaskId).FirstOrDefault();
                    if (task != null)
                    {
                        StudyPlanTask.StartDate = WorkingDayHelper.AddBusinessDays(task.StartDate, StudyPlanTask.OffSet);
                    }
                    StudyPlanTask.EndDate = WorkingDayHelper.AddBusinessDays(StudyPlanTask.StartDate, StudyPlanTask.Duration > 0 ? StudyPlanTask.Duration - 1 : 0);
                }
                return StudyPlanTask;
            }
            return null;
        }

        private string UpdateDependentTaskDate1(int StudyPlanTaskId, ref List<StudyPlanTask> reftasklist)
        {
            int studyPlanId = reftasklist.Select(s => s.StudyPlanId).FirstOrDefault();
            int ProjectId = _context.StudyPlan.Where(x => x.Id == studyPlanId).Select(s => s.ProjectId).FirstOrDefault();
            var holidaylist = _holidayMasterRepository.GetHolidayList(ProjectId);
            var weekendlist = _weekEndMasterRepository.GetWorkingDayList(ProjectId);

            WorkingDayHelper.InitholidayDate(holidaylist, weekendlist);

            var maintask = reftasklist.Find(x => x.Id == StudyPlanTaskId && x.DeletedDate == null);
            if (maintask != null)
            {
                if (maintask.ActivityType == ActivityType.FF)
                {
                    var task = reftasklist.Find(x => x.Id == maintask.DependentTaskId);
                    if (task != null)
                        maintask.EndDate = WorkingDayHelper.AddBusinessDays(task.EndDate, maintask.OffSet);
                    maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);

                    Update(maintask);
                }
                else if (maintask.ActivityType == ActivityType.FS)
                {
                    var task = reftasklist.Find(x => x.Id == maintask.DependentTaskId);
                    if (task != null)
                        maintask.StartDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.EndDate, maintask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextWorkingDay(task.EndDate), maintask.OffSet);
                    maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);

                    Update(maintask);
                }
                else if (maintask.ActivityType == ActivityType.SF)
                {
                    var task = reftasklist.Find(x => x.Id == maintask.DependentTaskId);
                    if (task != null)
                        maintask.EndDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.StartDate, maintask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextSubstarctWorkingDay(task.StartDate), maintask.OffSet);
                    maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);

                    Update(maintask);
                }
                else if (maintask.ActivityType == ActivityType.SS)
                {
                    var task = reftasklist.Find(x => x.Id == maintask.DependentTaskId);
                    if (task != null)
                        maintask.StartDate = WorkingDayHelper.AddBusinessDays(task.StartDate, maintask.OffSet);
                    maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);

                    Update(maintask);
                }
            }

            return "";
        }

        public DateTime GetNextWorkingDate(NextWorkingDateParameterDto parameterDto)
        {
            int ProjectId = _context.StudyPlan.Where(x => x.Id == parameterDto.StudyPlanId).Select(s => s.ProjectId).FirstOrDefault();
            var holidaylist = _holidayMasterRepository.GetHolidayList(ProjectId);
            var weekendlist = _weekEndMasterRepository.GetWorkingDayList(ProjectId);
            WorkingDayHelper.InitholidayDate(holidaylist, weekendlist);
            var nextworkingdate = WorkingDayHelper.AddBusinessDays(parameterDto.StartDate, parameterDto.Duration > 0 ? parameterDto.Duration - 1 : 0);
            return nextworkingdate;
        }

        public string ValidateweekEnd(NextWorkingDateParameterDto parameterDto)
        {
            int ProjectId = _context.StudyPlan.Where(x => x.Id == parameterDto.StudyPlanId).Select(s=>s.ProjectId).SingleOrDefault();
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

            if (studyplan != null)
            {
                result.StudyPlanId = studyplan.Id;
                var tasklist = All.Where(x => x.DeletedDate == null && x.StudyPlanId == studyplan.Id && x.Id != StudyPlanTaskId).OrderBy(x => x.TaskOrder).
               ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();

                result.StudyPlanTask = tasklist;
            }
            else
            {
                var TaskMaster = _context.StudyPlan.Where(x => x.Id == StudyPlanTaskId && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();
                var data = new StudyPlan();
                data.Id = 0;
                data.StartDate = TaskMaster.StartDate;
                data.EndDate = TaskMaster.EndDate;
                data.ProjectId = ProjectId;
                data.TaskTemplateId = TaskMaster.TaskTemplateId;
                _context.StudyPlan.Add(data);
                _context.Save();
                result.StudyPlanId = data.Id;
            }

            return result;
        }

        public StudyPlanTaskChartDto GetDocChart(int projectId)
        {
            StudyPlanTaskChartDto result = new StudyPlanTaskChartDto();

            var StudyPlanTask = All.Include(x => x.StudyPlan).Where(x => x.StudyPlan.DeletedDate == null && x.StudyPlan.ProjectId == projectId && x.DeletedDate == null).ToList();

            var TodayDate = DateTime.Now;
            result.All = StudyPlanTask.Count;

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

                if (TodayDate < item.StartDate && chartType == CtmsChartType.NotStarted)
                {
                        data.Add(item);
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
                if (studyplans != null )
                {
                    foreach (var item in studyplans)
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

            if (result != null)
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
                .Exists(x => x.ResourceType == (search.ResourceId == (int)ResourceTypeEnum.Manpower ? ResourceTypeEnum.Manpower.GetDescription() : ResourceTypeEnum.Material.GetDescription()))).ToList();

            if (search.ResourceSubId.HasValue)
                result = result.Where(s => s.TaskResource
                .Exists(x => x.ResourceSubType == (
                search.ResourceSubId == (int)SubResourceType.Permanent ? SubResourceType.Permanent.GetDescription() :
                search.ResourceSubId == (int)SubResourceType.Contract ? SubResourceType.Contract.GetDescription() :
                search.ResourceSubId == (int)SubResourceType.Consumable ? SubResourceType.Consumable.GetDescription() : SubResourceType.NonConsumable.GetDescription()
                ))).ToList();

            if (search.RoleId.HasValue)
                result = result.Where(s => s.TaskResource.Exists(x => x.Role == _context.SecurityRole.Where(s => s.Id == search.RoleId).Select(x => x.RoleName).FirstOrDefault())).ToList();

            if (search.UserId.HasValue)
                result = result.Where(s => s.TaskResource.Exists(x => x.User == _context.Users.Where(s => s.Id == search.UserId).Select(x => x.UserName).FirstOrDefault())).ToList();

            if (search.DesignationId.HasValue)
                result = result.Where(s => s.TaskResource.Exists(x => x.Designation == _context.Designation.Where(s => s.Id == search.DesignationId).Select(x => x.NameOFDesignation).FirstOrDefault())).ToList();

            if (search.ResourceNotAdded == true)
                result = result.Where(s => s.TaskResource.Count == 0).ToList();

            if (search.ResourceAdded == true)
                result = result.Where(s => s.TaskResource.Count != 0).ToList();

            return result;
        }

        public List<DropDownDto> GetRollDropDown(int studyplanId)
        {
            var studyPlanTaskDada = _context.StudyPlanTask.Where(x => x.StudyPlanId == studyplanId && x.DeletedDate == null).ToList();
            var data = _context.StudyPlanResource.Include(x => x.StudyPlanTask).Include(d => d.ResourceType).ThenInclude(r => r.Role).
                  Where(s => studyPlanTaskDada.Select(f => f.Id).Contains(s.StudyPlanTaskId) && s.ResourceType.RoleId != null).
                  Select(x => new DropDownDto { Id = x.ResourceType.Role.Id, Value = x.ResourceType.Role.RoleName, IsDeleted = x.ResourceType.Role.DeletedDate != null }).Distinct().ToList();

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
        public List<StudyPlanTaskDto> getBudgetPlaner(bool isDeleted, int studyId, int siteId, int countryId)
        {
            var result = new List<StudyPlanTaskDto>();
            if (countryId > 0)
            {
                var projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == studyId
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
                                                        && x.Id == siteId
                                                        && x.DeletedDate == null).ToList();

                var studyplans = _context.StudyPlan.Include(s => s.Currency).Where(x => projectIds.Select(f => f.Id).Contains(x.ProjectId) && x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();
                if (studyplans != null)
                {
                    foreach (var item in studyplans)
                    {
                        var tasklist = All.Where(x => false ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == item.Id).OrderBy(x => x.TaskOrder).
                         ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();
                        tasklist.ForEach(task =>
                        {
                            task.GlobalCurrencySymbol = item.Currency != null ? item.Currency.CurrencySymbol : "$";
                            task.StudayName = _context.Project.Where(s => s.Id == studyId && s.DeletedBy == null).Select(r => r.ProjectCode).FirstOrDefault();
                            task.CountryName = _context.Country.Where(s => s.Id == countryId && s.DeletedBy == null).Select(r => r.CountryName).FirstOrDefault();
                            task.SiteName = _context.Project.Where(s => s.Id == siteId && s.DeletedBy == null).Select(r => r.ProjectCode == null ? r.ManageSite.SiteName : r.ProjectCode).FirstOrDefault();
                        });
                        result = tasklist;
                    }
                }
            }
            else if (siteId > 0)
            {
                var studyplan = _context.StudyPlan.Include(s => s.Currency).Where(x => x.ProjectId == siteId && x.DeletedDate == null).OrderByDescending(x => x.Id).LastOrDefault();
                if (studyplan != null)
                {
                    var tasklist = All.Where(x => false ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == studyplan.Id).OrderBy(x => x.TaskOrder).
                    ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();
                    tasklist.ForEach(task =>
                    {
                        task.GlobalCurrencySymbol = studyplan.Currency.CurrencySymbol;
                        task.StudayName = _context.Project.Where(s => s.Id == studyId && s.DeletedBy == null).Select(r => r.ProjectCode).FirstOrDefault();
                        task.CountryName = _context.Country.Where(s => s.Id == countryId && s.DeletedBy == null).Select(r => r.CountryName).FirstOrDefault();
                        task.SiteName = _context.Project.Where(s => s.Id == siteId && s.DeletedBy == null).Select(r => r.ProjectCode == null ? r.ManageSite.SiteName : r.ProjectCode).FirstOrDefault();
                    });

                    result = tasklist;
                }
            }
            else
            {
                var studyplan = _context.StudyPlan.Include(s => s.Currency).Where(x => x.ProjectId == studyId && x.DeletedDate == null).OrderByDescending(x => x.Id).LastOrDefault();
                if (studyplan != null)
                {
                    var tasklist = All.Where(x => false ? x.DeletedDate != null : x.DeletedDate == null && x.StudyPlanId == studyplan.Id).OrderBy(x => x.TaskOrder).
                    ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();
                    tasklist.ForEach(task =>
                    {
                        task.GlobalCurrencySymbol = studyplan.Currency.CurrencySymbol;
                        task.StudayName = _context.Project.Where(s => s.Id == studyId && s.DeletedBy == null).Select(r => r.ProjectCode).FirstOrDefault();
                        task.CountryName = _context.Country.Where(s => s.Id == countryId && s.DeletedBy == null).Select(r => r.CountryName).FirstOrDefault();
                        task.SiteName = _context.Project.Where(s => s.Id == siteId && s.DeletedBy == null).Select(r => r.ProjectCode == null ? r.ManageSite.SiteName : r.ProjectCode).FirstOrDefault();
                    });

                    result = tasklist;
                }
            }

            if (result != null)
                foreach (var item in result)
                {
                    var resourcelist = _context.StudyPlanResource.Include(x => x.ResourceType).Include(r => r.StudyPlanTask).Where(s => s.DeletedDate == null && s.StudyPlanTaskId == item.Id)
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
                       Unit = x.ResourceType.Unit.UnitName,
                       NumberOfUnit = x.NoOfUnit,
                       Cost = x.ResourceType.Cost,
                       TotalCost = x.TotalCost,
                       ConvertTotalCost = x.ConvertTotalCost,
                       CurrencyType = x.ResourceType.Currency.CurrencySymbol + " - " + x.ResourceType.Currency.CurrencyName,
                       GlobalCurrencySymbol = x.StudyPlanTask.StudyPlan.Currency.CurrencySymbol,
                       LocalCurrencySymbol = x.ResourceType.Currency.CurrencySymbol,
                       CreatedDate = x.CreatedDate,
                       CreatedByUser = x.CreatedByUser.UserName,
                       LocalCurrencyRate = _context.CurrencyRate.Where(s => s.StudyPlanId == x.StudyPlanTask.StudyPlanId && s.CurrencyId == x.ResourceType.CurrencyId && s.DeletedBy == null).Select(t => t.LocalCurrencyRate).FirstOrDefault(),
                   }).ToList();
                    item.TaskResource = resourcelist;
                }
            return result;
        }
    }
}

