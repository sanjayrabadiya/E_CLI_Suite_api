using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;

namespace GSC.Respository.Medra
{
    public interface IMeddraSmqListRepository : IGenericRepository<MeddraSmqList>
    {
        int AddSmqListFileData(SaveFileDto obj);
    }
}