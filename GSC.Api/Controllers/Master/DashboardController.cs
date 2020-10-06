using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class DashboardController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;

        public DashboardController(IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
        }

        [HttpGet]
        [Route("GetMyTaskList/{ProjectId}")]
        public IActionResult GetMyTaskList(int ProjectId)
        {
            List<DashboardDto> objdashboard = new List<DashboardDto>();
            var eTMFData = _projectArtificateDocumentApproverRepository.GetEtmfMyTaskList(ProjectId);
            var eConsentData = _econsentReviewDetailsRepository.GetEconsentMyTaskList(ProjectId);
            objdashboard.AddRange(eTMFData);
            objdashboard.AddRange(eConsentData);
            return Ok(objdashboard);
        }
    }
}