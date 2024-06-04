using GSC.Api.Controllers.Common;
using GSC.Helper;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetPlanerController : BaseController
    {
        private readonly IStudyPlanTaskRepository _studyPlanTaskRepository;
        public BudgetPlanerController(IStudyPlanTaskRepository studyPlanTaskRepository)
        {
            _studyPlanTaskRepository = studyPlanTaskRepository;
        }

        [HttpGet("{isDeleted:bool?}/{studyId:int}/{siteId:int}/{countryId:int}/{filter}")]
        public IActionResult Get(bool isDeleted, int studyId, int siteId, int countryId, CtmsStudyTaskFilter filter)
        {
            var studyplan = _studyPlanTaskRepository.getBudgetPlaner(isDeleted, studyId, siteId, countryId, filter);
            return Ok(studyplan);
        }

        [HttpGet]
        [Route("GetCountryDropdown/{parentId}")]
        public IActionResult GetCountryDropdown(int parentId)
        {
            return Ok(_studyPlanTaskRepository.GetBudgetCountryDropDown(parentId));
        }

        [HttpGet]
        [Route("GetSiteDropdown/{parentId}")]
        public IActionResult GetSiteDropdown(int parentId)
        {
            return Ok(_studyPlanTaskRepository.GetBudgetSiteDropDown(parentId));
        }
    }
}
