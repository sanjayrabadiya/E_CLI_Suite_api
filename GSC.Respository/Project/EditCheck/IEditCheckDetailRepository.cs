using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.EditCheck;

namespace GSC.Respository.Project.EditCheck
{
    public interface IEditCheckDetailRepository : IGenericRepository<EditCheckDetail>
    {
        EditCheckDetailDto GetDetailById(int id);
        void UpdateEditDetail(EditCheckDetail editCheckDetail);
        ProjectDesignVariable GetCollectionSources(string annotation, int projectDesignId);
    }
}