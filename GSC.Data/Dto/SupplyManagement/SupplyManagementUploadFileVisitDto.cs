using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementUploadFileVisitDto : BaseDto
    {
        public int SupplyManagementUploadFileDetailId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public string Value { get; set; }
    }
}
