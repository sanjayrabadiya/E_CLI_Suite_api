using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class ManageSiteRepository : GenericRespository<ManageSite, GscContext>, IManageSiteRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ManageSiteRepository(IUnitOfWork<GscContext> uow,
IJwtTokenAccesser jwtTokenAccesser)
: base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public IList<ManageSiteDto> GetManageSiteList(int Id)
        {
            return FindByInclude(t => t.Id == Id && t.DeletedBy == null).Select(c =>
                new ManageSiteDto
                {
                    Id = c.Id,
                    //InvestigatorContactId = c.InvestigatorContactId,
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
            if (All.Any(x => x.Id != objSave.Id && x.SiteName == objSave.SiteName && x.ContactName == objSave.ContactName && x.DeletedDate == null))
                return "Duplicate Site : " + objSave.SiteName;

            return "";
        }
        public List<DropDownDto> GetManageSiteDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.SiteName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
