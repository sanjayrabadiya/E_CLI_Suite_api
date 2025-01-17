﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
   public interface IProjectWorkplaceDetailRepository : IGenericRepository<EtmfProjectWorkPlace>
    {
        List<DropDownDto> GetCountryByWorkplace(int ParentProjectId);
        List<DropDownDto> GetSiteByWorkplace(int ParentProjectId);
    }
}
