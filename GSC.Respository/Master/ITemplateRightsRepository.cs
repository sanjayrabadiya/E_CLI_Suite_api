using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface ITemplateRightsRepository : IGenericRepository<TemplateRights>
    {
        List<DropDownDto> GetDrugDropDown();
        string Duplicate(TemplateRights objSave);
    }
}