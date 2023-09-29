using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Generalconfig;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Domain.Context;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.GeneralConfig
{
    public class EmailConfigurationEditCheckDetailRepository : GenericRespository<EmailConfigurationEditCheckDetail>, IEmailConfigurationEditCheckDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public EmailConfigurationEditCheckDetailRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
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
                                        IsDeleted = z.DeletedDate != null ? true : false,
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
                                        //ProjectDesignPeriodId = z.ProjectDesignTemplateId > 0 && z.ProjectDesignTemplate != null && z.ProjectDesignTemplate.ProjectDesignVisit != null ? z.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId : 0,

                                        //ProjectDesignVisitId = z.ProjectDesignTemplateId > 0 && z.ProjectDesignTemplate != null ? z.ProjectDesignTemplate.ProjectDesignVisitId : 0,
                                        Operator = z.Operator,
                                        CollectionValue = z.CollectionValue,
                                        LogicalOperator = z.LogicalOperator,
                                        IsDeleted = z.DeletedDate != null ? true : false,
                                        startParens = z.startParens,
                                        endParens = z.endParens,
                                        CheckBy = z.CheckBy,
                                        VariableAnnotation = z.VariableAnnotation
                                    }).FirstOrDefault();
            if (data != null && data.ProjectDesignTemplateId > 0)
            {

                var projectDesignTemplate = _context.ProjectDesignTemplate.Include(s => s.ProjectDesignVisit).ThenInclude(s => s.ProjectDesignPeriod).Where(s => s.Id == data.ProjectDesignTemplateId).FirstOrDefault();
                data.ProjectDesignId = projectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId;
                data.ProjectDesignPeriodId = projectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId;
                data.ProjectDesignVisitId = projectDesignTemplate.ProjectDesignVisitId;


            }
            return data;
        }

    }
}
