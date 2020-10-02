using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IIecirbRepository _iecirbRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IManageSiteRepository _manageSiteRepository;

        public InvestigatorContactController(IInvestigatorContactRepository investigatorContactRepository,
            IUnitOfWork uow, IMapper mapper, IIecirbRepository iecirbRepository,
            ICityRepository cityRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository,
            IManageSiteRepository manageSiteRepository)
        {
            _investigatorContactRepository = investigatorContactRepository;
            _uow = uow;
            _mapper = mapper;
            _iecirbRepository = iecirbRepository;
            _cityRepository = cityRepository;
            _stateRepository = stateRepository;
            _countryRepository = countryRepository;
            _manageSiteRepository = manageSiteRepository;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var investigatorContacts = _investigatorContactRepository.All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<InvestigatorContactGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            return Ok(investigatorContacts);
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
            investigatorContactDto.StateId = investigatorContact.City.State.Id;
            investigatorContactDto.CountryId = investigatorContact.City.State.Country.Id;
            investigatorContactDto.IecirbName = _iecirbRepository.Find(investigatorContactDto.IecirbId).IECIRBName;
            investigatorContactDto.CityName = _cityRepository.Find(investigatorContactDto.CityId).CityName;
            investigatorContactDto.StateName = _stateRepository.Find(investigatorContact.City.State.Id).StateName;
            investigatorContactDto.CountryName = _countryRepository.Find(investigatorContact.City.State.Country.Id).CountryName;
            investigatorContactDto.SiteName = _manageSiteRepository.Find(investigatorContactDto.ManageSiteId).SiteName;
            investigatorContactDto.SiteAddress = _manageSiteRepository.Find(investigatorContactDto.ManageSiteId).SiteAddress;


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

            _investigatorContactRepository.AddOrUpdate(investigatorContact);
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