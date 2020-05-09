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
    public class InvestigatorContactController : BaseController
    {
        private readonly IInvestigatorContactRepository _investigatorContactRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;

        public InvestigatorContactController(IInvestigatorContactRepository investigatorContactRepository,
            IUserRepository userRepository,
            ICityRepository cityRepository,
            ICompanyRepository companyRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper)
        {
            _investigatorContactRepository = investigatorContactRepository;
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
         //   return Ok(_investigatorContactRepository.GetInvestigatorContact(isDeleted));
            var investigatorContacts = _investigatorContactRepository.FindByInclude(x => x.IsDeleted == isDeleted
               , t => t.City, t => t.City.State, t => t.City.State.Country).OrderByDescending(x => x.Id).ToList();
            var investigatorContactsDto = _mapper.Map<IEnumerable<InvestigatorContactDto>>(investigatorContacts);
            investigatorContactsDto.ForEach(b =>
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
            return Ok(investigatorContactsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var investigatorContact = _investigatorContactRepository
                .FindByInclude(x => x.Id == id, x => x.City, x => x.City.State, x => x.City.State.Country)
                .SingleOrDefault();
            if (investigatorContact == null)
                return BadRequest();

            var investigatorContactDto = _mapper.Map<InvestigatorContactDto>(investigatorContact);

            if (investigatorContact.City != null)
            {
                investigatorContactDto.CityId = investigatorContact.City.Id;
                investigatorContactDto.CityName = investigatorContact.City.CityName;
            }

            if (investigatorContact.City?.State != null)
            {
                investigatorContactDto.StateId = investigatorContact.City.State.Id;
                investigatorContactDto.StateName = investigatorContact.City.State.StateName;
            }

            if (investigatorContact.City?.State?.Country != null)
            {
                investigatorContactDto.CountryId = investigatorContact.City.State.Country.Id;
                investigatorContactDto.CountryName = investigatorContact.City.State.Country.CountryName;
            }

            return Ok(investigatorContactDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] InvestigatorContactDto investigatorContactDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            investigatorContactDto.Id = 0;
            var investigatorContact = _mapper.Map<InvestigatorContact>(investigatorContactDto);
            var validate = _investigatorContactRepository.Duplicate(investigatorContact);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _investigatorContactRepository.Add(investigatorContact);
            if (_uow.Save() <= 0) throw new Exception("Creating Investigator Contact failed on save.");
            return Ok(investigatorContact.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] InvestigatorContactDto investigatorContactDto)
        {
            if (investigatorContactDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var investigatorContact = _mapper.Map<InvestigatorContact>(investigatorContactDto);
            investigatorContact.Id = investigatorContactDto.Id;
            var validate = _investigatorContactRepository.Duplicate(investigatorContact);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }


            Delete(investigatorContact.Id);
            investigatorContact.Id = 0;
            _investigatorContactRepository.Add(investigatorContact);

            if (_uow.Save() <= 0) throw new Exception("Updating Investigator Contact failed on save.");
            return Ok(investigatorContact.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _investigatorContactRepository.Find(id);

            if (record == null)
                return NotFound();

            _investigatorContactRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _investigatorContactRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _investigatorContactRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _investigatorContactRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetCityAreaDropDown/{cityId}")]
        public IActionResult GetCityAreaDropDown(int cityId)
        {
            return Ok(_investigatorContactRepository.GetInvestigatorContactDropDown(cityId));
        }

        [HttpGet]
        [Route("GetInvestigatorContactDropDown")]
        public IActionResult GetInvestigatorContactDropDown()
        {
            return Ok(_investigatorContactRepository.GetAllInvestigatorContactDropDown());
        }
    }
}