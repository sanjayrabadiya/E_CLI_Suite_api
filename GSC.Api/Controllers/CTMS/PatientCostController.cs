using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientCostController : BaseController
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

        [HttpGet]
        [Route("CheckVisitData/{isDeleted:bool?}/{studyId:int}")]
        public IActionResult CheckVisitData(bool isDeleted, int studyId)
        {
            var studyplan = _patientCostRepository.CheckVisitData(isDeleted, studyId);
            return Ok(studyplan);
        }

        [HttpGet]
        [Route("GetPullPatientCost/{isDeleted:bool?}/{studyId:int}/{procedureId:int?}/{ispull:bool?}")]
        public IActionResult GetPullPatientCost(bool isDeleted, int studyId, int? procedureId,bool ispull)
        {
            var studyplan = _patientCostRepository.GetPullPatientCost(isDeleted, studyId, procedureId, ispull);
            return Ok(studyplan);
        }

        [HttpGet("{isDeleted:bool?}/{studyId:int}")]
        public IActionResult Get(bool isDeleted, int studyId)
        {
            var studyplan = _patientCostRepository.GetPatientCostGrid(isDeleted, studyId);
            return Ok(studyplan);
        }

        [HttpPost]
        public IActionResult Post([FromBody] List<ProcedureVisitdadaDto> ProcedureVisitdadaDto)
        {
            var validate = _patientCostRepository.Duplicate(ProcedureVisitdadaDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _patientCostRepository.AddPatientCost(ProcedureVisitdadaDto);
            return Ok(true);
        }

        [HttpGet]
        [Route("DeletePatientCost/{projectId:int?}/{procedureId:int}")]
        public IActionResult DeletePatientCost(int projectId, int procedureId)
        {
           _patientCostRepository.DeletePatientCost(projectId, procedureId);
            return Ok();
        }

        //add new Visit

        [HttpPut("addVisit")]
        public IActionResult addVisit([FromBody] ProcedureVisitdadaDto data)
        {
            if (data.ProjectId <= 0) return BadRequest();

            //Duplicat vivit Check
            if (_patientCostRepository.FindBy(x => x.ProjectId == data.ProjectId && x.DeletedBy == null && x.VisitName == data.VisitName).Count()>0)
            {
                ModelState.AddModelError("Message", "Duplicate "+ data.VisitName +" Visit Name");
                return BadRequest(ModelState);
            }

            //Add new visit
            var tastMaster = _mapper.Map<PatientCost>(data);
                _patientCostRepository.Add(tastMaster);
                _uow.Save();

            //Add visit in allready added  pation cost
            var patientCost1 = _context.PatientCost.Where(s => s.ProjectId == data.ProjectId && s.ProcedureId != null && s.DeletedBy == null).
            Select(t => new PatientCost
            {
                ProjectId = data.ProjectId,
                ProcedureId = t.ProcedureId,
                VisitName=data.VisitName,
                VisitDescription=data.VisitDescription,
                Rate = t.Rate,
                CurrencyRateId = t.CurrencyRateId,
                CurrencyId = t.CurrencyId,
                IfPull = t.IfPull,
            }).Distinct().ToList();

            patientCost1.ForEach(t =>
            {
                _context.PatientCost.Add(t);
                _context.Save();
            });
            return Ok(data);
        }

        [HttpGet("GetAddedVisitDataList/{isDeleted:bool?}/{projectId}")]
        public IActionResult GetAddedVisitDataList(bool isDeleted, int projectId)
        {
            var task = _patientCostRepository.FindByInclude(x => x.ProjectId == projectId && x.DeletedBy == null && x.ProcedureId == null).Distinct().ToList();
            return Ok(task);
        }

        [HttpGet]
        [Route("DeleteVisit/{projectId:int?}/{visitName}")]
        public IActionResult DeleteVisit(int projectId, string visitName)
        {
            var record = _patientCostRepository.FindByInclude(x => x.ProjectId == projectId && x.VisitName== visitName && x.DeletedBy == null).ToList();
            if (record == null) return NotFound();

            record.ForEach(x => _patientCostRepository.Delete(x));
            _uow.Save();

            return Ok();
        }
    }
}
