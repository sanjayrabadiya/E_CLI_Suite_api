using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class AnnotationTypeController : BaseController
    {
        private readonly IAnnotationTypeRepository _annotationTypeRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public AnnotationTypeController(IAnnotationTypeRepository annotationTypeRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _annotationTypeRepository = annotationTypeRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var annotationTypes = _annotationTypeRepository.All.Where(x =>
                (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).ToList();
            var annotationTypesDto = _mapper.Map<IEnumerable<AnnotationTypeDto>>(annotationTypes);
            return Ok(annotationTypesDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var annotationType = _annotationTypeRepository.Find(id);
            var annotationTypeDto = _mapper.Map<AnnotationTypeDto>(annotationType);
            return Ok(annotationTypeDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] AnnotationTypeDto annotationTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            annotationTypeDto.Id = 0;
            var annotationType = _mapper.Map<AnnotationType>(annotationTypeDto);
            _annotationTypeRepository.Add(annotationType);
            if (_uow.Save() <= 0) throw new Exception("Creating Annotation Type failed on save.");
            return Ok(annotationType.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] AnnotationTypeDto annotationTypeDto)
        {
            if (annotationTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var annotationType = _mapper.Map<AnnotationType>(annotationTypeDto);

            _annotationTypeRepository.Update(annotationType);
            if (_uow.Save() <= 0) throw new Exception("Updating Annotation Type failed on save.");
            return Ok(annotationType.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _annotationTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _annotationTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _annotationTypeRepository.Find(id);

            if (record == null)
                return NotFound();
            _annotationTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetAnnotationTypeDropDown")]
        public IActionResult GetannotationTypeDropDown()
        {
            return Ok(_annotationTypeRepository.GetAnnotationTypeDropDown());
        }
    }
}