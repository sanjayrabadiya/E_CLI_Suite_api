using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IVersionEffectRepository : IGenericRepository<ScreeningEntry>
    {
        void ApplyNewVersion(int projectDesignId, bool isTrial, double versionNumber);
    }
}
