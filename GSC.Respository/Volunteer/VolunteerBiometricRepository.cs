using GSC.Common.GenericRespository;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Volunteer
{
    public class VolunteerBiometricRepository : GenericRespository<VolunteerBiometric>,
        IVolunteerBiometricRepository
    {
        public VolunteerBiometricRepository(IGSCContext context)
            : base(context)
        {
        }
    }
}