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
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Location
{
    [Route("api/[controller]")]
    public class CountryController : BaseController
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CountryController(
            ICountryRepository countryRepository,
              IUserRepository userRepository,
              ICompanyRepository companyRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _countryRepository = countryRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var countries = _countryRepository.GetCountryList(isDeleted);
            return Ok(countries);
            //var countries = _countryRepository.FindBy(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();
        }

        [HttpGet("{id}")]
        public IActionResult GetCountry([FromRoute] int id)
        {
            var country = _countryRepository.Find(id);
            if (country == null)
                return BadRequest();

            return Ok(_mapper.Map<CountryDto>(country));
        }

        [HttpPost]
        public IActionResult CreateCountry([FromBody] InsertCountryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var country = _mapper.Map<Country>(dto);


            var validate = _countryRepository.DuplicateCountry(country);

            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }


            _countryRepository.Add(country);

            _uow.Save();

            return Ok(country.Id);
        }

        [HttpPut]
        public IActionResult UpdateCountry([FromBody] UpdateCountryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var country = _mapper.Map<Country>(dto);

            var validate = _countryRepository.DuplicateCountry(country);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _countryRepository.AddOrUpdate(country);

            if (_uow.Save() <= 0) throw new Exception("Updating Country failed on save.");

            return Ok(country.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _countryRepository.Find(id);

            if (record == null)
                return NotFound();

            _countryRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _countryRepository.Find(id);

            if (record == null)
                return NotFound();


            var validate = _countryRepository.DuplicateCountry(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }


            _countryRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetCountryDropDown")]
        public IActionResult GetCountryDropDown()
        {
            return Ok(_countryRepository.GetCountryDropDown());
        }

        [HttpGet]
        [Route("GetProjectCountryDropDown")]
        public IActionResult GetProjectCountryDropDown()
        {
            return Ok(_countryRepository.GetProjectCountryDropDown());
        }

        [HttpGet]
        [Route("GetCountryByProjectIdDropDown/{ParentProjectId}")]
        public IActionResult GetCountryByProjectIdDropDown(int ParentProjectId)
        {
            return Ok(_countryRepository.GetCountryByProjectIdDropDown(ParentProjectId));
        }
        [HttpGet]
        [Route("GetCountryByProjectIdDropDownDepot/{ParentProjectId}/{id}")]
        public IActionResult GetCountryByProjectIdDropDownDepot(int ParentProjectId, int id)
        {
            return Ok(_countryRepository.GetCountryByProjectIdDropDownDepot(ParentProjectId, id));
        }
    }
}