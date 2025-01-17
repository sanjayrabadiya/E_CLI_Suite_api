﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class PassthroughSiteContractRepository : GenericRespository<PassthroughSiteContract>, IPassthroughSiteContractRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public PassthroughSiteContractRepository(IGSCContext context,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }
        public IList<PassthroughSiteContractGridDto> GetPassthroughSiteContractList(bool isDeleted, int siteContractId)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.SiteContractId == siteContractId).
                ProjectTo<PassthroughSiteContractGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public string Duplicate(PassthroughSiteContractDto passthroughSiteContractDto)
        {
            if (All.Any(x => x.Id != passthroughSiteContractDto.Id && x.SiteContractId == passthroughSiteContractDto.SiteContractId && x.PassThroughCostActivityId == passthroughSiteContractDto.PassThroughCostActivityId && x.DeletedDate == null))
            {
                return "Duplicate Pass Through";
            }
            return "";
        }

        public List<decimal> GetPassthroughTotalAmount(int parentProjectId, int siteId, int passThroughCostActivityId)
        {
            List<decimal> obj = new List<decimal>();
            var siteCountryId = _context.Project.Include(m => m.ManageSite).
              Where(w => w.Id == siteId && w.ParentProjectId == parentProjectId && w.DeletedBy == null).Select(x => x.ManageSite.City.State.CountryId).FirstOrDefault();

            var PassThroughCost = _context.PassThroughCost.Where(s => s.PassThroughCostActivityId == passThroughCostActivityId && s.ProjectId == parentProjectId && s.CountryId == siteCountryId && s.DeletedBy == null).
                       Sum(d => d.CurrencyRate.LocalCurrencyRate * d.Rate).GetValueOrDefault();

            obj.Add(PassThroughCost);
            obj.Add(siteCountryId);

            return obj;
        }
    }
}
