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
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class CityAreaController : BaseController
    {
        private readonly ICityAreaRepository _cityAreaRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CityAreaController(ICityAreaRepository cityAreaRepository,
            IUserRepository userRepository,
            ICityRepository cityRepository,
            ICompanyRepository companyRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _cityAreaRepository = cityAreaRepository;
            _cityRepository = cityRepository;
            _stateRepository = stateRepository;
            _userRepository = userRepository;
            _countryRepository = countryRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {

            var cityAreas = _cityAreaRepository.GetCityAreaList(isDeleted);
            cityAreas.ForEach(b =>
            {
                b.CityName = _cityRepository.Find((int)b.CityId).CityName;
                b.StateName = _stateRepository.Find(b.City.StateId).StateName;
                b.CountryName = _countryRepository.Find(b.City.State.CountryId).CountryName;
            });
            return Ok(cityAreas);
            ////  return Ok(_cityAreaRepository.GetCitiesArea(isDeleted));
            //var cityAreas = _cityAreaRepository.FindByInclude(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            //    , t => t.City,t=>t.City.State,t=>t.City.State.Country).OrderByDescending(x => x.Id).ToList();
            //return Ok(cityAreasDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var cityArea = _cityAreaRepository
                .FindByInclude(x => x.Id == id, x => x.City, x => x.City.State, x => x.City.State.Country)
                .SingleOrDefault();
            if (cityArea == null)
                return BadRequest();

            var cityAreaDto = _mapper.Map<CityAreaDto>(cityArea);

            if (cityArea.City?.State != null)
                cityAreaDto.StateId = cityArea.City.State.Id;

            if (cityArea.City?.State?.Country != null)
                cityAreaDto.CountryId = cityArea.City.State.Country.Id;

            return Ok(cityAreaDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] CityAreaDto cityAreaDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            cityAreaDto.Id = 0;
            var cityArea = _mapper.Map<CityArea>(cityAreaDto);
            _cityAreaRepository.Add(cityArea);
            if (_uow.Save() <= 0) throw new Exception("Creating city area failed on save.");
            return Ok(cityArea.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CityAreaDto cityAreaDto)
        {
            if (cityAreaDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var cityArea = _mapper.Map<CityArea>(cityAreaDto);
            _cityAreaRepository.AddOrUpdate(cityArea);

            if (_uow.Save() <= 0) throw new Exception("Updating City Area failed on save.");
            return Ok(cityArea.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _cityAreaRepository.Find(id);

            if (record == null)
                return NotFound();

            _cityAreaRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _cityAreaRepository.Find(id);

            if (record == null)
                return NotFound();
            _cityAreaRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetCityAreaDropDown/{cityId}")]
        public IActionResult GetCityAreaDropDown(int cityId)
        {
            return Ok(_cityAreaRepository.GetCityAreaDropDown(cityId));
        }

        [HttpGet("AutoCompleteSearch")]
        public IActionResult AutoCompleteSearch(string searchText)
        {
            var result = _cityAreaRepository.AutoCompleteSearch(searchText, true);
            return Ok(result);
        }
    }
}