using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System.Collections.Generic;

namespace GSC.Respository.Master
{
    public interface ISiteRepository : IGenericRepository<Site>
    {
        string Duplicate(Site objSave);
        List<SiteGridDto> GetSiteList(bool isDeleted);
        List<SiteGridDto> GetSiteById(int InvestigatorContactId, bool isDeleted);
    }
}