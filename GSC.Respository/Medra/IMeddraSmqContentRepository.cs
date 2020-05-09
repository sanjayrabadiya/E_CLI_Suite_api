using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;

namespace GSC.Respository.Medra
{
    public interface IMeddraSmqContentRepository : IGenericRepository<MeddraSmqContent>
    {
        int AddSmqContentFileData(SaveFileDto obj);
    }
}