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
    public class CityAreaRepository : GenericRespository<CityArea, GscContext>, ICityAreaRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public CityAreaRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetCityAreaDropDown(int cityId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.CityId == cityId)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.AreaName, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public List<CityAreaDto> GetCitiesArea(bool isDeleted)
        {
            var cityAreas = All.Where(x =>
                (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).Select(c => new CityAreaDto
            {
                AreaName = c.AreaName,
                CityName = c.City.CityName,
                IsDeleted = c.DeletedDate != null,
                CountryName = c.City.State.Country.CountryName,
                StateName = c.City.State.StateName,
                City = c.City,
                CityId = c.CityId,
                Id = c.Id,
                CountryId = c.City.State.Country.Id,
                StateId = c.City.State.Id,
                CompanyId = c.CompanyId
            }).OrderByDescending(x => x.Id).ToList();

            return cityAreas;
        }

        public IList<DropDownDto> AutoCompleteSearch(string searchText, bool isAutoSearch = false)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return new List<DropDownDto>();
            searchText = searchText.Trim();

            var query = All.Where(x => x.DeletedDate == null).AsQueryable();

            query = query.Where(x =>
                x.AreaName.Contains(searchText)
            );

            if (isAutoSearch)
                query = query.Take(7);

            return query.Select(t => new DropDownDto
            {
                Id = t.Id,
                Value = t.AreaName
            }).ToList();
        }

        public List<CityAreaDto> GetCityAreaAll(bool isDeleted)
        {
            var cityareas = All.Where(x =>
                (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).Select(c => new CityAreaDto
            {
                Id = c.Id,
                AreaName = c.AreaName,
                CityName = c.City.CityName,
                IsDeleted = c.DeletedDate != null
            }).ToList();

            return cityareas;
        }
    }
}