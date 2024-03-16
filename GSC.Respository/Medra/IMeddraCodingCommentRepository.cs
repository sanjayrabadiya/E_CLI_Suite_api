using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;

namespace GSC.Respository.Medra
{
    public interface IMeddraCodingCommentRepository : IGenericRepository<MeddraCodingComment>
    {
        MeddraCodingComment GetLatest(int MeddraCodingId);
        MeddraCodingComment CheckWhileScopingVersionUpdate(int MeddraCodingId);
        IList<MeddraCodingCommentDto> GetData(int MeddraCodingId);
    }
}