using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface ISiteContractRepository : IGenericRepository<SiteContract>
    {
        string Duplicate(SiteContractDto SiteContractDto);
        IList<SiteContractGridDto> GetSiteContractList(bool isDeleted, int studyId, int siteId);
        void CreateContractTemplateFormat(ContractTemplateFormat contractTemplateFormat, SiteContractDto siteContractDto);
    }
}
