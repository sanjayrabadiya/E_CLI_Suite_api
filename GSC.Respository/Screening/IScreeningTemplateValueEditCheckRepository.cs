using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateValueEditCheckRepository : IGenericRepository<ScreeningTemplateValueEditCheck>
    {
        void Insert(ScreeningTemplateValueEditCheck objSave);
        void InsertUpdate(ScreeningTemplateValueEditCheckDto objSave, bool isUpdate);
        List<EditCheckTargetValidation> EditCheckSet(int screeningTemplateId, bool isFromQuery);
        void CloseSystemQuery(int screeningTemplateId, int projectDesignVariableId);
        void UpdateById(ScreeningTemplateValueEditCheck objSave);
    }
}