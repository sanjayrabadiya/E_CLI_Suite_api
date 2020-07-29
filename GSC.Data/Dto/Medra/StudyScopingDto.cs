using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Medra;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.UserMgt;
using GSC.Helper.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class StudyScopingDto : BaseDto
    {
        public int ProjectId { get; set; }
        public bool IsByAnnotation { get; set; }
        public int ScopingBy { get; set; }
        public int ProjectDesignPeriodId { get; set; }
        public string ProjectDesignPeriodName { get; set; }
        public int VisitId { get; set; }
        public string VisitName { get; set; }
        public int TemplateId { get; set; }
        public int VariableAnnotation{get;set;}
        public string TemplateName { get; set; }
        public int? DomainId { get; set; }
        public int? ProjectDesignVariableId { get; set; }
        public int[] ProjectDesignVariableIds { get; set; }
        public int? DictionaryId { get; set; }
        public int MedraConfigId { get; set; }
        public int CoderProfile { get; set; }
        public int? CoderApprover { get; set; }
        public int? CompanyId { get; set; }
        public string ProjectName { get; set; }
        public string DomainName { get; set; }
        public string VariableName { get; set; }
        public string DictionaryName { get; set; }
        public string VersionName { get; set; }
        public string FieldName { get; set; }
        public string CoderProfileName { get; set; }
        public string CoderApproverName { get; set; }
        public Domain Domain { get; set; }        
        public Variable Variable { get; set; } 
        public MedraVersion MedraVersion { get; set; }      
        public Entities.Master.Project Project { get; set; }
        public bool? IsEnable { get; set; }
    }
}
