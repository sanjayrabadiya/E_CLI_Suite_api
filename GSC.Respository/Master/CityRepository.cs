using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Location;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Location;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class CityRepository : GenericRespository<City, GscContext>, ICityRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public CityRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public string DuplicateCity(City objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.CityCode == objSave.CityCode && x.DeletedDate == null))
                return "Duplicate City code : " + objSave.CityCode;

            if (All.Any(x => x.Id != objSave.Id && x.CityName == objSave.CityName && x.DeletedDate == null))
                return "Duplicate City name : " + objSave.CityName;

            return "";
        }

        public List<DropDownDto> GetCityDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.CityName, Code = c.CityCode}).OrderBy(o => o.Value)
                .ToList();
        }

        public List<CityDto> GetCities(bool isDeleted)
        {
            var cities = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.IsDeleted == isDeleted).OrderByDescending(x => x.Id).Select(c => new CityDto
                    {
                        CityCode = c.CityCode,
                        CityName = c.CityName,
                        IsDeleted = c.IsDeleted,
                        CountryName = c.State.Country.CountryName,
                        State = c.State,
                        Id = c.Id,
                        CountryId = c.State.Country.Id,
                        StateId = c.State.Id
                    }
                )
                .ToList();
            return cities;
        }

        public IList<DropDownDto> AutoCompleteSearch(string searchText, bool isAutoSearch = false)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return new List<DropDownDto>();
            searchText = searchText.Trim();

            var query = All.Where(x => x.DeletedDate == null).AsQueryable();

            query = query.Where(x =>
                x.CityName.Contains(searchText)
            );

            if (isAutoSearch)
                query = query.Take(7);

            return query.Select(t => new DropDownDto
            {
                Id = t.Id,
                Value = t.CityName
            }).ToList();
        }
    }
}