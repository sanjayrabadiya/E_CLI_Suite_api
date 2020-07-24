using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceSubSectionArtifactRepository : IGenericRepository<ProjectWorkplaceSubSectionArtifact>
    {
        string Duplicate(ProjectWorkplaceSubSectionArtifact objSave);
        ProjectWorkplaceSubSectionDto getSectionDetail(ProjectWorkplaceSubSectionArtifactDto projectWorkplaceSubSectionArtifactDto);
        List<DropDownDto> GetDrodDown(int subsectionId);
        ProjectWorkplaceSubSectionArtifactDto UpdateArtifactDetail(ProjectWorkplaceSubSectionArtifactDto projectWorkplaceSubSectionDto);
        string DeletArtifactDetailFolder(int id);


    }
}
