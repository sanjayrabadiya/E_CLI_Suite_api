using System;
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
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;

        public ManageSiteController(IManageSiteRepository manageSiteRepository,
            IUserRepository userRepository,
            ICityRepository cityRepository,
            ICompanyRepository companyRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper)
        {
            _manageSiteRepository = manageSiteRepository;
            _cityRepository = cityRepository;
            _stateRepository = stateRepository;
            _userRepository = userRepository;
            _countryRepository = countryRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var manageSite = _manageSiteRepository.FindByInclude(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null
               , t => t.City, t => t.City.State, t => t.City.State.Country).OrderByDescending(x => x.Id).ToList();
            var manageSiteDto = _mapper.Map<IEnumerable<ManageSiteDto>>(manageSite);
            manageSiteDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find((int)b.CreatedBy).UserName;
                b.CityName = _cityRepository.Find((int)b.CityId).CityName;
                b.StateName = _stateRepository.Find(b.City.StateId).StateName;
                b.CountryName = _countryRepository.Find(b.City.State.CountryId).CountryName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(manageSiteDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var manageSite = _manageSiteRepository
    .FindByInclude(x => x.Id == id, x => x.City, x => x.City.State, x => x.City.State.Country)
    .SingleOrDefault();
            if (manageSite == null)
                return BadRequest();

            var manageSiteDto = _mapper.Map<ManageSiteDto>(manageSite);

            if (manageSite.City != null)
            {
                manageSiteDto.CityId = manageSite.City.Id;
                manageSiteDto.CityName = manageSite.City.CityName;
            }

            if (manageSite.City?.State != null)
            {
                manageSiteDto.StateId = manageSite.City.State.Id;
                manageSiteDto.StateName = manageSite.City.State.StateName;
            }

            if (manageSite.City?.State?.Country != null)
            {
                manageSiteDto.CountryId = manageSite.City.State.Country.Id;
                manageSiteDto.CountryName = manageSite.City.State.Country.CountryName;
            }
            //if (id <= 0) return BadRequest();

            //var manageSite = _manageSiteRepository.GetManageSiteList(id);
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
            if (_uow.Save() <= 0) throw new Exception("Creating Site failed on save.");
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

            manageSite.Id = manageSiteDto.Id;

            /* Added by Darshil for effective Date on 24-07-2020 */
            _manageSiteRepository.AddOrUpdate(manageSite);

            if (_uow.Save() <= 0) throw new Exception("Updating Site failed on save.");
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
    }
}
