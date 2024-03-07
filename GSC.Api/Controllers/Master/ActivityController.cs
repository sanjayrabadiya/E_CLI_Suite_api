using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Respository.Master;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ActivityController : BaseController
    {
        private readonly IActivityRepository _activityRepository;
        private readonly ICtmsActivityRepository _ctmsActivityRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ActivityController(IActivityRepository activityRepository,
            ICtmsActivityRepository ctmsActivityRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _activityRepository = activityRepository;
            _ctmsActivityRepository = ctmsActivityRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var activitysDto = _activityRepository.GetActivityList(isDeleted);
            return Ok(activitysDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var activity = _activityRepository.Find(id);
            var activityDto = _mapper.Map<ActivityDto>(activity);
            return Ok(activityDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ActivityDto activityDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            activityDto.Id = 0;
            var activity = _mapper.Map<Activity>(activityDto);
            var validate = _activityRepository.Duplicate(activity);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _activityRepository.Add(activity);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Contact Type failed on save.");
                return BadRequest(ModelState);
            }

            return Ok(activity.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ActivityDto activityDto)
        {
            if (activityDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var activity = _mapper.Map<Activity>(activityDto);
            var validate = _activityRepository.Duplicate(activity);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _activityRepository.Update(activity);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Contact Type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(activity.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _activityRepository.Find(id);

            if (record == null)
                return NotFound();

            _activityRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _activityRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _activityRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _activityRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetActivityDropDown")]
        public IActionResult GetActivityDropDown()
        {
            return Ok(_activityRepository.GetActivityDropDown());
        }

        [HttpGet]
        [Route("GetActivityDropDownByModuleId/{moduleId}")]
        public IActionResult GetActivityDropDownByModuleId(int moduleId)
        {
            return Ok(_activityRepository.GetActivityDropDownByModuleId(moduleId));
        }

        [HttpGet]
        [Route("GetCtmsActivity")]
        public IActionResult GetCtmsActivity()
        {
            return Ok(_ctmsActivityRepository.GetCtmsActivityList());
        }

        [HttpGet]
        [Route("GetActivityForFormList/{tabNumber}")]
        public IActionResult GetActivityForFormList(int tabNumber)
        {
            return Ok(_activityRepository.GetActivityForFormList(tabNumber));
        }
    }
}