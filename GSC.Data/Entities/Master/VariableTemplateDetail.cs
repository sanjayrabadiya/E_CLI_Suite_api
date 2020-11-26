using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class VariableTemplateDetail : BaseEntity, ICommonAduit
    {
        public int VariableTemplateId { get; set; }
        public int VariableId { get; set; }
        public int SeqNo { get; set; }
        public string Note { get; set; }
        public Variable Variable { get; set; }

        [NotMapped] public string VariableCategoryName { get; set; }
    }
}