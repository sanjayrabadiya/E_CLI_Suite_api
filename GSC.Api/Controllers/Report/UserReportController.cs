using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using Microsoft.AspNetCore.Mvc;
using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Report;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Authorization;
using GSC.Helper;
using GSC.Respository.ProjectRight;

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class UserReportController : BaseController
    {
        private readonly IProjectRightRepository _ProjectRightRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IMapper _mapper;
        public UserReportController(IProjectRightRepository projectRightRepository,
           
            IJwtTokenAccesser jwtTokenAccesser,
            IUnitOfWork<GscContext> uow, IMapper mapper)
        {
            _ProjectRightRepository = projectRightRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetUserReport")]
        public IActionResult GetUserReport([FromQuery]UserReportSearchDto filters)
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
