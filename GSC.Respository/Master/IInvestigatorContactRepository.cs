using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IInvestigatorContactRepository : IGenericRepository<InvestigatorContact>
    {
        List<DropDownDto> GetInvestigatorContactDropDown(int cityId);
        List<InvestigatorContactDto> GetInvestigatorContact(bool isDeleted);
        string Duplicate(InvestigatorContact objSave);

        List<DropDownDto> GetAllInvestigatorContactDropDown();
    }
}