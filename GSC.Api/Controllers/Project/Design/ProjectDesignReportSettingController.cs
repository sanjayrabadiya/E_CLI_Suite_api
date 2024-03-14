using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using GSC.Respository.Project.Design;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    public class ProjectDesignReportSettingController : BaseController
    {
        private readonly IProjectDesignReportSettingRepository _projectDesignReportSettingRepository;

        public ProjectDesignReportSettingController(IProjectDesignReportSettingRepository projectDesignReportSettingRepository)
        {
            _projectDesignReportSettingRepository = projectDesignReportSettingRepository;
        }

        [HttpGet]
        [Route("GetByProjectId/{projectDesignId}")]
        public IActionResult GetByProjectId(int projectDesignId)
        {
            if (projectDesignId <= 0)
            {
                return BadRequest();
            }
            var client = _projectDesignReportSettingRepository.FindBy(x => x.ProjectDesignId == projectDesignId && x.DeletedBy == null).FirstOrDefault();

            return Ok(client);
        }
    }
}