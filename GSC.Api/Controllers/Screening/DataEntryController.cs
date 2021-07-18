using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Helper;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class DataEntryController : BaseController
    {
        private readonly IDataEntryRespository _dataEntryRespository;
        private readonly IUnitOfWork _uow;
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        public DataEntryController(IDataEntryRespository dataEntryRespository, IUnitOfWork uow,
            IScreeningVisitRepository screeningVisitRepository)
        {
            _dataEntryRespository = dataEntryRespository;
            _screeningVisitRepository = screeningVisitRepository;
            _uow = uow;
        }


        [HttpGet]
        [Route("GetDataEntriesBySubjectForGrid/{projectDesignPeriodId}/{parentProjectId}/{projectId}")]
        public async Task<IActionResult> GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int parentProjectId, int projectId)
        {
            var result = await _dataEntryRespository.GetDataEntriesBySubjectForGrid(projectDesignPeriodId, parentProjectId, projectId);
            return Ok(result);
        }


        [HttpPost]
        [TransactionRequired]
        [Route("VisitStatusUpdate")]
        public IActionResult VisitStatusUpdate([FromBody] ScreeningVisitHistoryDto screeningVisitHistoryDto)
        {

            if (_screeningVisitRepository.IsPatientScreeningFailure(screeningVisitHistoryDto.ScreeningVisitId))
            {
                ModelState.AddModelError("Message", "You can't change visit status!");
                return BadRequest(ModelState);
            }

            if (screeningVisitHistoryDto.VisitStatusId == ScreeningVisitStatus.ReSchedule)
            {
                var validation = _screeningVisitRepository.CheckScheduleDate(screeningVisitHistoryDto);
                if (!string.IsNullOrEmpty(validation))
                {
                    ModelState.AddModelError("Message", validation);
                    return BadRequest(ModelState);
                }
            }

            _screeningVisitRepository.StatusUpdate(screeningVisitHistoryDto);
            _uow.Save();
            return Ok();
        }

        [HttpPut]
        [TransactionRequired]
        [Route("OpenVisit")]
        public IActionResult OpenVisit([FromBody] ScreeningVisitDto screeningVisitDto)
        {
            if (_screeningVisitRepository.IsPatientScreeningFailure(screeningVisitDto.ScreeningVisitId))
            {
                ModelState.AddModelError("Message", "You can't change visit status!");
                return BadRequest(ModelState);
            }

            _screeningVisitRepository.OpenVisit(screeningVisitDto);
            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("GetTemplateForVisit/{screeningVisitId}/{templateStatus}")]
        public IActionResult GetTemplateForVisit(int screeningVisitId, ScreeningTemplateStatus templateStatus)
        {
            return Ok(_dataEntryRespository.GetTemplateForVisit(screeningVisitId, templateStatus));
        }

        [HttpGet]
        [Route("GetTemplateVisitMyQuery/{screeningVisitId}/{parentProjectId}")]
        public IActionResult GetTemplateVisitMyQuery(int screeningVisitId, int parentProjectId)
        {
            return Ok(_dataEntryRespository.GetTemplateVisitMyQuery(screeningVisitId, parentProjectId));
        }

        [HttpGet]
        [Route("GetTemplateVisitWorkFlow/{screeningVisitId}/{reviewLevel}")]
        public IActionResult GetTemplateVisitWorkFlow(int screeningVisitId, short reviewLevel)
        {
            return Ok(_dataEntryRespository.GetTemplateVisitWorkFlow(screeningVisitId, reviewLevel));
        }

        [HttpGet]
        [Route("GetTemplateVisitQuery/{screeningVisitId}/{queryStatus}")]
        public IActionResult GetTemplateVisitQuery(int screeningVisitId, QueryStatus queryStatus)
        {
            return Ok(_dataEntryRespository.GetTemplateVisitQuery(screeningVisitId, queryStatus));
        }

        [HttpPost("VisitRepeat")]
        [TransactionRequired]
        public IActionResult VisitRepeat([FromBody] ScreeningVisitDto screeningVisitDto)
        {
            if (!_screeningVisitRepository.ValidateRepeatVisit(screeningVisitDto.ScreeningVisitId))
            {
                ModelState.AddModelError("Message", "You can't create repeat visit!");
                return BadRequest(ModelState);
            }
            _screeningVisitRepository.VisitRepeat(screeningVisitDto);
            _uow.Save();
            return Ok(screeningVisitDto.ScreeningVisitId);
        }

        [HttpGet]
        [Route("GetDataEntriesStatus/{projectId}")]
        public IActionResult GetDataEntriesStatus(int projectId)
        {
            return Ok(_dataEntryRespository.GetDataEntriesStatus(projectId));
        }

        [HttpGet]
        [Route("GetVisitStatus/{projectId}")]
        public IActionResult GetVisitStatus(int projectId)
        {
            return Ok(_screeningVisitRepository.GetVisitStatus(projectId));
        }

        [HttpGet]
        [Route("GetMyTemplateView/{parentProjectId}/{projectId}")]
        public IActionResult GetMyTemplateView(int parentProjectId, int projectId)
        {
            return Ok(_dataEntryRespository.GetMyTemplateView(parentProjectId, projectId));
        }

    }
}