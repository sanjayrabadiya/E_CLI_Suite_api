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
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class PatientMilestoneRepository : GenericRespository<PatientMilestone>, IPatientMilestoneRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public PatientMilestoneRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public IList<PatientMilestoneGridDto> GetPaymentMilestoneList(int parentProjectId, bool isDeleted)
        {
            var PaymentMilestoneData = new List<PatientMilestoneGridDto>();

                PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == parentProjectId ).
                            ProjectTo<PatientMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return PaymentMilestoneData;
        }

        public string DuplicatePaymentMilestone(PatientMilestone paymentMilestone)
        {
            return "";
        }

        public decimal GetEstimatedMilestoneAmount(PatientMilestoneDto paymentMilestoneDto)
        {
            decimal EstimatedTotal = 0;
  
                foreach (var visit in paymentMilestoneDto.PatientCostIds)
                {
                    EstimatedTotal += _context.PatientCost.Where(s => s.Id == visit && s.DeletedBy == null).Sum(d => d.FinalCost).GetValueOrDefault();
                }
            return EstimatedTotal;
        }

        public void AddPaymentMilestoneVisitDetail(PatientMilestoneDto paymentMilestoneDto)
        {
            foreach (var item in paymentMilestoneDto.PatientCostIds)
            {
                var paymentMilestoneVisitDetail = new PaymentMilestoneVisitDetail();
                paymentMilestoneVisitDetail.Id = 0;
                paymentMilestoneVisitDetail.PatientMilestoneId = paymentMilestoneDto.Id;
                paymentMilestoneVisitDetail.PatientCostId = item;
                _context.PaymentMilestoneVisitDetail.Add(paymentMilestoneVisitDetail);
                _context.Save();
            }
        }
        public void DeletePaymentMilestoneVisitDetail(int Id)
        {
            var paymentMilestoneVisitDetail = _context.PaymentMilestoneVisitDetail.Where(s => s.PatientMilestoneId == Id && s.DeletedBy == null).ToList();
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
            var paymentMilestoneVisitDetail = _context.PaymentMilestoneVisitDetail.Where(s => s.PatientMilestoneId == Id && s.DeletedBy != null).ToList();
            paymentMilestoneVisitDetail.ForEach(s =>
            {
                s.DeletedDate = null;
                s.DeletedBy = null;
                _context.PaymentMilestoneVisitDetail.Update(s);
                _context.Save();
            });
        }

        public List<DropDownDto> GetVisitDropDown(int parentProjectId)
        {
            var data = _context.PatientCost.Include(s => s.ProjectDesignVisit).Where(d => d.ProjectId == parentProjectId && d.ProcedureId != null && d.DeletedBy == null)
                  .Select(c => new DropDownDto
                  {
                      Id = c.Id,
                      Value = c.ProjectDesignVisit.DisplayName,
                  }).ToList();
            return data;
        }

        public BudgetPaymentFinalCostDto GetFinalPatienTotal(int projectId)
        {
            BudgetPaymentFinalCostDto data = new BudgetPaymentFinalCostDto();
            decimal? totalFinalCost = 0;
            decimal? total = 0;
            var patientcostprocedTemp = new List<PatientCostGridData>();
            var duplicates = _context.PatientCost.Include(s => s.Procedure).Where(x => x.DeletedBy == null && x.ProjectId == projectId && x.ProcedureId != null).GroupBy(i => i.Procedure.CurrencyId).Where(x => x.Count() > 1).Select(val => val.Key).ToList();
            for (var i = 0; i < duplicates.Count; i++)
            {
                patientcostprocedTemp = _context.PatientCost.Include(s => s.Procedure).Where(x => x.DeletedBy == null && x.ProjectId == projectId && x.ProcedureId != null && x.Procedure.CurrencyId == duplicates[i]).
                Select(t => new PatientCostGridData
                {
                    ProcedureId = t.ProcedureId,
                    PatientCount = t.PatientCount
                }).Distinct().ToList();

                var PatientCostVisit = _context.PatientCost.Include(s => s.ProjectDesignVisit).
                    Where(x => patientcostprocedTemp.Select(r => r.ProcedureId).Contains(x.ProcedureId) && x.ProjectId == projectId && x.DeletedBy == null).
                    GroupBy(g => g.ProjectDesignVisitId)
                    .Select(t => new VisitGridData
                    {
                        FinalCost = t.Sum(r => r.FinalCost)
                    }).ToList();
                totalFinalCost = 0;
                PatientCostVisit.ForEach(s => {
                    totalFinalCost += s.FinalCost;
                });
                total += totalFinalCost * patientcostprocedTemp.Select(s => s.PatientCount).FirstOrDefault();
            }

            //one time Add Paybal Amount id diduct in main total
            var patientPaybalAmount = _context.PatientMilestone.Where(w => w.DeletedDate == null && w.ProjectId == projectId).Sum(s => s.PaybalAmount);

            data.PatientCostAmount = Convert.ToDecimal(total- patientPaybalAmount);

            return data;
        }
    }
}
