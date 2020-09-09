using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IInvestigatorContactDetailRepository : IGenericRepository<InvestigatorContactDetail>
    {
        IList<InvestigatorContactDetailGridDto> GetContactList(int InvestigatorContactId, bool isDeleted);

        string DuplicateContact(InvestigatorContactDetail objSave);

        List<DropDownDto> GetInvestigatorContactDetailDropDown();
    }
}
