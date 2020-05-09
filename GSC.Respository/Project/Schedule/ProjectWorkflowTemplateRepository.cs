using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Project.Schedule;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Project.Schedule
{
    public class ProjectScheduleTemplateRepository : GenericRespository<ProjectScheduleTemplate, GscContext>,
        IProjectScheduleTemplateRepository
    {
        public ProjectScheduleTemplateRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser) :
            base(uow, jwtTokenAccesser)
        {
        }
    }
}