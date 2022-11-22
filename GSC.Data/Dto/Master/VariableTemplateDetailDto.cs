using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class VariableTemplateDetailDto : BaseDto
    {
        public int VariableTemplateId { get; set; }
        public int VariableId { get; set; }
        public int SeqNo { get; set; }
        public string Note { get; set; }
        public string Name { get; set; }
        public Variable Variable { get; set; }
        public string CollectionSourcesName { get; set; }
        public CoreVariableType Type { get; set; }
        public string DataTypeName { get; set; }
        public string VariableCategoryName { get; set; }
    }
}