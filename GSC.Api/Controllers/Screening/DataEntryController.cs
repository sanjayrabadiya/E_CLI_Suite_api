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

            _screeningVisitRepository.StatusUpdate(screeningVisitHistoryDto);
            _uow.Save();
            return Ok();
        }

        [HttpPut]
        [TransactionRequired]
        [Route("OpenVisit/{screeningVisitId}/{visitDate}")]
        public IActionResult OpenVisit(int screeningVisitId, DateTime visitDate)
        {
            if (_screeningVisitRepository.IsPatientScreeningFailure(screeningVisitId))
            {
                ModelState.AddModelError("Message", "You can't change visit status!");
                return BadRequest(ModelState);
            }

            _screeningVisitRepository.OpenVisit(screeningVisitId, visitDate);
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
        [Route("GetTemplateVisitMyQuery/{screeningVisitId}/{myLevel}")]
        public IActionResult GetTemplateVisitMyQuery(int screeningVisitId, short myLevel)
        {
            return Ok(_dataEntryRespository.GetTemplateVisitMyQuery(screeningVisitId, myLevel));
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

    }
}