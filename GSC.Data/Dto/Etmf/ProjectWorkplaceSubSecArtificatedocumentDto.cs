﻿using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectWorkplaceSubSecArtificatedocumentDto : BaseDto
    {
        public int ProjectWorkplaceSubSectionArtifactId { get; set; }
        public string DocumentName { get; set; }
        public FileModel FileModel { get; set; }
        public string DocPath { get; set; }
        public string Projectname { get; set; }
        public string Countryname { get; set; }
        public string Sitename { get; set; }
        public string Zonename { get; set; }
        public string Sectionname { get; set; }
        public string SubsectionName { get; set; }
        public string Artificatename { get; set; }
        public int FolderType { get; set; }
        public int ProjectId { get; set; }
        public int CompanyId { get; set; }
        public string FileName { get; set; }
        public ArtifactDocStatusType Status { get; set; }
        public string Version { get; set; }
        public int? ParentDocumentId { get; set; }
        public bool? IsAccepted { get; set; }

    }

    public class CommonArtifactDocumentDto
    {
        public int Id { get; set; }
        public int ProjectWorkplaceSubSectionArtifactId { get; set; }
        public int EtmfArtificateMasterLbraryId { get; set; }
        public int ProjectWorkplaceArtificateId { get; set; }
        public string DocumentName { get; set; }
        public string ExtendedName { get; set; }
        public FileModel FileModel { get; set; }
        public string DocPath { get; set; }
        public string FullDocPath { get; set; }
        public string Artificatename { get; set; }
        public List<DocumentUsers> Reviewer { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string Version { get; set; }
        public string CreatedByUser { get; set; }
        public double Level { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool SendBy { get; set; }
        public bool IsSendBack { get; set; }
        public string ReviewStatus { get; set; }
        public bool IsReview { get; set; }
        public bool? IsAccepted { get; set; }
        public string ApprovedStatus { get; set; }
        public List<DocumentUsers> Approver { get; set; }
        public bool IsApproveDoc { get; set; }
        public bool IsNotRequired { get; set; }
    }

    public class DocumentUsers
    {
        public string UserName { get; set; }
    }
    public class CustomParameter
    {
        public int id { get; set; }
        public string fileName { get; set; }
        public string documentData { get; set; }
        public bool AddHistory { get; set; }
    }

}
