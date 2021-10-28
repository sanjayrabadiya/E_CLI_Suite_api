using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class SyncConfiguration : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int ReportScreenId { get; set; }
        public int FolderType { get; set; }
        public IList<SyncConfigurationDetails> SyncConfigurationDetails { get; set; }
        //public int AppScreenId { get; set; }
        //public int? ProjectWorkplaceSubSectionArtifactId { get; set; }
        //public int? ProjectWorkplaceArtificateId { get; set; }      
        //public Entities.Master.Project Project { get; set; }
        //public AppScreen AppScreen { get; set; }
    }
}
