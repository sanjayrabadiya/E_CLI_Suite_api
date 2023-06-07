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
                                        ProjectDesignTemplateId = z.ProjectDesignTemplateId,
                                        ProjectDesignVariableId = z.ProjectDesignVariableId,
                                        ProjectDesignPeriodId = z.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId,
                                        ProjectDesignId = z.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId,
                                        Operator = z.Operator,
                                        CollectionValue = z.CollectionValue,
                                        LogicalOperator = z.LogicalOperator,
                                        IsDeleted = z.DeletedDate != null ? true : false,
                                        startParens = z.startParens,
                                        endParens = z.endParens,
                                        OperatorName = z.Operator.GetDescription(),
                                        VisitName = z.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                                        TemplateName = z.ProjectDesignTemplate.TemplateName,
                                        VariableName = z.ProjectDesignVariable.VariableName,
                                        ReasonOth = z.ReasonOth,
                                        ReasonName = z.AuditReasonId > 0 ? _context.AuditReason.Where(a => a.Id == z.AuditReasonId).FirstOrDefault().ReasonName : ""
                                    }).ToList()
                }).FirstOrDefault();
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
                                        ProjectDesignTemplateId = z.ProjectDesignTemplateId,
                                        ProjectDesignVariableId = z.ProjectDesignVariableId,
                                        ProjectDesignPeriodId = z.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId,
                                        ProjectDesignId = z.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId,
                                        ProjectDesignVisitId = z.ProjectDesignTemplate.ProjectDesignVisitId,
                                        Operator = z.Operator,
                                        CollectionValue = z.CollectionValue,
                                        LogicalOperator = z.LogicalOperator,
                                        IsDeleted = z.DeletedDate != null ? true : false,
                                        startParens = z.startParens,
                                        endParens = z.endParens
                                    }).FirstOrDefault();
            return data;
        }

    }
}
