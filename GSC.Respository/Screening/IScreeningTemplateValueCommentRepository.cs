using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateValueCommentRepository : IGenericRepository<ScreeningTemplateValueComment>
    {
        IList<ScreeningTemplateValueCommentDto> GetComments(int screeningTemplateValueId);
    }
}