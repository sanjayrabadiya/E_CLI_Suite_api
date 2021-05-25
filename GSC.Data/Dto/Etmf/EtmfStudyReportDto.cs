using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class EtmfStudyReportDto : BaseDto
    {
        public string projectCode { get; set; }
        public string folderName { get; set; }
        public string countrysiteName { get; set; }
        public string zoneName { get; set; }
        public string sectionName { get; set; }
        public string subSectionName { get; set; }
        public string artificateName { get; set; }
        public string documentName { get; set; }
        public string version { get; set; }
        public string status { get; set; }
        public WorkplaceStatus statusId { get; set; }
        public string action { get; set; }
        public string userName { get; set; }
        public DateTime? actionDate { get; set; }
        public string auditReason { get; set; }
        public string auditComment { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public double Level { get; set; }
    }

    public class StudyReportSearchDto : BaseDto
    {
        public int projectId { get; set; }
        public int? folderId { get; set; }
        public int? countrySiteId { get; set; }
        public int? zoneId { get; set; }
        public int? sectionId { get; set; }
        public int? artificateId { get; set; }
        public int? statusId { get; set; }
    }
}
