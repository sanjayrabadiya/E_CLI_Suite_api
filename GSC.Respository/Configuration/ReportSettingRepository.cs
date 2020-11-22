using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Configuration
{
    public class ReportSettingRepository : GenericRespository<ReportSetting, GscContext>, IReportSettingRepository
    {
        public ReportSettingRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}