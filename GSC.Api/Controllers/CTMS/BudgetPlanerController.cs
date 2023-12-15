
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetPlanerController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IStudyPlanTaskRepository _studyPlanTaskRepository;

        public BudgetPlanerController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, IStudyPlanRepository studyPlanRepository, IGSCContext context, IStudyPlanTaskRepository studyPlanTaskRepository,
            IUploadSettingRepository uploadSettingRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _studyPlanTaskRepository = studyPlanTaskRepository;
        }

        [HttpGet("{isDeleted:bool?}/{studyId:int}/{siteId:int}/{countryId:int}")]
        public IActionResult Get(bool isDeleted, int studyId, int siteId, int countryId)
        {
            var studyplan = _studyPlanTaskRepository.getBudgetPlaner(isDeleted, studyId, siteId, countryId);
            return Ok(studyplan);
        }
    }
}
