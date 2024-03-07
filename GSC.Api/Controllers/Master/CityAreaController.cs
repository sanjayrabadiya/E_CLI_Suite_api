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
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class CityAreaController : BaseController
    {
        private readonly ICityAreaRepository _cityAreaRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CityAreaController(ICityAreaRepository cityAreaRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _cityAreaRepository = cityAreaRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var cityAreas = _cityAreaRepository.GetCityAreaList(isDeleted);
            return Ok(cityAreas);
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
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating city area failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(cityArea.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CityAreaDto cityAreaDto)
        {
            if (cityAreaDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var cityArea = _mapper.Map<CityArea>(cityAreaDto);
            _cityAreaRepository.AddOrUpdate(cityArea);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating City Area failed on save.");
                return BadRequest(ModelState);
            }
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