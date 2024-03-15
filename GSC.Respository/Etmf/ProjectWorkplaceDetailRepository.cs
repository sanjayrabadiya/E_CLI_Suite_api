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
    public class ProjectWorkplaceDetailRepository : GenericRespository<EtmfProjectWorkPlace>, IProjectWorkplaceDetailRepository
    {
        private readonly IGSCContext _context;
        public ProjectWorkplaceDetailRepository(IGSCContext context)
           : base(context)
        {
            _context = context;
        }

        public List<DropDownDto> GetCountryByWorkplace(int ParentProjectId)
        {
            var data = (from workplace in _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == ParentProjectId)
                        join workplacedetail in _context.EtmfProjectWorkPlace.Where(x => x.DeletedDate == null && x.WorkPlaceFolderId == (int)WorkPlaceFolder.Country) on workplace.Id equals workplacedetail.EtmfProjectWorkPlaceId
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
            var data = (from workplace in _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == ParentProjectId)
                        join workplacedetail in _context.EtmfProjectWorkPlace.Where(x => x.DeletedDate == null && x.WorkPlaceFolderId == (int)WorkPlaceFolder.Site) on workplace.Id equals workplacedetail.EtmfProjectWorkPlaceId
                        join project in _context.Project.Where(x => x.DeletedDate == null) on workplacedetail.ItemId equals project.Id
                        select new DropDownDto
                        {
                            Id = workplacedetail.Id,
                            Value = workplacedetail.ItemName
                        }).ToList();

            return data;

        }
    }
}
