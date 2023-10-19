using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Audit;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class TaskMasterRepository : GenericRespository<TaskMaster>, ITaskMasterRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IStudyPlanRepository _studyPlanRepository;
        public TaskMasterRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, IStudyPlanRepository studyPlanRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _studyPlanRepository = studyPlanRepository;
        }

        public List<TaskMasterGridDto> GetTasklist(bool isDeleted, int templateId)
        {
            return All.Where(x => (isDeleted ? (x.DeletedDate != null) : (x.DeletedDate == null)) && (x.TaskTemplateId == templateId)).OrderBy(x => x.TaskOrder).
                   ProjectTo<TaskMasterGridDto>(_mapper.ConfigurationProvider).ToList();
        }
        public int UpdateTaskOrder(TaskmasterDto taskmasterDto)
        {
            if (taskmasterDto.Position == "above")
            {
                var data = All.Where(x => x.TaskTemplateId == taskmasterDto.TaskTemplateId && x.TaskOrder >= taskmasterDto.TaskOrder && x.DeletedDate == null).ToList();
                foreach (var item in data)
                {
                    item.TaskOrder = ++item.TaskOrder;
                    Update(item);
                }
                return taskmasterDto.TaskOrder;
            }
            if (taskmasterDto.Position == "below")
            {
                var data = All.Where(x => x.TaskTemplateId == taskmasterDto.TaskTemplateId && x.TaskOrder > taskmasterDto.TaskOrder && x.DeletedDate == null).ToList();
                foreach (var item in data)
                {
                    item.TaskOrder = ++item.TaskOrder;
                    Update(item);
                }
                return ++taskmasterDto.TaskOrder;
            }
            else
            {
                var count = All.Where(x => x.TaskTemplateId == taskmasterDto.TaskTemplateId && x.DeletedDate == null).Count();
                return ++count;
            }

        }

        public List<AuditTrailDto> GetTaskHistory(int id)
        {
            var result = _context.AuditTrail.Where(x => x.RecordId == id && x.TableName == "TaskMaster" && x.Action == "Modified")
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
        public string AddTaskToSTudyPlan(TaskmasterDto taskmasterDto)
        {
            var task = _context.StudyPlanTask.Where(x => x.TaskId == taskmasterDto.Id).ToList();
            if (task != null)
            {
                _context.StudyPlanTask.RemoveRange(task);
                _context.Save();
            }

            string str = string.Empty;
            var lstStudyPlan = new List<StudyPlanDto>();
            var TaskMaster = _context.StudyPlan.Include(x => x.Project).Where(x => x.TaskTemplateId == taskmasterDto.TaskTemplateId && x.DeletedDate == null).ToList();
            if (TaskMaster.Count > 0)
            {
                var projectid = TaskMaster.Where(x => x.Project.ParentProjectId == null).Select(x => x.Project.Id).FirstOrDefault();
                var sitedata = TaskMaster.Where(x => x.Project.ParentProjectId != null).FirstOrDefault();
                foreach (var item in taskmasterDto.RefrenceTypes)
                {
                    if (sitedata == null && (item.RefrenceType == Helper.RefrenceType.Sites || item.RefrenceType == Helper.RefrenceType.Country))
                    {
                        var sites = _context.Project.Where(x => x.DeletedDate == null && x.ParentProjectId == projectid).ToList();
                        sites.ForEach(s =>
                        {
                            var data = new StudyPlanDto();
                            data.StartDate = TaskMaster.FirstOrDefault().StartDate;
                            data.EndDate = TaskMaster.FirstOrDefault().EndDate;
                            data.ProjectId = s.Id;
                            data.TaskTemplateId = taskmasterDto.TaskTemplateId;
                            lstStudyPlan.Add(data);
                        });
                    }
                    else if (sitedata != null && (item.RefrenceType == Helper.RefrenceType.Sites || item.RefrenceType == Helper.RefrenceType.Country))
                    {
                        var sites = TaskMaster.Where(x => x.DeletedDate == null && x.Project.ParentProjectId != null).ToList();
                        sites.ForEach(s =>
                        {
                            var data = new StudyPlanDto();
                            data.Id = s.Id;
                            data.StartDate = s.StartDate;
                            data.EndDate = s.EndDate;
                            data.ProjectId = s.ProjectId;
                            data.TaskTemplateId = taskmasterDto.TaskTemplateId;
                            lstStudyPlan.Add(data);
                        });
                    }
                    else if (item.RefrenceType == Helper.RefrenceType.Study)
                    {
                        var sites = TaskMaster.Where(x => x.DeletedDate == null && x.Project.ParentProjectId == null).ToList();
                        sites.ForEach(s =>
                        {
                            var data = new StudyPlanDto();
                            data.Id = s.Id;
                            data.StartDate = s.StartDate;
                            data.EndDate = s.EndDate;
                            data.ProjectId = s.ProjectId;
                            data.TaskTemplateId = taskmasterDto.TaskTemplateId;
                            lstStudyPlan.Add(data);
                        });
                    }
                    //else if (item.RefrenceType == Helper.RefrenceType.Country)
                    //{
                    //    var study = TaskMaster.Where(x => x.DeletedDate == null && x.Project.ParentProjectId == null && x.Project.CountryId != null).ToList();
                    //    study.ForEach(s =>
                    //    {
                    //        var data = new StudyPlanDto();
                    //        data.Id = s.Id;
                    //        data.StartDate = s.StartDate;
                    //        data.EndDate = s.EndDate;
                    //        data.ProjectId = s.ProjectId;
                    //        data.TaskTemplateId = taskmasterDto.TaskTemplateId;
                    //        lstStudyPlan.Add(data);
                    //    });

                    //    var sites = _context.Project.Where(x => x.DeletedDate == null && x.ParentProjectId == projectid && x.CountryId != null).ToList();
                    //    sites.ForEach(s =>
                    //    {
                    //        var data = new StudyPlanDto();
                    //        data.StartDate = TaskMaster.FirstOrDefault().StartDate;
                    //        data.EndDate = TaskMaster.FirstOrDefault().EndDate;
                    //        data.ProjectId = s.Id;
                    //        data.TaskTemplateId = taskmasterDto.TaskTemplateId;
                    //        lstStudyPlan.Add(data);
                    //    });
                    //}

                }
                //if (sitedata == null && taskmasterDto.RefrenceType == Helper.RefrenceType.Sites)
                //{
                //    var sites = _context.Project.Where(x => x.DeletedDate == null && x.ParentProjectId == projectid).ToList();
                //    sites.ForEach(s =>
                //    {
                //        var data = new StudyPlanDto();
                //        data.StartDate = TaskMaster.FirstOrDefault().StartDate;
                //        data.EndDate = TaskMaster.FirstOrDefault().EndDate;
                //        data.ProjectId = s.Id;
                //        data.TaskTemplateId = taskmasterDto.TaskTemplateId;
                //        lstStudyPlan.Add(data);
                //    });
                //}
                //else if (sitedata != null && taskmasterDto.RefrenceType == Helper.RefrenceType.Sites)
                //{
                //    var sites = TaskMaster.Where(x => x.DeletedDate == null && x.Project.ParentProjectId != null).ToList();
                //    sites.ForEach(s =>
                //    {
                //        var data = new StudyPlanDto();
                //        data.Id = s.Id;
                //        data.StartDate = s.StartDate;
                //        data.EndDate = s.EndDate;
                //        data.ProjectId = s.ProjectId;
                //        data.TaskTemplateId = taskmasterDto.TaskTemplateId;
                //        lstStudyPlan.Add(data);
                //    });
                //}
                //else if (taskmasterDto.RefrenceType == Helper.RefrenceType.Study)
                //{
                //    var sites = TaskMaster.Where(x => x.DeletedDate == null && x.Project.ParentProjectId == null).ToList();
                //    sites.ForEach(s =>
                //    {
                //        var data = new StudyPlanDto();
                //        data.Id = s.Id;
                //        data.StartDate = s.StartDate;
                //        data.EndDate = s.EndDate;
                //        data.ProjectId = s.ProjectId;
                //        data.TaskTemplateId = taskmasterDto.TaskTemplateId;
                //        lstStudyPlan.Add(data);
                //    });
                //}
                //else if (taskmasterDto.RefrenceType == Helper.RefrenceType.Country)
                //{
                //    var study = TaskMaster.Where(x => x.DeletedDate == null && x.Project.ParentProjectId == null).ToList();
                //    study.ForEach(s =>
                //    {
                //        var data = new StudyPlanDto();
                //        data.Id = s.Id;
                //        data.StartDate = s.StartDate;
                //        data.EndDate = s.EndDate;
                //        data.ProjectId = s.ProjectId;
                //        data.TaskTemplateId = taskmasterDto.TaskTemplateId;
                //        lstStudyPlan.Add(data);
                //    });

                //    var sites = _context.Project.Where(x => x.DeletedDate == null && x.ParentProjectId == projectid).ToList();
                //    sites.ForEach(s =>
                //    {
                //        var data = new StudyPlanDto();
                //        data.StartDate = TaskMaster.FirstOrDefault().StartDate;
                //        data.EndDate = TaskMaster.FirstOrDefault().EndDate;
                //        data.ProjectId = s.Id;
                //        data.TaskTemplateId = taskmasterDto.TaskTemplateId;
                //        lstStudyPlan.Add(data);
                //    });
                //}
                if (lstStudyPlan.Count > 0)
                {
                    foreach (var item in lstStudyPlan)
                    {
                        var studyplan = _mapper.Map<StudyPlan>(item);
                        var data = _context.StudyPlan.Where(x => x.ProjectId == item.ProjectId && x.DeletedDate == null).FirstOrDefault();
                        if (data == null)
                        {
                            _studyPlanRepository.Add(studyplan);
                            _context.Save();
                        }
                        else
                        {
                            var TaskMasterdata = _context.StudyPlan.Include(x => x.Project).Where(x => x.TaskTemplateId == taskmasterDto.TaskTemplateId && x.DeletedDate == null && x.ProjectId == item.ProjectId).FirstOrDefault();
                            if (TaskMasterdata != null)
                            {
                                studyplan.Id = TaskMasterdata.Id;
                            }
                        }
                        if (studyplan.Id > 0)
                            str = _studyPlanRepository.ImportTaskMasterDataFromTaskMaster(studyplan, taskmasterDto.Id);
                    }
                }

            }
            return str;
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
