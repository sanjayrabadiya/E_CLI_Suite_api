using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignTemplateNoteRepository : IGenericRepository<ProjectDesignTemplateNote>
    {
        List<ProjectDesignTemplateNoteGridDto> GetProjectDesignTemplateNoteList(int templateId);
    }
}