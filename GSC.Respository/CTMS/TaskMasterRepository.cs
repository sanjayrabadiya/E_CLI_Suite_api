using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Audit;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class TaskMasterRepository : GenericRespository<TaskMaster>, ITaskMasterRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IStudyPlanRepository _studyPlanRepository;

        public TaskMasterRepository(IGSCContext context, IMapper mapper, IStudyPlanRepository studyPlanRepository) : base(context)
        {
            _mapper = mapper;
            _context = context;
            _studyPlanRepository = studyPlanRepository;
        }

        public List<TaskMasterGridDto> GetTasklist(bool isDeleted, int templateId)
        {
            return All
                .Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.TaskTemplateId == templateId)
                .OrderBy(x => x.TaskOrder)
                .ProjectTo<TaskMasterGridDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public int UpdateTaskOrder(TaskmasterDto taskmasterDto)
        {
            var data = All
                .Where(x => x.TaskTemplateId == taskmasterDto.TaskTemplateId && x.DeletedDate == null)
                .ToList();

            if (taskmasterDto.Position == "above")
            {
                foreach (var item in data.Where(x => x.TaskOrder >= taskmasterDto.TaskOrder))
                {
                    item.TaskOrder++;
                    Update(item);
                }
                return taskmasterDto.TaskOrder;
            }

            if (taskmasterDto.Position == "below")
            {
                foreach (var item in data.Where(x => x.TaskOrder > taskmasterDto.TaskOrder))
                {
                    item.TaskOrder++;
                    Update(item);
                }
                return taskmasterDto.TaskOrder + 1;
            }

            return data.Count;
        }

        public List<AuditTrailDto> GetTaskHistory(int id)
        {
            return _context.AuditTrail
                .Where(x => x.RecordId == id && x.TableName == "TaskMaster" && x.Action == "Modified")
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
        }

        public string AddTaskToSTudyPlan(TaskmasterDto taskmasterDto)
        {
            RemoveStudyPlanTask(taskmasterDto);

            var lstStudyPlan = new List<StudyPlanDto>();
            var taskMasters = _context.StudyPlan
                .Include(x => x.Project)
                .Where(x => x.TaskTemplateId == taskmasterDto.TaskTemplateId && x.DeletedDate == null)
                .ToList();

            if (taskMasters.Any())
            {
                var projectId = taskMasters.FirstOrDefault(x => x.Project.ParentProjectId == null)?.Project.Id;
                var siteData = taskMasters.FirstOrDefault(x => x.Project.ParentProjectId != null);

                foreach (var item in taskmasterDto.RefrenceTypes)
                {
                    if (item.RefrenceType == Helper.RefrenceType.Sites)
                    {
                        AddSitesToStudyPlan(taskmasterDto, lstStudyPlan, projectId, siteData, taskMasters);
                    }
                    else if (item.RefrenceType == Helper.RefrenceType.Study || item.RefrenceType == Helper.RefrenceType.Country)
                    {
                        AddStudyOrCountryToStudyPlan(taskmasterDto, lstStudyPlan, taskMasters);
                    }
                }

                return SaveStudyPlans(lstStudyPlan, taskmasterDto);
            }

            return string.Empty;
        }

        private void AddSitesToStudyPlan(TaskmasterDto taskmasterDto, List<StudyPlanDto> lstStudyPlan, int? projectId, StudyPlan siteData, List<StudyPlan> taskMasters)
        {
            if (siteData == null)
            {
                var sites = _context.Project
                    .Where(x => x.DeletedDate == null && x.ParentProjectId == projectId)
                    .ToList();

                foreach (var site in sites)
                {
                    lstStudyPlan.Add(new StudyPlanDto
                    {
                        StartDate = taskMasters.First().StartDate,
                        EndDate = taskMasters.First().EndDate,
                        ProjectId = site.Id,
                        TaskTemplateId = taskmasterDto.TaskTemplateId
                    });
                }
            }
            else
            {
                foreach (var site in taskMasters.Where(x => x.Project.ParentProjectId != null))
                {
                    lstStudyPlan.Add(new StudyPlanDto
                    {
                        Id = site.Id,
                        StartDate = site.StartDate,
                        EndDate = site.EndDate,
                        ProjectId = site.ProjectId,
                        TaskTemplateId = taskmasterDto.TaskTemplateId
                    });
                }
            }
        }

        private void AddStudyOrCountryToStudyPlan(TaskmasterDto taskmasterDto, List<StudyPlanDto> lstStudyPlan, List<StudyPlan> taskMasters)
        {
            foreach (var taskMaster in taskMasters.Where(x => x.Project.ParentProjectId == null))
            {
                lstStudyPlan.Add(new StudyPlanDto
                {
                    Id = taskMaster.Id,
                    StartDate = taskMaster.StartDate,
                    EndDate = taskMaster.EndDate,
                    ProjectId = taskMaster.ProjectId,
                    TaskTemplateId = taskmasterDto.TaskTemplateId
                });
            }
        }

        private string SaveStudyPlans(List<StudyPlanDto> lstStudyPlan, TaskmasterDto taskmasterDto)
        {
            foreach (var item in lstStudyPlan)
            {
                var studyPlan = _mapper.Map<StudyPlan>(item);
                var existingPlan = _context.StudyPlan
                    .FirstOrDefault(x => x.ProjectId == item.ProjectId && x.DeletedDate == null);

                if (existingPlan == null)
                {
                    _studyPlanRepository.Add(studyPlan);
                    _context.Save();
                }
                else
                {
                    var taskMasterData = _context.StudyPlan
                        .Include(x => x.Project)
                        .FirstOrDefault(x => x.TaskTemplateId == taskmasterDto.TaskTemplateId && x.DeletedDate == null && x.ProjectId == item.ProjectId);

                    if (taskMasterData != null)
                    {
                        studyPlan.Id = taskMasterData.Id;
                    }
                }

                if (studyPlan.Id > 0)
                {
                    return _studyPlanRepository.ImportTaskMasterDataFromTaskMaster(studyPlan, taskmasterDto.Id);
                }
            }

            return string.Empty;
        }

        public void RemoveStudyPlanTask(TaskmasterDto taskmasterDto)
        {
            var tasks = _context.StudyPlanTask.Where(x => x.TaskId == taskmasterDto.Id).ToList();
            if (tasks.Any())
            {
                _context.StudyPlanTask.RemoveRange(tasks);
                _context.Save();
            }
        }

        public void AddRefrenceTypes(TaskmasterDto taskmasterDto)
        {
            foreach (var item in taskmasterDto.RefrenceTypes)
            {
                item.TaskMasterId = taskmasterDto.Id;
                _context.RefrenceTypes.Add(item);
                _context.Save();
            }
        }
    }
}
