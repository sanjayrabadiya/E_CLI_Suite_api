using GSC.Common.GenericRespository;
using GSC.Data.Entities.LogReport;

namespace GSC.Respository.LogReport
{
    public interface IUserLoginReportRespository : IGenericRepository<UserLoginReport>
    {
        int SaveLog(string msg, int? userId, string userName, int? roleId);
    }
}