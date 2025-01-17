﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementConfigurationRepository : IGenericRepository<SupplyManagementConfiguration>
    {
        List<SupplyManagementConfigurationGridDto> GetSupplyManagementTemplateList(bool isDeleted);
        string Duplicate(SupplyManagementConfiguration objSave);
        SupplyManagementConfiguration GetTemplateByScreenCode(string screenCode);
    }
}