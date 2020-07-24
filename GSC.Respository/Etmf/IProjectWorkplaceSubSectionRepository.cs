using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceSubSectionRepository : IGenericRepository<ProjectWorkplaceSubSection>
    {
        string Duplicate(ProjectWorkplaceSubSection objSave);
        ProjectWorkplaceSubSectionDto getSectionDetail(ProjectWorkplaceSubSectionDto projectWorkplaceSubSectionDto);

    }
}
