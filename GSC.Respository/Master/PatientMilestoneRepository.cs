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

        public List<decimal> GetEstimatedMilestoneAmount(int ParentProjectId, int visitId)
        {
            List<decimal> obj = new List<decimal>();

            var EstimatedTotal = _context.PatientCost.Where(s => s.ProjectDesignVisitId == visitId && s.ProcedureId !=null && s.ProjectId== ParentProjectId && s.DeletedBy == null).Sum(d => d.FinalCost * d.PatientCount).GetValueOrDefault();
            var TotalPatient = _context.PatientCost.Where(s => s.ProjectDesignVisitId == visitId && s.ProcedureId != null && s.ProjectId == ParentProjectId && s.DeletedBy == null).Sum(d => d.PatientCount);
            
            obj.Add(EstimatedTotal);
            obj.Add(TotalPatient);

            return obj;
        }

        public List<DropDownDto> GetVisitDropDown(int parentProjectId)
        {
            var data = _context.PatientCost.Include(s => s.ProjectDesignVisit).Where(d => d.ProjectId == parentProjectId && d.ProcedureId != null && d.DeletedBy == null)
                  .Select(c => new DropDownDto
                  {
                      Id = c.Id,
                      Value = c.ProjectDesignVisit.DisplayName,
                      ExtraData=c.ProjectDesignVisitId

                  }).Distinct().ToList();
            return data;
        }

        public decimal GetFinalPatienTotal(int projectId)
        {
            //first time Total get from Budget Payment FinalCost
            decimal? paymentFinalCost = _context.PatientMilestone.Where(s => s.ProjectId == projectId && s.DeletedBy == null).OrderBy(s => s.Id).Select(r => r.visitTotal).LastOrDefault();
            paymentFinalCost ??= _context.BudgetPaymentFinalCost.Where(x => x.ProjectId == projectId && x.MilestoneType == MilestoneType.PatientCost && x.DeletedDate == null).Select(s => s.FinalTotalAmount).FirstOrDefault();

            return paymentFinalCost ?? 0;

        }
    }
}
