﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ManageSiteController : BaseController
    {
        private readonly IManageSiteRepository _manageSiteRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public ManageSiteController(IManageSiteRepository manageSiteRepository,
            IUnitOfWork uow, IMapper mapper, IGSCContext context)
        {
            _manageSiteRepository = manageSiteRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var manageSite = _manageSiteRepository.GetManageSites(isDeleted);
            return Ok(manageSite);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var manageSite = _manageSiteRepository
                    .FindByInclude(x => x.Id == id, x => x.City, x => x.City.State, x => x.City.State.Country, x => x.ManageSiteRole)
                    .SingleOrDefault();
            if (manageSite == null)
                return BadRequest();

            if (manageSite.ManageSiteRole != null)
                manageSite.ManageSiteRole = manageSite.ManageSiteRole.Where(x => x.DeletedDate == null).ToList();

            var manageSiteDto = _mapper.Map<ManageSiteDto>(manageSite);
            manageSiteDto.StateId = manageSite.City.State.Id;
            manageSiteDto.CountryId = manageSite.City.State.Country.Id;

            manageSiteDto.CityName = manageSite.City.CityName;
            manageSiteDto.StateName = manageSite.City.State.StateName;
            manageSiteDto.CountryName = manageSite.City.State.Country.CountryName;
            return Ok(manageSiteDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ManageSiteDto manageSiteDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            manageSiteDto.Id = 0;
            var manageSite = _mapper.Map<ManageSite>(manageSiteDto);
            var validate = _manageSiteRepository.Duplicate(manageSite);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _manageSiteRepository.Add(manageSite);
            manageSite.ManageSiteRole.ForEach(x =>
            {
                _context.ManageSiteRole.Add(x);
            });
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Site failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(manageSite.Id);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] ManageSiteDto manageSiteDto)
        {
            if (manageSiteDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var manageSite = _mapper.Map<ManageSite>(manageSiteDto);

            var validate = _manageSiteRepository.Duplicate(manageSite);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _manageSiteRepository.UpdateRole(manageSite);
            /* Added by Darshil for effective Date on 24-07-2020 */
            _manageSiteRepository.Update(manageSite);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Site failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(manageSite.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _manageSiteRepository.Find(id);

            if (record == null)
                return NotFound();

            _manageSiteRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            var record = _manageSiteRepository.Find(id);

            if (record == null)
                return NotFound();
            _manageSiteRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetManageSiteDropDown")]
        public IActionResult GetManageSiteDropDown()
        {
            return Ok(_manageSiteRepository.GetManageSiteDropDown());
        }

        [HttpPost]
        [Route("GetExperienceDetails")]
        public IActionResult GetExperienceDetails([FromBody] ExperienceFillter experience)
        {
            var data = _manageSiteRepository.GetExperienceDetails(experience);
            return Ok(data);
        }
    }
}
