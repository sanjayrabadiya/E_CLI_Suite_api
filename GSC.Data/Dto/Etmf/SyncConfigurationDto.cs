using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class SyncConfigurationMasterDto : BaseDto
    {
        [Required(ErrorMessage = "Report Name is required.")]
        public int ReportScreenId { get; set; }
        [Required(ErrorMessage = "Version is required.")]
        public string Version { get; set; }
        public List<ConfigurationData> SyncConfigurationMasterDetails { get; set; }
    }

    public class ConfigurationData: BaseDto
    {
        public WorkPlaceFolder WorkPlaceFolder { get; set; }
        public string WorkPlaceFolderName { get; set; }
        public int ZoneMasterLibraryId { get; set; }
        public int SectionMasterLibraryId { get; set; }
        public int ArtificateMasterLbraryId { get; set; }
    }

    public class SyncConfigurationAuditDto
    {
        public int Key { get; set; }
        public string ReportName { get; set; }
        public string Version { get; set; }
        public string ZonName { get; set; }
        public string SectionName { get; set; }
        public string ArtificateName { get; set; }
        public string ReasonName { get; set; }
        public string Notes { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public string Activity { get; set; }
        public string ActivityBy { get; set; }
        public DateTime? ActivityDate { get; set; }


    }

  
}
