using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Custom;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.Data.SqlClient;
using System.Linq;


namespace GSC.Respository.Screening
{
    public class ScreeningProgress : IScreeningProgress
    {
        private readonly IUnitOfWork<GscContext> _uow;
        public ScreeningProgress(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser)
        {
            _uow = uow;
        }
        public ScreeningProgressDto GetScreeningProgress(int screeningEntryId, int screeningTemplateId)
        {
            var screeningProgressDto = new ScreeningProgressDto();
            screeningProgressDto.ScreeningCnt = ScreeningCount(screeningEntryId);
            screeningProgressDto.TemplateCnt = TemplateCount(screeningTemplateId);
            return screeningProgressDto;
        }

        int ScreeningCount(int id)
        {

            var sqlquery = @"
                Declare @TotalCnt AS INT

                SELECT @TotalCnt=ISNULL(COUNT(ProjectDesignVariable.Id),1) FROM ScreeningVisit WITH(NOLOCK)
                INNER JOIN ScreeningTemplate WITH(NOLOCK) ON ScreeningTemplate.ScreeningVisitId=ScreeningVisit.Id AND ScreeningTemplate.DeletedDate IS NULL
                INNER JOIN ProjectDesignTemplate WITH(NOLOCK) ON ProjectDesignTemplate.Id=ScreeningTemplate.ProjectDesignTemplateId 
                AND ProjectDesignTemplate.DeletedDate IS NULL
                INNER JOIN ProjectDesignVariable WITH(NOLOCK) ON ProjectDesignVariable.ProjectDesignTemplateId=ProjectDesignTemplate.Id AND ProjectDesignVariable.DeletedDate IS NULL
                WHERE ScreeningVisit.DeletedDate IS NULL AND ScreeningVisit.ScreeningEntryId=@Id

                Declare @ScreeningCnt AS INT

                SELECT @ScreeningCnt=ISNULL(COUNT(ScreeningTemplateValue.Id),0) FROM ScreeningVisit WITH(NOLOCK)
                    INNER JOIN ScreeningTemplate WITH(NOLOCK) ON ScreeningTemplate.ScreeningVisitId=ScreeningVisit.Id AND ScreeningTemplate.DeletedDate IS NULL
                    INNER JOIN ScreeningTemplateValue WITH(NOLOCK) ON ScreeningTemplateValue.ScreeningTemplateId=ScreeningTemplate.Id
                    AND ScreeningTemplateValue.DeletedDate IS NULL AND (ISNULL(ScreeningTemplateValue.Value,'')<>''
                    OR EXISTS (SELECT 1 FROM ScreeningTemplateValueChild AS child WITH(NOLOCK)
                    WHERE child.ScreeningTemplateValueId = ScreeningTemplateValue.Id AND child.DeletedDate IS NULL))
                    WHERE ScreeningVisit.DeletedDate IS NULL AND ScreeningVisit.ScreeningEntryId=@Id

                if (@TotalCnt=0)
	                set @TotalCnt=1

                UPDATE ScreeningEntry SET  Progress=((@ScreeningCnt * 100) / @TotalCnt) WHERE Id=@Id

                SELECT ((@ScreeningCnt * 100) / @TotalCnt) AS Cnt";

            var result = _uow.FromSql<CntTable>(sqlquery, new SqlParameter("@Id", id)).ToList().FirstOrDefault();

            if (result != null)
                return result.Cnt;

            return 0;
        }

        int TemplateCount(int id)
        {
            var sqlquery = @"                
                Declare @TotalCnt AS INT

                SELECT @TotalCnt=ISNULL(COUNT(ProjectDesignVariable.Id),0) FROM ScreeningTemplate WITH(NOLOCK)
                INNER JOIN ProjectDesignTemplate WITH(NOLOCK) ON ProjectDesignTemplate.Id=ScreeningTemplate.ProjectDesignTemplateId 
                INNER JOIN ProjectDesignVariable WITH(NOLOCK) ON ProjectDesignVariable.ProjectDesignTemplateId=ProjectDesignTemplate.Id AND ProjectDesignVariable.DeletedDate IS NULL
                WHERE ScreeningTemplate.Id=@Id

                Declare @ScreeningCnt AS INT

                SELECT @ScreeningCnt=ISNULL(COUNT(ScreeningTemplateValue.Id),0) FROM ScreeningTemplate WITH(NOLOCK)
                INNER JOIN ScreeningTemplateValue WITH(NOLOCK) ON ScreeningTemplateValue.ScreeningTemplateId=ScreeningTemplate.Id
                AND (ISNULL(ScreeningTemplateValue.Value,'')<>''
                OR EXISTS (SELECT 1 FROM ScreeningTemplateValueChild AS child WITH(NOLOCK)
                        WHERE child.ScreeningTemplateValueId = ScreeningTemplateValue.Id))
                WHERE ScreeningTemplate.Id=@Id

                if (@TotalCnt=0)
	                set @TotalCnt=1

                UPDATE ScreeningTemplate SET  Progress=((@ScreeningCnt * 100) / @TotalCnt) WHERE Id=@Id

                SELECT ((@ScreeningCnt * 100) / @TotalCnt) AS Cnt";

            var result = _uow.FromSql<CntTable>(sqlquery, new SqlParameter("@Id", id)).ToList().FirstOrDefault();

            if (result != null)
                return result.Cnt;

            return 0;
        }

    }
}
