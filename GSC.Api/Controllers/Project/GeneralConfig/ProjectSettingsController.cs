using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.GeneralConfig;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Respository.Project.GeneralConfig;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Project.GeneralConfig
{
    [Route("api/[controller]")]
    public class ProjectSettingsController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectSettingsRepository _projectSettingsRepository;
        private readonly IUserAccessRepository _userAccessRepository;
        private readonly IGSCContext _context;
        public ProjectSettingsController(
            IUnitOfWork uow, IMapper mapper, IProjectSettingsRepository projectSettingsRepository, IUserAccessRepository userAccessRepository, IGSCContext context)
        {
            _uow = uow;
            _mapper = mapper;
            _projectSettingsRepository = projectSettingsRepository;
            _userAccessRepository = userAccessRepository;
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var projectSettings = _projectSettingsRepository.FindBy(x => x.ProjectId == id).FirstOrDefault();
            var projectSettingsDto = _mapper.Map<ProjectSettingsDto>(projectSettings);
            return Ok(projectSettingsDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ProjectSettingsDto projectSettingsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            projectSettingsDto.Id = 0;
            var projectSettings = _mapper.Map<ProjectSettings>(projectSettingsDto);

            _projectSettingsRepository.Add(projectSettings);
            if (_uow.Save() <= 0) throw new Exception("Creating ctms settings failed on save.");

            //Add by Mitul ->CTMS on Bydeful Add CTMS Access table
             _userAccessRepository.AddProjectRight(projectSettingsDto.ProjectId, projectSettingsDto.IsCtms);

            return Ok(projectSettings.Id);
        }
        [HttpPut]
        public IActionResult Put([FromBody] ProjectSettingsDto projectSettingsDto)
        {
            if (projectSettingsDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var projectSettings = _mapper.Map<ProjectSettings>(projectSettingsDto);

            _projectSettingsRepository.Update(projectSettings);

            if (_uow.Save() <= 0) throw new Exception("Update ctms settings failed on save.");

            //Add by Mitul ->CTMS on Bydeful Add CTMS Access table
             _userAccessRepository.AddProjectRight(projectSettingsDto.ProjectId, projectSettingsDto.IsCtms);
            return Ok(projectSettings.Id);
        }

        [HttpGet]
        [Route("GetParentProjectDropDownEicf")]
        public IActionResult GetParentProjectDropDownEicf()
        {
            return Ok(_projectSettingsRepository.GetParentProjectDropDownEicf());
        }

        [HttpGet]
        [Route("GetParentProjectDropDownScreening")]
        public IActionResult GetParentProjectDropDownScreening()
        {
            return Ok(_projectSettingsRepository.GetParentProjectDropDownScreening());
        }
        [HttpGet]
        [Route("GetProjectConfiguration/{id}")]
        public IActionResult GetProjectConfiguration(int id)
        {
            var projectdesign = _context.ProjectDesign.Where(s => s.Id == id).FirstOrDefault();
            if (projectdesign == null)
                return Ok(null);
            var projectSetting = _context.ProjectSettings.Where(s => s.ProjectId == projectdesign.ProjectId).FirstOrDefault();
            return Ok(projectSetting);
        }
    }
}
