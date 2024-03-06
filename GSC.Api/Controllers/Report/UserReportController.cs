using Microsoft.AspNetCore.Mvc;
using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Report;
using GSC.Respository.ProjectRight;

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class UserReportController : BaseController
    {
        private readonly IProjectRightRepository _ProjectRightRepository; 
        public UserReportController(IProjectRightRepository projectRightRepository
           )
        {
            _ProjectRightRepository = projectRightRepository;
        
        }

        [HttpPost]
        [Route("GetUserReport")]
        public IActionResult GetUserReport([FromBody]UserReportSearchDto filters)
        {
            if (filters.UserId <= 0)
            {
                return BadRequest();
            }
            if(filters.UserId<=3)
            {
                return Ok( _ProjectRightRepository.GetUserReportList(filters));
            }
            else
                return Ok( _ProjectRightRepository.GetLoginLogoutReportList(filters));
            
        }

        
    }
}
