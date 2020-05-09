using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateReviewRepository : IGenericRepository<ScreeningTemplateReview>
    {
        List<ScreeningTemplateReviewDto> GetTemplateReviewHistory(int id);

        IList<ReviewDto> GetReviewLevel(int projectId);
    }
}