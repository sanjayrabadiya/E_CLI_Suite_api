using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Location;
using GSC.Data.Entities.Location;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Location
{
    [Route("api/[controller]")]
    public class CityController : BaseController
    {
        private readonly ICityAreaRepository _areaRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;

        public CityController(ICityRepository cityRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository,
            ICityAreaRepository areaRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper)
        {
            _cityRepository = cityRepository;
            _stateRepository = stateRepository;
            _userRepository = userRepository;
            _countryRepository = countryRepository;
            _companyRepository = companyRepository;
            _areaRepository = areaRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult GetCities(bool isDeleted)
        {
         //   return Ok(_cityRepository.GetCities(isDeleted));
            var citys = _cityRepository.FindByInclude(x => x.IsDeleted == isDeleted
               , t => t.State, t => t.State.Country).OrderByDescending(x => x.Id).ToList();
            var cityAreasDto = _mapper.Map<IEnumerable<CityDto>>(citys);
            cityAreasDto.ForEach(b =>
            {
                b.StateName = _stateRepository.Find(b.StateId).StateName;
                b.CountryName = _countryRepository.Find(b.State.CountryId).CountryName;
                if (b.CreatedBy != null)
                    b.CreatedByUser = _userRepository.Find((int)b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(cityAreasDto);
        }

        [HttpGet("{id}")]
        public IActionResult GetCity([FromRoute] int id)
        {
            var city = _cityRepository.FindByInclude(x => x.Id == id, x => x.State, x => x.State.Country)
                .SingleOrDefault();
            if (city == null)
                return BadRequest();

            var cityDto = _mapper.Map<CityDto>(city);

            if (city.State?.Country != null)
                cityDto.CountryId = city.State.Country.Id;

            return Ok(cityDto);
        }

        [HttpPost]
        public IActionResult CreateCity([FromBody] InsertCityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var city = _mapper.Map<City>(dto);
            var validate = _cityRepository.DuplicateCity(city);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _cityRepository.Add(city);
            _uow.Save();

            return Ok();
        }

        [HttpPut]
        public IActionResult UpdateCity([FromBody] UpdateCityDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var city = _mapper.Map<City>(dto);
            var validate = _cityRepository.DuplicateCity(city);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(city.Id);
            city.Id = 0;
            _cityRepository.Add(city);

            if (_uow.Save() <= 0) throw new Exception("Updating City failed on save.");

            return Ok(city.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _cityRepository.Find(id);

            if (record == null)
                return NotFound();

            _cityRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _cityRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _cityRepository.DuplicateCity(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _cityRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetCitiesByState/{id}")]
        public IActionResult GetCitiesByState(int id)
        {
            return Ok(_cityRepository.All.Where(x => x.StateId == id));
        }


        [HttpGet("GetAreasByCity/{id}")]
        public IActionResult GetAreasByCity(int id)
        {
            return Ok(_areaRepository.GetCityAreaDropDown(id));
        }


        [HttpGet]
        [Route("GetCityDropDown")]
        public IActionResult GetCityDropDown()
        {
            return Ok(_cityRepository.GetCityDropDown());
        }

        [HttpGet("AutoCompleteSearch")]
        public IActionResult AutoCompleteSearch(string searchText)
        {
            var result = _cityRepository.AutoCompleteSearch(searchText, true);
            return Ok(result);
        }
    }
}