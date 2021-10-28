using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EconsentReviewDetailsAuditParameterDto
    {
        [Required(ErrorMessage = "Study is required.")]
        public int ParentProjectId { get; set; }
        public int ProjectId { get; set; }
        public int DocumentId { get; set; }
        public List<DropDownDto> SubjectIds { get; set; }
        public ScreeningPatientStatus PatientStatusId { get; set; }
        public ICFAction ActionId { get; set; }
        public bool isExcel { get; set; }
    }
    public class EconsentReviewDetailsAuditGridDto : BaseAuditDto
    {
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public int Key { get; set; }
        public string ScreeningNumber { get; set; }
        public string RandomizationNumber { get; set; }
        public string Initial { get; set; }
        public string DocumentName { get; set; }
        public string Version { get; set; }
        public string LanguageName { get; set; }
        public string Activity { get; set; }
        public string PatientStatus { get; set; }
    }
}
