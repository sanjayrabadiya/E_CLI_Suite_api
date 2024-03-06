using GSC.Common.GenericRespository;
using GSC.Data.Entities.Project.Generalconfig;


namespace GSC.Respository.Project.GeneralConfig
{
    public interface IEmailConfigurationEditCheckDetailRepository : IGenericRepository<EmailConfigurationEditCheckDetail>
    {
        EmailConfigurationEditCheckDto GetDetailList(int id);
        EmailConfigurationEditCheckDetailDto GetDetail(int id);

    }
}