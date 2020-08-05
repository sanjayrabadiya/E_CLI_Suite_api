using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IContactTypeRepository : IGenericRepository<ContactType>
    {
        List<DropDownDto> GetContactTypeDropDown();
        string Duplicate(ContactType objSave);
        List<ContactTypeGridDto> GetContactTypeList(bool isDeleted);
    }
}