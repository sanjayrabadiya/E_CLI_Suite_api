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
    public class StateController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly IStateRepository _stateRepository;
        private readonly IUnitOfWork<GscContext> _uow;

        public StateController(
            IStateRepository stateRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            ICountryRepository countryRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _stateRepository = stateRepository;
            _userRepository = userRepository;
            _countryRepository = countryRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult GetStates(bool isDeleted)
        {
            var states = _stateRepository.FindByInclude(x => x.IsDeleted == isDeleted
               , t => t.Country).OrderByDescending(x => x.Id).ToList();
            //var states = _stateRepository
            //    .FindByInclude(
            //        x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) &&
            //             x.IsDeleted == isDeleted, x => x.Country)
            //    .Select(x => new StateDto
            //    {
            //        Id = x.Id,
            //        CountryId = x.CountryId,
            //        CountryName = x.Country.CountryName,
            //        IsDeleted = x.IsDeleted,
            //        StateName = x.StateName
            //    }).OrderByDescending(x => x.Id).ToList();
            var stateDtoDto = _mapper.Map<IEnumerable<StateDto>>(states);
            stateDtoDto.ForEach(b =>
            {
                b.StateName = _stateRepository.Find(b.Id).StateName;
                b.CountryName = _countryRepository.Find(b.CountryId).CountryName;
                if (b.CreatedBy != null)
                    b.CreatedByUser = _userRepository.Find((int)b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });

            return Ok(stateDtoDto);
        }

        [HttpGet("GetStateDropDown/{id}")]
        public IActionResult GetStateDropDown(int id)
        {
            return Ok(_stateRepository.GetStateDropDown(id));
        }

        [HttpGet("{id}")]
        public IActionResult GetState([FromRoute] int id)
        {
            var country = _stateRepository.Find(id);
            if (country == null)
                return BadRequest();

            return Ok(_mapper.Map<StateDto>(country));
        }

        [HttpPost]
        public IActionResult CreateState([FromBody] InsertStateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var state = _mapper.Map<State>(dto);
            var validate = _stateRepository.DuplicateState(state);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _stateRepository.Add(state);
            _uow.Save();

            return Ok();
        }

        [HttpPut]
        public IActionResult UpdateState([FromBody] UpdateStateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var state = _mapper.Map<State>(dto);
            var validate = _stateRepository.DuplicateState(state);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _stateRepository.AddOrUpdate(state);

            if (_uow.Save() <= 0) throw new Exception("Updating State failed on save.");

            return Ok(state.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _stateRepository.Find(id);

            if (record == null)
                return NotFound();

            _stateRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _stateRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _stateRepository.DuplicateState(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _stateRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}