using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkPlaceZoneRepository : GenericRespository<ProjectWorkPlaceZone>, IProjectWorkPlaceZoneRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public ProjectWorkPlaceZoneRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetProjectWorkPlaceZoneDropDown(int CountryId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.ProjectWorkplaceDetailId == CountryId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.EtmfZoneMasterLibrary.ZonName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetProjectByZone(int ParentProjectId)
        {
            var data = (from workplace in _context.ProjectWorkplace.Where(x => x.ProjectId == ParentProjectId)
                        join workplacedetail in _context.ProjectWorkplaceDetail.Where(x => x.DeletedDate == null && x.WorkPlaceFolderId == (int)WorkPlaceFolder.Trial) on workplace.Id equals workplacedetail.ProjectWorkplaceId
                        join zone in _context.ProjectWorkPlaceZone.Where(x => x.DeletedDate == null) on workplacedetail.Id equals zone.ProjectWorkplaceDetailId
                        select new DropDownDto
                        {
                            Id = zone.Id,
                            Value = zone.EtmfZoneMasterLibrary.ZonName
                        }).ToList();

            return data;

        }

    }
}
