

using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Respository.EditCheckImpact
{
    public interface IEditCheckImpactRepository : IGenericRepository<ScreeningTemplate>
    {
        List<EditCheckValidateDto> CheckValidation(DesignScreeningTemplateDto projectDesignTemplateDto, List<Data.Dto.Screening.ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic, bool isQuery);
        List<EditCheckTargetValidationList> VariableValidateProcess(int screeningEntryId, int screeningTemplateId, string value, int projectDesignTemplateId, int projectDesignVariableId, List<EditCheckIds> editCheckIds, bool isQueryRaise, int screeningVisitId, int? projectDesignVisitId, bool isNa, ScreeningTemplateStatus status);
        List<EditCheckTargetValidationList> UpdateVariale(List<EditCheckValidateDto> editCheckValidateDto, bool isVariable, bool isQueryRaise);
        int InsertScreeningValue(int screeningTemplateId, int projectDesignVariableId, string value, string note, bool isSoftFetch, CollectionSources? collectionSource, bool isDisable);
    }
}
