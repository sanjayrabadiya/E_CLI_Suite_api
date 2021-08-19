using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationApprovalTemplateHistoryController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IVerificationApprovalTemplateHistoryRepository _verificationApprovalTemplateHistoryRepository;
        private readonly IUnitOfWork _uow;

        public VerificationApprovalTemplateHistoryController(IVerificationApprovalTemplateHistoryRepository verificationApprovalTemplateHistoryRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _verificationApprovalTemplateHistoryRepository = verificationApprovalTemplateHistoryRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
    }
}
