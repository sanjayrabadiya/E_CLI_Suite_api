using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectWorkPlaceZoneController : BaseController
    {
        private readonly IProjectWorkPlaceZoneRepository _projectWorkPlaceZoneRepository;

        public ProjectWorkPlaceZoneController(
            IProjectWorkPlaceZoneRepository projectWorkPlaceZoneRepository
            )
        {

            _projectWorkPlaceZoneRepository = projectWorkPlaceZoneRepository;

        }


        [HttpGet]
        [Route("GetProjectWorkPlaceZoneDropDown/{id}")]
        public IActionResult GetProjectWorkPlaceZoneDropDown(int id)
        {
            return Ok(_projectWorkPlaceZoneRepository.GetProjectWorkPlaceZoneDropDown(id));
        }

        [HttpGet]
        [Route("GetProjectByZone/{projectid}")]
        public IActionResult GetProjectByZone(int projectid)
        {
            return Ok(_projectWorkPlaceZoneRepository.GetProjectByZone(projectid));
        }
    }
}