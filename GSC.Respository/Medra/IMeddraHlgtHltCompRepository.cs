using GSC.Common.GenericRespository;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;

namespace GSC.Respository.Medra
{
    public interface IMeddraHlgtHltCompRepository : IGenericRepository<MeddraHlgtHltComp>
    {
        int AddHlgtHltFileData(SaveFileDto obj);
    }
}