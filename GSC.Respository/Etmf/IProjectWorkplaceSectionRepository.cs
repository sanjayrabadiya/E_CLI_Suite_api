﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceSectionRepository : IGenericRepository<EtmfProjectWorkPlace>
    {
        List<DropDownDto> GetProjectWorkPlaceSectionDropDown(int zoneId);
    }
}
