using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;


namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class StudyPlanResourceController : BaseController
    {

        private readonly IStudyPlanResourceRepository _studyPlanTaskResourceRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        
        public StudyPlanResourceController(IStudyPlanResourceRepository studyPlanTaskResourceRepository,
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
            var taskResource = _studyPlanTaskResourceRepository.GetTaskResourceList(isDeleted, studyPlanTaskId);
            return Ok(taskResource);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var taskResource = _studyPlanTaskResourceRepository.ResourceById(id);
            return Ok(taskResource);
        }

        [HttpPost]
        public IActionResult Post([FromBody] StudyPlanResourceDto studyPlanResourceDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            studyPlanResourceDto.Id = 0;

            var taskResource = _mapper.Map<StudyPlanResource>(studyPlanResourceDto);
                var validate = _studyPlanTaskResourceRepository.Duplicate(taskResource);
                if (!string.IsNullOrEmpty(validate))
                {
                    ModelState.AddModelError("Message", validate);
                    return BadRequest(ModelState);
                }
                _studyPlanTaskResourceRepository.Add(taskResource);        

            if (_uow.Save() <= 0) return Ok(new Exception("Creating Resource failed on save."));
            //Update TotalCost in Study level and task level
            _studyPlanTaskResourceRepository.TotalCostUpdate(taskResource);

            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] StudyPlanResourceDto studyPlanResourceDto)
        {
            if (studyPlanResourceDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var taskResource = _mapper.Map<StudyPlanResource>(studyPlanResourceDto);
            var validate = _studyPlanTaskResourceRepository.Duplicate(taskResource);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _studyPlanTaskResourceRepository.Update(taskResource);

            if (_uow.Save() <= 0) return Ok(new Exception("Updating Resource failed on save."));
            //Update TotalCost in Study level and task level
            _studyPlanTaskResourceRepository.TotalCostUpdate(taskResource);
            return Ok(taskResource.Id);
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
        
        [HttpGet]
        [Route("ValidationCurrency/{studyPlantaskId}/{resourceId}/{studyplanId}")]
        public IActionResult ValidationCurrency(int studyPlantaskId ,int resourceId,int studyplanId)
        {
            var validate = _studyPlanTaskResourceRepository.ValidationCurrency(resourceId,studyplanId);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            var taskResource = _studyPlanTaskResourceRepository.GetResourceInf(studyplanId, resourceId);
            return Ok(taskResource);
        }
    }
}