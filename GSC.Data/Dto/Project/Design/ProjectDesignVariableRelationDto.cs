using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Design
{
  

    public class ProjectDesignVariableRelationDto
    {
        public int RelationProjectDesignVariableId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int Id { get; set; }
        public int ProjectDesignVisitId { get; set; }

    }
}
