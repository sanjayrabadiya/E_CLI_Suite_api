using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.EditCheck;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Project.EditCheck
{
    public interface IRuleRepository2 : IGenericRepository<EditCheckDetail>
    {
        //void DefaultInsertVariableRule(ScreeningTemplate screeningTemplate, int projectDesignId, int domainId);
        //List<EditCheckDetail> GetTemplateAndDomain(int projectDesignId);
        //void EnableDisableTemplate(int projectDesignTemplateId, int screeningEntryId);

        //void ValidateRuleByTemplate(int screeningTemplateId, int projectDesignTemplateId, int screeningEntryId,
        //    int projectDesignId, bool isParent);

        //void ValidateRuleByEditCheckDetailId(VariableEditCheckDto variableEditCheckDto);
        //List<int> GetprojectDesignVariableIds();
    }
}