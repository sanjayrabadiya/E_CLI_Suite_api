using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Rights;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Project.Rights
{
    public interface IProjectModuleRightsRepository : IGenericRepository<GSC.Data.Entities.Project.Rights.ProjectModuleRights>
    {
        void Save(StudyModuleDto details);
    }
}
