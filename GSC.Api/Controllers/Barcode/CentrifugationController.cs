using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Respository.Barcode;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.Barcode
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentrifugationController : BaseController
    {
        private readonly ICentrifugationRepository _centrifugationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CentrifugationController(ICentrifugationRepository centrifugationRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _centrifugationRepository = centrifugationRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{StudyId}")]
        public IActionResult Get(int StudyId)
        {
            var centrifugation = _centrifugationRepository.All.Where(x => x.DeletedDate == null && x.ProjectId == StudyId).OrderByDescending(x => x.Id).FirstOrDefault();
            return Ok(centrifugation);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CentrifugationDto centrifugationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            centrifugationDto.Id = 0;
            var centrifugation = _mapper.Map<Centrifugation>(centrifugationDto);
           
            _centrifugationRepository.Add(centrifugation);
            if (_uow.Save() <= 0) throw new Exception("Creating Centrifugation failed on save.");
            return Ok(centrifugation.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CentrifugationDto centrifugationDto)
        {
            if (centrifugationDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var centrifugation = _mapper.Map<Centrifugation>(centrifugationDto);
            _centrifugationRepository.AddOrUpdate(centrifugation);
            if (_uow.Save() <= 0) throw new Exception("Updating Centrifugation failed on save.");
            return Ok(centrifugation.Id);
        }

    }
}
