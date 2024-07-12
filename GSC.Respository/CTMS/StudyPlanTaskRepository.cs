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
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Data.Entities.Master;

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
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public StudyPlanTaskRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, IHolidayMasterRepository holidayMasterRepository,
            IWeekEndMasterRepository weekEndMasterRepository,
            IProjectRightRepository projectRightRepository,
            IProjectRepository projectRepository,
             IUploadSettingRepository uploadSettingRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _projectRepository = projectRepository;
            _holidayMasterRepository = holidayMasterRepository;
            _projectRightRepository = projectRightRepository;
            _weekEndMasterRepository = weekEndMasterRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public StudyPlanTaskGridDto GetStudyPlanTaskList(bool isDeleted, int studyPlanId, int projectId, CtmsStudyTaskFilter filterType, int siteId, int countryId)
        {
            var result = new StudyPlanTaskGridDto
            {
                StudyPlanId = studyPlanId,
                StudyPlanTask = new List<StudyPlanTaskDto>(),
                StudyPlanTaskTemp = new List<StudyPlanTaskDto>()
            };

            var todayDate = DateTime.Now;
            var studyIds = GetStudyIds(projectId, filterType, siteId);
            var parentStudyPlan = _context.StudyPlan.FirstOrDefault(x => x.DeletedDate == null && x.Id == studyPlanId);

            if (parentStudyPlan != null)
            {
                result.StartDate = parentStudyPlan.StartDate;
                result.EndDate = parentStudyPlan.EndDate;
                result.EndDateDay = parentStudyPlan.EndDate;
            }

            var studyPlans = GetStudyPlans(filterType, studyIds, projectId);

            foreach (var studyPlan in studyPlans)
            {
                var taskList = GetTasks(isDeleted, studyPlan.Id, filterType, countryId);
                var taskListResource = GetTasksResource(isDeleted, studyPlan.Id, filterType, countryId);

                UpdateTaskStatus(taskList, todayDate);

                result.StudyPlanTask.AddRange(taskList);
                result.StudyPlanTaskTemp.AddRange(taskListResource);
            }

            UpdateSubTasks(result.StudyPlanTask, todayDate, isDeleted);

            AddTaskResources(result.StudyPlanTaskTemp);

            return result;
        }

        private List<int> GetStudyIds(int projectId, CtmsStudyTaskFilter filterType, int siteId)
        {
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            var ids = _projectRepository.All
                .Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) &&
                            x.DeletedDate == null &&
                            x.ParentProjectId == projectId &&
                            projectList.Any(c => c == x.Id))
                .Select(s => s.Id)
                .ToList();

            var studyIds = new List<int>();

            if (filterType == CtmsStudyTaskFilter.All || filterType == CtmsStudyTaskFilter.Country)
            {
                studyIds.AddRange(ids);
                studyIds.Add(projectId);
            }
            if (filterType == CtmsStudyTaskFilter.Site)
            {
                if (siteId == 0)
                    studyIds.AddRange(ids);
                else
                    studyIds.Add(siteId);
            }

            return studyIds;
        }

        private List<StudyPlan> GetStudyPlans(CtmsStudyTaskFilter filterType, List<int> studyIds, int projectId)
        {
            return _context.StudyPlan
                .Where(x => (filterType != CtmsStudyTaskFilter.Study ? studyIds.Contains(x.ProjectId) : x.ProjectId == projectId) && x.DeletedDate == null)
                .ToList();
        }

        private List<StudyPlanTaskDto> GetTasks(bool isDeleted, int studyPlanId, CtmsStudyTaskFilter filterType, int countryId)
        {
            var taskList = All
                .Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) &&
                            x.StudyPlanId == studyPlanId &&
                            (filterType == CtmsStudyTaskFilter.Country ? (countryId <= 0 ? x.IsCountry : x.CountryId == countryId) : filterType == CtmsStudyTaskFilter.All || !x.IsCountry))
                .Include(i => i.StudyPlan.Project)
                .OrderBy(x => x.TaskOrder)
                .ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider)
                .ToList();

            taskList.ForEach(x =>
            {
                if (x.ParentProjectId != null)
                {
                    x.StudayName = _context.Project.First(y => y.Id == x.ParentProjectId && x.DeletedDate == null)?.ProjectCode ?? "";
                }
            });

            return taskList;
        }

        private List<StudyPlanTaskDto> GetTasksResource(bool isDeleted, int studyPlanId, CtmsStudyTaskFilter filterType, int countryId)
        {
            return All
                .Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) &&
                            x.StudyPlanId == studyPlanId &&
                            (filterType == CtmsStudyTaskFilter.Country ? (countryId <= 0 ? x.IsCountry : x.CountryId == countryId) : filterType == CtmsStudyTaskFilter.All || !x.IsCountry))
                .OrderBy(x => x.TaskOrder)
                .ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        private void UpdateTaskStatus(List<StudyPlanTaskDto> taskList, DateTime todayDate)
        {
            taskList.ForEach(task =>
            {
                if (todayDate < task.StartDate && task.ActualStartDate == null && task.ActualEndDate == null)
                    task.Status = CtmsChartType.NotStarted.GetDescription();
                else if (task.ActualStartDate != null && task.ActualEndDate == null && task.EndDate > task.ActualStartDate)
                    task.Status = CtmsChartType.OnGoingDate.GetDescription();
                else if (task.StartDate < todayDate && task.EndDate > todayDate && task.ActualStartDate == null)
                    task.Status = CtmsChartType.DueDate.GetDescription();
                else if (task.ActualStartDate != null && task.ActualEndDate != null && task.EndDate >= task.ActualEndDate)
                    task.Status = CtmsChartType.Completed.GetDescription();
                else if ((task.EndDate < task.ActualEndDate) || (task.EndDate < todayDate && task.ActualStartDate != null && task.ActualEndDate == null) || (task.EndDate < todayDate && task.ActualStartDate == null && task.ActualEndDate == null))
                    task.Status = CtmsChartType.DeviatedDate.GetDescription();
            });
        }

        private void UpdateSubTasks(List<StudyPlanTaskDto> taskList, DateTime todayDate, bool isDeleted)
        {
            taskList.ForEach(parentTask =>
            {
                var subTaskList = All
                    .Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.StudyPlanId == parentTask.StudyPlanId && x.ParentId == parentTask.Id)
                    .Include(i => i.StudyPlan.Project)
                    .OrderBy(x => x.TaskOrder)
                    .ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider)
                    .ToList();

                if (subTaskList.Any())
                {
                    UpdateParentTaskDates(parentTask, subTaskList);
                    UpdateParentTaskStatus(parentTask, todayDate);

                    var changeData = All.First(o => o.Id == parentTask.Id);
                    if (changeData.ActualStartDate != parentTask.ActualStartDate || changeData.ActualEndDate != parentTask.ActualEndDate)
                    {
                        changeData.ActualStartDate = parentTask.ActualStartDate;
                        changeData.ActualEndDate = parentTask.ActualEndDate;
                    }

                    if (changeData.StartDate != parentTask.StartDate || changeData.EndDate != parentTask.EndDate)
                    {
                        changeData.StartDate = parentTask.StartDate ?? DateTime.Now;
                        changeData.EndDate = parentTask.EndDate ?? DateTime.Now;
                        changeData.Duration = (parentTask.EndDate?.AddDays(1) - parentTask.StartDate).Value.Days;
                    }

                    changeData.Percentage = (int)(subTaskList.Average(a => a.Percentage) ?? 0);
                    parentTask.Percentage = changeData.Percentage;

                    Update(changeData);
                    _context.Save();

                    subTaskList.ForEach(s => s.IsParentTask = false);
                }

                UpdateTaskStatus(subTaskList, todayDate);
                parentTask.IsParentTask = true;
                parentTask.Subtasks = subTaskList;
            });
        }

        private void UpdateParentTaskDates(StudyPlanTaskDto parentTask, List<StudyPlanTaskDto> subTaskList)
        {
            parentTask.ActualStartDate = subTaskList.Min(s => s.ActualStartDate);
            parentTask.ActualEndDate = subTaskList.Max(s => s.ActualEndDate);
            parentTask.StartDate = subTaskList.Min(s => s.StartDate);
            parentTask.EndDate = subTaskList.Max(s => s.EndDate);
            parentTask.EndDateDay = subTaskList.Max(s => s.EndDate);
            parentTask.DurationDay = (parentTask.EndDate?.AddDays(1) - parentTask.StartDate).Value.Days;
        }

        private void UpdateParentTaskStatus(StudyPlanTaskDto parentTask, DateTime todayDate)
        {
            if (todayDate < parentTask.StartDate && parentTask.ActualStartDate == null && parentTask.ActualEndDate == null)
                parentTask.Status = CtmsChartType.NotStarted.GetDescription();
            else if (parentTask.ActualStartDate != null && parentTask.ActualEndDate == null && parentTask.EndDate > parentTask.ActualStartDate)
                parentTask.Status = CtmsChartType.OnGoingDate.GetDescription();
            else if (parentTask.StartDate < todayDate && parentTask.EndDate > todayDate && parentTask.ActualStartDate == null)
                parentTask.Status = CtmsChartType.DueDate.GetDescription();
            else if (parentTask.ActualStartDate != null && parentTask.ActualEndDate != null && parentTask.EndDate >= parentTask.ActualEndDate)
                parentTask.Status = CtmsChartType.Completed.GetDescription();
            else if ((parentTask.EndDate < parentTask.ActualEndDate) || (parentTask.EndDate < todayDate && parentTask.ActualStartDate != null && parentTask.ActualEndDate == null) || (parentTask.EndDate < todayDate && parentTask.ActualStartDate == null && parentTask.ActualEndDate == null))
                parentTask.Status = CtmsChartType.DeviatedDate.GetDescription();
        }

        private void AddTaskResources(List<StudyPlanTaskDto> taskList)
        {
            taskList.ForEach(item =>
            {
                var resourceList = _context.StudyPlanResource.Include(x => x.ResourceType)
                    .Where(s => s.DeletedDate == null && s.StudyPlanTaskId == item.Id)
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
                    })
                    .ToList();

                item.TaskResource = resourceList;
            });
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
                    var data = _mapper.Map<StudyPlanTask>(taskData);
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
            var studyplan = _context.StudyPlan.FirstOrDefault(x => x.Id == taskmasterDto.StudyPlanId);
            if (studyplan == null) return "";

            bool isTaskWithinPlanDates = taskmasterDto.StartDate >= studyplan.StartDate &&
                                         taskmasterDto.StartDate <= studyplan.EndDate &&
                                         taskmasterDto.EndDate <= studyplan.EndDate &&
                                         taskmasterDto.EndDate >= studyplan.StartDate;

            if (!isTaskWithinPlanDates)
            {
                return "Plan Date Add between Plan Start and End Date";
            }

            if (taskmasterDto.ParentId > 0)
            {
                var parentTask = All.FirstOrDefault(x => x.Id == taskmasterDto.ParentId);
                if (parentTask != null)
                {
                    bool isTaskWithinParentDates = taskmasterDto.StartDate >= parentTask.StartDate &&
                                                   taskmasterDto.StartDate <= parentTask.EndDate &&
                                                   taskmasterDto.EndDate <= parentTask.EndDate &&
                                                   taskmasterDto.EndDate >= parentTask.StartDate;

                    if (!isTaskWithinParentDates)
                    {
                        return "Child Task Add between Parent Task Start and End Date";
                    }
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

        private void UpdateDependentTaskDate(int studyPlanTaskId)
        {
            var dependentTask = All.FirstOrDefault(x => x.Id == studyPlanTaskId);
            if (dependentTask == null) return;

            var mainTask = All.FirstOrDefault(x => x.Id == dependentTask.Id && x.DeletedDate == null);
            if (mainTask == null) return;

            var task = All.FirstOrDefault(x => x.Id == dependentTask.DependentTaskId);
            if (task == null) return;

            switch (dependentTask.ActivityType)
            {
                case ActivityType.FF:
                    mainTask.EndDate = WorkingDayHelper.AddBusinessDays(task.EndDate, dependentTask.OffSet);
                    break;
                case ActivityType.FS:
                    mainTask.StartDate = WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextWorkingDay(task.EndDate), dependentTask.OffSet);
                    break;
                case ActivityType.SF:
                    mainTask.EndDate = WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextSubstarctWorkingDay(task.StartDate), dependentTask.OffSet);
                    break;
                case ActivityType.SS:
                    mainTask.StartDate = WorkingDayHelper.AddBusinessDays(task.StartDate, dependentTask.OffSet);
                    break;
            }

            if (dependentTask.ActivityType == ActivityType.FF || dependentTask.ActivityType == ActivityType.SF)
            {
                mainTask.StartDate = WorkingDayHelper.SubtractBusinessDays(mainTask.EndDate, mainTask.Duration > 0 ? mainTask.Duration - 1 : 0);
            }
            else
            {
                mainTask.EndDate = WorkingDayHelper.AddBusinessDays(mainTask.StartDate, mainTask.Duration > 0 ? mainTask.Duration - 1 : 0);
            }

            Update(mainTask);
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
        public StudyPlanTask UpdateDependentTaskDate(StudyPlanTask studyPlanTask)
        {
            int projectId = _context.StudyPlan
                                    .Where(x => x.Id == studyPlanTask.StudyPlanId)
                                    .Select(s => s.ProjectId)
                                    .FirstOrDefault();
            var holidayList = _holidayMasterRepository.GetHolidayList(projectId);
            var weekendList = _weekEndMasterRepository.GetWorkingDayList(projectId);

            WorkingDayHelper.InitholidayDate(holidayList, weekendList);

            if (studyPlanTask.DependentTaskId <= 0) return null;

            var dependentTask = All.FirstOrDefault(x => x.Id == studyPlanTask.DependentTaskId);
            if (dependentTask == null) return null;

            switch (studyPlanTask.ActivityType)
            {
                case ActivityType.FF:
                    studyPlanTask.EndDate = WorkingDayHelper.AddBusinessDays(dependentTask.EndDate, studyPlanTask.OffSet);
                    break;
                case ActivityType.FS:
                    studyPlanTask.StartDate = studyPlanTask.isMileStone
                        ? WorkingDayHelper.AddBusinessDays(dependentTask.EndDate, studyPlanTask.OffSet)
                        : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextWorkingDay(dependentTask.EndDate), studyPlanTask.OffSet);
                    break;
                case ActivityType.SF:
                    studyPlanTask.EndDate = studyPlanTask.isMileStone
                        ? WorkingDayHelper.AddBusinessDays(dependentTask.StartDate, studyPlanTask.OffSet)
                        : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextSubstarctWorkingDay(dependentTask.StartDate), studyPlanTask.OffSet);
                    break;
                case ActivityType.SS:
                    studyPlanTask.StartDate = WorkingDayHelper.AddBusinessDays(dependentTask.StartDate, studyPlanTask.OffSet);
                    break;
            }

            if (studyPlanTask.ActivityType == ActivityType.FF || studyPlanTask.ActivityType == ActivityType.SF)
            {
                studyPlanTask.StartDate = WorkingDayHelper.SubtractBusinessDays(studyPlanTask.EndDate, studyPlanTask.Duration > 0 ? studyPlanTask.Duration - 1 : 0);
            }
            else
            {
                studyPlanTask.EndDate = WorkingDayHelper.AddBusinessDays(studyPlanTask.StartDate, studyPlanTask.Duration > 0 ? studyPlanTask.Duration - 1 : 0);
            }

            return studyPlanTask;
        }


        private string UpdateDependentTaskDate1(int studyPlanTaskId, ref List<StudyPlanTask> taskList)
        {
            int studyPlanId = taskList.Select(s => s.StudyPlanId).FirstOrDefault();
            int projectId = GetProjectId(studyPlanId);

            InitializeWorkingDayHelper(projectId);

            var mainTask = GetMainTask(studyPlanTaskId, taskList);
            if (mainTask == null) return "";

            var dependentTask = GetDependentTask(mainTask, taskList);
            if (dependentTask == null) return "";

            UpdateTaskDates(mainTask, dependentTask);
            Update(mainTask);

            return "";
        }

        private int GetProjectId(int studyPlanId)
        {
            return _context.StudyPlan
                           .Where(x => x.Id == studyPlanId)
                           .Select(s => s.ProjectId)
                           .FirstOrDefault();
        }

        private void InitializeWorkingDayHelper(int projectId)
        {
            var holidayList = _holidayMasterRepository.GetHolidayList(projectId);
            var weekendList = _weekEndMasterRepository.GetWorkingDayList(projectId);
            WorkingDayHelper.InitholidayDate(holidayList, weekendList);
        }

        private StudyPlanTask GetMainTask(int studyPlanTaskId, List<StudyPlanTask> taskList)
        {
            return taskList.Find(x => x.Id == studyPlanTaskId && x.DeletedDate == null);
        }

        private StudyPlanTask GetDependentTask(StudyPlanTask mainTask, List<StudyPlanTask> taskList)
        {
            return taskList.Find(x => x.Id == mainTask.DependentTaskId);
        }

        private void UpdateTaskDates(StudyPlanTask mainTask, StudyPlanTask dependentTask)
        {
            switch (mainTask.ActivityType)
            {
                case ActivityType.FF:
                    mainTask.EndDate = WorkingDayHelper.AddBusinessDays(dependentTask.EndDate, mainTask.OffSet);
                    mainTask.StartDate = WorkingDayHelper.SubtractBusinessDays(mainTask.EndDate, mainTask.Duration > 0 ? mainTask.Duration - 1 : 0);
                    break;
                case ActivityType.FS:
                    mainTask.StartDate = mainTask.isMileStone
                        ? WorkingDayHelper.AddBusinessDays(dependentTask.EndDate, mainTask.OffSet)
                        : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextWorkingDay(dependentTask.EndDate), mainTask.OffSet);
                    mainTask.EndDate = WorkingDayHelper.AddBusinessDays(mainTask.StartDate, mainTask.Duration > 0 ? mainTask.Duration - 1 : 0);
                    break;
                case ActivityType.SF:
                    mainTask.EndDate = mainTask.isMileStone
                        ? WorkingDayHelper.AddBusinessDays(dependentTask.StartDate, mainTask.OffSet)
                        : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextSubstarctWorkingDay(dependentTask.StartDate), mainTask.OffSet);
                    mainTask.StartDate = WorkingDayHelper.SubtractBusinessDays(mainTask.EndDate, mainTask.Duration > 0 ? mainTask.Duration - 1 : 0);
                    break;
                case ActivityType.SS:
                    mainTask.StartDate = WorkingDayHelper.AddBusinessDays(dependentTask.StartDate, mainTask.OffSet);
                    mainTask.EndDate = WorkingDayHelper.AddBusinessDays(mainTask.StartDate, mainTask.Duration > 0 ? mainTask.Duration - 1 : 0);
                    break;
            }
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
                if (TaskMaster != null)
                {
                    data.StartDate = TaskMaster.StartDate;
                    data.EndDate = TaskMaster.EndDate;
                    data.TaskTemplateId = TaskMaster.TaskTemplateId;

                    data.Id = 0;
                    data.ProjectId = ProjectId;
                    _context.StudyPlan.Add(data);
                    _context.Save();
                    result.StudyPlanId = data.Id;
                }
            }

            return result;
        }

        // Get document chart code start
        public StudyPlanTaskChartDto GetDocChart(int projectId, CtmsStudyTaskFilter filterType, int countryId, int siteId)
        {
            var result = new StudyPlanTaskChartDto();
            var studyIds = GetStudyIds(projectId, filterType, siteId);

            var studyPlanTasks = GetStudyPlanTasks(filterType, countryId, projectId, studyIds);
            result.All = studyPlanTasks.Count;

            var todayDate = DateTime.Now;

            foreach (var item in studyPlanTasks)
            {
                UpdateChartDto(item, todayDate, result);
            }

            return result;
        }

        private List<StudyPlanTask> GetStudyPlanTasks(CtmsStudyTaskFilter filterType, int countryId, int projectId, List<int> studyIds)
        {
            return All.Include(x => x.StudyPlan)
                .Where(x => x.StudyPlan.DeletedDate == null
                            && (filterType != CtmsStudyTaskFilter.Study
                                ? studyIds.Contains(x.StudyPlan.ProjectId)
                                : x.StudyPlan.ProjectId == projectId)
                            && x.DeletedDate == null
                            && (filterType == CtmsStudyTaskFilter.Country
                                ? countryId <= 0 ? x.IsCountry : x.CountryId == countryId
                                : filterType == CtmsStudyTaskFilter.All || !x.IsCountry))
                .ToList();
        }

        private void UpdateChartDto(StudyPlanTask item, DateTime todayDate, StudyPlanTaskChartDto result)
        {
            if (IsNotStarted(item, todayDate))
            {
                result.NotStartedDate += 1;
                return;
            }
            if (IsOnGoing(item, todayDate))
            {
                result.OnGoingDate += 1;
                return;
            }
            if (IsDue(item, todayDate))
            {
                result.DueDate += 1;
                return;
            }
            if (IsComplete(item))
            {
                result.Complete += 1;
                return;
            }
            if (IsDeviated(item, todayDate))
            {
                result.DeviatedDate += 1;
            }
        }

        private bool IsNotStarted(StudyPlanTask item, DateTime todayDate)
        {
            return todayDate < item.StartDate && item.ActualStartDate == null && item.ActualEndDate == null;
        }

        private bool IsOnGoing(StudyPlanTask item, DateTime todayDate)
        {
            return item.ActualStartDate != null && item.ActualEndDate == null
                   && item.EndDate > item.ActualStartDate && item.EndDate > todayDate;
        }

        private bool IsDue(StudyPlanTask item, DateTime todayDate)
        {
            return item.StartDate < todayDate && item.EndDate > todayDate && item.ActualStartDate == null;
        }

        private bool IsComplete(StudyPlanTask item)
        {
            return item.ActualStartDate != null && item.ActualEndDate != null && item.EndDate >= item.ActualEndDate;
        }

        private bool IsDeviated(StudyPlanTask item, DateTime todayDate)
        {
            return item.EndDate < item.ActualEndDate
                   || (item.EndDate < todayDate && item.ActualStartDate != null && item.ActualEndDate == null)
                   || (item.EndDate < todayDate && item.ActualStartDate == null && item.ActualEndDate == null);
        }

        // Document chat code end

        // Chart report code start
        public List<StudyPlanTaskChartReportDto> GetChartReport(int projectId, CtmsChartType? chartType, CtmsStudyTaskFilter filterType)
        {
            var studyIds = GetStudyIds(projectId, filterType, 0);
            var studyPlanTasks = GetStudyPlanTasks(filterType, projectId, studyIds);

            var todayDate = DateTime.Now;
            var filteredTasks = FilterTasksByChartType(studyPlanTasks, chartType, todayDate);

            return CreateReportDtos(filteredTasks, chartType);
        }

        private List<StudyPlanTask> GetStudyPlanTasks(CtmsStudyTaskFilter filterType, int projectId, List<int> studyIds)
        {
            return All.Include(x => x.StudyPlan)
                .Where(x => x.StudyPlan.DeletedDate == null
                            && (filterType != CtmsStudyTaskFilter.Study
                                ? studyIds.Contains(x.StudyPlan.ProjectId)
                                : x.StudyPlan.ProjectId == projectId)
                            && x.DeletedDate == null
                            && (filterType == CtmsStudyTaskFilter.All || x.IsCountry == (filterType == CtmsStudyTaskFilter.Country)))
                .ToList();
        }

        private List<StudyPlanTask> FilterTasksByChartType(List<StudyPlanTask> tasks, CtmsChartType? chartType, DateTime todayDate)
        {
            return tasks.Where(item => IsTaskMatchingChartType(item, chartType, todayDate)).ToList();
        }

        private bool IsTaskMatchingChartType(StudyPlanTask item, CtmsChartType? chartType, DateTime todayDate)
        {
            return (chartType == CtmsChartType.NotStarted && IsNotStarted(item, todayDate)) ||
                   (chartType == CtmsChartType.OnGoingDate && IsOnGoing(item, todayDate)) ||
                   (chartType == CtmsChartType.DueDate && IsDue(item, todayDate)) ||
                   (chartType == CtmsChartType.Completed && IsComplete(item)) ||
                   (chartType == CtmsChartType.DeviatedDate && IsDeviated(item, todayDate));
        }

        private List<StudyPlanTaskChartReportDto> CreateReportDtos(List<StudyPlanTask> tasks, CtmsChartType? chartType)
        {
            return tasks.Select(x => new StudyPlanTaskChartReportDto
            {
                Id = x.Id,
                Duration = x.Duration,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                TaskName = x.TaskName,
                NoOfDeviatedDay = chartType == CtmsChartType.DeviatedDate && x.ActualEndDate != null ? (x.ActualEndDate - x.EndDate).Value.Days : 0,
            }).ToList();
        }

        // Chart report code end

        // Resource Management search code start
        private bool IsValidProject(Data.Entities.Master.Project project)
        {
            return _projectRightRepository.All.Any(a => a.ProjectId == project.Id
                                                        && a.UserId == _jwtTokenAccesser.UserId
                                                        && a.RoleId == _jwtTokenAccesser.RoleId
                                                        && a.DeletedDate == null
                                                        && a.RollbackReason == null);
        }
        private void PopulateTaskResources(List<StudyPlanTaskDto> tasks)
        {
            foreach (var item in tasks)
            {
                var resourceList = _context.StudyPlanResource.Include(x => x.ResourceType)
                    .Where(s => s.DeletedDate == null && s.StudyPlanTaskId == item.Id)
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
                    })
                    .ToList();

                item.TaskResource = resourceList;
            }
        }

        private void ApplyFilters(ref List<StudyPlanTaskDto> tasks, ResourceMgmtFilterDto search)
        {
            if (search.ResourceId.HasValue)
            {
                var resourceType = GetResourceTypeDescription(search.ResourceId.Value);
                tasks = tasks.Where(s => s.TaskResource.Any(x => x.ResourceType == resourceType)).ToList();
            }

            if (search.ResourceSubId.HasValue)
            {
                var resourceSubType = GetResourceType(search.ResourceSubId);
                tasks = tasks.Where(s => s.TaskResource.Any(x => x.ResourceSubType == resourceSubType)).ToList();
            }

            if (search.RoleId.HasValue)
            {
                var roleName = _context.SecurityRole.Where(s => s.Id == search.RoleId).Select(x => x.RoleName).FirstOrDefault();
                tasks = tasks.Where(s => s.TaskResource.Any(x => x.Role == roleName)).ToList();
            }

            if (search.UserId.HasValue)
            {
                var userName = _context.Users.Where(s => s.Id == search.UserId).Select(x => x.UserName).FirstOrDefault();
                tasks = tasks.Where(s => s.TaskResource.Any(x => x.User == userName)).ToList();
            }

            if (search.DesignationId.HasValue)
            {
                var designationName = _context.Designation.Where(s => s.Id == search.DesignationId).Select(x => x.NameOFDesignation).FirstOrDefault();
                tasks = tasks.Where(s => s.TaskResource.Any(x => x.Designation == designationName)).ToList();
            }

            if (search.ResourceNotAdded == true)
            {
                tasks = tasks.Where(s => !s.TaskResource.Any()).ToList();
            }

            if (search.ResourceAdded == true)
            {
                tasks = tasks.Where(s => s.TaskResource.Any()).ToList();
            }
        }

        private string GetResourceTypeDescription(int resourceId)
        {
            return resourceId == (int)ResourceTypeEnum.Manpower
                ? ResourceTypeEnum.Manpower.GetDescription()
                : ResourceTypeEnum.Material.GetDescription();
        }


        private List<int> GetProjectIds(ResourceMgmtFilterDto search)
        {
            if (search.countryId <= 0)
            {
                return new List<int>();
            }

            var projectIds = _projectRepository.All
                .Include(x => x.ManageSite)
                .Where(x => x.ParentProjectId == search.siteId
                            && IsValidProject(x)
                            && x.ManageSite.City.State.CountryId == search.countryId
                            && x.DeletedDate == null)
                .Select(s => s.Id)
                .ToList();

            if (!projectIds.Any())
            {
                projectIds = _projectRepository.All
                    .Include(x => x.ManageSite)
                    .Where(x => IsValidProject(x)
                                && x.ManageSite.City.State.CountryId == search.countryId
                                && x.Id == search.siteId
                                && x.DeletedDate == null)
                    .Select(s => s.Id)
                    .ToList();
            }

            return projectIds;
        }

        private List<StudyPlanTaskDto> GetStudyPlanTasks(List<StudyPlan> studyPlans)
        {
            return studyPlans.SelectMany(item => All
                .Where(x => x.DeletedDate == null && x.StudyPlanId == item.Id)
                .OrderBy(x => x.TaskOrder)
                .ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider)
                .ToList())
                .ToList();
        }

        public List<StudyPlanTaskDto> ResourceMgmtSearch(ResourceMgmtFilterDto search)
        {
            var result = new List<StudyPlanTaskDto>();

            var projectIds = GetProjectIds(search);

            if (projectIds.Any())
            {
                var studyPlans = _context.StudyPlan
                    .Where(x => projectIds.Contains(x.ProjectId) && x.DeletedDate == null)
                    .OrderByDescending(x => x.Id)
                    .ToList();

                result = GetStudyPlanTasks(studyPlans);
            }
            else if (search.countryId <= 0)
            {
                var studyPlan = _context.StudyPlan
                    .Where(x => x.ProjectId == search.siteId && x.DeletedDate == null)
                    .OrderByDescending(x => x.Id)
                    .LastOrDefault();

                if (studyPlan != null)
                {
                    result = GetStudyPlanTasks(new List<StudyPlan> { studyPlan });
                }
            }

            PopulateTaskResources(result);
            ApplyFilters(ref result, search);

            return result;
        }

        // Resource Management search code end
        public string GetResourceType(int? ResourceSubId)
        {
            if (ResourceSubId == (int)SubResourceType.Permanent)
                return SubResourceType.Permanent.GetDescription();
            else if (ResourceSubId == (int)SubResourceType.Contract)
                return SubResourceType.Contract.GetDescription();
            else if (ResourceSubId == (int)SubResourceType.Consumable)
                return SubResourceType.Consumable.GetDescription();
            else
                return SubResourceType.NonConsumable.GetDescription();
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

        // Get Budget Planer Code Start

        public List<StudyPlanTaskDto> GetBudgetPlaner(bool isDeleted, int studyId, int siteId, int countryId, CtmsStudyTaskFilter filterType)
        {
            var studyIds = GetStudyIds(studyId, filterType, siteId);

            var studyPlans = GetStudyPlans(studyIds, studyId, filterType);

            var result = GetStudyPlanTasks(studyPlans, filterType, countryId);

            PopulateTaskResourcesForBudgetPlaner(result);

            return result.Where(s => s.TaskResource.Count != 0).ToList();
        }
        private List<StudyPlan> GetStudyPlans(List<int> studyIds, int studyId, CtmsStudyTaskFilter filterType)
        {
            return _context.StudyPlan
                .Include(s => s.Currency)
                .Where(x => (filterType != CtmsStudyTaskFilter.Study ? studyIds.Contains(x.ProjectId) : x.ProjectId == studyId)
                            && x.DeletedDate == null)
                .OrderByDescending(x => x.Id)
                .ToList();
        }

        private List<StudyPlanTaskDto> GetStudyPlanTasks(List<StudyPlan> studyPlans, CtmsStudyTaskFilter filterType, int countryId)
        {
            return studyPlans.SelectMany(item =>
            {
                var tasklist = All
                    .Where(x => x.DeletedDate == null && x.StudyPlanId == item.Id
                                && (filterType == CtmsStudyTaskFilter.Country ? countryId <= 0 ? x.IsCountry : x.CountryId == countryId : filterType == CtmsStudyTaskFilter.All || !x.IsCountry))
                    .OrderBy(x => x.TaskOrder)
                    .ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider)
                    .ToList();

                tasklist.ForEach(task =>
                {
                    task.GlobalCurrencySymbol = item.Currency != null ? item.Currency.CurrencySymbol : "$";
                });

                return tasklist;
            }).ToList();
        }

        private void PopulateTaskResourcesForBudgetPlaner(List<StudyPlanTaskDto> tasks)
        {
            foreach (var item in tasks)
            {
                var resourcelist = _context.StudyPlanResource
                    .Include(x => x.ResourceType)
                    .Include(r => r.StudyPlanTask)
                    .Where(s => s.DeletedDate == null && s.StudyPlanTaskId == item.Id)
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
                        CurrencyType = $"{x.ResourceType.Currency.CurrencySymbol} - {x.ResourceType.Currency.CurrencyName}",
                        GlobalCurrencySymbol = x.StudyPlanTask.StudyPlan.Currency.CurrencySymbol,
                        LocalCurrencySymbol = x.ResourceType.Currency.CurrencySymbol,
                        CreatedDate = x.CreatedDate,
                        CreatedByUser = x.CreatedByUser.UserName,
                        LocalCurrencyRate = _context.CurrencyRate
                            .Where(s => s.StudyPlanId == x.StudyPlanTask.StudyPlanId && s.CurrencyId == x.ResourceType.CurrencyId && s.DeletedBy == null)
                            .Select(t => t.LocalCurrencyRate)
                            .FirstOrDefault()
                    })
                    .ToList();

                item.TaskResource = resourcelist;
            }
        }


        // Get Budget Planer Code End      


        public List<StudyPlanTaskDto> GetSubTaskList(int parentTaskId)
        {
            var taskList = All.Where(x => x.DeletedDate == null
            && (x.DependentTaskId == parentTaskId || x.ParentId == parentTaskId))
                .ProjectTo<StudyPlanTaskDto>(_mapper.ConfigurationProvider).ToList();

            return taskList;
        }

        public ParentTaskDate GetChildStartEndDate(int parentTaskId)
        {
            var taskDateList = All.Where(x => x.DeletedDate == null
            && x.ParentId == parentTaskId);

            if (taskDateList.Any())
            {
                var minDate = taskDateList.Min(s => s.ActualStartDate);
                var maxDate = taskDateList.Max(s => s.ActualEndDate);

                var objDate = new ParentTaskDate
                {
                    StartDate = minDate,
                    EndDate = maxDate
                };

                return objDate;
            }

            return null;
        }

        public string AddSiteTask(StudyPlantaskParameterDto taskmasterDto)
        {
            var project = _context.StudyPlan.First(x => x.Id == taskmasterDto.StudyPlanId);
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            var ids = _projectRepository.All.Where(x =>
                     (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                     && x.DeletedDate == null && x.ParentProjectId == project.ProjectId
                     && projectList.Any(c => c == x.Id)).Select(s => s.Id).ToList();

            var studyPlans = _context.StudyPlan.Where(x => x.DeletedDate == null && ids.Contains(x.ProjectId)).ToList();

            if (studyPlans.Count <= 0)
            {
                return "Site not found in this project";
            }

            foreach (var plan in studyPlans)
            {
                taskmasterDto.Id = 0;
                var tastMaster = _mapper.Map<StudyPlanTask>(taskmasterDto);
                tastMaster.IsCountry = taskmasterDto.RefrenceType == RefrenceType.Country;
                tastMaster.ProjectId = plan.ProjectId;
                tastMaster.StudyPlanId = plan.Id;
                tastMaster.TaskOrder = UpdateTaskOrder(taskmasterDto);
                var data = UpdateDependentTaskDate(tastMaster);
                if (data != null)
                {
                    tastMaster.StartDate = data.StartDate;
                    tastMaster.EndDate = data.EndDate;
                    tastMaster.Percentage = data.Percentage;
                }
                if (taskmasterDto.TaskDocumentFileModel?.Base64?.Length > 0 && taskmasterDto.TaskDocumentFileModel?.Base64 != null)
                {
                    var backupModel = new FileModel()
                    {
                        Base64 = taskmasterDto.TaskDocumentFileModel.Base64,
                        Extension = taskmasterDto.TaskDocumentFileModel.Extension
                    };
                    tastMaster.TaskDocumentFilePath = DocumentService.SaveUploadDocument(backupModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms, "StudyPlanTask");
                }
                Add(tastMaster);
                UpdateTaskOrderSequence(taskmasterDto.Id);
            }

            _context.Save();
            return "";
        }


        public string AddCountryTask(StudyPlantaskParameterDto taskmasterDto)
        {
            var project = _context.StudyPlan.First(x => x.Id == taskmasterDto.StudyPlanId);
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            var sites = _projectRepository.All.Where(x =>
                     (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                     && x.DeletedDate == null && x.ParentProjectId == project.ProjectId
                     && projectList.Any(c => c == x.Id))
                     .Include(i => i.ManageSite.City.State)
                     .GroupBy(g => g.ManageSite.City.State.CountryId)
                     .Select(s => s.First(x => x.ManageSite.City.State.CountryId == s.Key)).ToList();

            if (sites.Count <= 0)
            {
                return "Site not found in this project";
            }

            foreach (var plan in sites)
            {
                taskmasterDto.Id = 0;
                var tastMaster = _mapper.Map<StudyPlanTask>(taskmasterDto);
                tastMaster.IsCountry = taskmasterDto.RefrenceType == RefrenceType.Country;
                tastMaster.CountryId = plan.ManageSite.City.State.CountryId;
                tastMaster.ProjectId = plan.Id;
                tastMaster.StudyPlanId = taskmasterDto.StudyPlanId;
                tastMaster.TaskOrder = UpdateTaskOrder(taskmasterDto);
                var data = UpdateDependentTaskDate(tastMaster);
                if (data != null)
                {
                    tastMaster.StartDate = data.StartDate;
                    tastMaster.EndDate = data.EndDate;
                    tastMaster.Percentage = data.Percentage;
                }
                if (taskmasterDto.TaskDocumentFileModel?.Base64?.Length > 0 && taskmasterDto.TaskDocumentFileModel?.Base64 != null)
                {
                    var backupModel = new FileModel()
                    {
                        Base64 = taskmasterDto.TaskDocumentFileModel.Base64,
                        Extension = taskmasterDto.TaskDocumentFileModel.Extension
                    };
                    tastMaster.TaskDocumentFilePath = DocumentService.SaveUploadDocument(backupModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms, "StudyPlanTask");
                }
                Add(tastMaster);
                UpdateTaskOrderSequence(taskmasterDto.Id);
            }

            _context.Save();
            return "";
        }


        public List<DropDownDto> GetCountryDropDown(int parentProjectId)
        {
            var studyPlan = _context.StudyPlan.FirstOrDefault(x => x.DeletedDate == null && x.ProjectId == parentProjectId);

            var countrylist = All.Include(i => i.StudyPlan).Where(x => x.DeletedDate == null && x.CountryId != null && x.StudyPlanId == studyPlan.Id).Include(i => i.Country).GroupBy(g => g.CountryId)
                .Select(c => new DropDownDto { Id = c.Key.Value, Value = c.First().Country.CountryName }).OrderBy(o => o.Value).ToList();

            return countrylist;
        }

        public List<DropDownDto> GetSiteDropDown(int parentProjectId)
        {
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            var ids = _projectRepository.All.Where(x =>
                     (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                     && x.DeletedDate == null && x.ParentProjectId == parentProjectId
                     && projectList.Any(c => c == x.Id)).Select(s => s.Id).ToList();


            return All.Where(x => x.DeletedDate == null && ids.Contains(x.StudyPlan.ProjectId)).Include(i => i.StudyPlan.Project).GroupBy(g => g.StudyPlan.ProjectId)
                .Select(c => new DropDownDto { Id = c.Key, Value = c.First().StudyPlan.Project.ManageSite.SiteName }).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetBudgetCountryDropDown(int parentProjectId)
        {
            var studyPlan = _context.StudyPlan.FirstOrDefault(x => x.DeletedDate == null && x.ProjectId == parentProjectId);


            var countrylist = _context.StudyPlanResource.Include(x => x.ResourceType).Include(r => r.StudyPlanTask).Where(s => s.DeletedDate == null
           && s.StudyPlanTask.CountryId != null && s.StudyPlanTask.StudyPlanId == studyPlan.Id).Include(i => i.StudyPlanTask.Country).GroupBy(g => g.StudyPlanTask.CountryId)
                .Select(c => new DropDownDto { Id = c.Key.Value, Value = c.First().StudyPlanTask.Country.CountryName }).OrderBy(o => o.Value).ToList();

            return countrylist;
        }

        public List<DropDownDto> GetBudgetSiteDropDown(int parentProjectId)
        {
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            var ids = _projectRepository.All.Where(x =>
                     (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                     && x.DeletedDate == null && x.ParentProjectId == parentProjectId
                     && projectList.Any(c => c == x.Id)).Select(s => s.Id).ToList();


            var siteList = _context.StudyPlanResource.Include(x => x.ResourceType).Include(r => r.StudyPlanTask).Where(s => s.DeletedDate == null
            && ids.Contains(s.StudyPlanTask.StudyPlan.ProjectId)).Include(i => i.StudyPlanTask.StudyPlan.Project).GroupBy(g => g.StudyPlanTask.StudyPlan.ProjectId)
                .Select(c => new DropDownDto { Id = c.Key, Value = c.First().StudyPlanTask.StudyPlan.Project.ManageSite.SiteName ?? c.First().StudyPlanTask.StudyPlan.Project.ProjectCode }).OrderBy(o => o.Value).ToList();

            return siteList;
        }
        public List<DashboardDto> GetCtmsMyTaskList(int ProjectId)
        {
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            var ids = _projectRepository.All.Where(x =>
                     (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                     && x.DeletedDate == null && x.ParentProjectId == ProjectId
                     && projectList.Any(c => c == x.Id)).Select(s => s.Id).ToList();
            ids.Add(ProjectId);

            var studyPlans = _context.StudyPlan.Where(x => x.DeletedDate == null && ids.Contains(x.ProjectId)).ToList();

            var listDashboardMyTasks = new List<DashboardDto>();

            foreach (var plan in studyPlans)
            {
                var TaskList = _context.StudyPlanResource.Where(x => x.DeletedDate == null && x.StudyPlanTask.StudyPlanId == plan.Id
                && x.ResourceType.UserId == _jwtTokenAccesser.UserId && x.ResourceType.RoleId == _jwtTokenAccesser.RoleId
                && x.ResourceType.ResourceTypes == ResourceTypeEnum.Manpower && x.StudyPlanTask.ActualEndDate == null).Include(i => i.StudyPlanTask)
                 .Select(s => new DashboardDto
                 {
                     Id = s.StudyPlanTask.StudyPlanId,
                     TaskInformation = s.StudyPlanTask.TaskName,
                     ExtraData = s.StudyPlanTask.StudyPlan.IsPlanApproval,
                     CreatedDate = s.CreatedDate,
                     CreatedByUser = _context.Users.Where(x => x.Id == s.CreatedBy).FirstOrDefault().UserName,
                     DueDate = s.StudyPlanTask.ActualStartDate,
                     Module = "CTMS",
                     ControlType = DashboardMyTaskType.CTMSMyTask,
                     ActivityId = s.StudyPlanTask.StudyPlan.ProjectId
                 }).OrderByDescending(x => x.CreatedDate).ToList();

                listDashboardMyTasks.AddRange(TaskList);
            }

            return listDashboardMyTasks;
        }
    }
}

