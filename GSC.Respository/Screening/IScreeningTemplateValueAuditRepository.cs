using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateValueAuditRepository : IGenericRepository<ScreeningTemplateValueAudit>
    {
        IList<ScreeningAuditDto> GetAudits(int screeningTemplateValueId);
        void Save(ScreeningTemplateValueAudit audit);
        IList<ScreeningAuditDto> GetAuditHistoryByScreeningEntry(int id);
    }
}