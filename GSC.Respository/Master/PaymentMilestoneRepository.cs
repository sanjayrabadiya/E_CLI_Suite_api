using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class PaymentMilestoneRepository : GenericRespository<PaymentMilestone>, IPaymentMilestoneRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public PaymentMilestoneRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, IProjectRepository projectRepository, IProjectRightRepository projectRightRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _projectRepository = projectRepository;
            _context = context;
            _projectRightRepository = projectRightRepository;
        }

        public IList<PaymentMilestoneGridDto> GetPaymentMilestoneList(int parentProjectId, int? siteId, int? countryId, bool isDeleted)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && (x.ProjectId == parentProjectId || x.SiteId == siteId || x.CountryId == countryId)).
            ProjectTo<PaymentMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string DuplicatePaymentMilestone(PaymentMilestone objSave)
        {
            //if (All.Any(x => x.Id != objSave.Id && x.InvestigatorContactId == objSave.InvestigatorContactId && x.HolidayName == objSave.HolidayName.Trim() && x.DeletedDate == null))
            //    return "Duplicate Holiday : " + objSave.HolidayName;

            return "";
        }
        public List<DropDownDto> GetTaskListforMilestone(int parentProjectId, int? siteId, int? countryId)
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

            return _context.StudyPlanTask.Where(x => studyPlan.Select(f => f.Id).Contains(x.StudyPlanId) && x.DeletedDate == null && (x.isMileStone == true || x.DependentTaskId == null)).OrderByDescending(x => x.Id)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.TaskName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
        public decimal GetEstimatedMilestoneAmount(PaymentMilestoneDto paymentMilestoneDto)
        {
            decimal EstimatedTotal = 0;
            for (int i = 0; i < paymentMilestoneDto.StudyPlanTaskIds.Length; i++)
            {
                var studyPlanTaskData = _context.StudyPlanTask.Where(s => (s.Id == paymentMilestoneDto.StudyPlanTaskIds[i] || s.DependentTaskId == paymentMilestoneDto.StudyPlanTaskIds[i]) && s.DeletedBy == null).ToList();
                foreach (var item in studyPlanTaskData)
                {
                    EstimatedTotal += _context.StudyPlanResource.Where(s => s.StudyPlanTaskId == item.Id && s.DeletedBy == null).Sum(d => d.ConvertTotalCost).GetValueOrDefault();
                }
            }
            return EstimatedTotal;
        }
        public void AddPaymentMilestoneTaskDetail(PaymentMilestoneDto paymentMilestoneDto)
        {
            if (paymentMilestoneDto.StudyPlanTaskIds != null)
            {
                for (int i = 0; i < paymentMilestoneDto.StudyPlanTaskIds.Length; i++)
                {
                    var paymentMilestoneTaskDetail = new PaymentMilestoneTaskDetail();
                    paymentMilestoneTaskDetail.Id = 0;
                    paymentMilestoneTaskDetail.PaymentMilestoneId = paymentMilestoneDto.Id;
                    paymentMilestoneTaskDetail.StudyPlanTaskId = paymentMilestoneDto.StudyPlanTaskIds[i];
                    _context.PaymentMilestoneTaskDetail.Add(paymentMilestoneTaskDetail);
                    _context.Save();
                }
            }

        }
        public void DeletePaymentMilestoneTaskDetail(int Id)
        {
            var paymentMilestoneTaskDetail = _context.PaymentMilestoneTaskDetail.Where(s => s.PaymentMilestoneId == Id && s.DeletedBy == null).ToList();
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
            var paymentMilestoneTaskDetail = _context.PaymentMilestoneTaskDetail.Where(s => s.PaymentMilestoneId == Id && s.DeletedBy != null).ToList();
            paymentMilestoneTaskDetail.ForEach(s =>
            {
                s.DeletedDate = null;
                s.DeletedBy = null;
                _context.PaymentMilestoneTaskDetail.Update(s);
                _context.Save();
            });
        }
    }
}
