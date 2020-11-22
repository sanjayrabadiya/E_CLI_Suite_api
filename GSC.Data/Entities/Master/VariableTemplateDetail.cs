using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class VariableTemplateDetail : BaseEntity
    {
        public int VariableTemplateId { get; set; }
        public int VariableId { get; set; }
        public int SeqNo { get; set; }
        public string Note { get; set; }
        public Variable Variable { get; set; }

        [NotMapped] public string VariableCategoryName { get; set; }
    }
}