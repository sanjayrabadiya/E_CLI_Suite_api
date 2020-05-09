using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Entities.Project.EditCheck;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Project.EditCheck
{
    public class EditCheckDetailRepository : GenericRespository<EditCheckDetail, GscContext>, IEditCheckDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public EditCheckDetailRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(uow,
            jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public EditCheckDetailDto GetDetailById(int id)
        {
            return All.Where(x => x.Id == id).Select(c => new EditCheckDetailDto
            {
                EditCheckId = c.EditCheckId,
                Id = c.Id,
                CheckBy = c.CheckBy,
                ByAnnotation = c.ByAnnotation,
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
                    : ""
            }).FirstOrDefault();
        }

        public string Validate(EditCheckDetail editCheckDetail)
        {
            if (!ValidateMathOperator(editCheckDetail))
                return "Only allow math operators or Non math operators!";

            if (!CheckLogicalOperator(editCheckDetail))
                return "Missing Logical Operator!";

            return "";
        }

        bool CheckLogicalOperator(EditCheckDetail editCheckDetail)
        {
            if (editCheckDetail.IsTarget) return true;

            var data = All.AsNoTracking().Where(x => x.DeletedDate == null &&
            x.Id != editCheckDetail.Id &&
            x.IsTarget == editCheckDetail.IsTarget &&
            !x.EditCheck.IsFormula &&
            x.EditCheckId == editCheckDetail.EditCheckId).
            Select(r => r.Operator).ToList();

            data.Add(editCheckDetail.Operator);

            if (data.Count() == data.Count(x => x != null))
                return true;

            if (data.Count() != (data.Count(x => x != null) + 1))
                return false;

            return true;
        }


        bool ValidateMathOperator(EditCheckDetail editCheckDetail)
        {
            if (editCheckDetail.Operator == null) return true;

            var data = All.Where(x => x.DeletedDate == null &&
            x.Id != editCheckDetail.Id &&
            x.EditCheckId == editCheckDetail.EditCheckId &&
            x.Operator != null).ToList();

            if (data.Any(x => !x.IsTarget && ((Operator)x.Operator).CheckMathOperator()))
            {
                if (!editCheckDetail.IsTarget && !((Operator)editCheckDetail.Operator).CheckMathOperator())
                {
                    return false;
                }
            }

            if (data.Any(x => !x.IsTarget && !((Operator)x.Operator).CheckMathOperator()))
            {
                if (!editCheckDetail.IsTarget && ((Operator)editCheckDetail.Operator).CheckMathOperator())
                {
                    return false;
                }
            }


            return true;
        }


        public void UpdateEditDetail(EditCheckDetail editCheckDetail)
        {

            var result = All.Where(x => x.DeletedDate == null && x.EditCheckId == editCheckDetail.EditCheckId).ToList();
            result.ForEach(x =>
            {
                if (editCheckDetail.IsTarget == x.IsTarget && editCheckDetail.Operator == x.Operator)
                    x.Message = editCheckDetail.Message;

                x.IsSameTemplate = x.CheckBy == EditCheckRuleBy.ByVariableAnnotation;

                if (x.CheckBy == EditCheckRuleBy.ByVariable)
                {
                    x.IsSameTemplate = result.Any(t => t.ProjectDesignTemplateId == x.ProjectDesignTemplateId && t.IsTarget != x.IsTarget);
                }

                Update(x);
            });
            Context.SaveChanges(_jwtTokenAccesser);
        }
    }
}