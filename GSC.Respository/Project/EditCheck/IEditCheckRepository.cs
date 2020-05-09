using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.EditCheck;

namespace GSC.Respository.Project.EditCheck
{
    public interface IEditCheckRepository : IGenericRepository<Data.Entities.Project.EditCheck.EditCheck>
    {
        EditCheckDto GetEditCheckDetail(int id, bool isDeleted);
        void SaveEditCheck(Data.Entities.Project.EditCheck.EditCheck editCheck);
        List<EditCheckDto> GetAll(int projectDesignId, bool isDeleted);
        Data.Entities.Project.EditCheck.EditCheck UpdateFormula(int id);
        Data.Entities.Project.EditCheck.EditCheck CopyTo(int id);
       
    }
}