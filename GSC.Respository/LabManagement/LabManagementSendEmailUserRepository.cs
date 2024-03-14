using GSC.Common.GenericRespository;
using GSC.Data.Entities.LabManagement;
using GSC.Domain.Context;

namespace GSC.Respository.LabManagement
{
    public class LabManagementSendEmailUserRepository : GenericRespository<LabManagementSendEmailUser>, ILabManagementSendEmailUserRepository
    {

        public LabManagementSendEmailUserRepository(IGSCContext context
            )
           : base(context)
        {
        }
    }
}
