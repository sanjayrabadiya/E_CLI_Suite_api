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
using GSC.Respository.EmailSender;
using GSC.Respository.ProjectRight;
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
            var PaymentMilestoneTask = _context.PaymentMilestoneTaskDetail.Include(i => i.ResourceMilestone).Where(w => studyPlan.Select(f => f.ProjectId).Contains(w.ResourceMilestone.ProjectId) && w.DeletedBy == null).ToList();

           var data = _context.StudyPlanTask.Where(x => studyPlan.Select(f => f.Id).Contains(x.StudyPlanId) && x.DeletedDate == null && x.IsPaymentMileStone && !PaymentMilestoneTask.Select(f => f.StudyPlanTaskId).Contains(x.Id)).OrderByDescending(x => x.Id).ToList();
            //.Select(c => new  { Id = c.Id, Value = c.TaskName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();

            var DropDownDto = new List<DropDownTaskListforMilestoneDto>();
            data.ForEach(x => {
               var DependentData=_context.StudyPlanTask.Where(s=>s.Id ==x.DependentTaskId && s.DeletedBy==null).FirstOrDefault();
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
        public decimal GetEstimatedMilestoneAmount(ResourceMilestoneDto paymentMilestoneDto)
        {
            decimal EstimatedTotal = 0;
            if (paymentMilestoneDto.StudyPlanTaskIds != null)
            {
                foreach (var item in paymentMilestoneDto.StudyPlanTaskIds)
                {
                    var studyPlanTaskData = _context.StudyPlanTask.Where(s => (s.Id == item || s.DependentTaskId == item) && s.DeletedBy == null).ToList();
                    foreach (var item2 in studyPlanTaskData)
                    {
                        EstimatedTotal += _context.StudyPlanResource.Where(s => s.StudyPlanTaskId == item2.Id && s.DeletedBy == null).Sum(d => d.ConvertTotalCost).GetValueOrDefault();
                    }
                }
            }
            return EstimatedTotal;
        }
        public void AddPaymentMilestoneTaskDetail(ResourceMilestoneDto paymentMilestoneDto)
        {
            foreach (var item in paymentMilestoneDto.StudyPlanTaskIds)
            {
                var paymentMilestoneTaskDetail = new PaymentMilestoneTaskDetail();
                paymentMilestoneTaskDetail.Id = 0;
                paymentMilestoneTaskDetail.ResourceMilestoneId = paymentMilestoneDto.Id;
                paymentMilestoneTaskDetail.StudyPlanTaskId = item;
                _context.PaymentMilestoneTaskDetail.Add(paymentMilestoneTaskDetail);
                _context.Save();
            }
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
            var siteIds = _context.Project.Where(s => s.DeletedDate == null && s.ParentProjectId == projectId).Select(s => s.Id).ToList();
            BudgetPaymentFinalCostDto data = new BudgetPaymentFinalCostDto();

            var resourcecost = _context.StudyPlanResource.
                                Include(s => s.StudyPlanTask).
                                ThenInclude(s => s.StudyPlan)
                                .Where(s => s.DeletedBy == null && s.StudyPlanTask.StudyPlan.ProjectId == projectId || siteIds.Contains(s.StudyPlanTask.StudyPlan.ProjectId)).Sum(s => s.ConvertTotalCost);

            //one time Add Paybal Amount id diduct in main total
            var resourcePaybalAmount = _context.ResourceMilestone.Where(w => w.DeletedDate == null && w.ProjectId == projectId).Sum(s => s.PaybalAmount);
            data.ProfessionalCostAmount = Convert.ToDecimal(resourcecost - resourcePaybalAmount);

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
    }
}
