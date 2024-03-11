
using GSC.Common.GenericRespository;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;


namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementKitDetailRepository : GenericRespository<SupplyManagementKITDetail>, ISupplyManagementKitDetailRepository
    {     

        public SupplyManagementKitDetailRepository(IGSCContext context)
            : base(context)
        {
        }
    }
}
