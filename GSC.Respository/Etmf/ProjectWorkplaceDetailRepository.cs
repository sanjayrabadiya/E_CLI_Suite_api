using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceDetailRepository : GenericRespository<ProjectWorkplaceDetail, GscContext>, IProjectWorkplaceDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ProjectWorkplaceDetailRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser)
           : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetCountryByWorkplace(int ParentProjectId)
        {
            var data = (from workplace in Context.ProjectWorkplace.Where(x => x.ProjectId == ParentProjectId)
                        join workplacedetail in Context.ProjectWorkplaceDetail.Where(x => x.DeletedDate == null && x.WorkPlaceFolderId == (int) WorkPlaceFolder.Country ) on workplace.Id equals workplacedetail.ProjectWorkplaceId
                        join country in Context.Country.Where(x => x.DeletedDate == null) on workplacedetail.ItemId equals country.Id
                        select new DropDownDto
                        {
                            Id = workplacedetail.Id,
                            Value = country.CountryName
                        }).ToList();

            return data;

        }


        public List<DropDownDto> GetSiteByWorkplace(int ParentProjectId)
        {
            var data = (from workplace in Context.ProjectWorkplace.Where(x => x.ProjectId == ParentProjectId)
                        join workplacedetail in Context.ProjectWorkplaceDetail.Where(x => x.DeletedDate == null && x.WorkPlaceFolderId == (int)WorkPlaceFolder.Site) on workplace.Id equals workplacedetail.ProjectWorkplaceId
                        join project in Context.Project.Where(x => x.DeletedDate == null) on workplacedetail.ItemId equals project.Id
                        select new DropDownDto
                        {
                            Id = workplacedetail.Id,
                            Value = project.ProjectCode + " - " + project.ProjectName
                        }).ToList();

            return data;

        }

    }
}
