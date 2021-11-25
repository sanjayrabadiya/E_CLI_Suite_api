using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class SyncConfigurationMaster : BaseEntity
    {
        public int ReportScreenId { get; set; }
        public string Version { get; set; }
        public ReportScreen ReportScreen { get; set; }
        public List<SyncConfigurationMasterDetails> SyncConfigurationMasterDetails { get; set; }
    }

    public class SyncConfigurationMasterDetails: BaseEntity
    {       
        public int SyncConfigurationMasterId { get; set; }
        [ForeignKey("SyncConfigurationMasterId")]
        public SyncConfigurationMaster SyncConfigurationMaster { get; set; }
        public WorkPlaceFolder WorkPlaceFolder { get; set; }
        public int ZoneMasterLibraryId { get; set; }
        public int SectionMasterLibraryId { get; set; }
        public int ArtificateMasterLbraryId { get; set; }
        public List<SyncConfigurationMasterDetailsAudit> SyncConfigurationMasterDetailsAudit { get; set; }

    }

    public class SyncConfigurationMasterDetailsAudit : BaseEntity
    {        
        
        public int ReportScreenId { get; set; }
        public string Version { get; set; }
        public ReportScreen ReportScreen { get; set; }
        public int SyncConfigrationDetailId { get; set; }
        [ForeignKey("SyncConfigrationDetailId")]
        public SyncConfigurationMasterDetails SyncConfigurationMasterDetails { get; set; }       
        public int? ZoneMasterLibraryId { get; set; }
        [ForeignKey("ZoneMasterLibraryId")]
        public EtmfZoneMasterLibrary EtmfZoneMasterLibrary { get; set; }
        public int? SectionMasterLibraryId { get; set; }
        [ForeignKey("SectionMasterLibraryId")]
        public EtmfSectionMasterLibrary EtmfSectionMasterLibrary { get; set; }
        public int? ArtificateMasterLbraryId { get; set; }
        [ForeignKey("ArtificateMasterLbraryId")]
        public EtmfArtificateMasterLbrary EtmfArtificateMasterLbrary { get; set; }
        public int? ReasonId { get; set; }        
        [ForeignKey("ReasonId")]
        public AuditReason AuditReason { get; set; }
        public string Note { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public string Activity { get; set; }



    }
}
