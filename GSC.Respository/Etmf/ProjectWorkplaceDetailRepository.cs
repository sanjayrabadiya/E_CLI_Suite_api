using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceDetailRepository : GenericRespository<ProjectWorkplaceDetail>, IProjectWorkplaceDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        public ProjectWorkplaceDetailRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetCountryByWorkplace(int ParentProjectId)
        {
            var data = (from workplace in _context.ProjectWorkplace.Where(x => x.ProjectId == ParentProjectId)
                        join workplacedetail in _context.ProjectWorkplaceDetail.Where(x => x.DeletedDate == null && x.WorkPlaceFolderId == (int)WorkPlaceFolder.Country) on workplace.Id equals workplacedetail.ProjectWorkplaceId
                        join country in _context.Country.Where(x => x.DeletedDate == null) on workplacedetail.ItemId equals country.Id
                        select new DropDownDto
                        {
                            Id = workplacedetail.Id,
                            Value = country.CountryName
                        }).ToList();

            return data;

        }


        public List<DropDownDto> GetSiteByWorkplace(int ParentProjectId)
        {
            var data = (from workplace in _context.ProjectWorkplace.Where(x => x.ProjectId == ParentProjectId)
                        join workplacedetail in _context.ProjectWorkplaceDetail.Where(x => x.DeletedDate == null && x.WorkPlaceFolderId == (int)WorkPlaceFolder.Site) on workplace.Id equals workplacedetail.ProjectWorkplaceId
                        join project in _context.Project.Where(x => x.DeletedDate == null) on workplacedetail.ItemId equals project.Id
                        select new DropDownDto
                        {
                            Id = workplacedetail.Id,
                            Value = project.ProjectCode
                        }).ToList();

            return data;

        }
    }
}
