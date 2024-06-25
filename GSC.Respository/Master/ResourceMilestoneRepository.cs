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

        public IList<ResourceMilestoneGridDto> GetPaymentMilestoneList(bool isDeleted, int studyId, int siteId, int countryId, CtmsStudyTaskFilter filterType)
        {
            var PaymentMilestoneData = new List<ResourceMilestoneGridDto>();

            if (filterType == CtmsStudyTaskFilter.Study)
            {
                PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && (x.ProjectId == studyId) && x.SiteId==0 && x.CountryId==0).
                             ProjectTo<ResourceMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            }
            else if (filterType == CtmsStudyTaskFilter.Site)
            {
                if (siteId != 0)
                {
                    PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == studyId && x.SiteId == siteId && x.CountryId == 0 ).
                             ProjectTo<ResourceMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
                }
                else
                {
                    PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.SiteId != 0).
                                 ProjectTo<ResourceMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
                }
            }
            else if ( filterType == CtmsStudyTaskFilter.Country)
            {
                if(countryId != 0) 
                {
                    PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == studyId && x.CountryId == countryId && x.SiteId == 0).
                             ProjectTo<ResourceMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
                }
                else
                {
                    PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.CountryId != 0).
                             ProjectTo<ResourceMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
                }
                
            }
            else
            {
                PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == studyId).
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
            if (All.Any(x =>
                x.Id != paymentMilestone.Id && x.StudyPlanTaskId == paymentMilestone.StudyPlanTaskId &&
                x.ProjectId == paymentMilestone.ProjectId && x.DeletedDate == null && x.StudyPlanTaskId != null))
            {
                return "Duplicate Visit ";
            }
            return "";
        }
        public List<DropDownTaskListforMilestoneDto> GetTaskListforMilestone(int studyId, int siteId, int countryId, CtmsStudyTaskFilter filterType)
        {

            var result = new List<StudyPlanTaskDto>();

            var studyIds = new List<int>();

            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            var ids = _projectRepository.All.Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                     && x.DeletedDate == null && x.ParentProjectId == studyId
                     && projectList.Any(c => c == x.Id)).Select(s => s.Id).ToList();

            if (filterType == CtmsStudyTaskFilter.All || filterType == CtmsStudyTaskFilter.Country)
            {
                studyIds.AddRange(ids);
                studyIds.Add(studyId);
            }
            if (filterType == CtmsStudyTaskFilter.Site)
            {
                if (siteId == 0)
                    studyIds.AddRange(ids);
                else
                    studyIds.Add(siteId);
            }

            var studyplans = _context.StudyPlan.Include(s => s.Currency).Where(x => (filterType != CtmsStudyTaskFilter.Study ? studyIds.Contains(x.ProjectId) :
                x.ProjectId == studyId) && x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();

            //Onetime Task Seleect then not get in list 
            var PaymentMilestoneTask = _context.ResourceMilestone.Where(w => studyplans.Select(f => f.ProjectId).Contains(w.ProjectId) && w.DeletedBy == null).ToList();

            var data = _context.StudyPlanTask.Where(x => studyplans.Select(f => f.Id).Contains(x.StudyPlanId) && x.DeletedDate == null && x.IsPaymentMileStone &&
            (filterType == CtmsStudyTaskFilter.Country ? countryId <= 0 ? x.IsCountry : x.CountryId == countryId : filterType == CtmsStudyTaskFilter.All || !x.IsCountry)
            && !PaymentMilestoneTask.Select(f => f.StudyPlanTaskId).Contains(x.Id)).OrderByDescending(x => x.Id).ToList();

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
        public decimal GetFinalResourceTotal(int projectId)
        {
            //first time Total get from Budget Payment FinalCost
            decimal? paymentFinalCost = _context.ResourceMilestone.Where(s=>s.ProjectId== projectId && s.DeletedBy==null).OrderBy(s=>s.Id).Select(r=>r.ResourceTotal).LastOrDefault();
            paymentFinalCost ??= _context.BudgetPaymentFinalCost.Where(x => x.ProjectId == projectId && x.MilestoneType == MilestoneType.ProfessionalCost && x.DeletedDate == null).Select(s=>s.FinalTotalAmount).FirstOrDefault();

            return paymentFinalCost ?? 0;
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
                 (cTMSPaymentDue == CTMSPaymentDue.NextMonthDue && x.DueDate.Value.Date >= DateTime.Now.AddMonths(1).GetFirstDateOfMonth().Date && x.DueDate.Value.Date <= DateTime.Now.AddMonths(1).GetLastDateOfMonth()) :
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
        public string UpdatePaybalAmount(ResourceMilestone paymentMilestone)
        {
            var resourceMilestoneData = _context.ResourceMilestone.Where(s => s.ProjectId == paymentMilestone.ProjectId && s.DeletedBy == null).OrderBy(s => s.Id).LastOrDefault();
            if (resourceMilestoneData != null)
            {
                resourceMilestoneData.ResourceTotal += paymentMilestone.PaybalAmount;
                _context.ResourceMilestone.Update(resourceMilestoneData);
                _context.Save();
            }
            return "";
        }

        public List<DropDownDto> GetBudgetCountryDropDown(int parentProjectId)
        {
            var studyPlan = _context.StudyPlan.FirstOrDefault(x => x.DeletedDate == null && x.ProjectId == parentProjectId);
            var countrylist = _context.StudyPlanTask.Where(s => s.DeletedDate == null
            && s.CountryId != null && s.StudyPlanId == studyPlan.Id && s.IsPaymentMileStone).Include(i => i.Country).GroupBy(g => g.CountryId)
                .Select(c => new DropDownDto { Id = c.Key.Value, Value = c.First().Country.CountryName }).OrderBy(o => o.Value).ToList();

            return countrylist;
        }

        public List<DropDownDto> GetBudgetSiteDropDown(int parentProjectId)
        {
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            var ids = _projectRepository.All.Where(x =>
                     (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                     && x.DeletedDate == null && x.ParentProjectId == parentProjectId
                     && projectList.Any(c => c == x.Id)).Select(s => s.Id).ToList();


            var siteList = _context.StudyPlanTask.Where(s => s.DeletedDate == null
            && ids.Contains(s.StudyPlan.ProjectId) && s.IsPaymentMileStone).Include(i => i.StudyPlan.Project).GroupBy(g => g.StudyPlan.ProjectId)
                .Select(c => new DropDownDto { Id = c.Key, Value = c.First().StudyPlan.Project.ManageSite.SiteName ?? c.First().StudyPlan.Project.ProjectCode }).OrderBy(o => o.Value).ToList();

            return siteList;
        }
    }
}
