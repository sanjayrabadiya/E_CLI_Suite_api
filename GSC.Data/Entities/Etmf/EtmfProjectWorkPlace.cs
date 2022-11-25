using GSC.Common.Base;
using GSC.Helper;
using System;
using System.Collections.Generic;
using GSC.Common.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Etmf
{
    public class EtmfProjectWorkPlace : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public string Version { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
        public List<EtmfProjectWorkPlace> ProjectWorkplaceDetails { get; set; }


        public int EtmfProjectWorkPlaceId { get; set; }
        public int WorkPlaceFolderId { get; set; }
        public int ItemId { get; set; }
        public int TableTag { get; set; }
        public string ItemName { get; set; }
        public EtmfProjectWorkPlace ProjectWorkPlace { get; set; }
        public List<EtmfUserPermission> EtmfUserPermission { get; set; }

        public int EtmfMasterLibraryId { get; set; }
        public EtmfMasterLibrary EtmfMasterLibrary { get; set; }
        public int? CompanyId { get; set; }

        public int EtmfArtificateMasterLbraryId { get; set; }
        public int? ParentArtificateId { get; set; }
        public bool IsNotRequired { get; set; }
        public EtmfArtificateMasterLbrary EtmfArtificateMasterLbrary { get; set; }
        public List<ProjectWorkplaceArtificatedocument> ProjectWorkplaceArtificatedocument { get; set; }



        public string SubSectionName { get; set; }

        public string ArtifactName { get; set; }
        public List<ProjectWorkplaceSubSecArtificatedocument> ProjectWorkplaceSubSecArtificatedocument { get; set; }
    }
}
