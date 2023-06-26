using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitEmailConfigurationController : BaseController
    {
        private readonly IVisitEmailConfigurationRepository _visitEmailConfigurationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public VisitEmailConfigurationController(
            IVisitEmailConfigurationRepository visitEmailConfigurationRepository,
        IUnitOfWork uow, IMapper mapper,
            IGSCContext context
          )
        {
            _visitEmailConfigurationRepository = visitEmailConfigurationRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("{isDeleted:bool?}/{projectDesignVisitId}")]
        public IActionResult Get(bool isDeleted,int projectDesignVisitId)
        {
            var visitEmail = _visitEmailConfigurationRepository.GetVisitEmailConfigurationList(isDeleted,projectDesignVisitId);
            return Ok(visitEmail);
        }
    }
}
