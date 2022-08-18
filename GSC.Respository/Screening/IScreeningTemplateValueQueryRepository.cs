using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateValueQueryRepository : IGenericRepository<ScreeningTemplateValueQuery>
    {
        IList<ScreeningTemplateValueQueryGridDto> GetQueries(int screeningTemplateValueId);
        void AcknowledgeQuery(ScreeningTemplateValueQuery screeningTemplateValueQuery);

        void SelfGenerate(ScreeningTemplateValueQuery screeningTemplateValueQuery,
            ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto,
            ScreeningTemplateValue screeningTemplateValue,
            ScreeningTemplate screeningTemplate);

        void ReviewQuery(ScreeningTemplateValue screeningTemplateValue,
            ScreeningTemplateValueQuery screeningTemplateValueQuery);

        void GenerateQuery(ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto,
            ScreeningTemplateValueQuery screeningTemplateValueQuery, ScreeningTemplateValue screeningTemplateValue);

        void UpdateQuery(ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto,
            ScreeningTemplateValueQuery screeningTemplateValueQuery, ScreeningTemplateValue screeningTemplateValue);

        List<DashboardQueryStatusDto> GetDashboardTotalQueryStatus(int projectId);
        List<DashboardQueryStatusDto> GetDashboardQueryStatusBySite(int projectId);
        List<DashboardQueryStatusDto> GetDashboardQueryStatusByRolewise(int projectId);
        List<DashboardQueryStatusDto> GetDashboardQueryStatusByVisitwise(int projectId);

        IList<QueryManagementDto> GetQueryEntries(QuerySearchDto filters);
        IList<QueryManagementDto> GetGenerateQueryBy(int projectId);
        IList<DropDownDto> GetDataEntryBy(int projectId);
        List<DashboardQueryStatusDto> GetDashboardOpenQuerySitewise(int projectId);
        IList<ScreeningQueryDto> GetScreeningQuery(int parentProjectId, int projectId);

        void ReviewAllQuery(UpdateAllQueryStatus updateAllQueryStatus);
        void AcknowledgeAllQuery(UpdateAllQueryStatus updateAllQueryStatus);

        void Save(ScreeningTemplateValueQuery screeningTemplateValueQuery);

        IList<QueryManagementDto> GetScreeningQueryEntries(ScreeningQuerySearchDto filters);
    }
}