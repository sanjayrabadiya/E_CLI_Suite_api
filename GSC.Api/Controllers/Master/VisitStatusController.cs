using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Helper;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class VisitStatusController : BaseController
    {
        private readonly IVisitStatusRepository _visitStatusRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;


        public VisitStatusController(IVisitStatusRepository visitStatusRepository,
    IUnitOfWork uow,
    IMapper mapper,
    IJwtTokenAccesser jwtTokenAccesser)
        {
            _visitStatusRepository = visitStatusRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {

            var visitStatus = _visitStatusRepository.GetVisitStatusList(isDeleted);
            return Ok(visitStatus);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var visitStatus = _visitStatusRepository.Find(id);
            var visitStatusDto = _mapper.Map<VisitStatusDto>(visitStatus);
            return Ok(visitStatusDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VisitStatusDto visitStatusDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            visitStatusDto.Id = 0;
            var visitStatus = _mapper.Map<VisitStatus>(visitStatusDto);
            var validate = _visitStatusRepository.Duplicate(visitStatus);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _visitStatusRepository.Add(visitStatus);
            if (_uow.Save() <= 0) throw new Exception("Creating Visit Status failed on save.");
            return Ok(visitStatusDto.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VisitStatusDto visitStatusDto)
        {
            if (visitStatusDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var visitStatus = _mapper.Map<VisitStatus>(visitStatusDto);
            var validate = _visitStatusRepository.Duplicate(visitStatus);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by darshil for effective Date on 17-08-2020 */
            _visitStatusRepository.AddOrUpdate(visitStatus);

            if (_uow.Save() <= 0) throw new Exception("Updating visit Status failed on save.");
            return Ok(visitStatus.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _visitStatusRepository.Find(id);

            if (record == null)
                return NotFound();

            _visitStatusRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _visitStatusRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _visitStatusRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _visitStatusRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetVisitStatusDropDown")]
        public IActionResult GetVisitStatusDropDown()
        {
            return Ok(_visitStatusRepository.GetVisitStatusDropDown());
        }

        [HttpGet]
        [Route("GetAutoVisitStatusDropDown")]
        public IActionResult GetAutoVisitStatusDropDown()
        {
            return Ok(_visitStatusRepository.GetAutoVisitStatusDropDown());
        }

        [HttpGet]
        [Route("GetManualVisitStatusDropDown")]
        public IActionResult GetManualVisitStatusDropDown()
        {
            return Ok(_visitStatusRepository.GetManualVisitStatusDropDown());
        }
    }
}
