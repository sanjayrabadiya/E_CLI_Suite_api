using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateReviewRepository : IGenericRepository<ScreeningTemplateReview>
    {
        List<ScreeningTemplateReviewDto> GetTemplateReviewHistory(int id);

        IList<ReviewDto> GetReviewLevel(int projectId);
        void RollbackReview(RollbackReviewTemplateDto rollbackReviewTemplateDto);
        void Save(int screeningTemplateId, ScreeningTemplateStatus status, short reviewLevel);
    }
}