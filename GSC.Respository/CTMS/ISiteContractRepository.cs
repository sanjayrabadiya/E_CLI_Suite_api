﻿using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.Master
{
    public interface ISiteContractRepository : IGenericRepository<SiteContract>
    {
        IList<SiteContractGridDto> GetSiteContractList(bool isDeleted, int studyId, int siteId);
    }
}
