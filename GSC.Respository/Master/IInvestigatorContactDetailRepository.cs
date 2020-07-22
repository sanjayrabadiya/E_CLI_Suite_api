using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IInvestigatorContactDetailRepository :  IGenericRepository<InvestigatorContactDetail>
    {
        IList<InvestigatorContactDetailDto> GetContactList(int clientId);

        string DuplicateContact(InvestigatorContactDetail objSave);
    }
}
