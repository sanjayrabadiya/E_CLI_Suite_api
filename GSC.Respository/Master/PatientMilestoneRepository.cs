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

            PaymentMilestoneData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == parentProjectId).
                        ProjectTo<PatientMilestoneGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return PaymentMilestoneData;
        }

        public string DuplicatePaymentMilestone(PatientMilestone paymentMilestone)
        {
            return "";
        }

        public decimal GetEstimatedMilestoneAmount(int visitId)
        {
            decimal EstimatedTotal = 0;
            EstimatedTotal = _context.PatientCost.Where(s => s.Id == visitId && s.DeletedBy == null).Sum(d => d.FinalCost).GetValueOrDefault();
            return EstimatedTotal;
        }

        public List<DropDownDto> GetVisitDropDown(int parentProjectId)
        {
            var data = _context.PatientCost.Include(s => s.ProjectDesignVisit).Where(d => d.ProjectId == parentProjectId && d.ProcedureId != null && d.DeletedBy == null)
                  .Select(c => new DropDownDto
                  {
                      Id = c.Id,
                      Value = c.ProjectDesignVisit.DisplayName,
                  }).Distinct().ToList();
            return data;
        }

        public BudgetPaymentFinalCostDto GetFinalPatienTotal(int projectId)
        {
            BudgetPaymentFinalCostDto data = new BudgetPaymentFinalCostDto();
            var patientPaybalAmount = _context.BudgetPaymentFinalCost.FirstOrDefault(x => x.ProjectId == projectId && x.DeletedDate == null && x.MilestoneType == Helper.MilestoneType.PatientCost);
            data.PatientCostAmount = patientPaybalAmount?.FinalTotalAmount ?? 0;
            return data;
        }
    }
}
