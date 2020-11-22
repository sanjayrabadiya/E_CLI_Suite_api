using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using Microsoft.AspNetCore.Mvc;
using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Report;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Authorization;
using GSC.Respository.ProjectRight;
using GSC.Shared;

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class ProjectTrainingController : BaseController
    {
        private readonly IProjectRightRepository _ProjectRightRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public ProjectTrainingController(IProjectRightRepository projectRightRepository,
           
            IJwtTokenAccesser jwtTokenAccesser,
            IUnitOfWork uow, IMapper mapper)
        {
            _ProjectRightRepository = projectRightRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetProjectTrainingReport")]
        public IActionResult GetProjectTrainingReport([FromQuery]ProjectTrainigAccessSearchDto filters)
        {
            if (filters.ProjectId <= 0)
            {
                return BadRequest();
            }

             var auditsDto = _ProjectRightRepository.GetProjectTrainingReportList(filters);
           // var auditsDto = "";
            return Ok(auditsDto);
        }
    }
}
