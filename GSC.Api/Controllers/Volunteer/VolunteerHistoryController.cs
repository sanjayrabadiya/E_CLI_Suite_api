using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Respository.Volunteer;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Volunteer
{
    [Route("api/[controller]")]
    public class VolunteerHistoryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVolunteerHistoryRepository _volunteerHistoryRepository;

        public VolunteerHistoryController(IVolunteerHistoryRepository volunteerHistoryRepository,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _volunteerHistoryRepository = volunteerHistoryRepository;
            _uow = uow;
            _mapper = mapper;
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var volunteerHistory = _volunteerHistoryRepository.Find(id);
            var volunteerHistoryDto = _mapper.Map<VolunteerHistoryDto>(volunteerHistory);
            return Ok(volunteerHistoryDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] VolunteerHistoryDto volunteerHistoryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            volunteerHistoryDto.Id = 0;
            var volunteerHistory = _mapper.Map<VolunteerHistory>(volunteerHistoryDto);
            _volunteerHistoryRepository.Add(volunteerHistory);
            if (_uow.Save() <= 0) throw new Exception("Creating volunteer history failed on save.");
            return Ok(volunteerHistory.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VolunteerHistoryDto volunteerHistoryDto)
        {
            if (volunteerHistoryDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var volunteerHistory = _mapper.Map<VolunteerHistory>(volunteerHistoryDto);
            _volunteerHistoryRepository.Update(volunteerHistory);
            if (_uow.Save() <= 0) throw new Exception("Updating volunteer history failed on save.");
            return Ok(volunteerHistory.Id);
        }
    }
}