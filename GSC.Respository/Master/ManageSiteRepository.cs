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
using GSC.Shared.JWTAuth;

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
            if (All.Any(x => x.Id != objSave.Id && x.SiteAddress == objSave.SiteAddress.Trim() && x.DeletedDate == null))
                return "Duplicate Site Address: " + objSave.SiteAddress;

            return "";
        }
        public List<DropDownDto> GetManageSiteDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.Status == true && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.SiteName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
        public void UpdateRole(ManageSite ManageSite)
        {
            var siterole = _context.ManageSiteRole.Where(x => x.ManageSiteId == ManageSite.Id
                                                               && ManageSite.ManageSiteRole.Select(x => x.TrialTypeId).Contains(x.TrialTypeId)
                                                               && x.DeletedDate == null).ToList();

            ManageSite.ManageSiteRole.ForEach(z =>
            {
                var role = siterole.Where(x => x.ManageSiteId == z.ManageSiteId && x.TrialTypeId == z.TrialTypeId).FirstOrDefault();
                if (role == null)
                {
                    _context.ManageSiteRole.Add(z);
                }
            });

            var managesiteRole = _context.ManageSiteRole.Where(x => x.ManageSiteId == ManageSite.Id && x.DeletedDate == null)
                .ToList();

            managesiteRole.ForEach(t =>
            {
                var role = siterole.Where(x => x.ManageSiteId == t.ManageSiteId && x.TrialTypeId == t.TrialTypeId).FirstOrDefault();
                if (role == null)
                {
                    //delete
                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _context.ManageSiteRole.Update(t);
                }
            });
        }
    }
}
