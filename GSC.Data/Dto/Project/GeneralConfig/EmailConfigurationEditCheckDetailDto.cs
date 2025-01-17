﻿using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Email;
using System.Collections.Generic;


namespace GSC.Data.Entities.Project.Generalconfig
{
    public class EmailConfigurationEditCheckDetailDto : BaseDto
    {
        public int EmailConfigurationEditCheckId { get; set; }
        public int? ProjectDesignTemplateId { get; set; }
        public int? ProjectDesignVariableId { get; set; }
        public Operator Operator { get; set; }
        public string LogicalOperator { get; set; }
        public string startParens { get; set; }
        public string endParens { get; set; }
        public string CollectionValue { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public int? ProjectDesignId { get; set; }

        public int? ProjectDesignPeriodId { get; set; }

        public int? ProjectDesignVisitId { get; set; }

        public string OperatorName { get; set; }
        public string TemplateName { get; set; }
        public string VariableName { get; set; }
        public string VisitName { get; set; }
        public string ReasonName { get; set; }

        public string PeriodName { get; set; }

        public string QueryFormula { get; set; }

        public string InputValue { get; set; }

        public string FieldName { get; set; }

        public DataType? dataType { get; set; }

        public CollectionSources? CollectionSource { get; set; }

        public EditCheckRuleBy CheckBy { get; set; }

        public string VariableAnnotation { get; set; }

        public string CheckByName { get; set; }

        public int? ProjectId { get; set; }
    }

    public class EmailConfigurationEditCheckDetailGridDto : BaseAuditDto
    {
        public int EmailConfigurationEditCheckId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public Operator Operator { get; set; }
        public string startParens { get; set; }
        public string endParens { get; set; }
        public string OperatorName { get; set; }
        public string TemplateName { get; set; }
        public string VariableName { get; set; }
        public string VisitName { get; set; }
        public string CollectionValue { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ReasonName { get; set; }

        public string LogicalOperator { get; set; }

    }

    public class EmailConfigurationEditCheckResult
    {
        public int Id { get; set; }
        public bool IsValid { get; set; }
        public string SampleText { get; set; }
        public string Result { get; set; }
        public string ResultMessage { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string ProductType { get; set; }

    }
    public class EmailConfigurationEditCheckSendEmail
    {
        public string TemplateName { get; set; }
        public string VariableName { get; set; }
        public string VisitName { get; set; }

        public string StudyCode { get; set; }

        public string SiteCode { get; set; }

        public string SiteName { get; set; }
        public string ScreeningNo { get; set; }
        public string RandomizationNo { get; set; }

        public string Subject { get; set; }
        public string EmailBody { get; set; }

        public int EmailConfigurationEditCheckId { get; set; }

        public string CurrentDate { get; set; }

        public string CompanyName { get; set; }

        public bool IsSMS { get; set; }
    }
    public class EmailList
    {
        public string Email { get; set; }

        public int? UserId { get; set; }

        public int RoleId { get; set; }

        public string Phone { get; set; }

    }

    public class EmailConfigurationEditCheckSendEmailResult
    {
        public List<EmailList> emails { get; set; }

        public EmailConfigurationEditCheckSendEmail emaildata { get; set; }

        public EmailMessage EmailMessage { get; set; }
    }
}
