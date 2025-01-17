﻿using System;
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
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CityController(ICityRepository cityRepository,
            ICityAreaRepository areaRepository,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _cityRepository = cityRepository;
            _areaRepository = areaRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult GetCities(bool isDeleted)
        {
            var cities = _cityRepository.GetCityList(isDeleted);

            return Ok(cities);

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

            return Ok(city.Id);
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

            /* Added by swati for effective Date on 02-06-2019 */
            _cityRepository.AddOrUpdate(city);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating City failed on save.");
                return BadRequest(ModelState);
            }

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
            return Ok(_cityRepository.GetCityByStateDropDown(id));
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