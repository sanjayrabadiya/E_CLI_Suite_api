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
    public class PassThroughCostRepository : GenericRespository<PassThroughCost>, IPassThroughCostRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public PassThroughCostRepository(IGSCContext context,
            IMapper mapper) : base(context)
        {
            _mapper = mapper;
            _context = context;
        }
        public IList<PassThroughCostGridDto> GetpassThroughCostGrid(bool isDeleted, int studyId)
        {
            var passThroughCostGrid= All.Where(x => x.ProjectId == studyId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
            ProjectTo<PassThroughCostGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            foreach (var task in passThroughCostGrid)
            {
                if (task != null)
                {
                     task.GlobleCurrencySymbol = _context.Currency.Where(s=>s.Id == task.GlobleCurrencyId && s.DeletedBy==null).Select(x => x.CurrencySymbol).FirstOrDefault();
                }
            }
            return passThroughCostGrid;
        }
        public string Duplicate(PassThroughCostDto passThroughCostDto)
        {
            if (All.Any(x => x.Id != passThroughCostDto.Id && x.PassThroughCostActivityId == passThroughCostDto.PassThroughCostActivityId && x.CountryId == passThroughCostDto.CountryId&& x.DeletedDate == null))
            {
                return "Duplicate Pass Through Cost";
            }
            return "";

        }
        public List<DropDownPassThroughCostDto> GetCountriesDropDown(int projectId)
        {
            var studyPlan = GetstudyPlan(projectId);
            var CurrencyRate = _context.CurrencyRate.Where(s => s.StudyPlanId == studyPlan.Id  && s.DeletedDate == null).ToList();

            //get Country only  Added Currency Rate in StudyPlan
            return _context.Currency.Include(s=>s.Country).Where(z=> CurrencyRate.Select(y => y.CurrencyId).Contains(z.Id) && z.DeletedBy==null)
                .Select(c => new DropDownPassThroughCostDto
                {
                    Id = c.Country.Id,
                    Value = c.Country.CountryName,
                    CurrencyType = c.CurrencyName + " - " + c.CurrencySymbol,
                    CurrencySymbol = c.CurrencySymbol,
                    LocCurrencyId = c.Id
                }).ToList();
        }
        public PassThroughCost ConvertIntoGlobuleCurrency(PassThroughCost passThroughCost)
        {
            var studyPlan =  GetstudyPlan(passThroughCost.ProjectId);
            var Currency = _context.Currency.Include(s => s.Country).Where( z=>z.CountryId == passThroughCost.CountryId && z.DeletedBy == null).ToList();
            var CurrencyRate = _context.CurrencyRate.Where(s => s.StudyPlanId == studyPlan.Id && Currency.Select(y => y.Id).Contains((int)s.CurrencyId) && s.DeletedDate == null).FirstOrDefault();

            if(CurrencyRate != null)
            {
                passThroughCost.Total = passThroughCost.Total * CurrencyRate.LocalCurrencyRate;
                passThroughCost.CurrencyRateId = CurrencyRate.Id;
            }

            return passThroughCost;
        }
        public StudyPlan GetstudyPlan(int projectId)
        {
            return _context.StudyPlan.Where(s => s.ProjectId == projectId && s.DeletedDate == null).FirstOrDefault();
        }
    }
}
