using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;

namespace GSC.Respository.Medra
{
    public interface IMeddraSocHlgtCompRepository : IGenericRepository<MeddraSocHlgtComp>
    {
        int AddSocHlgtFileData(SaveFileDto obj);
    }
}