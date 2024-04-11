using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.Extension;
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
            var data = All.Where(x => (isdelete ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == projectId).
                  ProjectTo<BudgetPaymentFinalCostGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            if (data.Count > 0)
            {
                data[0].GlobleCurrencySymbol = _context.StudyPlan.Include(s => s.Currency).Where(s => s.DeletedBy == null && s.ProjectId == projectId).Select(d => d.Currency.CurrencySymbol).FirstOrDefault();
            }
            return data;
        }

        public BudgetPaymentFinalCostDto GetFinalBudgetCost(int projectId)
        {
            var siteIds = _context.Project.Where(s => s.DeletedDate == null && s.ParentProjectId == projectId).Select(s => s.Id).ToList();
            BudgetPaymentFinalCostDto data = new BudgetPaymentFinalCostDto();
            var resourcecost = _context.StudyPlanResource.
                                Include(s => s.StudyPlanTask).
                                ThenInclude(s => s.StudyPlan)
                                .Where(s => s.DeletedBy == null && s.StudyPlanTask.StudyPlan.ProjectId == projectId || siteIds.Contains(s.StudyPlanTask.StudyPlan.ProjectId)).Sum(s => s.ConvertTotalCost);
            data.ProfessionalCostAmount = Convert.ToDecimal(resourcecost);

            var patientCost = _context.PatientCost.Where(s => s.DeletedBy == null && s.ProjectId == projectId).Sum(s => s.FinalCost);
            data.PatientCostAmount = Convert.ToDecimal(patientCost);

            var passThrouCost = _context.PassThroughCost.Where(s => s.DeletedBy == null && s.ProjectId == projectId).Sum(s => s.Total);
            data.PassThroughCost = Convert.ToDecimal(passThrouCost);

            return data;
        }

        public string Duplicate(BudgetPaymentFinalCostDto budgetPaymentFinalCostDto)
        {
            return All.Any(x => x.Id != budgetPaymentFinalCostDto.Id && x.MilestoneType == budgetPaymentFinalCostDto.MilestoneType && x.ProjectId == budgetPaymentFinalCostDto.ProjectId && x.DeletedDate == null)
                ? "Duplicate Pass Final Cost Type :" + budgetPaymentFinalCostDto.MilestoneType
                : "";
        }
    }
}
