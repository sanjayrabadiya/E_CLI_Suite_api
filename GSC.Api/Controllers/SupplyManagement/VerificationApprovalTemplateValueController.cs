using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
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
    public class VerificationApprovalTemplateValueController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IVerificationApprovalTemplateRepository _verificationApprovalTemplateRepository;
        private readonly IVerificationApprovalTemplateValueRepository _verificationApprovalTemplateValueRepository;
        private readonly IUnitOfWork _uow;
        private readonly IVerificationApprovalTemplateValueAuditRepository _verificationApprovalTemplateValueAuditRepository;
        private readonly IVerificationApprovalTemplateValueChildRepository _verificationApprovalTemplateValueChildRepository;
        public VerificationApprovalTemplateValueController(IVerificationApprovalTemplateValueRepository verificationApprovalTemplateValueRepository,
            IVerificationApprovalTemplateRepository verificationApprovalTemplateRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IVerificationApprovalTemplateValueAuditRepository verificationApprovalTemplateValueAuditRepository,
            IVerificationApprovalTemplateValueChildRepository verificationApprovalTemplateValueChildRepository)
        {
            _verificationApprovalTemplateRepository = verificationApprovalTemplateRepository;
            _verificationApprovalTemplateValueRepository = verificationApprovalTemplateValueRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _verificationApprovalTemplateValueAuditRepository = verificationApprovalTemplateValueAuditRepository;
            _verificationApprovalTemplateValueChildRepository = verificationApprovalTemplateValueChildRepository;
        }


        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] VerificationApprovalTemplateValueDto verificationApprovalTemplateValueDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //var value = _verificationApprovalTemplateValueRepository.GetValueForAudit(verificationApprovalTemplateValueDto);

            //var screeningTemplateValue = _mapper.Map<VerificationApprovalTemplateValue>(verificationApprovalTemplateValueDto);
            //screeningTemplateValue.Id = 0;

            //_screeningTemplateValueRepository.Add(screeningTemplateValue);

            //var aduit = new ScreeningTemplateValueAudit
            //{
            //    ScreeningTemplateValue = screeningTemplateValue,
            //    Value = verificationApprovalTemplateValueDto.IsNa ? "N/A" : value,
            //    OldValue = verificationApprovalTemplateValueDto.OldValue,
            //};
            //_screeningTemplateValueAuditRepository.Save(aduit);

            //_screeningTemplateValueChildRepository.Save(screeningTemplateValue);
          
            _uow.Save();

            return Ok();
        }
    }
}
