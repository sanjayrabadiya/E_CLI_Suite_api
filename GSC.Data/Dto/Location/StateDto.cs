using GSC.Data.Entities.Common;
using System;

namespace GSC.Data.Dto.Location
{
    public class StateDto : BaseDto
    {
        public string StateName { get; set; }

        public int CountryId { get; set; }

        public string CountryName { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        //public int? CreatedBy { get; set; }
        //public int? DeletedBy { get; set; }
        //public int? ModifiedBy { get; set; }
        //public DateTime? CreatedDate { get; set; }
        //public DateTime? ModifiedDate { get; set; }
        //public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        //public string CompanyName { get; set; }
    }
}