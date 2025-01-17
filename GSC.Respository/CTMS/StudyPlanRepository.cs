﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EmailSender;
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
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;

        public StudyPlanRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, IHolidayMasterRepository holidayMasterRepository, IWeekEndMasterRepository weekEndMasterRepository, IProjectRightRepository projectRightRepository, IEmailSenderRespository emailSenderRespository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _holidayMasterRepository = holidayMasterRepository;
            _weekEndMasterRepository = weekEndMasterRepository;
            _projectRightRepository = projectRightRepository;
            _emailSenderRespository = emailSenderRespository;
        }

        public List<StudyPlanGridDto> GetStudyplanList(bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectCTMSRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<StudyPlanGridDto>();

            var projectsctms = _context.ProjectSettings.Where(x => x.IsCtms && x.DeletedDate == null && projectList.Contains(x.ProjectId)).Select(x => x.ProjectId).ToList();
            var ctmsProjectList = _context.Project.Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.ProjectCode != null && projectsctms.Any(c => c == x.Id)).ToList();

            var StudyplanData1 = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.Project.ParentProjectId == null && ctmsProjectList.Select(c => c.Id).Contains(x.ProjectId)).OrderByDescending(x => x.Id).ToList();
            TotalCostStudyUpdate(StudyplanData1);


            var studyPlanGridDto = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.Project.ParentProjectId == null && ctmsProjectList.Select(c => c.Id).Contains(x.ProjectId)).OrderByDescending(x => x.Id).
             ProjectTo<StudyPlanGridDto>(_mapper.ConfigurationProvider).ToList();


            return studyPlanGridDto;
        }
        public void TotalCostStudyUpdate(List<StudyPlan> StudyPlan)
        {
            StudyPlan.ForEach(i =>
            {
                //PatientCostVisit Total with PatientCount
                decimal? totalFinalCost = 0;
                decimal? total = 0;
                var patientcostprocedTemp = new List<PatientCostGridData>();
                var duplicates = _context.PatientCost.Include(s => s.Procedure).Where(x => x.DeletedBy == null && x.ProjectId == i.ProjectId && x.ProcedureId != null).GroupBy(i => i.Procedure.CurrencyId).Where(x => x.Count() > 0).Select(val => val.Key).ToList();
                for (var k = 0; k < duplicates.Count; k++)
                {
                    patientcostprocedTemp = _context.PatientCost.Include(s => s.Procedure).Where(x => x.DeletedBy == null && x.ProjectId == i.ProjectId && x.ProcedureId != null && x.Procedure.CurrencyId == duplicates[k]).
                    Select(t => new PatientCostGridData
                    {
                        ProcedureId = t.ProcedureId,
                        PatientCount = t.PatientCount
                    }).Distinct().ToList();

                    var PatientCostVisit = _context.PatientCost.Include(s => s.ProjectDesignVisit).
                        Where(x => patientcostprocedTemp.Select(r => r.ProcedureId).Contains(x.ProcedureId) && x.ProjectId == i.ProjectId && x.DeletedBy == null).
                        GroupBy(g => g.ProjectDesignVisitId)
                        .Select(t => new VisitGridData
                        {
                            FinalCost = t.Sum(r => r.FinalCost)
                        }).ToList();
                    totalFinalCost = 0;
                    PatientCostVisit.ForEach(s =>
                    {
                        totalFinalCost += s.FinalCost;
                    });
                    total += totalFinalCost * patientcostprocedTemp.Select(s => s.PatientCount).FirstOrDefault();
                }
                decimal? TotalResourceCost = _context.StudyPlanTask.Where(s => s.StudyPlanId == i.Id && s.DeletedBy == null).Sum(d => d.TotalCost);
                decimal? TotalPatientCost = Convert.ToDecimal(total);
                decimal? TotalPassThroughCost = _context.PassThroughCost.Where(s => s.ProjectId == i.ProjectId && s.DeletedBy == null).Sum(d => d.Total);
                decimal? TotalFinalCost = _context.BudgetPaymentFinalCost.Where(s => s.ProjectId == i.ProjectId && s.DeletedBy == null).Sum(d => d.FinalTotalAmount);

                i.TotalCost = TotalResourceCost + TotalPatientCost + TotalPassThroughCost;
                i.TotalFinalCost = TotalFinalCost;
                _context.StudyPlan.UpdateRange(i);
                _context.Save();
            });


        }

        public string ImportTaskMasterData(StudyPlan studyplan)
        {
            var holidaylist = _holidayMasterRepository.GetHolidayList(studyplan.ProjectId);
            var weekendlist = _weekEndMasterRepository.GetWorkingDayList(studyplan.ProjectId);
            WorkingDayHelper.InitholidayDate(holidaylist, weekendlist);

            var ParentProject = _context.Project.Where(x => x.Id == studyplan.ProjectId).Select(d => d.ParentProjectId).FirstOrDefault();

            var tasklist = _context.RefrenceTypes.Include(d => d.TaskMaster).Where(x => x.DeletedDate == null && x.TaskMaster.TaskTemplateId == studyplan.TaskTemplateId
            && (ParentProject == null ? x.RefrenceType == RefrenceType.Country || x.RefrenceType == RefrenceType.Study
            : x.RefrenceType == RefrenceType.Sites))
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
                    RefrenceType = t.RefrenceType,
                    IsCountry = t.RefrenceType == RefrenceType.Country
                }).ToList();

            if (tasklist.Any(x => x.IsCountry))
            {
                var countryTasks = tasklist.Where(x => x.IsCountry).ToList();
                tasklist.RemoveAll(r => r.IsCountry);
                countryTasks.ForEach(t =>
                {
                    var countryTaskList = _context.Project.Where(x =>
                        (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                        && x.DeletedDate == null && x.ParentProjectId == studyplan.ProjectId)
                        .Include(i => i.ManageSite.City.State)
                        .GroupBy(g => g.ManageSite.City.State.CountryId)
                        .Select(s => s.First(x => x.ManageSite.City.State.CountryId == s.Key)).AsEnumerable()
                        .Select(c => new StudyPlanTask
                        {
                            StudyPlanId = t.StudyPlanId,
                            TaskId = t.TaskId,
                            TaskName = t.TaskName,
                            ParentId = t.ParentId,
                            isMileStone = t.isMileStone,
                            TaskOrder = t.TaskOrder,
                            Duration = t.Duration,
                            StartDate = studyplan.StartDate,
                            EndDate = t.EndDate,
                            DependentTaskId = t.DependentTaskId,
                            ActivityType = t.ActivityType,
                            OffSet = t.OffSet,
                            RefrenceType = t.RefrenceType,
                            IsCountry = true,
                            CountryId = c.ManageSite.City.State.CountryId,
                            ProjectId = c.Id
                        }).ToList();

                    tasklist.AddRange(countryTaskList);
                });
            }

            tasklist.ForEach(t =>
            {
                var data = UpdateDependentTaskDate(t, ref tasklist);
                if (data != null)
                {
                    t.StartDate = data.StartDate;
                    t.EndDate = data.EndDate;
                    t.DependentTask = tasklist.Find(d => d.TaskId == t.DependentTaskId);
                    t.DependentTaskId = null;
                }

                if (t.ParentId > 0)
                {
                    t.Parent = tasklist.Find(x => x.TaskId == t.ParentId);
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
                if (studyPlanTaskList.Count > 0)
                {
                    i.StartDate = studyPlanTaskList.Min(x => x.StartDate);
                    i.EndDate = studyPlanTaskList.Max(x => x.EndDate);
                }
            });

            _context.StudyPlan.UpdateRange(studyPlanList);
            _context.Save();
        }
        #region User will able to add Multiple Local currency in Study plan 
        public void CurrencyRateAdd(StudyPlanDto objSave)
        {
            objSave.CurrencyRateList.ForEach(i =>
            {
                var currencyRateData = new CurrencyRate();
                currencyRateData.Id = 0;
                currencyRateData.StudyPlanId = objSave.Id;
                currencyRateData.GlobalCurrencyId = objSave.CurrencyId;
                currencyRateData.CurrencyId = i.localCurrencyId;
                currencyRateData.LocalCurrencyRate = i.localCurrencyRate;
                _context.CurrencyRate.Add(currencyRateData);
                _context.Save();
            });
        }
        public void CurrencyRateUpdate(StudyPlanDto objSave)
        {
            var CurrencyRateData = _context.CurrencyRate.Where(x => x.StudyPlanId == objSave.Id && x.DeletedDate == null).ToList();
            CurrencyRateData.ForEach(t =>
            {
                if (CurrencyRateData != null)
                {
                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _context.CurrencyRate.Update(t);
                    _context.Save();
                }
            });
            objSave.CurrencyRateList.ForEach(i =>
            {
                var currencyRateData = new CurrencyRate();
                currencyRateData.Id = 0;
                currencyRateData.StudyPlanId = objSave.Id;
                currencyRateData.GlobalCurrencyId = objSave.CurrencyId;
                currencyRateData.CurrencyId = i.localCurrencyId;
                currencyRateData.LocalCurrencyRate = i.localCurrencyRate;
                _context.CurrencyRate.Add(currencyRateData);
                _context.Save();
            });
        }
        #endregion
        private StudyPlanTask UpdateDependentTaskDate(StudyPlanTask maintask, ref List<StudyPlanTask> tasklist)
        {
            if (maintask.DependentTaskId <= 0) return null;

            var dependentTask = tasklist.Find(x => x.TaskId == maintask.DependentTaskId);
            if (dependentTask == null) return null;

            DateTime CalculateStartDate(DateTime baseDate, int duration)
            {
                return WorkingDayHelper.SubtractBusinessDays(baseDate, duration > 0 ? duration - 1 : 0);
            }

            DateTime CalculateEndDate(DateTime baseDate, int offset)
            {
                return WorkingDayHelper.AddBusinessDays(baseDate, offset);
            }

            switch (maintask.ActivityType)
            {
                case ActivityType.FF:
                    maintask.EndDate = CalculateEndDate(dependentTask.EndDate, maintask.OffSet);
                    maintask.StartDate = CalculateStartDate(maintask.EndDate, maintask.Duration);
                    break;

                case ActivityType.FS:
                    maintask.StartDate = maintask.isMileStone
                        ? CalculateEndDate(dependentTask.EndDate, maintask.OffSet)
                        : CalculateEndDate(WorkingDayHelper.GetNextWorkingDay(dependentTask.EndDate), maintask.OffSet);
                    maintask.EndDate = CalculateEndDate(maintask.StartDate, maintask.Duration - 1);
                    break;

                case ActivityType.SF:
                    maintask.EndDate = maintask.isMileStone
                        ? CalculateEndDate(dependentTask.StartDate, maintask.OffSet)
                        : CalculateEndDate(WorkingDayHelper.GetNextSubstarctWorkingDay(dependentTask.StartDate), maintask.OffSet);
                    maintask.StartDate = CalculateStartDate(maintask.EndDate, maintask.Duration);
                    break;

                case ActivityType.SS:
                    maintask.StartDate = CalculateEndDate(dependentTask.StartDate, maintask.OffSet);
                    maintask.EndDate = CalculateEndDate(maintask.StartDate, maintask.Duration - 1);
                    break;
            }

            return maintask;
        }
        public string ValidateTask(StudyPlanTask taskmasterDto, List<StudyPlanTask> tasklist, StudyPlan studyplan)
        {
            if (taskmasterDto.StartDate >= studyplan.StartDate && taskmasterDto.StartDate <= studyplan.EndDate
                && taskmasterDto.EndDate <= studyplan.EndDate && taskmasterDto.EndDate >= studyplan.StartDate)
            {
                if (taskmasterDto.ParentId > 0)
                {
                    var parentdate = tasklist.Find(x => x.TaskId == taskmasterDto.ParentId);
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

            var ParentProject = _context.Project.Where(x => x.Id == studyplan.ProjectId).Select(s => s.ParentProjectId).FirstOrDefault();

            var tasklist = _context.RefrenceTypes.Include(d => d.TaskMaster).Where(x => x.DeletedDate == null && x.TaskMaster.TaskTemplateId == studyplan.TaskTemplateId
                      && (ParentProject == null ? x.RefrenceType == RefrenceType.Country || x.RefrenceType == RefrenceType.Study
                      : x.RefrenceType == RefrenceType.Sites))
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
                              RefrenceType = t.RefrenceType,
                              IsCountry = t.RefrenceType == RefrenceType.Country

                          }).ToList();

            tasklist.ForEach(t =>
            {
                var data = UpdateDependentTaskDate(t, ref tasklist);
                if (data != null)
                {
                    t.StartDate = data.StartDate;
                    t.EndDate = data.EndDate;
                    t.DependentTask = tasklist.Find(d => d.TaskId == t.DependentTaskId);
                    t.DependentTaskId = null;
                }

                if (t.ParentId > 0)
                {
                    t.Parent = tasklist.Find(x => x.TaskId == t.ParentId);
                    t.ParentId = null;
                }
            });

            _context.StudyPlanTask.AddRange(tasklist);
            _context.Save();


            return "";

        }
        public bool UpdateApprovalPlan(int id, bool ifPlanApproval)
        {
            var data = All.Where(s => s.DeletedBy == null && s.Id == id).FirstOrDefault();
            if (data != null)
            {
                data.IsBudgetApproval = !ifPlanApproval;
                _context.StudyPlan.Update(data);
                _context.Save();
                return data.IsBudgetApproval;
            }
            return false;

        }
        //Add by Mitul on 06-12-2023 get History in AuditTrail Deleted=Revoke And Added,Modified=Gran
        public List<ApprovalPlanHistory> GetApprovalPlanHistory(int id, string columnName)
        {

            var result = _context.AuditTrail.Where(x => x.RecordId == id && x.TableName == "StudyPlan" && x.ColumnName == columnName)
               .Select(x => new ApprovalPlanHistory
               {
                   Id = x.Id,
                   TableName = x.TableName,
                   RecordId = x.RecordId,
                   IsApproval = x.NewValue != "No",
                   ReasonOth = x.ReasonOth,
                   ReasonName = x.Reason,
                   ApprovalOn = x.CreatedDate,
                   ApprovalBy = x.User.UserName,
                   ApprovalRole = x.UserRole,
                   TimeZone = x.TimeZone,
                   IpAddress = x.IpAddress
               }).OrderBy(r => r.Id).ToList();

            return result;
        }
        public string SendMail(int id, bool ifPlanApproval, TriggerType triggerType)
        {
            var StudyPlan = _context.StudyPlan.Where(w => w.Id == id && w.DeletedBy == null).FirstOrDefault();
            if (StudyPlan != null)
            {
                var CtmsApprovalWorkFlowDetail = _context.CtmsApprovalUsers.Include(j => j.Users).Include(i => i.CtmsApprovalRoles).ThenInclude(p => p.Project).
                    Where(w => w.CtmsApprovalRoles.ProjectId == StudyPlan.ProjectId && w.CtmsApprovalRoles.TriggerType == triggerType && w.DeletedBy == null).ToList();

                CtmsApprovalWorkFlowDetail.ForEach(i =>
                {

                    _emailSenderRespository.SendMailCtmsApproval(i, ifPlanApproval);
                });
            }
            return "";
        }


        public string PullSite(int projectId)
        {
            var sites = _context.Project.Where(x => x.DeletedDate == null
            && x.ParentProjectId == projectId
            && !All.Any(a => a.DeletedDate == null && a.ProjectId == x.Id)).ToList();

            if (!sites.Any())
            {
                return "Latest site not found";
            }

            var studyplanDto = All.FirstOrDefault(x => x.DeletedDate == null && x.ProjectId == projectId);
            if (studyplanDto != null)
            {
                foreach (var s in sites)
                {
                    var data = new StudyPlan();
                    data.StartDate = studyplanDto.StartDate;
                    data.EndDate = studyplanDto.EndDate;
                    data.ProjectId = s.Id;
                    data.TaskTemplateId = studyplanDto.TaskTemplateId;
                    data.CurrencyId = studyplanDto.CurrencyId;
                    var validatecode = Duplicate(data);
                    if (!string.IsNullOrEmpty(validatecode))
                    {
                        return validatecode;
                    }
                    Add(data);
                    if (_context.Save() <= 0) return "Study plan is failed on save.";
                }
            }
            return "";
        }
    }
}
