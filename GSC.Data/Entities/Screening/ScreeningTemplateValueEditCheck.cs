using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.EditCheck;
using GSC.Helper;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateValueEditCheck : BaseEntity
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int ScreeningEntryId { get; set; }
        public int EditCheckDetailId { get; set; }
        public EditCheckDetail EditCheckDetail { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        public DataType? DataType { get; set; }
        public EditCheckValidateType ValidateType { get; set; }
        
    }
}