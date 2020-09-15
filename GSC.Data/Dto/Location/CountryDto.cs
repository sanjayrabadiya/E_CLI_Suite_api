using GSC.Data.Entities.Common;
using System;

namespace GSC.Data.Dto.Location
{
    public class CountryDto : BaseDto
    {
        public string CountryName { get; set; }

        public string CountryCallingCode { get; set; }

        public string CountryCode { get; set; }

        public int? CompanyId { get; set; }
    }

    public class CountryGridDto : BaseAuditDto
    {
        public string CountryName { get; set; }
        public string CountryCallingCode { get; set; }
        public string CountryCode { get; set; }

    }
}