using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MeddraCoding : BaseEntity, ICommonAduit
    {
        public int MeddraConfigId { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public int? MeddraLowLevelTermId { get; set; }
        public int? MeddraSocTermId { get; set; }
        public CodedType CodedType { get; set; }
        public CodedType CodingType { get; set; }
        public int? ApprovedBy { get; set; }
        public bool IsApproved { get; set; }
        public int? CompanyId { get; set; }
        public Screening.ScreeningTemplateValue ScreeningTemplateValue { get; set; }
        public MeddraLowLevelTerm MeddraLowLevelTerm { get; set; }
        public int? CreatedRole { get; set; }

        public int? LastUpdateBy { get; set; }
        public DateTime? ApproveDate { get; set; }
    }
}
