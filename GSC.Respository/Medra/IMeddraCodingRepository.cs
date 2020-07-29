using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Medra;

namespace GSC.Respository.Medra
{
    public interface IMeddraCodingRepository : IGenericRepository<MeddraCoding>
    {
        List<MeddraCodingVariableDto> SearchMain(MeddraCodingSearchDto meddraCodingSearchDto);
        MeddraCodingMainDto GetVariableCount(MeddraCodingSearchDto meddraCodingDto);
        List<DropDownDto> MeddraCodingVariableDropDown(int ProjectId);
        IList<MeddraCodingSearchDetails> GetMedDRACodingDetails(MeddraCodingSearchDto filters);
        IList<MeddraCodingSearchDetails> AutoCodes(MeddraCodingSearchDto meddraCodingSearchDto);
        void UpdateScopingVersion(StudyScoping model);
        void UpdateSelfCorrection(int ScreeningTemplateValueId);
        MeddraCoding CheckForRecode(int ScreeningTemplateValueId);
        MeddraCodingMainDto GetCoderandApprovalProfile(int ProjectDesignVariableId);
        MeddraCoding GetRecordForComment(int ScreeningTemplateValueId);
        void UpdateEditCheck(int ScreeningTemplateValueId);
    }
}