using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class LettersFormate : BaseEntity, ICommonAduit
    {
        public string LetterCode { get; set; }
        public string LetterName { get; set; }
        public string Description { get; set; }
        public string LetterBody { get; set; }
    }
}
