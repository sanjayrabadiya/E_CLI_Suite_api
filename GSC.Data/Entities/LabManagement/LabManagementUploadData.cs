using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Data.Entities.LabManagement
{
    public class LabManagementUploadData : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int LabManagementConfigurationId { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public LabManagementConfiguration LabManagementConfiguration { get; set; }
        public Master.Project Project { get; set; }
        public IList<LabManagementUploadExcelData> LabManagementUploadExcelDatas { get; set; } = null;
        public LabManagementUploadStatus LabManagementUploadStatus { get; set; }
    }
}
