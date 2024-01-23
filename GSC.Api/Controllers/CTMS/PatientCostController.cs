using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class  PatientCostController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IPatientCostRepository _patientCostRepository;
        private readonly IGSCContext _context;

        public PatientCostController(IUnitOfWork uow, IMapper mapper,
            IPatientCostRepository studyPlanRepository, IGSCContext context)
        {
            _uow = uow;
            _mapper = mapper;
            _patientCostRepository = studyPlanRepository;
            _context = context;
        }

        [HttpGet("{isDeleted:bool?}/{studyId:int}")]
        public IActionResult Get(bool isDeleted, int studyId)
        {
            var studyplan = _patientCostRepository.getBudgetPlaner(isDeleted, studyId);
            return Ok(studyplan);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProcedureVisitdadaDto procedureDtoDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            procedureDtoDto.Id = 0;
    

            //_procedureRepository.Add(procedure);
            //if (_uow.Save() <= 0) throw new Exception("Creating Procedure failed on save.");
            return Ok();
        }

    }
}
