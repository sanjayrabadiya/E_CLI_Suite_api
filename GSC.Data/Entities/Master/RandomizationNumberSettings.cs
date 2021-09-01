using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Master
{
    public class RandomizationNumberSettings : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public bool IsManualRandomNo { get; set; }
        public bool IsSiteDependentRandomNo { get; set; }
        public int RandomNoLength { get; set; }
        public bool? IsAlphaNumRandomNo { get; set; }
        public int RandomizationNoseries { get; set; }
        public int? RandomNoStartsWith { get; set; }
        public string PrefixRandomNo { get; set; }
    }
}
