using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EmailSender;
using GSC.Respository.ProjectRight;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class ResourceMilestoneRepository : GenericRespository<ResourceMilestone>, IResourceMilestoneRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IEmailSenderRespository _emailSenderRespository;
        public ResourceMilestoneRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, IProjectRepository projectRepository, IProjectRightRepository projectRightRepository, IEmailSenderRespository emailSenderRespository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _projectRepository = projectRepository;
            _context = context;
            _projectRightRepository = projectRightRepository;
            _emailSenderRespository = emailSenderRespository;
        }

        public IList<ResourceMilestoneGridDto> GetPaymentMilestoneList(int parentProjectId, int? siteId, int? countryId, bool isDeleted)
        {
            var PaymentMilestoneData = new List<ResourceMilestoneGridDto>();

            if (parentProjectId != 0 && siteId == 0 && countryId == 0)
            {
                PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && (x.ProjectId == parentProjectId)).
                             ProjectTo<ResourceMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            }
            else if (parentProjectId != 0 && siteId != 0 && countryId == 0)
            {
                PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == parentProjectId && x.SiteId == siteId).
                             ProjectTo<ResourceMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            }
            else if (parentProjectId != 0 && siteId == 0 && countryId != 0)
            {
                PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == parentProjectId && x.CountryId == countryId).
                             ProjectTo<ResourceMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            }
            else
            {
                PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == parentProjectId && x.CountryId == countryId && x.SiteId == siteId).
                            ProjectTo<ResourceMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            }

            PaymentMilestoneData.ForEach(x =>
            {
                x.SitedName = _context.Project.Include(s => s.ManageSite).Where(w => w.Id == x.SiteId).Select(d => d.ProjectCode == null ? d.ManageSite.SiteName : d.ProjectCode).FirstOrDefault();
            });
            return PaymentMilestoneData;
        }
        public string DuplicatePaymentMilestone(ResourceMilestone paymentMilestone)
        {
            return "";
        }
        public List<DropDownTaskListforMilestoneDto> GetTaskListforMilestone(int parentProjectId, int? siteId, int? countryId)
        {
            var studyPlan = new List<StudyPlan>();

            if (countryId > 0)
            {
                var projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == parentProjectId
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

                studyPlan = _context.StudyPlan.Where(x => projectIds.Select(f => f.Id).Contains(x.ProjectId) && x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();
            }
            else
            {
                studyPlan = _context.StudyPlan.Where(x => x.ProjectId == parentProjectId && x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();
            }

            //Onetime Task Seleect then not get in list 
            var PaymentMilestoneTask = _context.ResourceMilestone.Where(w => studyPlan.Select(f => f.ProjectId).Contains(w.ProjectId) && w.DeletedBy == null).ToList();

            var data = _context.StudyPlanTask.Where(x => studyPlan.Select(f => f.Id).Contains(x.StudyPlanId) && x.DeletedDate == null && x.IsPaymentMileStone && !PaymentMilestoneTask.Select(f => f.StudyPlanTaskId).Contains(x.Id)).OrderByDescending(x => x.Id).ToList();

            var DropDownDto = new List<DropDownTaskListforMilestoneDto>();
            data.ForEach(x =>
            {
                var DependentData = _context.StudyPlanTask.Where(s => s.Id == x.DependentTaskId && s.DeletedBy == null).FirstOrDefault();
                DropDownTaskListforMilestoneDto d1 = new DropDownTaskListforMilestoneDto();
                if (DependentData != null)
                {
                    d1.Id = x.Id;
                    d1.Value = x.TaskName;
                    d1.IsDeleted = x.DeletedDate != null;
                    d1.DependetTaskName = DependentData.TaskName;
                    d1.ScheduleStartDate = DependentData.StartDate;
                    d1.ScheduleEndDate = DependentData.EndDate;
                    d1.ActualStartDate = DependentData.ActualStartDate;
                    d1.ActualEndDate = DependentData.ActualEndDate;
                }
                else
                {
                    d1.Id = x.Id;
                    d1.Value = x.TaskName;
                    d1.IsDeleted = x.DeletedDate != null;
                    d1.DependetTaskName = x.TaskName;
                    d1.ScheduleStartDate = x.StartDate;
                    d1.ScheduleEndDate = x.EndDate;
                    d1.ActualStartDate = x.ActualStartDate;
                    d1.ActualEndDate = x.ActualEndDate;
                }

                DropDownDto.Add(d1);
            });

            return DropDownDto;
        }
        public void DeletePaymentMilestoneTaskDetail(int Id)
        {
            var paymentMilestoneTaskDetail = _context.PaymentMilestoneTaskDetail.Where(s => s.ResourceMilestoneId == Id && s.DeletedBy == null).ToList();
            paymentMilestoneTaskDetail.ForEach(s =>
            {
                s.DeletedDate = DateTime.UtcNow;
                s.DeletedBy = _jwtTokenAccesser.UserId;
                _context.PaymentMilestoneTaskDetail.Update(s);
                _context.Save();
            });
        }
        public void ActivePaymentMilestoneTaskDetail(int Id)
        {
            var paymentMilestoneTaskDetail = _context.PaymentMilestoneTaskDetail.Where(s => s.ResourceMilestoneId == Id && s.DeletedBy != null).ToList();
            paymentMilestoneTaskDetail.ForEach(s =>
            {
                s.DeletedDate = null;
                s.DeletedBy = null;
                _context.PaymentMilestoneTaskDetail.Update(s);
                _context.Save();
            });
        }
        public BudgetPaymentFinalCostDto GetFinalResourceTotal(int projectId)
        {
            BudgetPaymentFinalCostDto data = new BudgetPaymentFinalCostDto();
            var paymentFinalCost = _context.BudgetPaymentFinalCost.FirstOrDefault(x => x.ProjectId == projectId && x.MilestoneType == MilestoneType.ProfessionalCost && x.DeletedDate == null);
            data.ProfessionalCostAmount = paymentFinalCost?.FinalTotalAmount ?? 0;
            return data;
        }

        public async Task SendDueResourceMilestoneEmail()
        {
            var dueDates = await All.Where(x => x.DeletedDate == null && x.DueDate != null && x.DueDate.Value.Date >= DateTime.Now.Date).ToListAsync();

            foreach (var due in dueDates)
            {
                _emailSenderRespository.SendDueResourceMilestoneEmail(due);
            }
        }

        public IList<ResourceMilestoneGridDto> GetTaskPaymentDueList(int parentProjectId, int? siteId, int? countryId, bool isDeleted, CTMSPaymentDue cTMSPaymentDue)
        {
            var paymentMilestoneData = All.Where(x =>
                (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) &&
                x.ProjectId == parentProjectId &&
                (siteId == 0 || x.SiteId == siteId) &&
                (countryId == 0 || x.CountryId == countryId)).ProjectTo<ResourceMilestoneGridDto>(_mapper.ConfigurationProvider).AsEnumerable().Where(x => (x.DueDate != null ?
                 (cTMSPaymentDue == CTMSPaymentDue.Due && x.DueDate.Value.Date <= DateTime.Now.Date) ||
                 (cTMSPaymentDue == CTMSPaymentDue.CurrentDueDate && x.DueDate.Value.Date > DateTime.Now.Date && x.DueDate.Value.Date <= DateTime.Now.GetLastDateOfMonth().Date) ||
                 (cTMSPaymentDue == CTMSPaymentDue.NextMonthDue && x.DueDate.Value.Date > DateTime.Now.AddMonths(1).GetFirstDateOfMonth().Date && x.DueDate.Value.Date <= DateTime.Now.AddMonths(1).GetLastDateOfMonth()) :
                 false)).OrderByDescending(x => x.Id).ToList();

            paymentMilestoneData.ForEach(x =>
            {
                x.SitedName = _context.Project
                    .Include(s => s.ManageSite)
                    .Where(w => w.Id == x.SiteId)
                    .Select(d => d.ProjectCode ?? d.ManageSite.SiteName)
                    .FirstOrDefault();
            });

            return paymentMilestoneData;
        }

        public IList<ResourceMilestoneGridDto> GetTaskPaymentBudgetList()
        {
            var paymentMilestoneData = All.Where(x => x.DeletedDate == null).ProjectTo<ResourceMilestoneGridDto>(_mapper.ConfigurationProvider).ToList();
            return paymentMilestoneData;
        }
    }
}
