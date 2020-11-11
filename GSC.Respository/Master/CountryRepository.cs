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
using GSC.Helper;
using GSC.Respository.ProjectRight;

namespace GSC.Respository.Master
{
    public class CountryRepository : GenericRespository<Country, GscContext>, ICountryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IMapper _mapper;

        public CountryRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository,
            IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
            _mapper = mapper;
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
            if (All.Any(x => x.Id != objSave.Id && x.CountryCode == objSave.CountryCode && x.DeletedDate == null))
                return "Duplicate Country code : " + objSave.CountryCode;

            if (All.Any(x => x.Id != objSave.Id && x.CountryName == objSave.CountryName && x.DeletedDate == null))
                return "Duplicate Country name : " + objSave.CountryName;

            if (!string.IsNullOrEmpty(objSave.CountryCallingCode))
                if (All.Any(x =>
                    x.Id != objSave.Id && x.CountryCallingCode == objSave.CountryCallingCode && x.DeletedDate == null))
                    return "Duplicate Country calling code : " + objSave.CountryCallingCode;

            return "";
        }

       

        public List<DropDownDto> GetCountryByProjectIdDropDown(int ParentProjectId)
        {

            return Context.Project.Where(x => x.DeletedDate == null && (x.ParentProjectId == ParentProjectId || x.Id == ParentProjectId)).Select(r => new DropDownDto
            {
                Id = r.CountryId,
                Value = r.Country.CountryName,
                Code = r.Country.CountryCode
            }).Distinct().OrderBy(o => o.Value).ToList();

        }

        public List<CountryGridDto> GetCountryList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
       ProjectTo<CountryGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}