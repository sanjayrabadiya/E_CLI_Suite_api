using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class MasterLibraryDto
    {

        public int Id{ get; set; }
        public string Version { get; set; }
        public string ZoneName { get; set; }
        public string Level { get; set; }
        public string Zoneno { get; set; }
        public string SectionName { get; set; }
        public string SectionNo { get; set; }
        public string ArtificateName { get; set; }
        public string ArtificateNo { get; set; }
        public int InclusionType { get; set; }
        public string DeviceSponDoc { get; set; }
        public string DeviceInvesDoc { get; set; }
        public string NondeviceSponDoc { get; set; }
        public string NondeviceInvesDoc { get; set; }
        public string StudyArtificates { get; set; }
        public string TrailLevelDoc { get; set; }
        public string CountryLevelDoc { get; set; }
        public string SiteLevelDoc { get; set; }
    }
}
