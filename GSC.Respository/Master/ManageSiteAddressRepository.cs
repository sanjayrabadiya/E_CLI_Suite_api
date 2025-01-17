﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Master
{
    public class ManageSiteAddressRepository : GenericRespository<ManageSiteAddress>, IManageSiteAddressRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public ManageSiteAddressRepository(IGSCContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
            _context = context;
        }

        public string Duplicate(ManageSiteAddress objSave)
        {
            if (_context.ManageSiteAddress.Any(x => x.Id != objSave.Id && x.ManageSiteId == objSave.ManageSiteId && x.SiteAddress == objSave.SiteAddress.Trim() && x.DeletedDate == null))
                return "Duplicate Site Address: " + objSave.SiteAddress;

            return "";
        }

        public List<ManageSiteAddressGridDto> GetManageSiteAddress(int id, bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ManageSiteId == id)
            .ProjectTo<ManageSiteAddressGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<DropDownDto> GetSiteAddressDropdown(int id)
        {
            var dropList = All.Where(x => x.DeletedDate == null && x.ManageSiteId == id)
                .Select(s => new DropDownDto()
                {
                    Id = s.Id,
                    Value = s.SiteAddress + ", " + s.City.CityName + ", " + s.City.State.StateName + ", " + s.City.State.Country.CountryName
                }).ToList();

            return dropList;
        }

        public List<DropDownDto> GetSiteAddressDropdownForMangeStudy(int projectId, int manageSiteId)
        {
            var selectedList = _context.ProjectSiteAddress.Where(x => x.DeletedDate == null && x.ProjectId == projectId && x.ManageSiteId == manageSiteId)
                .Select(s => s.ManageSiteAddressId).ToList();

            var dropList = All.Where(x => x.DeletedDate == null && x.ManageSiteId == manageSiteId && !selectedList.Contains(x.Id))
                .Select(s => new DropDownDto()
                {
                    Id = s.Id,
                    Value = s.SiteAddress + ", " + s.City.CityName + ", " + s.City.State.StateName + ", " + s.City.State.Country.CountryName
                }).ToList();

            return dropList;
        }
    }
}
