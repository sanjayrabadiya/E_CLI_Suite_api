using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Entities.Medra
{
    public class StudyScoping : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }        
        public bool IsByAnnotation { get; set; }
        public int? DomainId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int MedraConfigId { get; set; }       
        public int CoderProfile { get; set; }
        public int? CoderApprover { get; set; }
        public int? CompanyId { get; set; }
        public Domain Domain { get; set; }        
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public MedraConfig MedraConfig { get; set; }
        public Entities.Master.Project Project { get; set; }
    }
}
