using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IDomainClassRepository : IGenericRepository<DomainClass>
    {
        string ValidateDomainClass(DomainClass objSave);
        List<DropDownDto> GetDomainClassDropDown();
    }
}