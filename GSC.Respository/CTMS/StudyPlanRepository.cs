using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class StudyPlanRepository : GenericRespository<StudyPlan>, IStudyPlanRepository
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IHolidayMasterRepository _holidayMasterRepository;
        private readonly IWeekEndMasterRepository _weekEndMasterRepository;
        private readonly IStudyPlanTaskRepository _studyPlanTaskRepository;
        private readonly IProjectRightRepository _projectRightRepository;

        public StudyPlanRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, IHolidayMasterRepository holidayMasterRepository, IWeekEndMasterRepository weekEndMasterRepository, IStudyPlanTaskRepository studyPlanTaskRepository, IProjectRightRepository projectRightRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _holidayMasterRepository = holidayMasterRepository;
            _weekEndMasterRepository = weekEndMasterRepository;
            _studyPlanTaskRepository = studyPlanTaskRepository;
            _projectRightRepository = projectRightRepository;
        }

        public List<StudyPlanGridDto> GetStudyplanList(bool isDeleted)
        {
            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var projectsctms = _context.ProjectSettings.Where(x => x.IsCtms == true && x.DeletedDate == null && projectList.Contains(x.ProjectId)).Select(x => x.ProjectId).ToList();
            var ctmsProjectList = _context.Project.Where(x =>(x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)&& x.ProjectCode != null && projectsctms.Any(c => c == x.Id)).ToList();
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.Project.ParentProjectId == null && ctmsProjectList.Select(c=>c.Id).Contains(x.ProjectId)).OrderByDescending(x => x.Id).
                   ProjectTo<StudyPlanGridDto>(_mapper.ConfigurationProvider).ToList();
        }

        public string ImportTaskMasterData(StudyPlan studyplan)
        {

            var holidaylist = _holidayMasterRepository.GetHolidayList(studyplan.ProjectId);
            var weekendlist = _weekEndMasterRepository.GetWorkingDayList(studyplan.ProjectId);
            WorkingDayHelper.InitholidayDate(holidaylist, weekendlist);

            var ParentProject = _context.Project.Where(x => x.Id == studyplan.ProjectId).FirstOrDefault().ParentProjectId;

            var tasklist = _context.RefrenceTypes.Include(d => d.TaskMaster).Where(x => x.DeletedDate == null && x.TaskMaster.TaskTemplateId == studyplan.TaskTemplateId
            && (ParentProject == null ? x.RefrenceType == RefrenceType.Country || x.RefrenceType == RefrenceType.Study
            : x.RefrenceType == RefrenceType.Country || x.RefrenceType == RefrenceType.Sites))
                .Select(t => new StudyPlanTask
                {
                    StudyPlanId = studyplan.Id,
                    TaskId = t.TaskMaster.Id,
                    TaskName = t.TaskMaster.TaskName,
                    ParentId = t.TaskMaster.ParentId,
                    isMileStone = t.TaskMaster.IsMileStone,
                    TaskOrder = t.TaskMaster.TaskOrder,
                    Duration = t.TaskMaster.Duration,
                    StartDate = studyplan.StartDate,
                    EndDate = WorkingDayHelper.AddBusinessDays(studyplan.StartDate, t.TaskMaster.Duration > 0 ? t.TaskMaster.Duration - 1 : 0),
                    DependentTaskId = t.TaskMaster.DependentTaskId,
                    ActivityType = t.TaskMaster.ActivityType,
                    OffSet = t.TaskMaster.OffSet,
                    RefrenceType = t.RefrenceType
                }).ToList();

            tasklist.ForEach(t =>
            {
                var data = UpdateDependentTaskDate(t, ref tasklist);
                if (data != null)
                {
                    t.StartDate = data.StartDate;
                    t.EndDate = data.EndDate;
                    t.Parent = t;
                    t.DependentTask = tasklist.FirstOrDefault(d => d.TaskId == t.DependentTaskId);
                    t.DependentTaskId = null;
                }

                if (t.ParentId > 0)
                {
                    t.Parent = tasklist.FirstOrDefault(x => x.TaskId == t.ParentId);
                    t.ParentId = null;
                }
            });

            _context.StudyPlanTask.AddRange(tasklist);
            _context.Save();


            return "";
        }

        public void PlanUpdate(int ProjectId)
        {
            var projectIds = _context.Project.Where(x => x.Id == ProjectId || x.ParentProjectId == ProjectId).Select(t => t.Id).ToList();

            var studyPlanList = _context.StudyPlan.Where(x => projectIds.Contains(x.ProjectId) && x.DeletedDate == null).ToList();

            var studyPlanTaskList = _context.StudyPlanTask.Where(x => x.DeletedDate == null && studyPlanList.Select(x => x.Id).Contains(x.StudyPlanId)).ToList();

            studyPlanList.ForEach(i =>
            {
                if(studyPlanTaskList.Count() > 0)
                { 
                i.StartDate = studyPlanTaskList.Min(x => x.StartDate);
                i.EndDate = studyPlanTaskList.Max(x => x.EndDate);
                }
            });

            _context.StudyPlan.UpdateRange(studyPlanList);
            _context.Save();
        }

        private StudyPlanTask UpdateDependentTaskDate(StudyPlanTask maintask, ref List<StudyPlanTask> tasklist)
        {
            if (maintask.DependentTaskId > 0)
            {
                if (maintask.ActivityType == ActivityType.FF)
                {
                    var task = tasklist.Where(x => x.TaskId == maintask.DependentTaskId).FirstOrDefault();
                    if (task != null)
                    {
                        maintask.EndDate = WorkingDayHelper.AddBusinessDays(task.EndDate, maintask.OffSet);
                        maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                    }
                }
                else if (maintask.ActivityType == ActivityType.FS)
                {
                    var task = tasklist.Where(x => x.TaskId == maintask.DependentTaskId).FirstOrDefault();
                    if (task != null)
                    {
                        maintask.StartDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.EndDate, maintask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextWorkingDay(task.EndDate), maintask.OffSet);
                        maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                    }
                }
                else if (maintask.ActivityType == ActivityType.SF)
                {
                    var task = tasklist.Where(x => x.TaskId == maintask.DependentTaskId).FirstOrDefault();
                    if (task != null)
                    {
                        maintask.EndDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.StartDate, maintask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextSubstarctWorkingDay(task.StartDate), maintask.OffSet);
                        maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                    }
                }
                else if (maintask.ActivityType == ActivityType.SS)
                {
                    var task = tasklist.Where(x => x.TaskId == maintask.DependentTaskId).FirstOrDefault();
                    if (task != null)
                    {
                        maintask.StartDate = WorkingDayHelper.AddBusinessDays(task.StartDate, maintask.OffSet);
                        maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                    }
                }
                return maintask;
            }
            return null;
        }

        public string ValidateTask(StudyPlanTask taskmasterDto, List<StudyPlanTask> tasklist, StudyPlan studyplan)
        {
            if (taskmasterDto.StartDate >= studyplan.StartDate && taskmasterDto.StartDate <= studyplan.EndDate
                && taskmasterDto.EndDate <= studyplan.EndDate && taskmasterDto.EndDate >= studyplan.StartDate)
            {
                if (taskmasterDto.ParentId > 0)
                {
                    var parentdate = tasklist.Where(x => x.TaskId == taskmasterDto.ParentId).FirstOrDefault();
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

        public string Duplicate(StudyPlan objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId && x.DeletedDate == null))
                return "Duplicate Study";

            return "";
        }

        public string ImportTaskMasterDataFromTaskMaster(StudyPlan studyplan, int id)
        {

            var holidaylist = _holidayMasterRepository.GetHolidayList(studyplan.ProjectId);
            var weekendlist = _weekEndMasterRepository.GetWorkingDayList(studyplan.ProjectId);
            WorkingDayHelper.InitholidayDate(holidaylist, weekendlist);

            var ParentProject = _context.Project.Where(x => x.Id == studyplan.ProjectId).FirstOrDefault().ParentProjectId;

            var tasklist = _context.RefrenceTypes.Include(x => x.TaskMaster).Where(x => x.TaskMaster.DeletedDate == null && x.TaskMaster.Id == id
            && (ParentProject == null ? x.RefrenceType == RefrenceType.Study
            : x.RefrenceType == RefrenceType.Country || x.RefrenceType == RefrenceType.Sites) && x.DeletedBy == null)
                .Select(t => new StudyPlanTask
                {
                    StudyPlanId = studyplan.Id,
                    TaskId = t.TaskMaster.Id,
                    TaskName = t.TaskMaster.TaskName,
                    ParentId = t.TaskMaster.ParentId,
                    isMileStone = t.TaskMaster.IsMileStone,
                    TaskOrder = t.TaskMaster.TaskOrder,
                    Duration = t.TaskMaster.Duration,
                    StartDate = studyplan.StartDate,
                    EndDate = WorkingDayHelper.AddBusinessDays(studyplan.StartDate, t.TaskMaster.Duration > 0 ? t.TaskMaster.Duration - 1 : 0),
                    DependentTaskId = t.TaskMaster.DependentTaskId,
                    ActivityType = t.TaskMaster.ActivityType,
                    OffSet = t.TaskMaster.OffSet,
                    RefrenceType = t.RefrenceType
                }).ToList();


            //var tasklist = _context.TaskMaster.Where(x => x.DeletedDate == null && x.Id == id
            //&& (ParentProject == null ? x.RefrenceType == RefrenceType.Country || x.RefrenceType == RefrenceType.Study
            //: x.RefrenceType == RefrenceType.Country || x.RefrenceType == RefrenceType.Sites))
            //    .Select(t => new StudyPlanTask
            //    {
            //        StudyPlanId = studyplan.Id,
            //        TaskId = t.Id,
            //        TaskName = t.TaskName,
            //        ParentId = t.ParentId,
            //        isMileStone = t.IsMileStone,
            //        TaskOrder = t.TaskOrder,
            //        Duration = t.Duration,
            //        StartDate = studyplan.StartDate,
            //        EndDate = WorkingDayHelper.AddBusinessDays(studyplan.StartDate, t.Duration > 0 ? t.Duration - 1 : 0),
            //        DependentTaskId = t.DependentTaskId,
            //        ActivityType = t.ActivityType,
            //        OffSet = t.OffSet,
            //        RefrenceType = t.RefrenceType
            //    }).ToList();

            tasklist.ForEach(t =>
            {
                var tasklist1 = _context.RefrenceTypes.Include(x => x.TaskMaster).Where(x => x.TaskMaster.DeletedDate == null && x.TaskMaster.Id == t.DependentTaskId
            && (ParentProject == null ? x.RefrenceType == RefrenceType.Study
            : x.RefrenceType == RefrenceType.Country || x.RefrenceType == RefrenceType.Sites) && x.DeletedBy == null)
                .Select(t => new StudyPlanTask
                {
                    StudyPlanId = studyplan.Id,
                    TaskId = t.TaskMaster.Id,
                    TaskName = t.TaskMaster.TaskName,
                    ParentId = t.TaskMaster.ParentId,
                    isMileStone = t.TaskMaster.IsMileStone,
                    TaskOrder = t.TaskMaster.TaskOrder,
                    Duration = t.TaskMaster.Duration,
                    StartDate = studyplan.StartDate,
                    EndDate = WorkingDayHelper.AddBusinessDays(studyplan.StartDate, t.TaskMaster.Duration > 0 ? t.TaskMaster.Duration - 1 : 0),
                    DependentTaskId = t.TaskMaster.DependentTaskId,
                    ActivityType = t.TaskMaster.ActivityType,
                    OffSet = t.TaskMaster.OffSet,
                    RefrenceType = t.RefrenceType
                }).ToList();

                var data = UpdateDependentTaskDate(t, ref tasklist1);
                if (data != null)
                {
                    t.StartDate = data.StartDate;
                    t.EndDate = data.EndDate;
                    t.Parent = t;
                    t.DependentTask = tasklist1.FirstOrDefault(d => d.TaskId == t.DependentTaskId);
                    t.DependentTaskId = null;
                }

                if (t.ParentId > 0)
                {
                    t.Parent = tasklist1.FirstOrDefault(x => x.TaskId == t.ParentId);
                    t.ParentId = null;
                }
            });

            _context.StudyPlanTask.AddRange(tasklist);
            _context.Save();

            return "";
        }
    }
}
