﻿using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IProductFomRepository : IGenericRepository<MProductForm>
    {
        List<DropDownDto> GetProductFormDropDown();
        string Duplicate(MProductForm objSave);
    }
}