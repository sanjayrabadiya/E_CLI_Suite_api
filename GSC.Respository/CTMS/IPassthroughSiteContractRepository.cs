﻿using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.Master
{
    public interface IPassthroughSiteContractRepository : IGenericRepository<PassthroughSiteContract>
    {
        IList<PassthroughSiteContractGridDto> GetPassthroughSiteContractList(bool isDeleted, int siteContractId);
        string Duplicate(PassthroughSiteContractDto PassthroughSiteContractDto);
        List<decimal> GetPassthroughTotalAmount(int parentProjectId, int siteId, int passThroughCostActivityId);
    }
}
