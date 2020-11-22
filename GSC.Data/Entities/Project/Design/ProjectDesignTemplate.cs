using System.Collections.Generic;
using GSC.Common.Base;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignTemplate : BaseEntity
    {
        public int ProjectDesignVisitId { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string ActivityName { get; set; }
        public int VariableTemplateId { get; set; }
        public IList<ProjectDesignVariable> Variables { get; set; }
        public int? ParentId { get; set; }
        public int DesignOrder { get; set; }
        public int? DomainId { get; set; }
        public bool IsRepeated { get; set; }
        public bool IsParticipantView { get; set; }
        public Domain Domain { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public VariableTemplate VariableTemplate { get; set; }
    }
}