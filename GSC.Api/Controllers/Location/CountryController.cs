using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Location;
using GSC.Data.Entities.Location;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
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
        private readonly IUnitOfWork<GscContext> _uow;

        public CountryController(
            ICountryRepository countryRepository,
              IUserRepository userRepository,
              ICompanyRepository companyRepository,
            IUnitOfWork<GscContext> uow,
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
            var countries = _countryRepository.FindBy(x => x.IsDeleted == isDeleted).OrderByDescending(x => x.Id).ToList();

            var countriesDto = _mapper.Map<IEnumerable<CountryDto>>(countries).ToList();
            countriesDto.ForEach(b =>
            {
                if(b.CreatedBy !=null)
                    b.CreatedByUser = _userRepository.Find((int)b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(countriesDto);
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

            return Ok();
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

            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(country.Id);
            country.Id = 0;
            _countryRepository.Add(country);

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
        [Route("GetCountryByParentProjectIdDropDown/{ParentProjectId}")]
        public IActionResult GetCountryByParentProjectIdDropDown(int ParentProjectId)
        {
            return Ok(_countryRepository.GetCountryByParentProjectIdDropDown(ParentProjectId));
        }
    }
}