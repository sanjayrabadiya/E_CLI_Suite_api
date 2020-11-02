using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Centeral
{
    [Route("api/[controller]")]
    [ApiController]
    public class CenteralController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _uow;

        public CenteralController(IProjectRepository projectRepository,IUnitOfWork uow, IMapper mapper) {

            _projectRepository = projectRepository;
            _uow = uow;
            _mapper = mapper;         

        }

        [HttpPost]
        public IActionResult Post([FromBody] StydyDetails details)
        {  
            return Ok();
        }
    }
}
