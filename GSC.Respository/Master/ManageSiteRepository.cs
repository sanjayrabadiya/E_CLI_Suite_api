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

namespace GSC.Respository.Master
{
    public class ManageSiteRepository : GenericRespository<ManageSite>, IManageSiteRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public ManageSiteRepository(IGSCContext context,
        IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
        : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<ManageSiteGridDto> GetManageSites(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<ManageSiteGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }

        public IList<ManageSiteDto> GetManageSiteList(int Id)
        {
            return FindByInclude(t => t.Id == Id && t.DeletedBy == null).Select(c =>
                new ManageSiteDto
                {
                    Id = c.Id,
                    SiteName = c.SiteName,
                    ContactName = c.ContactName,
                    SiteEmail = c.SiteEmail,
                    ContactNumber = c.ContactNumber,
                    SiteAddress = c.SiteAddress,
                    Status = c.Status,
                    CompanyId = c.CompanyId
                }).OrderByDescending(t => t.Id).ToList();
        }

        public string Duplicate(ManageSite objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.SiteAddress == objSave.SiteAddress && x.DeletedDate == null))
                return "Duplicate Site Address: " + objSave.SiteAddress;

            return "";
        }
        public List<DropDownDto> GetManageSiteDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.SiteName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
        public void UpdateRole(ManageSite ManageSite)
        {
            var roleDelete = _context.ManageSiteRole.Where(x => x.ManageSiteId == ManageSite.Id).ToList();
            foreach (var item in roleDelete)
            {
                item.DeletedDate = DateTime.Now;
                _context.ManageSiteRole.Update(item);
            }

            for (var i = 0; i < ManageSite.ManageSiteRole.Count; i++)
            {
                var i1 = i;
                var siterole = _context.ManageSiteRole.Where(x => x.ManageSiteId == ManageSite.ManageSiteRole[i1].ManageSiteId
                                                               && x.TrialTypeId == ManageSite.ManageSiteRole[i1].TrialTypeId)
                    .FirstOrDefault();
                if (siterole != null)
                {
                    siterole.DeletedDate = null;
                    siterole.DeletedBy = null;
                    ManageSite.ManageSiteRole[i] = siterole;
                }
            }
        }
    }
}
