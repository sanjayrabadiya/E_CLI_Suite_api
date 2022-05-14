using GSC.Common.GenericRespository;
using GSC.Data.Entities.Project.Generalconfig;

namespace GSC.Respository.Project.GeneralConfig
{
    public interface ISendEmailOnVariableValueRepository : IGenericRepository<SendEmailOnVariableValue>
    {
        void test();
    }
}