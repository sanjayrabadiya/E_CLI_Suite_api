using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;

namespace GSC.Respository.Master
{
    public interface IDomainRepository : IGenericRepository<Data.Entities.Master.Domain>
    {
        string ValidateDomain(Data.Entities.Master.Domain objSave);

        List<DropDownDto> GetDomainDropDown();
        List<DomainDto> GetDomainAll(bool isDeleted);
        List<DropDownDto> GetDomainByProjectDesignDropDown(int projectDesignId);
    }
}