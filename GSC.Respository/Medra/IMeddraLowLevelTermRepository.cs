using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;

namespace GSC.Respository.Medra
{
    public interface IMeddraLowLevelTermRepository : IGenericRepository<MeddraLowLevelTerm>
    {
        int AddLltFileData(SaveFileDto obj);
        IList<MeddraCodingSearchDetails> GetManualCodes(MeddraCodingSearchDto search);
    }
}