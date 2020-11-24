﻿using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectWorkplaceArtificateDto : BaseDto
    {
        public int ProjectWorkplaceSectionId { get; set; }
        public int EtmfArtificateMasterLbraryId { get; set; }
        public int? ParentArtificateId { get; set; }
    }
    public class WorkplaceFolderDto
    {
        public int ProjectWorkplaceArtificateId { get; set; }
        public int? ParentArtificateId { get; set; }
        public int EtmfArtificateMasterLbraryId { get; set; }
        public string FolderName { get; set; }
        public int DocumentId { get; set; }
    }

    public class WorkplaceChartDto
    {
        public int All { get; set; }
        public int Missing { get; set; }
        public int PendingApprove { get; set; }
        public int AllPendingApprove { get; set; }
        public int AllDocument { get; set; }
        public int Final { get; set; }
        public int InComplete { get; set; }
        public int PendingReview { get; set; }
        public int AllPendingReview { get; set; }
    }
}
