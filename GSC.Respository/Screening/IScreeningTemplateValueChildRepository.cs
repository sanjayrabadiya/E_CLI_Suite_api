using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;
using System.Collections.Generic;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateValueChildRepository : IGenericRepository<ScreeningTemplateValueChild>
    {
        void Save(ScreeningTemplateValue screeningTemplateValue);
        void DeleteChild(List<int> Ids);
    }
}
