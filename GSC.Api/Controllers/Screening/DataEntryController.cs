using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Project.EditCheck;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class DataEntryController : BaseController
    {
        private readonly IDataEntryRespository _dataEntryRespository;
        private readonly IUnitOfWork _uow;

        public DataEntryController(IDataEntryRespository dataEntryRespository,            IUnitOfWork uow)
        {
            _dataEntryRespository = dataEntryRespository;
            _uow = uow;
        }


        [HttpGet]
        [Route("GetDataEntriesBySubjectForGrid/{projectDesignPeriodId}/{projectId}")]
        public IActionResult GetDataEntriesBySubjectForGrid(int projectDesignPeriodId, int projectId)
        {
            return Ok(_dataEntryRespository.GetDataEntriesBySubjectForGrid(projectDesignPeriodId, projectId));
        }

        [HttpGet]
        [Route("GetVisitForDataEntry/{attendanceId}/{screeningEntryId}")]
        public IActionResult GetVisitForDataEntry(int attendanceId, int screeningEntryId)
        {
            return Ok(_dataEntryRespository.GetVisitForDataEntry(attendanceId, screeningEntryId));
        }

    }
}