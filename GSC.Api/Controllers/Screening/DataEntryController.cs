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
        private readonly IScreeningTemplateValueEditCheckRepository _screeningTemplateValueEditCheckRepository;
        private readonly IUnitOfWork _uow;

        public DataEntryController(IDataEntryRespository dataEntryRespository,
            IScreeningTemplateValueEditCheckRepository screeningTemplateValueEditCheckRepository,
            IUnitOfWork uow)
        {
            _dataEntryRespository = dataEntryRespository;
            _screeningTemplateValueEditCheckRepository = screeningTemplateValueEditCheckRepository;
            _uow = uow;
        }


        [HttpGet]
        [Route("GetDataEntriesBySubject/{projectDesignPeriodId}/{projectId}")]
        public IActionResult GetDataEntriesBySubject(int projectDesignPeriodId, int projectId)
        {
            return Ok(_dataEntryRespository.GetDataEntriesBySubject(projectDesignPeriodId, projectId));
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

        [HttpGet]
        [Route("GetTemplateForVisit/{screeningEntryId}/{projectDesignVisitId}/{screeningStatus}/{isQuery}")]
        public IActionResult GetTemplateForVisit(int screeningEntryId, int projectDesignVisitId,
            ScreeningStatus screeningStatus, bool isQuery)
        {
            return Ok(_dataEntryRespository.GetTemplateForVisit(screeningEntryId, projectDesignVisitId, screeningStatus,
                isQuery));
        }

        [HttpPost("ValidateRuleByEditCheckDetailId")]
        public IActionResult ValidateRuleByEditCheckDetailId([FromBody] VariableEditCheckDto variableEditCheckDto)
        {
            //_ruleRepository.ValidateRuleByEditCheckDetailId(variableEditCheckDto);
            //_uow.Save();
            return Ok(_screeningTemplateValueEditCheckRepository.EditCheckSet(variableEditCheckDto.ScreeningTemplateId,
                true));
        }
    }
}