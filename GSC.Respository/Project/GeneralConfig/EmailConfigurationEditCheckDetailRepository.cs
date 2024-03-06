
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Domain.Context;
using GSC.Shared.Extension;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace GSC.Respository.Project.GeneralConfig
{
    public class EmailConfigurationEditCheckDetailRepository : GenericRespository<EmailConfigurationEditCheckDetail>, IEmailConfigurationEditCheckDetailRepository
    {

        private readonly IGSCContext _context;
        public EmailConfigurationEditCheckDetailRepository(IGSCContext context
            ) : base(context)
        {

            _context = context;
        }

        public EmailConfigurationEditCheckDto GetDetailList(int id)
        {
            var data = _context.EmailConfigurationEditCheck.
                Include(x => x.EmailConfigurationEditCheckDetailList).
                Where(x => x.Id == id).Select(x => new EmailConfigurationEditCheckDto
                {
                    Id = x.Id,
                    ProjectId = x.ProjectId,
                    SourceFormula = x.SourceFormula,
                    CheckFormula = x.CheckFormula,
                    ErrorMessage = x.ErrorMessage,
                    SampleResult = x.SampleResult,
                    Children = _context.EmailConfigurationEditCheckDetail
                                    .Include(s => s.ProjectDesignTemplate).
                                    ThenInclude(s => s.ProjectDesignVisit).
                                    ThenInclude(s => s.ProjectDesignPeriod).
                                    ThenInclude(s => s.ProjectDesign).Where(x => x.DeletedDate == null && x.EmailConfigurationEditCheckId == id).Select(z => new EmailConfigurationEditCheckDetailDto
                                    {
                                        Id = z.Id,
                                        EmailConfigurationEditCheckId = z.EmailConfigurationEditCheckId,
                                        ProjectDesignTemplateId = z.ProjectDesignTemplateId > 0 ? z.ProjectDesignTemplateId : 0,
                                        ProjectDesignVariableId = z.ProjectDesignVariableId > 0 ? z.ProjectDesignVariableId : 0,
                                        ProjectDesignPeriodId = z.ProjectDesignTemplateId > 0 ? z.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId : 0,
                                        Operator = z.Operator,
                                        CollectionValue = z.CollectionValue,
                                        LogicalOperator = z.LogicalOperator,
                                        IsDeleted = z.DeletedDate != null,
                                        startParens = z.startParens,
                                        endParens = z.endParens,
                                        OperatorName = z.Operator.GetDescription(),
                                        VisitName = z.ProjectDesignTemplateId > 0 && z.ProjectDesignTemplate.ProjectDesignVisit != null ? z.ProjectDesignTemplate.ProjectDesignVisit.DisplayName : "",
                                        TemplateName = z.ProjectDesignTemplateId > 0 && z.ProjectDesignTemplate != null ? z.ProjectDesignTemplate.TemplateName : "",
                                        VariableName = z.ProjectDesignVariableId > 0 && z.ProjectDesignVariable != null ? z.ProjectDesignVariable.VariableName : z.VariableAnnotation,
                                        ReasonOth = z.ReasonOth,
                                        ReasonName = z.AuditReasonId > 0 ? _context.AuditReason.Where(a => a.Id == z.AuditReasonId).FirstOrDefault().ReasonName : "",
                                        CheckBy = z.CheckBy,
                                        VariableAnnotation = z.VariableAnnotation,
                                        CheckByName = z.CheckBy.GetDescription()
                                    }).ToList()
                }).FirstOrDefault();
            if (data != null)
            {
                foreach (var item in data.Children)
                {
                    if (item.ProjectDesignTemplateId > 0)
                    {
                        var projectDesignTemplate = _context.ProjectDesignTemplate.Include(s => s.ProjectDesignVisit).ThenInclude(s => s.ProjectDesignPeriod).Where(s => s.Id == item.ProjectDesignTemplateId).FirstOrDefault();
                        if (projectDesignTemplate != null)
                            item.ProjectDesignId = projectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId;
                    }
                }
            }
            return data;
        }
        public EmailConfigurationEditCheckDetailDto GetDetail(int id)
        {

            var data = All.Include(s => s.ProjectDesignTemplate).
                                    ThenInclude(s => s.ProjectDesignVisit).
                                    ThenInclude(s => s.ProjectDesignPeriod).
                                    ThenInclude(s => s.ProjectDesign).Where(x => x.Id == id).Select(z => new EmailConfigurationEditCheckDetailDto
                                    {
                                        Id = z.Id,
                                        EmailConfigurationEditCheckId = z.EmailConfigurationEditCheckId,
                                        ProjectDesignTemplateId = z.ProjectDesignTemplateId > 0 ? z.ProjectDesignTemplateId : null,
                                        ProjectDesignVariableId = z.ProjectDesignVariableId > 0 ? z.ProjectDesignVariableId : null,
                                        Operator = z.Operator,
                                        CollectionValue = z.CollectionValue,
                                        LogicalOperator = z.LogicalOperator,
                                        IsDeleted = z.DeletedDate != null,
                                        startParens = z.startParens,
                                        endParens = z.endParens,
                                        CheckBy = z.CheckBy,
                                        VariableAnnotation = z.VariableAnnotation
                                    }).FirstOrDefault();
            if (data != null && data.ProjectDesignTemplateId > 0)
            {

                var projectDesignTemplate = _context.ProjectDesignTemplate.Include(s => s.ProjectDesignVisit).ThenInclude(s => s.ProjectDesignPeriod).Where(s => s.Id == data.ProjectDesignTemplateId).FirstOrDefault();
                if (projectDesignTemplate != null)
                {
                    data.ProjectDesignId = projectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId;
                    data.ProjectDesignPeriodId = projectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId;
                    data.ProjectDesignVisitId = projectDesignTemplate.ProjectDesignVisitId;
                }


            }
            return data;
        }

    }
}
