using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;
using System;

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
        public IActionResult GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int parentProjectId, int projectId)
        {
            return Ok(_dataEntryRespository.GetDataEntriesBySubjectForGrid(projectDesignPeriodId, parentProjectId, projectId));
        }


        [HttpPost]
        [Route("VisitStatusUpdate")]
        public IActionResult VisitStatusUpdate([FromBody] ScreeningVisitHistoryDto screeningVisitHistoryDto)
        {
            _screeningVisitRepository.StatusUpdate(screeningVisitHistoryDto);
            return Ok();
        }

        [HttpPut]
        [Route("OpenVisit/{screeningVisitId}/{visitDate}")]
        public IActionResult OpenVisit(int screeningVisitId, DateTime visitDate)
        {
            _screeningVisitRepository.OpenVisit(screeningVisitId, visitDate);
            return Ok();
        }

        [HttpGet]
        [Route("GetTemplateForVisit/{screeningEntryId}/{projectDesignVisitId}/{screeningStatus}/{isQuery}")]
        public IActionResult GetTemplateForVisit(int screeningEntryId, int projectDesignVisitId, int screeningStatus, bool isQuery)
        {
            return Ok(_dataEntryRespository.GetTemplateForVisit(screeningEntryId, projectDesignVisitId, screeningStatus,
                isQuery));
        }


    }
}