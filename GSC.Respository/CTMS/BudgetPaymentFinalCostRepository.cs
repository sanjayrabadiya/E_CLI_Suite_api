using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class BudgetPaymentFinalCostRepository : GenericRespository<BudgetPaymentFinalCost>, IBudgetPaymentFinalCostRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public BudgetPaymentFinalCostRepository(IGSCContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
            _context = context;
        }
        public List<BudgetPaymentFinalCostGridDto> GetBudgetPaymentFinalCostList(int projectId, bool isdelete)
        {
            return All.Where(x => (isdelete ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == projectId).
                   ProjectTo<BudgetPaymentFinalCostGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public BudgetPaymentFinalCost GetFinalBudgetCost(int projectId)
        {
            var siteIds = _context.Project.Where(s => s.DeletedDate == null && s.ParentProjectId == projectId).Select(s => s.Id).ToList();
            BudgetPaymentFinalCost data = new BudgetPaymentFinalCost();
            var resourcecost = _context.StudyPlanResource.
                                Include(s => s.StudyPlanTask).
                                ThenInclude(s => s.StudyPlan)
                                .Where(s => s.StudyPlanTask.StudyPlan.ProjectId == projectId || siteIds.Contains(s.StudyPlanTask.StudyPlan.ProjectId)).Sum(s => s.ConvertTotalCost);
            data.ProfessionalCostAmount = Convert.ToDecimal(resourcecost);

            var patientCost = _context.PatientCost.Where(s => s.ProjectId == projectId).Sum(s => s.FinalCost);
            data.PatientCostAmount = Convert.ToDecimal(patientCost);

            var passThrouCost = _context.PassThroughCost.Where(s => s.ProjectId == projectId).Sum(s => s.Total);
            data.PassThroughCost = Convert.ToDecimal(passThrouCost);

            return data;
        }
    }
}
