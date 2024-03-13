using System;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class InvestigatorContactDetailController : BaseController
    {
        private readonly IInvestigatorContactDetailRepository _investigatorContactDetailRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public InvestigatorContactDetailController(IInvestigatorContactDetailRepository investigatorContactDetailRepository,
    IUnitOfWork uow, IMapper mapper)
        {
            _investigatorContactDetailRepository = investigatorContactDetailRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{id}/{isDeleted:bool?}")]
        public IActionResult Get(int id, bool isDeleted)
        {
            if (id <= 0) return BadRequest();

            var investigatorContactDetail = _investigatorContactDetailRepository.All.Where(x => x.InvestigatorContactId == id && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                   ProjectTo<InvestigatorContactDetailGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            return Ok(investigatorContactDetail);
        }

        [HttpPost]
        public IActionResult Post([FromBody] InvestigatorContactDetailDto investigatorContactDetailDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            investigatorContactDetailDto.Id = 0;
            var investigatorContactDetail = _mapper.Map<InvestigatorContactDetail>(investigatorContactDetailDto);

            var validate = _investigatorContactDetailRepository.DuplicateContact(investigatorContactDetail);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }


            _investigatorContactDetailRepository.Add(investigatorContactDetail);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Investigator contact failed on save.");
                return BadRequest(ModelState);
            }
            var returnInvestigatorContactDetailDto = _mapper.Map<InvestigatorContactDetailDto>(investigatorContactDetail);
            return CreatedAtAction("Get", new { id = investigatorContactDetail.Id }, returnInvestigatorContactDetailDto);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] InvestigatorContactDetailDto investigatorContactDetailDto)
        {
            if (investigatorContactDetailDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var investigatorContactDetail = _mapper.Map<InvestigatorContactDetail>(investigatorContactDetailDto);

            var validate = _investigatorContactDetailRepository.DuplicateContact(investigatorContactDetail);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            investigatorContactDetail.Id = investigatorContactDetailDto.Id;

            /* Added by Darshil for effective Date on 16-07-2020 */
            _investigatorContactDetailRepository.AddOrUpdate(investigatorContactDetail);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Investigator contact failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(investigatorContactDetail.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _investigatorContactDetailRepository.Find(id);

            if (record == null)
                return NotFound();

            _investigatorContactDetailRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _investigatorContactDetailRepository.Find(id);

            if (record == null)
                return NotFound();
            _investigatorContactDetailRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetInvestigatorContactDetailDropDown")]
        public IActionResult GetInvestigatorContactDetailDropDown()
        {
            return Ok(_investigatorContactDetailRepository.GetInvestigatorContactDetailDropDown());
        }
    }
}
