using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class StudyPlanTaskResourceController : BaseController
    {

        private readonly IStudyPlanTaskResourceRepository _studyPlanTaskResourceRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public StudyPlanTaskResourceController(IStudyPlanTaskResourceRepository studyPlanTaskResourceRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _studyPlanTaskResourceRepository = studyPlanTaskResourceRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}/{studyPlanTaskId}")]
        public IActionResult Get(bool isDeleted, int studyPlanTaskId)
        {
            var studyPlanTaskResource = _studyPlanTaskResourceRepository.GetStudyPlanTaskResourceList(isDeleted, studyPlanTaskId);
            return Ok(studyPlanTaskResource);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var studyPlanTaskResource = _studyPlanTaskResourceRepository.Find(id);
            var studyPlanTaskResourceDto = _mapper.Map<StudyPlanTaskResourceDto>(studyPlanTaskResource);
            return Ok(studyPlanTaskResourceDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] StudyPlanTaskResourceDto studyPlanTaskResourceDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            foreach (var item in studyPlanTaskResourceDto.Users)
            {
                studyPlanTaskResourceDto.Id = 0;
                studyPlanTaskResourceDto.UserId = item;
                var studyPlanTaskResource = _mapper.Map<StudyPlanTaskResource>(studyPlanTaskResourceDto);
                var validate = _studyPlanTaskResourceRepository.Duplicate(studyPlanTaskResource);
                if (!string.IsNullOrEmpty(validate))
                {
                    ModelState.AddModelError("Message", validate);
                    return BadRequest(ModelState);
                }

                _studyPlanTaskResourceRepository.Add(studyPlanTaskResource);
            }

            if (_uow.Save() <= 0) throw new Exception("Creating Resource failed on save.");
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] StudyPlanTaskResourceDto studyPlanTaskResourceDto)
        {
            if (studyPlanTaskResourceDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var studyPlanTaskResource = _mapper.Map<StudyPlanTaskResource>(studyPlanTaskResourceDto);
            var validate = _studyPlanTaskResourceRepository.Duplicate(studyPlanTaskResource);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _studyPlanTaskResourceRepository.Update(studyPlanTaskResource);

            if (_uow.Save() <= 0) throw new Exception("Updating Resource failed on save.");
            return Ok(studyPlanTaskResource.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _studyPlanTaskResourceRepository.Find(id);

            if (record == null)
                return NotFound();

            _studyPlanTaskResourceRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _studyPlanTaskResourceRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _studyPlanTaskResourceRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _studyPlanTaskResourceRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}