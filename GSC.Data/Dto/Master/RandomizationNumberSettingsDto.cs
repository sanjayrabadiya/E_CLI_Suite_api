using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Master
{
    public class RandomizationNumberSettingsDto : BaseDto
    {
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public bool IsManualRandomNo { get; set; }
        public bool IsSiteDependentRandomNo { get; set; }
        public int RandomNoLength { get; set; }
        public bool? IsAlphaNumRandomNo { get; set; }
        public int RandomizationNoseries { get; set; }
        public int? RandomNoStartsWith { get; set; }
        public string PrefixRandomNo { get; set; }
        public bool DisableRow { get; set; }
        public List<RandomizationNumberSettingsDto> RandomizationNumberSettingsSites { get; set; }
    }
}
