﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementKitDetailRepository : IGenericRepository<SupplyManagementKITDetail>
    {
        
    }
}