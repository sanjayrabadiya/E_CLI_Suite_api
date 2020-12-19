using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Api.Controllers.Common;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppScreenPatientRightsController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IAppScreenPatientRightsRepository _appScreenPatientRightsRepository;

        public AppScreenPatientRightsController(IJwtTokenAccesser jwtTokenAccesser,
            IAppScreenPatientRightsRepository appScreenPatientRightsRepository)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _appScreenPatientRightsRepository = appScreenPatientRightsRepository;
        }

        [HttpGet]
        public IActionResult GetAppScreenPatientList(int projectid)
        {
            var data = _appScreenPatientRightsRepository.GetAppScreenPatientList(projectid);
            return Ok(data);
        }
    }
}
