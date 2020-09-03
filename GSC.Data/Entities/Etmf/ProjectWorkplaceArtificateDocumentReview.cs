﻿using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectArtificateDocumentReview : BaseEntity
    {
        public int ProjectWorkplaceArtificatedDocumentId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsReview { get; set; }
        public int CompanyId { get; set; }

        public ProjectWorkplaceArtificatedocument ProjectWorkplaceArtificatedDocument { get; set; }
    }
}