using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Location;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Location;
using GSC.Domain.Context;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class CountryRepository : GenericRespository<Country>, ICountryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public CountryRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
            _mapper = mapper;
            _context = context;
        }

        public List<DropDownDto> GetCountryDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.CountryName, Code = c.CountryCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetProjectCountryDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.CountryName, Code = c.CountryCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public string DuplicateCountry(Country objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.CountryCode == objSave.CountryCode.Trim() && x.DeletedDate == null))
                return "Duplicate Country code : " + objSave.CountryCode;

            if (All.Any(x => x.Id != objSave.Id && x.CountryName == objSave.CountryName.Trim() && x.DeletedDate == null))
                return "Duplicate Country name : " + objSave.CountryName;

            if (!string.IsNullOrEmpty(objSave.CountryCallingCode))
                if (All.Any(x =>
                    x.Id != objSave.Id && x.CountryCallingCode == objSave.CountryCallingCode.Trim() && x.DeletedDate == null))
                    return "Duplicate Country calling code : " + objSave.CountryCallingCode;

            return "";
        }



        public List<DropDownDto> GetCountryByProjectIdDropDown(int ParentProjectId)
        {

            var country = _context.Project
                .Where(x => x.DeletedDate == null && x.ParentProjectId == ParentProjectId && x.ManageSite != null).Select(r => new DropDownDto
                {
                    Id = (int)r.ManageSite.City.State.CountryId,
                    Value = r.ManageSite.City.State.Country.CountryName,
                    Code = r.ManageSite.City.State.Country.CountryCode
                }).Distinct().OrderBy(o => o.Value).ToList();
            return country;
        }
        public List<DropDownDto> GetCountryByProjectIdDropDownDepot(int ParentProjectId, int id)
        {

            var country = _context.Project
                .Where(x => x.DeletedDate == null && x.ParentProjectId == ParentProjectId && x.ManageSite != null).Select(r => new DropDownDto
                {
                    Id = (int)r.ManageSite.City.State.CountryId,
                    Value = r.ManageSite.City.State.Country.CountryName,
                    Code = r.ManageSite.City.State.Country.CountryCode
                }).Distinct().OrderBy(o => o.Value).ToList();
            if (country != null && id > 0 && !country.Any(x => x.Id == id))
            {
               return _context.Project
                .Where(x => x.Id == id && x.ParentProjectId == ParentProjectId && x.ManageSite != null).Select(r => new DropDownDto
                {
                    Id = (int)r.ManageSite.City.State.CountryId,
                    Value = r.ManageSite.City.State.Country.CountryName,
                    Code = r.ManageSite.City.State.Country.CountryCode
                }).Distinct().OrderBy(o => o.Value).ToList();
            }
            return country;
        }
        public List<CountryGridDto> GetCountryList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
       ProjectTo<CountryGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}