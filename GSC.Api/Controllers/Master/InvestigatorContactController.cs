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
        private readonly IInvestigatorContactDetailRepository _investigatorContactDetailRepository;
        private readonly ISiteRepository _siteRepository;

        public InvestigatorContactController(IInvestigatorContactRepository investigatorContactRepository, IInvestigatorContactDetailRepository investigatorContactDetailRepository, ISiteRepository siteRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _investigatorContactRepository = investigatorContactRepository;
            _uow = uow;
            _mapper = mapper;
            _investigatorContactDetailRepository = investigatorContactDetailRepository;
            _siteRepository = siteRepository;
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
            if (id <= 0) return BadRequest();
            var investigatorContact = _investigatorContactRepository.Find(id);
            var investigatorContactDto = _mapper.Map<InvestigatorContactDto>(investigatorContact);
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
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Investigator Contact failed on save.");
                return BadRequest(ModelState);
            }
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

            var investigatorContactDetail = _investigatorContactDetailRepository.FindByInclude(x => x.InvestigatorContactId == investigatorContactDto.Id, x => x.ContactType).ToList();
            var investigatorSite = _siteRepository.FindByInclude(x => x.InvestigatorContactId == investigatorContactDto.Id, x => x.ManageSite).ToList();

            _investigatorContactRepository.AddOrUpdate(investigatorContact);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Investigator Contact failed on save.");
                return BadRequest(ModelState);
            }

            foreach (var item in investigatorContactDetail)
            {
                item.Id = 0;
                item.InvestigatorContactId = investigatorContact.Id;
                _investigatorContactDetailRepository.Add(item);
            }

            foreach (var item in investigatorSite)
            {
                item.Id = 0;
                item.InvestigatorContactId = investigatorContact.Id;
                _siteRepository.Add(item);

            }
            _uow.Save();

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