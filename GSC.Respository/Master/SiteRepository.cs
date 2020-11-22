using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class SiteRepository : GenericRespository<Site, GscContext>, ISiteRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public SiteRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _uow = uow;
        }

        public List<SiteGridDto> GetSiteList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<SiteGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }


        public List<SiteGridDto> GetSiteById(int InvestigatorContactId, bool isDeleted)
        {
            return All.Where(x => x.InvestigatorContactId == InvestigatorContactId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                   ProjectTo<SiteGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(Site objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ManageSiteId == objSave.ManageSiteId && x.InvestigatorContactId == objSave.InvestigatorContactId && x.DeletedDate == null))
                return "Duplicate Site name : " + objSave.ManageSiteId;
            return "";
        }

        public string DeleteSite(SiteDto objSave)
        {
            var Sites = Context.Site.AsNoTracking().Where(x => x.InvestigatorContactId == objSave.InvestigatorContactId && x.DeletedDate == null
            && !objSave.ManageSiteIds.Contains(x.ManageSiteId)).ToList();
            foreach (var item in Sites)
            {
                Delete(item);
                _uow.Save();
            }
            return "";
        }

    }
}
