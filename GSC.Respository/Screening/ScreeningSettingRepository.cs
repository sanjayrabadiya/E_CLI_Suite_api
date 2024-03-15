using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Linq;

namespace GSC.Respository.Screening
{
    public class ScreeningSettingRepository : GenericRespository<ScreeningSetting>, IScreeningSettingRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ScreeningSettingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public ScreeningSettingDto GetProjectDefaultData()
        {
            var ScreeningSetting = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null)
                .Select(t => new ScreeningSettingDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    VisitId = t.VisitId,
                    StudyId = t.Project.ParentProjectId,
                    CountryId = t.Project.ManageSite.CompanyId,
                    UserId = t.UserId,
                }).FirstOrDefault();

            if (ScreeningSetting != null)
                if (_context.ProjectRight.Any(x => x.ProjectId == ScreeningSetting.ProjectId && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null))
                    return ScreeningSetting;
                else
                    return null;

            return ScreeningSetting;

        }
    }
}
