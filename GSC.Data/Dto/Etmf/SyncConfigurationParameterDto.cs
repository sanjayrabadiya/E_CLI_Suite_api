using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class SyncConfigurationParameterDto
    {
       // public int ReportScreenId { get; set; }
        public string ReportCode { get; set; }
        public int ProjectId { get; set; }
        public int SiteId { get; set; }
        public int CountryId { get; set; }
    }
    public class SyncConfigrationPathDetails
    {
        public int ProjectWorkplaceArtificateId { get; set; }
        public string ProjectCode { get; set; }
        public string WorkPlaceFolder { get; set; }
        public string ItemName { get; set; }
        public string ZonName { get; set; }
        public string SectionName { get; set; }
        public string ArtificateName { get; set; }
    }
}
