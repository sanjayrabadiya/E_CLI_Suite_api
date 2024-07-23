using GSC.Common.Base;
using GSC.Common.Common;
using System.Collections.Generic;

namespace GSC.Data.Entities.License
{
    public class LiecenceObj : BaseEntity,ICommonAduit
    {
        public string Object { get; set; }
    }

    public class LiecenceObject
    {
        public int NoofStudy { get; set; }
        public bool Duration { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpiredDate { get; set; }
        public bool MobileApp { get; set; }
        public bool ValidateUser { get; set; }
        public List<StudyLevel> StudyLevel { get; set; }
    }

    public class StudyLevel
    {
        public string StudyCode { get; set; }
        public bool DeviceValidation { get; set; }
    }
}
