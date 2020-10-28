﻿using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class EtmfAuditLogReportDto
    {
        public string projectCode { get; set; }
        public string folderName { get; set; }
        public string zoneName { get; set; }
        public string sectionName { get; set; }
        public string artificateName { get; set; }
        public string documentName { get; set; }
        public string version { get; set; }
        public string status { get; set; }
        public string action { get; set; }
        public string userName { get; set; }
        public DateTime? actionDate { get; set; }
    }

    public class EtmfAuditLogReportSearchDto : BaseDto
    {
        public int projectId { get; set; }
        public int? folderId { get; set; }
        public int? zoneId { get; set; }
        public int? sectionId { get; set; }
        public int? artificateId { get; set; }
    }
}