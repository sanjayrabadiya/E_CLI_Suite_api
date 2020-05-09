using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class VariableListDto
    {
        public int VariableId { get; set; }
        public string Name { get; set; }
        public CoreVariableType Type { get; set; }
        public int SeqNo { get; set; }
        public string Note { get; set; }
        public string CollectionSourcesName { get; set; }
        public string DataTypeName { get; set; }
    }
}