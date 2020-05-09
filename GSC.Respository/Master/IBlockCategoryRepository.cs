using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IBlockCategoryRepository : IGenericRepository<BlockCategory>
    {
        List<DropDownDto> GetBlockCategoryDropDown();
        string Duplicate(BlockCategory objSave);
    }
}