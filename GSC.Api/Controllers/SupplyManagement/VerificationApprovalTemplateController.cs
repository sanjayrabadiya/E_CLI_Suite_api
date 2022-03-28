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
        private readonly IVariableTemplateRepository _variableTemplateRepository;
        private readonly IProductVerificationDetailRepository _productVerificationDetail;
        private readonly IProductReceiptRepository _productReceiptRepository;
        private readonly IVerificationApprovalTemplateValueChildRepository _verificationApprovalTemplateValueChildRepository;
        private readonly IVerificationApprovalTemplateValueRepository _verificationApprovalTemplateValueRepository;
        private readonly IVerificationApprovalTemplateValueAuditRepository _verificationApprovalTemplateValueAuditRepository;
        private readonly ISupplyManagementConfigurationRepository _supplyManagementConfigurationRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IStudyLevelFormRepository _studyLevelFormRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public VerificationApprovalTemplateController(IVerificationApprovalTemplateRepository verificationApprovalTemplateRepository,
            IVerificationApprovalTemplateHistoryRepository verificationApprovalTemplateHistoryRepository,
            IProductVerificationDetailRepository productVerificationDetail,
            IProductReceiptRepository productReceiptRepository,
        IVerificationApprovalTemplateValueAuditRepository verificationApprovalTemplateValueAuditRepository,
        IUnitOfWork uow, IMapper mapper,
            IVariableTemplateRepository variableTemplateRepository,
            IVerificationApprovalTemplateValueChildRepository verificationApprovalTemplateValueChildRepository,
        IVerificationApprovalTemplateValueRepository verificationApprovalTemplateValueRepository,
        ISupplyManagementConfigurationRepository supplyManagementConfigurationRepository,
        IEmailSenderRespository emailSenderRespository,
        IUserRoleRepository userRoleRepository,
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
            _supplyManagementConfigurationRepository = supplyManagementConfigurationRepository;
            _userRoleRepository = userRoleRepository;
            _uow = uow;
            _mapper = mapper;
            _variableTemplateRepository = variableTemplateRepository;
            _emailSenderRespository = emailSenderRespository;
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

            verificationApprovalTemplate.VerificationApprovalTemplateHistory = new VerificationApprovalTemplateHistory();
            verificationApprovalTemplate.VerificationApprovalTemplateHistory.SendBy = _jwtTokenAccesser.UserId;
            verificationApprovalTemplate.VerificationApprovalTemplateHistory.SendOn = _jwtTokenAccesser.GetClientDate();
            verificationApprovalTemplate.VerificationApprovalTemplateHistory.Status = Helper.ProductVerificationStatus.SentForApproval;
            verificationApprovalTemplate.VerificationApprovalTemplateHistory.SecurityRoleId = _jwtTokenAccesser.RoleId;
            _verificationApprovalTemplateHistoryRepository.Add(verificationApprovalTemplate.VerificationApprovalTemplateHistory);

            // Set status Send for Approval
            var verificationDetail = _productVerificationDetail.Find(verificationApprovalTemplate.ProductVerificationDetailId);
            var receipt = _productReceiptRepository.Find(verificationDetail.ProductReceiptId);
            if (receipt != null)
            {
                receipt.Status = Helper.ProductVerificationStatus.SentForApproval;
                _productReceiptRepository.Update(receipt);
                //for email
                //var usersEmail = _userRoleRepository.GetUserEmailByRole(verificationApprovalTemplate.SecurityRoleId);
                //if (usersEmail != null)
                //    foreach (var item in usersEmail)
                //        _emailSenderRespository.SendApproveVerificationEmail(item);

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
            verificationApprovalTemplateDto.SecurityRoleId = _jwtTokenAccesser.RoleId;
            verificationApprovalTemplateDto.VerificationApprovalTemplateId = verification.VerificationApprovalTemplateId;

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

                    //for email
                    //var email = _context.Users.Find(verification.CreatedBy).Email;
                    //if (email != null)
                    //    _emailSenderRespository.RejectByApproverVerificationEmail(email);
                }
                else
                {
                    verificationApprovalTemplate.Status = Helper.ProductVerificationStatus.SentForApproval;
                    receipt.Status = Helper.ProductVerificationStatus.SentForApproval;

                    //for email
                    //var usersEmail = _userRoleRepository.GetUserEmailByRole(verificationTemplate.SecurityRoleId);
                    //if (usersEmail != null)
                    //    foreach (var item in usersEmail)
                    //        _emailSenderRespository.SendApproveVerificationEmail(item);
                }
                _productReceiptRepository.Update(receipt);
            }

            _verificationApprovalTemplateHistoryRepository.Add(verificationApprovalTemplate);
            if (_uow.Save() <= 0) throw new Exception("Updating Verification Approval Template failed on save.");
            return Ok(verificationApprovalTemplate.Id);
        }

        //[HttpPut]
        //[TransactionRequired]
        //[Route("SendByApprover")]
        //public IActionResult SendByApprover([FromBody] VerificationApprovalTemplateDto verificationApprovalTemplateDto)
        //{
        //    if (verificationApprovalTemplateDto.Id <= 0) return BadRequest();

        //    if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
        //    var verification = _verificationApprovalTemplateRepository.Find(verificationApprovalTemplateDto.Id);
        //    verification.IsApprove = verificationApprovalTemplateDto.IsApprove;
        //    verification.ApproveOn = null;
        //    if (verificationApprovalTemplateDto.IsApprove)
        //        verification.ApproveOn = _jwtTokenAccesser.GetClientDate();

        //    var verificationApprovalTemplate = _mapper.Map<VerificationApprovalTemplate>(verification);

        //    VerificationApprovalTemplateHistory history = new VerificationApprovalTemplateHistory();
        //    if (!verificationApprovalTemplateDto.IsApprove)
        //    {
        //        var detail = _verificationApprovalTemplateHistoryRepository.All.Where(x => x.VerificationApprovalTemplateId == verificationApprovalTemplateDto.Id).OrderByDescending(x => x.Id).LastOrDefault();
        //        history.VerificationApprovalTemplateId = detail.VerificationApprovalTemplateId;
        //        history.IsSendBack = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.IsSendBack;
        //        history.AuditReasonId = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.AuditReasonId;
        //        history.ReasonOth = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.ReasonOth;
        //        history.Status = Helper.ProductVerificationStatus.Rejected;
        //        history.SendBy = _jwtTokenAccesser.UserId;
        //        history.SendOn = _jwtTokenAccesser.GetClientDate();
        //        history.SecurityRoleId = _jwtTokenAccesser.RoleId;
        //        _verificationApprovalTemplateHistoryRepository.Add(history);
        //    }
        //    else
        //    {
        //        var detail = _verificationApprovalTemplateHistoryRepository.All.Where(x => x.VerificationApprovalTemplateId == verificationApprovalTemplateDto.Id).OrderByDescending(x => x.Id).LastOrDefault();
        //        history.VerificationApprovalTemplateId = detail.VerificationApprovalTemplateId;
        //        history.IsSendBack = false;
        //        history.AuditReasonId = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.AuditReasonId;
        //        history.ReasonOth = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.ReasonOth;
        //        history.Status = Helper.ProductVerificationStatus.Approved;
        //        history.SendBy = _jwtTokenAccesser.UserId;
        //        history.SendOn = _jwtTokenAccesser.GetClientDate();
        //        history.SecurityRoleId = _jwtTokenAccesser.RoleId;
        //        _verificationApprovalTemplateHistoryRepository.Add(history);
        //    }
        //    _verificationApprovalTemplateRepository.Update(verificationApprovalTemplate);

        //    //_uow.Save();
        //    //_context.DetachAllEntities();

        //    // Set status Send for Approval
        //    var verificationDetail = _productVerificationDetail.Find(verificationApprovalTemplate.ProductVerificationDetailId);
        //    var receipt = _productReceiptRepository.Find(verificationDetail.ProductReceiptId);
        //    if (receipt != null)
        //    {
        //        var email = _context.Users.Find(verification.CreatedBy).Email;
        //        if (verificationApprovalTemplateDto.IsApprove)
        //        {
        //            receipt.Status = Helper.ProductVerificationStatus.Approved;
        //            //for email
        //            //if (email != null)
        //            //    _emailSenderRespository.ApproveByApproverVerificationEmail(email);
        //        }
        //        else
        //        {
        //            receipt.Status = Helper.ProductVerificationStatus.Rejected;
        //            //for email
        //            //if (email != null)
        //            //    _emailSenderRespository.RejectByApproverVerificationEmail(email);
        //        }
        //        _productReceiptRepository.Update(receipt);
        //    }

        //    // Verification template value save
        //    if (verificationApprovalTemplateDto.VerificationApprovalTemplateValueList != null)
        //    {
        //        foreach (var item in verificationApprovalTemplateDto.VerificationApprovalTemplateValueList)
        //        {
        //            var value = _verificationApprovalTemplateValueRepository.GetValueForAudit(item);
        //            var verificationApproveTemplateValue = _mapper.Map<VerificationApprovalTemplateValue>(item);

        //            verificationApproveTemplateValue.VerificationApprovalTemplateId = verificationApprovalTemplateDto.Id;
        //            var Exists = _verificationApprovalTemplateValueRepository.All.Where(x => x.VerificationApprovalTemplateId == verificationApprovalTemplateDto.Id && x.StudyLevelFormVariableId == item.StudyLevelFormVariableId).FirstOrDefault();

        //            VerificationApprovalTemplateValueAudit audit = new VerificationApprovalTemplateValueAudit();

        //            if (Exists == null)
        //            {
        //                verificationApproveTemplateValue.Id = 0;
        //                _verificationApprovalTemplateValueRepository.Add(verificationApproveTemplateValue);

        //                var aduit = new VerificationApprovalTemplateValueAudit
        //                {
        //                    VerificationApprovalTemplateValue = verificationApproveTemplateValue,
        //                    Value = item.IsNa ? "N/A" : value,
        //                    OldValue = item.OldValue,
        //                };
        //                _verificationApprovalTemplateValueAuditRepository.Save(aduit);
        //                _verificationApprovalTemplateValueChildRepository.Save(verificationApproveTemplateValue);
        //            }
        //            else
        //            {
        //                var aduit = new VerificationApprovalTemplateValueAudit
        //                {
        //                    VerificationApprovalTemplateValueId = Exists.Id,
        //                    Value = item.IsNa ? "N/A" : value,
        //                    OldValue = item.OldValue,
        //                };
        //                _verificationApprovalTemplateValueAuditRepository.Save(aduit);
        //                if (item.IsDeleted)
        //                    _verificationApprovalTemplateValueRepository.DeleteChild(Exists.Id);

        //                _verificationApprovalTemplateValueChildRepository.Save(verificationApproveTemplateValue);
        //              //  if (verificationApproveTemplateValue.Id > 0)
        //                    _verificationApprovalTemplateValueRepository.Update(verificationApproveTemplateValue);
        //            }
        //        }
        //    }

        //    if (_uow.Save() <= 0) throw new Exception("Updating Verification Approval Template failed on save.");
        //    return Ok(verificationApprovalTemplate.Id);
        //}

        /// Save Variable value
        [HttpPut]
        [TransactionRequired]
        [Route("SendByApprover")]
        public IActionResult SaveVariableValue([FromBody] VerificationApprovalTemplateDto verificationApprovalTemplateDto)
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
                history.AuditReasonId = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.AuditReasonId;
                history.ReasonOth = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.ReasonOth;
                history.Status = Helper.ProductVerificationStatus.Rejected;
                history.SendBy = _jwtTokenAccesser.UserId;
                history.SendOn = _jwtTokenAccesser.GetClientDate();
                history.SecurityRoleId = _jwtTokenAccesser.RoleId;
                _verificationApprovalTemplateHistoryRepository.Add(history);
            }
            else
            {
                var detail = _verificationApprovalTemplateHistoryRepository.All.Where(x => x.VerificationApprovalTemplateId == verificationApprovalTemplateDto.Id).OrderByDescending(x => x.Id).LastOrDefault();
                history.VerificationApprovalTemplateId = detail.VerificationApprovalTemplateId;
                history.IsSendBack = false;
                history.AuditReasonId = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.AuditReasonId;
                history.ReasonOth = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.ReasonOth;
                history.Status = Helper.ProductVerificationStatus.Approved;
                history.SendBy = _jwtTokenAccesser.UserId;
                history.SendOn = _jwtTokenAccesser.GetClientDate();
                history.SecurityRoleId = _jwtTokenAccesser.RoleId;
                _verificationApprovalTemplateHistoryRepository.Add(history);
            }
            _verificationApprovalTemplateRepository.Update(verificationApprovalTemplate);

            // Set status Send for Approval
            var verificationDetail = _productVerificationDetail.Find(verificationApprovalTemplate.ProductVerificationDetailId);
            var receipt = _productReceiptRepository.Find(verificationDetail.ProductReceiptId);
            if (receipt != null)
            {
                var email = _context.Users.Find(verification.CreatedBy).Email;
                if (verificationApprovalTemplateDto.IsApprove)
                {
                    receipt.Status = Helper.ProductVerificationStatus.Approved;
                    //for email
                    //if (email != null)
                    //    _emailSenderRespository.ApproveByApproverVerificationEmail(email);
                }
                else
                {
                    receipt.Status = Helper.ProductVerificationStatus.Rejected;
                    //for email
                    //if (email != null)
                    //    _emailSenderRespository.RejectByApproverVerificationEmail(email);
                }
                _productReceiptRepository.Update(receipt);
            }

            if (verificationApprovalTemplateDto.VerificationApprovalTemplateValueList != null)
            {
                foreach (var item in verificationApprovalTemplateDto.VerificationApprovalTemplateValueList)
                {
                    item.VerificationApprovalTemplateId = verificationApprovalTemplateDto.Id;
                    var value = _verificationApprovalTemplateValueRepository.GetValueForAudit(item);
                    var verificationApprovalTemplateValue = _mapper.Map<VerificationApprovalTemplateValue>(item);

                    var Exists = _verificationApprovalTemplateValueRepository.All.Where(x => x.DeletedDate == null && x.VerificationApprovalTemplateId == verificationApprovalTemplateValue.VerificationApprovalTemplateId && x.StudyLevelFormVariableId == item.StudyLevelFormVariableId).FirstOrDefault();

                    if (Exists == null)
                    {
                        verificationApprovalTemplateValue.Id = 0;
                        _verificationApprovalTemplateValueRepository.Add(verificationApprovalTemplateValue);

                        var aduit = new VerificationApprovalTemplateValueAudit
                        {
                            VerificationApprovalTemplateValue = verificationApprovalTemplateValue,
                            Value = item.IsNa ? "N/A" : value,
                            OldValue = item.OldValue,
                        };
                        _verificationApprovalTemplateValueAuditRepository.Save(aduit);
                        _verificationApprovalTemplateValueChildRepository.Save(verificationApprovalTemplateValue);
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

                        _verificationApprovalTemplateValueChildRepository.Save(verificationApprovalTemplateValue);

                        verificationApprovalTemplateValue.Id = Exists.Id;
                        _verificationApprovalTemplateValueRepository.Update(verificationApprovalTemplateValue);
                    }
                }

                //var verificationApproveTemplate = _verificationApprovalTemplateRepository.Find(verificationApprovalTemplateDto.VerificationApprovalTemplateValueList[0].VerificationApprovalTemplateId);
                //_verificationApprovalTemplateRepository.Update(verificationApproveTemplate);

            }
            if (_uow.Save() <= 0) throw new Exception("Updating Variable failed on save.");
            return Ok(verificationApprovalTemplateDto.VerificationApprovalTemplateValueList[0].Id);
        }

    }
}
