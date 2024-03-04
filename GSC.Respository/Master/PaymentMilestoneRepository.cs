using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
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
           var PaymentMilestoneData =  All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && (x.ProjectId == parentProjectId || x.SiteId == siteId || x.CountryId == countryId)).
            ProjectTo<PaymentMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            PaymentMilestoneData.ForEach(x =>
            {
                x.SitedName = _context.Project.Include(s=>s.ManageSite).Where(w=>w.Id==x.SiteId).Select(d=>d.ProjectCode==null?d.ManageSite.SiteName:d.ProjectCode).FirstOrDefault();
            });
            return PaymentMilestoneData;
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
            if (paymentMilestoneDto.MilestoneType == MilestoneType.ResourceCost && paymentMilestoneDto.StudyPlanTaskIds != null)
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
            else if (paymentMilestoneDto.MilestoneType == MilestoneType.PatientCost && paymentMilestoneDto.PatientCostIds != null)
            {
                foreach (var visit in paymentMilestoneDto.PatientCostIds)
                {
                    EstimatedTotal += _context.PatientCost.Where(s => s.Id == visit && s.DeletedBy == null).Sum(d => d.FinalCost).GetValueOrDefault();

                }
            }     
            return EstimatedTotal;
        }
        //resourch
        public void AddPaymentMilestoneTaskDetail(PaymentMilestoneDto paymentMilestoneDto)
        {
            foreach (var item in paymentMilestoneDto.StudyPlanTaskIds)
            {
                var paymentMilestoneTaskDetail = new PaymentMilestoneTaskDetail();
                paymentMilestoneTaskDetail.Id = 0;
                paymentMilestoneTaskDetail.PaymentMilestoneId = paymentMilestoneDto.Id;
                paymentMilestoneTaskDetail.StudyPlanTaskId = item;
                _context.PaymentMilestoneTaskDetail.Add(paymentMilestoneTaskDetail);
                _context.Save();
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
        //visit
        public void AddPaymentMilestoneVisitDetail(PaymentMilestoneDto paymentMilestoneDto)
        {
            foreach (var item in paymentMilestoneDto.PatientCostIds)
            {
                var paymentMilestoneVisitDetail = new PaymentMilestoneVisitDetail();
                paymentMilestoneVisitDetail.Id = 0;
                paymentMilestoneVisitDetail.PaymentMilestoneId = paymentMilestoneDto.Id;
                paymentMilestoneVisitDetail.PatientCostId = item;
                _context.PaymentMilestoneVisitDetail.Add(paymentMilestoneVisitDetail);
                _context.Save();
            }
        }
        public void DeletePaymentMilestoneVisitDetail(int Id)
        {
            var paymentMilestoneVisitDetail = _context.PaymentMilestoneVisitDetail.Where(s => s.PaymentMilestoneId == Id && s.DeletedBy == null).ToList();
            paymentMilestoneVisitDetail.ForEach(s =>
            {
                s.DeletedDate = DateTime.UtcNow;
                s.DeletedBy = _jwtTokenAccesser.UserId;
                _context.PaymentMilestoneVisitDetail.Update(s);
                _context.Save();
            });
        }
        public void ActivePaymentMilestoneVisitDetail(int Id)
        {
            var paymentMilestoneVisitDetail = _context.PaymentMilestoneVisitDetail.Where(s => s.PaymentMilestoneId == Id && s.DeletedBy != null).ToList();
            paymentMilestoneVisitDetail.ForEach(s =>
            {
                s.DeletedDate = null;
                s.DeletedBy = null;
                _context.PaymentMilestoneVisitDetail.Update(s);
                _context.Save();
            });
        }
        public List<DropDownProcedureDto> GetParentProjectDropDown(int parentProjectId)
        {
            return _context.PatientCost.Include(s=>s.Procedure).Where(d=>d.ProjectId == parentProjectId && d.ProcedureId != null && d.DeletedBy==null)
                 .Select(c => new DropDownProcedureDto
                 {
                     Id = c.Procedure.Id,
                     Value = c.Procedure.Name,
                 }).Distinct().ToList();
        }
        public List<DropDownDto> GetVisitDropDown(int parentProjectId, int procedureId)
        {
           var data = _context.PatientCost.Include(s => s.ProjectDesignVisit).Where(d => d.ProjectId == parentProjectId && d.ProcedureId == procedureId && d.ProcedureId != null  && d.DeletedBy == null)
                 .Select(c => new DropDownDto{
                     Id = c.Id,
                     Value = c.ProjectDesignVisit.DisplayName,
                 }).ToList();
            return data;
        }
    }
}
