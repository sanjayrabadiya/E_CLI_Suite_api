using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface ILettersFormateRepository : IGenericRepository<LettersFormate>
    {
        List<LettersFormateGridDto> GetlettersFormateList(bool isDeleted);
        string Duplicate(LettersFormate objSave);
    }
}