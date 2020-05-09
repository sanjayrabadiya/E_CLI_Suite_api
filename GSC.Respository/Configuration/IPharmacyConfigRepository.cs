using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Configuration;

namespace GSC.Respository.Configuration
{
    public interface IPharmacyConfigRepository : IGenericRepository<PharmacyConfig>
    {
        List<DropDownDto> GetVariableTemplateByFormId(int formId);
    }
}