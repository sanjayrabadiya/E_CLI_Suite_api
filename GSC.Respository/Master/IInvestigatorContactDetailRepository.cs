using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IInvestigatorContactDetailRepository :  IGenericRepository<InvestigatorContactDetail>
    {
        IList<InvestigatorContactDetailDto> GetContactList(int clientId);

        IList<InvestigatorContactDetailDto> GetContactList(int projectId, bool isDeleted);

        string DuplicateContact(InvestigatorContactDetail objSave);

        List<DropDownDto> GetInvestigatorContactDetailDropDown();
    }
}
