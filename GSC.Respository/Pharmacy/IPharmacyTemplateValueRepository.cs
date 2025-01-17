﻿using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Pharmacy;

namespace GSC.Respository.Pharmacy
{
    public interface IPharmacyTemplateValueRepository : IGenericRepository<PharmacyTemplateValue>
    {      
        List<PharmacyTemplateValueDto> GetPharmacyTemplateTree(int pharmacyEntryId);

        PharmacyTemplateValue SaveValue(PharmacyTemplateValue pharmacyTemplateValue);

        VariableDto GetPharmacyVariable(VariableDto designVariableDto, int pharmacyEntryId);
    }
}