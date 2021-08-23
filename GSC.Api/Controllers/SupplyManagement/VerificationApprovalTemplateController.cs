using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Respository.Master;
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
    public class VerificationApprovalTemplateController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IVerificationApprovalTemplateRepository _verificationApprovalTemplateRepository;
        private readonly IVerificationApprovalTemplateHistoryRepository _verificationApprovalTemplateHistoryRepository;
        private readonly IVariableTemplateRepository _variableTemplateRepository;
        private readonly IProductVerificationDetailRepository _productVerificationDetail;
        private readonly IVerificationApprovalTemplateValueChildRepository _verificationApprovalTemplateValueChildRepository;
        private readonly IVerificationApprovalTemplateValueRepository _verificationApprovalTemplateValueRepository;
        private readonly IVerificationApprovalTemplateValueAuditRepository _verificationApprovalTemplateValueAuditRepository;
        private readonly IUnitOfWork _uow;

        public VerificationApprovalTemplateController(IVerificationApprovalTemplateRepository verificationApprovalTemplateRepository,
            IVerificationApprovalTemplateHistoryRepository verificationApprovalTemplateHistoryRepository,
            IProductVerificationDetailRepository productVerificationDetail,
            IVerificationApprovalTemplateValueAuditRepository verificationApprovalTemplateValueAuditRepository,
        IUnitOfWork uow, IMapper mapper,
            IVariableTemplateRepository variableTemplateRepository,
            IVerificationApprovalTemplateValueChildRepository verificationApprovalTemplateValueChildRepository,
        IVerificationApprovalTemplateValueRepository verificationApprovalTemplateValueRepository,
        IJwtTokenAccesser jwtTokenAccesser)
        {
            _verificationApprovalTemplateRepository = verificationApprovalTemplateRepository;
            _verificationApprovalTemplateHistoryRepository = verificationApprovalTemplateHistoryRepository;
            _verificationApprovalTemplateValueAuditRepository = verificationApprovalTemplateValueAuditRepository;
            _productVerificationDetail = productVerificationDetail;
            _verificationApprovalTemplateValueChildRepository = verificationApprovalTemplateValueChildRepository;
            _verificationApprovalTemplateValueRepository = verificationApprovalTemplateValueRepository;
            _uow = uow;
            _mapper = mapper;
            _variableTemplateRepository = variableTemplateRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet]
        [Route("GetTemplate/{id}")]
        public IActionResult GetTemplate([FromRoute] int id)
        {
            //var verificationApprovalTemplateId = _productVerificationDetail.Find(id).Id;
            var designTemplate = _variableTemplateRepository.GetVerificationApprovalTemplate(100);
            return Ok(_verificationApprovalTemplateRepository.GetVerificationApprovalTemplate(designTemplate, id));
        }

        [HttpPost]
        public IActionResult Post([FromBody] VerificationApprovalTemplateDto verificationApprovalTemplateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            verificationApprovalTemplateDto.Id = 0;
            var verificationApprovalTemplate = _mapper.Map<VerificationApprovalTemplate>(verificationApprovalTemplateDto);
            verificationApprovalTemplate.VariableTemplateId = 100;
            _verificationApprovalTemplateRepository.Add(verificationApprovalTemplate);

            verificationApprovalTemplate.VerificationApprovalTemplateHistory = new VerificationApprovalTemplateHistory();
            verificationApprovalTemplate.VerificationApprovalTemplateHistory.SendBy = _jwtTokenAccesser.UserId;
            verificationApprovalTemplate.VerificationApprovalTemplateHistory.SendOn = _jwtTokenAccesser.GetClientDate();
            _verificationApprovalTemplateHistoryRepository.Add(verificationApprovalTemplate.VerificationApprovalTemplateHistory);

            if (_uow.Save() <= 0) throw new Exception("Creating Verification Approval Template failed on save.");
            return Ok(verificationApprovalTemplate.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VerificationApprovalTemplateDto verificationApprovalTemplateDto)
        {
            if (verificationApprovalTemplateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var verification = _verificationApprovalTemplateRepository.Find(verificationApprovalTemplateDto.Id);
            verification.IsApprove = verificationApprovalTemplateDto.IsApprove;
            verification.ApproveOn = null;
            if (verificationApprovalTemplateDto.IsApprove)
                verification.ApproveOn = _jwtTokenAccesser.GetClientDate();

            var verificationApprovalTemplate = _mapper.Map<VerificationApprovalTemplate>(verification);

            VerificationApprovalTemplateHistory history = new VerificationApprovalTemplateHistory();
            if (!verificationApprovalTemplateDto.IsApprove)
            {
                var detail = _verificationApprovalTemplateHistoryRepository.All.Where(x => x.VerificationApprovalTemplateId == verificationApprovalTemplateDto.Id).OrderByDescending(x => x.Id).LastOrDefault();
                history.VerificationApprovalTemplateId = detail.VerificationApprovalTemplateId;
                history.IsSendBack = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.IsSendBack;
                history.SendBy = _jwtTokenAccesser.UserId;
                history.SendOn = _jwtTokenAccesser.GetClientDate();
                _verificationApprovalTemplateHistoryRepository.Add(history);
            }
            _verificationApprovalTemplateRepository.Update(verificationApprovalTemplate);

            if (_uow.Save() <= 0) throw new Exception("Updating Verification Approval Template failed on save.");
            return Ok(verificationApprovalTemplate.Id);
        }

        [HttpPut]
        [Route("SendBackApproveUpdate")]
        public IActionResult SendBackApproveUpdate([FromBody] VerificationApprovalTemplateHistoryDto verificationApprovalTemplateDto)
        {
            if (verificationApprovalTemplateDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var verification = _verificationApprovalTemplateHistoryRepository.Find(verificationApprovalTemplateDto.Id);
            verificationApprovalTemplateDto.Id = 0;
            verificationApprovalTemplateDto.IsSendBack = false;
            verificationApprovalTemplateDto.SendBy = _jwtTokenAccesser.UserId;
            verificationApprovalTemplateDto.SendOn = _jwtTokenAccesser.GetClientDate();
            verificationApprovalTemplateDto.VerificationApprovalTemplateId = verification.VerificationApprovalTemplateId;

            var verificationApprovalTemplate = _mapper.Map<VerificationApprovalTemplateHistory>(verificationApprovalTemplateDto);
            _verificationApprovalTemplateHistoryRepository.Add(verificationApprovalTemplate);
            if (_uow.Save() <= 0) throw new Exception("Updating Verification Approval Template failed on save.");
            return Ok(verificationApprovalTemplate.Id);
        }

        [HttpPut]
        [TransactionRequired]
        [Route("SendByApprover")]
        public IActionResult SendByApprover([FromBody] VerificationApprovalTemplateDto verificationApprovalTemplateDto)
        {
            if (verificationApprovalTemplateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var verification = _verificationApprovalTemplateRepository.Find(verificationApprovalTemplateDto.Id);
            verification.IsApprove = verificationApprovalTemplateDto.IsApprove;
            verification.ApproveOn = null;
            if (verificationApprovalTemplateDto.IsApprove)
                verification.ApproveOn = _jwtTokenAccesser.GetClientDate();

            var verificationApprovalTemplate = _mapper.Map<VerificationApprovalTemplate>(verification);

            VerificationApprovalTemplateHistory history = new VerificationApprovalTemplateHistory();
            if (!verificationApprovalTemplateDto.IsApprove)
            {
                var detail = _verificationApprovalTemplateHistoryRepository.All.Where(x => x.VerificationApprovalTemplateId == verificationApprovalTemplateDto.Id).OrderByDescending(x => x.Id).LastOrDefault();
                history.VerificationApprovalTemplateId = detail.VerificationApprovalTemplateId;
                history.IsSendBack = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.IsSendBack;
                history.SendBy = _jwtTokenAccesser.UserId;
                history.SendOn = _jwtTokenAccesser.GetClientDate();
                _verificationApprovalTemplateHistoryRepository.Add(history);
            }
            _verificationApprovalTemplateRepository.Update(verificationApprovalTemplate);


            // Verification template value save

            if (verificationApprovalTemplateDto.VerificationApprovalTemplateValueList != null)
            {
                foreach (var item in verificationApprovalTemplateDto.VerificationApprovalTemplateValueList)
                {
                    var value = _verificationApprovalTemplateValueRepository.GetValueForAudit(item);
                    var verificationApproveTemplateValue = _mapper.Map<VerificationApprovalTemplateValue>(item);

                    verificationApproveTemplateValue.VerificationApprovalTemplateId = verificationApprovalTemplateDto.Id;
                    var Exists = _verificationApprovalTemplateValueRepository.All.Where(x => x.VerificationApprovalTemplateId == verificationApprovalTemplateDto.Id && x.VariableId == item.VariableId).FirstOrDefault();

                    VerificationApprovalTemplateValueAudit audit = new VerificationApprovalTemplateValueAudit();

                    if (Exists == null)
                    {
                        verificationApproveTemplateValue.Id = 0;
                        _verificationApprovalTemplateValueRepository.Add(verificationApproveTemplateValue);

                        var aduit = new VerificationApprovalTemplateValueAudit
                        {
                            VerificationApprovalTemplateValue = verificationApproveTemplateValue,
                            Value = item.IsNa ? "N/A" : value,
                            OldValue = item.OldValue,
                        };
                        _verificationApprovalTemplateValueAuditRepository.Save(aduit);
                        _verificationApprovalTemplateValueChildRepository.Save(verificationApproveTemplateValue);
                    }
                    else
                    {
                        var aduit = new VerificationApprovalTemplateValueAudit
                        {
                            VerificationApprovalTemplateValueId = Exists.Id,
                            Value = item.IsNa ? "N/A" : value,
                            OldValue = item.OldValue,
                        };
                        _verificationApprovalTemplateValueAuditRepository.Save(aduit);
                        if (item.IsDeleted)
                            _verificationApprovalTemplateValueRepository.DeleteChild(Exists.Id);

                        _verificationApprovalTemplateValueChildRepository.Save(verificationApproveTemplateValue);
                        _verificationApprovalTemplateValueRepository.Update(verificationApproveTemplateValue);
                    }
                }
            }
            if (_uow.Save() <= 0) throw new Exception("Updating Verification Approval Template failed on save.");
            return Ok(verificationApprovalTemplate.Id);
        }

    }
}
