using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface ITemplateRightsRoleListRepository : IGenericRepository<TemplateRightsRoleList>
    {
        List<DropDownDto> GetTemplateRightsRoleDropDown();
        string Duplicate(TemplateRightsRoleList objSave);
    }
}