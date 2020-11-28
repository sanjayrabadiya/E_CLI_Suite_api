using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Report;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.Reports;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class JobMonitoringController : BaseController
    {
        private readonly IJobMonitoringRepository _jobMonitoringRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IUnitOfWork _uow;

        public JobMonitoringController(IJobMonitoringRepository jobMonitoringRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, IProjectDesignRepository projectDesignRepository)
        {
            _jobMonitoringRepository = jobMonitoringRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectDesignRepository = projectDesignRepository;
        }

        
        [HttpGet]        
        public IActionResult Get()
        {
            var JobMonitoring = _jobMonitoringRepository.JobMonitoringList();
            return Ok(JobMonitoring);
        }              
          
    }
}