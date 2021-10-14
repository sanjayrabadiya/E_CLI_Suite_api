using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Rights;
using GSC.Data.Entities.Master;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.Project.Rights;
using GSC.Respository.ProjectRight;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Centeral
{
    [Route("api/[controller]")]
    [ApiController]
    public class CenteralController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectModuleRightsRepository _projectModuleRightsRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;


        public CenteralController(IProjectRepository projectRepository, IUnitOfWork uow, IMapper mapper, IProjectModuleRightsRepository projectModuleRightsRepository, IEmailSenderRespository emailSenderRespository)
        {

            _projectRepository = projectRepository;
            _uow = uow;
            _mapper = mapper;
            _projectModuleRightsRepository = projectModuleRightsRepository;
            _emailSenderRespository = emailSenderRespository;
        }

        [HttpPost]
        public IActionResult Post([FromBody] StydyDetails details)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var project = _mapper.Map<Data.Entities.Master.Project>(details);
            _projectRepository.UpdateProject(project);
            return Ok(1);
        }

        [HttpPut]
        public IActionResult Put([FromBody] StydyDetails details)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var project = _mapper.Map<Data.Entities.Master.Project>(details);
            _projectRepository.UpdateProject(project);
            return Ok();
        }

        [HttpPost]
        [Route("ProjectModuleRight")]
        public IActionResult ProjectModuleRight([FromBody] StudyModuleDto details)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            _projectModuleRightsRepository.Save(details);
            return Ok();
        }
        [HttpGet]
        [Route("SendRegisterEMail")]
        public IActionResult SendRegisterEMail(string email,string password,string username)
        {
            //_emailSenderRespository()
            return Ok();
        }

    }
}
