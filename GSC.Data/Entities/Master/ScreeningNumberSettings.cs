using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Master
{
    public class ScreeningNumberSettings : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public bool IsManualScreeningNo { get; set; }
        public bool IsSiteDependentScreeningNo { get; set; }
        public int ScreeningLength { get; set; }
        public bool? IsAlphaNumScreeningNo { get; set; }
        public int ScreeningNoseries { get; set; }
        public int? ScreeningNoStartsWith { get; set; }
        public string? PrefixScreeningNo { get; set; }
    }
}
