using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public StudyPlanRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, IHolidayMasterRepository holidayMasterRepository, IWeekEndMasterRepository weekEndMasterRepository, IStudyPlanTaskRepository studyPlanTaskRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _holidayMasterRepository = holidayMasterRepository;
            _weekEndMasterRepository = weekEndMasterRepository;
            _studyPlanTaskRepository = studyPlanTaskRepository;
        }

        public List<StudyPlanGridDto> GetStudyplanList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).
                   ProjectTo<StudyPlanGridDto>(_mapper.ConfigurationProvider).ToList();

        }

        public string ImportTaskMasterData(StudyPlan studyplan)
        {
            var holidaylist = _holidayMasterRepository.GetHolidayList(studyplan.ProjectId);
            var weekendlist = _weekEndMasterRepository.GetworkingDayList(studyplan.ProjectId);
            WorkingDayHelper.InitholidayDate(holidaylist, weekendlist);

            var tasklist = _context.TaskMaster.Where(x => x.TaskTemplateId == studyplan.TaskTemplateId)
                .Select(t => new StudyPlanTask
                {
                    StudyPlanId = studyplan.Id,
                    TaskId = t.Id,
                    TaskName = t.TaskName,
                    ParentId = t.ParentId,
                    isMileStone = t.IsMileStone,
                    TaskOrder = t.TaskOrder,
                    Duration = t.Duration,
                    StartDate = studyplan.StartDate,
                    EndDate = WorkingDayHelper.AddBusinessDays(studyplan.StartDate, t.Duration > 0 ? t.Duration - 1 : 0),
                    DependentTaskId = t.DependentTaskId,
                    ActivityType = t.ActivityType,
                    OffSet = t.OffSet,
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
                    // t.DependentTaskId = data.DependentTaskId;
                }

                if (t.ParentId > 0)
                {
                    t.Parent = tasklist.FirstOrDefault(x => x.TaskId == t.ParentId);
                    t.ParentId = null;
                }
            });

            foreach (var item in tasklist)
            {
                var validate = ValidateTask(item, tasklist);
                if (!string.IsNullOrEmpty(validate))
                    return validate;
            }
            //foreach (var item in tasklist)
            //{
            //    if (item.RefrenceType == RefrenceType.Sites)
            //        _studyPlanTaskRepository.Add(item);
                   
            //}
            _context.StudyPlanTask.AddRange(tasklist);
            _context.Save();


            //var studyplantasklist = _context.StudyPlanTask.Where(x => x.StudyPlanId == studyplan.Id).ToList();
            //studyplantasklist.ForEach(t =>
            //{
            //    t.ParentId = t.ParentId == 0 ? 0 : studyplantasklist.Where(x => x.TaskId == t.ParentId && t.StudyPlanId == studyplan.Id).SingleOrDefault().Id;
            //    t.DependentTaskId = t.DependentTaskId == null ? 0 : studyplantasklist.Where(x => x.TaskId == t.DependentTaskId && t.StudyPlanId == studyplan.Id).SingleOrDefault().Id;
            //    t.TaskId = 0;
            //});
            //_context.StudyPlanTask.UpdateRange(studyplantasklist);
            //_context.Save();
            return "";
        }

        private StudyPlanTask UpdateDependentTaskDate(StudyPlanTask maintask, ref List<StudyPlanTask> tasklist)
        {
            if (maintask.DependentTaskId > 0)
            {
                //var dependenttask = All.Where(x => x.Id == StudyPlanTask.Id).SingleOrDefault();            
                //if (maintask.Id > 0)
                //    maintask = All.Where(x => x.Id == maintask.Id && x.DeletedDate == null).SingleOrDefault();

                if (maintask.ActivityType == ActivityType.FF)
                {
                    var task = tasklist.Where(x => x.TaskId == maintask.DependentTaskId).FirstOrDefault();
                    maintask.EndDate = WorkingDayHelper.AddBusinessDays(task.EndDate, maintask.OffSet);
                    maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                }
                else if (maintask.ActivityType == ActivityType.FS)
                {
                    var task = tasklist.Where(x => x.TaskId == maintask.DependentTaskId).FirstOrDefault();
                    maintask.StartDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.EndDate, maintask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextWorkingDay(task.EndDate), maintask.OffSet);
                    maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                }
                else if (maintask.ActivityType == ActivityType.SF)
                {
                    var task = tasklist.Where(x => x.TaskId == maintask.DependentTaskId).FirstOrDefault();
                    maintask.EndDate = maintask.isMileStone ? WorkingDayHelper.AddBusinessDays(task.StartDate, maintask.OffSet) : WorkingDayHelper.AddBusinessDays(WorkingDayHelper.GetNextSubstarctWorkingDay(task.StartDate), maintask.OffSet);
                    maintask.StartDate = WorkingDayHelper.SubtractBusinessDays(maintask.EndDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                }
                else if (maintask.ActivityType == ActivityType.SS)
                {
                    var task = tasklist.Where(x => x.TaskId == maintask.DependentTaskId).FirstOrDefault();
                    maintask.StartDate = WorkingDayHelper.AddBusinessDays(task.StartDate, maintask.OffSet);
                    maintask.EndDate = WorkingDayHelper.AddBusinessDays(maintask.StartDate, maintask.Duration > 0 ? maintask.Duration - 1 : 0);
                }
                return maintask;
            }
            return null;
        }

        private string ValidateTask(StudyPlanTask taskmasterDto, List<StudyPlanTask> tasklist)
        {
            var studyplan = _context.StudyPlan.Where(x => x.Id == taskmasterDto.StudyPlanId).FirstOrDefault();
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
                return "Duplicate Study: ";

            return "";
        }
    }
}
