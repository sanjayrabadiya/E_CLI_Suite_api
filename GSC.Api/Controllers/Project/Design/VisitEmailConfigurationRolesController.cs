using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitEmailConfigurationRolesController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public VisitEmailConfigurationRolesController(
            IUnitOfWork uow, IMapper mapper,
            IGSCContext context
          )
        {
            _uow = uow;
            _mapper = mapper;
            _context = context;
        }
    }
}
