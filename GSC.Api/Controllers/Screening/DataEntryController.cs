using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
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
        [Route("GetDataEntriesBySubjectForGrid/{projectDesignPeriodId}/{parentProjectId}/{projectId}")]
        public IActionResult GetDataEntriesBySubjectForGrid(int projectDesignPeriodId,int parentProjectId, int projectId)
        {
            return Ok(_dataEntryRespository.GetDataEntriesBySubjectForGrid(projectDesignPeriodId, parentProjectId, projectId));
        }

       
    }
}