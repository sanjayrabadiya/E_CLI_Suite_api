using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.Project.StudyLevelFormSetup;
using GSC.Respository.SupplyManagement;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IProductVerificationDetailRepository _productVerificationDetail;
        private readonly IProductReceiptRepository _productReceiptRepository;
        private readonly IVerificationApprovalTemplateValueChildRepository _verificationApprovalTemplateValueChildRepository;
        private readonly IVerificationApprovalTemplateValueRepository _verificationApprovalTemplateValueRepository;
        private readonly IVerificationApprovalTemplateValueAuditRepository _verificationApprovalTemplateValueAuditRepository;
        private readonly IStudyLevelFormRepository _studyLevelFormRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public VerificationApprovalTemplateController(IVerificationApprovalTemplateRepository verificationApprovalTemplateRepository,
            IVerificationApprovalTemplateHistoryRepository verificationApprovalTemplateHistoryRepository,
            IProductVerificationDetailRepository productVerificationDetail,
            IProductReceiptRepository productReceiptRepository,
            IVerificationApprovalTemplateValueAuditRepository verificationApprovalTemplateValueAuditRepository,
            IUnitOfWork uow, IMapper mapper,
            IVerificationApprovalTemplateValueChildRepository verificationApprovalTemplateValueChildRepository,
            IVerificationApprovalTemplateValueRepository verificationApprovalTemplateValueRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IGSCContext context,
            IStudyLevelFormRepository studyLevelFormRepository)
        {
            _verificationApprovalTemplateRepository = verificationApprovalTemplateRepository;
            _verificationApprovalTemplateHistoryRepository = verificationApprovalTemplateHistoryRepository;
            _verificationApprovalTemplateValueAuditRepository = verificationApprovalTemplateValueAuditRepository;
            _productVerificationDetail = productVerificationDetail;
            _verificationApprovalTemplateValueChildRepository = verificationApprovalTemplateValueChildRepository;
            _verificationApprovalTemplateValueRepository = verificationApprovalTemplateValueRepository;
            _productReceiptRepository = productReceiptRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _studyLevelFormRepository = studyLevelFormRepository;
        }

        [HttpGet]
        [Route("GetTemplate/{id}")]
        public IActionResult GetTemplate([FromRoute] int id)
        {
            var TemplateId = _verificationApprovalTemplateRepository.All.Where(x => x.ProductVerificationDetailId == id).FirstOrDefault().StudyLevelFormId;
            var designTemplate = _studyLevelFormRepository.GetReportFormVariableForVerification(TemplateId);
            return Ok(_verificationApprovalTemplateRepository.GetVerificationApprovalTemplate(designTemplate, id));
        }

        [HttpPost]
        public IActionResult Post([FromBody] VerificationApprovalTemplateDto verificationApprovalTemplateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ConfigDetial = _studyLevelFormRepository.GetTemplateForVerification((int)verificationApprovalTemplateDto.ProjectId);
            if (ConfigDetial == null)
            {
                ModelState.AddModelError("Message", "Please set first verification template from study level form");
                return BadRequest(ModelState);
            }

            verificationApprovalTemplateDto.Id = 0;
            var verificationApprovalTemplate = _mapper.Map<VerificationApprovalTemplate>(verificationApprovalTemplateDto);
            verificationApprovalTemplate.StudyLevelFormId = ConfigDetial.Id;
            _verificationApprovalTemplateRepository.Add(verificationApprovalTemplate);
            _verificationApprovalTemplateRepository.AddVerificationApprovalTemplateHistory(verificationApprovalTemplate, verificationApprovalTemplateDto);
            // Set status Send for Approval
            var verificationDetail = _productVerificationDetail.Find(verificationApprovalTemplate.ProductVerificationDetailId);
            var receipt = _productReceiptRepository.Find(verificationDetail.ProductReceiptId);
            if (receipt != null)
            {
                receipt.Status = Helper.ProductVerificationStatus.SentForApproval;
                _productReceiptRepository.Update(receipt);
            }

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
            if (!verificationApprovalTemplateDto.IsApprove)
            {
                _verificationApprovalTemplateRepository.Addverificationhistory(verificationApprovalTemplateDto);
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

            _verificationApprovalTemplateRepository.getverificationApprovalTemplateHistory(verificationApprovalTemplateDto);

            var verificationApprovalTemplate = _mapper.Map<VerificationApprovalTemplateHistory>(verificationApprovalTemplateDto);
            // Set status Send for Approval
            var verificationTemplate = _verificationApprovalTemplateRepository.Find(verificationApprovalTemplateDto.VerificationApprovalTemplateId);
            var verificationDetail = _productVerificationDetail.Find(verificationTemplate.ProductVerificationDetailId);
            var receipt = _productReceiptRepository.Find(verificationDetail.ProductReceiptId);
            if (receipt != null)
            {
                if (verificationApprovalTemplateDto.IsSendBack)
                {
                    verificationApprovalTemplate.Status = Helper.ProductVerificationStatus.Rejected;
                    receipt.Status = Helper.ProductVerificationStatus.Rejected;
                }
                else
                {
                    verificationApprovalTemplate.Status = Helper.ProductVerificationStatus.SentForApproval;
                    receipt.Status = Helper.ProductVerificationStatus.SentForApproval;
                }
                _productReceiptRepository.Update(receipt);
            }

            _verificationApprovalTemplateHistoryRepository.Add(verificationApprovalTemplate);
            if (_uow.Save() <= 0) throw new Exception("Updating Verification Approval Template failed on save.");
            return Ok(verificationApprovalTemplate.Id);
        }



        /// Save Variable value
        [HttpPut]
        [TransactionRequired]
        [Route("SendByApprover")]
        public IActionResult SaveVariableValue([FromBody] VerificationApprovalTemplateDto verificationApprovalTemplateDto)
        {
            if (verificationApprovalTemplateDto.Id <= 0) return BadRequest();
            verificationApprovalTemplateDto.SecurityRoleId = Convert.ToInt32(_jwtTokenAccesser.GetHeader("RoleId"));
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var verification = _verificationApprovalTemplateRepository.Find(verificationApprovalTemplateDto.Id);
            verification.IsApprove = verificationApprovalTemplateDto.IsApprove;
            verification.ApproveOn = null;
            if (verificationApprovalTemplateDto.IsApprove)
                verification.ApproveOn = _jwtTokenAccesser.GetClientDate();

            var verificationApprovalTemplate = _mapper.Map<VerificationApprovalTemplate>(verification);
            _verificationApprovalTemplateRepository.AddHistory(verificationApprovalTemplateDto);
            _verificationApprovalTemplateRepository.Update(verificationApprovalTemplate);

            // Set status Send for Approval
            _verificationApprovalTemplateRepository.SendStatusApproval(verificationApprovalTemplateDto, verificationApprovalTemplate.ProductVerificationDetailId, verification);
            _verificationApprovalTemplateRepository.Addvalues(verificationApprovalTemplateDto);
            if (_uow.Save() <= 0) throw new Exception("Updating Variable failed on save.");
            return Ok(verificationApprovalTemplateDto.VerificationApprovalTemplateValueList[0].Id);
        }

    }
}
