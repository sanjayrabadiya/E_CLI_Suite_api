using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignVisitStatusRepository : IGenericRepository<ProjectDesignVisitStatus>
    {
        //added by vipul for get visit status by visit id on 23092020

        ProjectDesignVisitStatusDto GetProjectDesignVisitStatusByVisitId(int VisitId);
    }
}