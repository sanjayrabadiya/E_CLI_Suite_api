using GSC.Common.GenericRespository;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Project.Design
{
    public class StudyVersionStatusRepository : GenericRespository<StudyVerionStatus>, IStudyVersionStatusRepository
    {
        public StudyVersionStatusRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) :
           base(context)
        {
        }

    }
}
