using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.EditCheck;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Project.EditCheck
{
    public class EditCheckDetailRepository : GenericRespository<EditCheckDetail>, IEditCheckDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public EditCheckDetailRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public EditCheckDetailDto GetDetailById(int id)
        {
            var result = All.Where(x => x.Id == id).Select(c => new EditCheckDetailDto
            {
                EditCheckId = c.EditCheckId,
                Id = c.Id,
                CheckBy = c.CheckBy,
                ByAnnotation = c.ByAnnotation,
                ProjectDesignId = c.EditCheck.ProjectDesignId,
                ProjectDesignTemplateId = c.ProjectDesignTemplateId,
                ProjectDesignVisitId = c.ProjectDesignVariable != null
                     ? c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.Id
                     : c.ProjectDesignTemplate.ProjectDesignVisit.Id,
                ProjectDesignPeriodId = c.ProjectDesignVariable != null
                     ? c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId
                     : c.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId,
                ProjectDesignVariableId = c.ProjectDesignVariableId,
                VariableAnnotation = c.VariableAnnotation,
                DomainId = c.DomainId,
                Operator = c.Operator,
                CollectionValue = c.CollectionValue,
                CollectionValue2 = c.CollectionValue2,
                IsTarget = c.IsTarget,
                IsReferenceValue = c.IsReferenceValue,
                StartParens = c.StartParens,
                EndParens = c.EndParens,
                IsSameTemplate = c.IsSameTemplate,
                LogicalOperator = c.LogicalOperator,
                Message = c.Message,
                ExtraData = _mapper.Map<List<ProjectDesignVariableValueDropDown>>(c.ProjectDesignVariable.Values.Where(b => b.DeletedDate == null).ToList()),
                QueryFormula = c.QueryFormula,
                PeriodName = c.ProjectDesignVariable != null
                     ? c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                     : "",
                TemplateName = c.ProjectDesignTemplate.TemplateName,
                VariableName = c.ProjectDesignVariable.VariableName,
                VisitName = c.ProjectDesignVariable != null
                     ? c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName
                     : "",
                FetchingProjectDesignTemplateId = c.FetchingProjectDesignTemplateId,
                FetchingProjectDesignVariableId = c.FetchingProjectDesignVariableId,
                FetchingProjectDesignVisitId = _context.ProjectDesignTemplate.Where(x => x.Id == c.FetchingProjectDesignTemplateId).FirstOrDefault() != null ? _context.ProjectDesignTemplate.Where(x => x.Id == c.FetchingProjectDesignTemplateId).FirstOrDefault().ProjectDesignVisitId : 0
            }).FirstOrDefault();

            if (result.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
            {
                var variableAnnotation = GetCollectionSources(result.VariableAnnotation, result.ProjectDesignId);
                result.CollectionSource = variableAnnotation?.CollectionSource;
                result.DataType = variableAnnotation?.DataType;
                if (variableAnnotation.Values != null)
                    result.ExtraData = _mapper.Map<List<ProjectDesignVariableValueDropDown>>(variableAnnotation.Values.Where(x => x.DeletedDate == null).ToList());
            }

            return result;

        }

        public ProjectDesignVariable GetCollectionSources(string annotation, int projectDesignId)
        {
            if (string.IsNullOrEmpty(annotation)) return null;

            var annotationVariable = _context.ProjectDesignVariable.Include(t => t.Values).Where(a => a.Annotation == annotation
                       && a.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == projectDesignId).FirstOrDefault();

            return annotationVariable;

        }

        public void UpdateEditDetail(EditCheckDetail editCheckDetail)
        {

            var result = All.Where(x => x.DeletedDate == null && x.EditCheckId == editCheckDetail.EditCheckId).ToList();
            result.ForEach(x =>
            {
                if (editCheckDetail.IsTarget == x.IsTarget && editCheckDetail.Operator == x.Operator)
                    x.Message = editCheckDetail.Message;

                x.IsSameTemplate = x.CheckBy == EditCheckRuleBy.ByVariableAnnotation;

                if (x.CheckBy == EditCheckRuleBy.ByVariable || x.CheckBy == EditCheckRuleBy.ByVariableRule)
                {
                    x.IsSameTemplate = result.Any(t => t.ProjectDesignTemplateId == x.ProjectDesignTemplateId && t.IsTarget != x.IsTarget);
                }

                Update(x);
            });
            _context.Save();
        }
    }
}