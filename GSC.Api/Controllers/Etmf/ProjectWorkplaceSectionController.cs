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
    public class ProjectWorkplaceSectionController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IProjectWorkplaceSectionRepository _projectWorkplaceSectionRepository;
      
        public ProjectWorkplaceSectionController(IProjectRepository projectRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IProjectWorkplaceSectionRepository projectWorkplaceSectionRepository
            )
        {
            _uow = uow;
            _mapper = mapper;
            _projectWorkplaceSectionRepository = projectWorkplaceSectionRepository;
        }


        [HttpGet]
        [Route("GetProjectWorkPlaceSectionDropDown/{zoneid}")]
        public IActionResult GetProjectWorkPlaceSectionDropDown(int zoneid)
        {
            return Ok(_projectWorkplaceSectionRepository.GetProjectWorkPlaceSectionDropDown(zoneid));
        }

    }
}