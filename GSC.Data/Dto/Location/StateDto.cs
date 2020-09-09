using GSC.Data.Entities.Common;
using System;

namespace GSC.Data.Dto.Location
{
    public class StateDto : BaseDto
    {
        public string StateName { get; set; }

        public int CountryId { get; set; }

        public string CountryName { get; set; }

        public int? CompanyId { get; set; }
    }

    public class StateGridDto : BaseAuditDto
    {
        public string StateName { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
    }
}