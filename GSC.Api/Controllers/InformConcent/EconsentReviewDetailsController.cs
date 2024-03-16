using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using GSC.Shared.JWTAuth;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Data.Entities.InformConcent;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentReviewDetailsController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEconsentReviewDetailsAuditRepository _econsentReviewDetailsAuditRepository;

        public EconsentReviewDetailsController(IUnitOfWork uow,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IRandomizationRepository randomizationRepository,
            IJwtTokenAccesser jwtTokenAccesser, IGSCContext context,
            IEconsentReviewDetailsAuditRepository econsentReviewDetailsAuditRepository)
        {
            _uow = uow;
            _context = context;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _randomizationRepository = randomizationRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _econsentReviewDetailsAuditRepository = econsentReviewDetailsAuditRepository;
        }

        [HttpGet]
        [Route("GetEconsentDocumentHeaders")]
        public IActionResult GetEconsentDocumentHeaders()
        {
            var sectionsHeaders = _econsentReviewDetailsRepository.GetEconsentDocumentHeaders();
            return Ok(sectionsHeaders);
        }

        [HttpGet]
        [Route("GetEconsentSectionHeaders/{id}")]
        public IActionResult GetEconsentSectionHeaders(int id)
        {
            var sectionsHeaders = _econsentReviewDetailsRepository.GetEconsentSectionHeaders(id);
            return Ok(sectionsHeaders);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var data = _econsentReviewDetailsRepository.Find(id);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetEconsentDocumentHeadersByDocumentId/{documentId}")]
        public IActionResult GetEconsentDocumentHeadersByDocumentId(int documentId)
        {
            var sectionsHeaders = _econsentReviewDetailsRepository.GetEconsentDocumentHeadersByDocumentId(documentId);
            return Ok(sectionsHeaders);
        }


        [HttpPost]
        [Route("ImportSectionData/{id}/{sectionno}")]
        public string ImportSectionData(int id, int sectionno)
        {
            var jsonnew = _econsentReviewDetailsRepository.ImportSectionData(id, sectionno);
            return jsonnew;
        }

        [HttpPost]
        [Route("ImportSectionDataHtml/{id}/{sectionno}")]
        public IActionResult ImportSectionDataHtml(int id, int sectionno)
        {
            var htmlFile = _econsentReviewDetailsRepository.ImportSectionDataHtml(id, sectionno);
            return Ok(htmlFile);
        }

        [HttpPut]
        public IActionResult Put([FromBody] EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            return Ok(_econsentReviewDetailsRepository.UpdateDocument(econsentReviewDetailsDto));
        }

        [HttpGet]
        [Route("GetEconsentDocument/{id}")]
        public IActionResult GetEconsentDocument(int id)
        {
            var document = _econsentReviewDetailsRepository.GetEconsentDocument(id);
            return document;
        }

        [HttpPut]
        [Route("ApprovePatient")]
        public IActionResult ApprovePatient([FromBody] EconsentDocumetViwerDto econsentreviewdetails)
        {
            int revieId = _econsentReviewDetailsRepository.ApproveWithDrawPatient(econsentreviewdetails, false);
            return Ok(revieId);
        }

        [HttpPut]
        [Route("WithDrawPatient")]
        [TransactionRequired]
        public IActionResult WithDrawPatient([FromBody] EconsentDocumetViwerDto econsentreviewdetails)
        {
            var roleName = _jwtTokenAccesser.RoleName;

            var randomization = _randomizationRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();

            if (roleName == "LAR")
            {
                randomization = _randomizationRepository.FindBy(x => x.LARUserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            }

            var reviewDetails = _context.EconsentReviewDetails.Where(x => x.RandomizationId == randomization.Id && x.DeletedDate == null
            && (roleName == "LAR" ? x.IsLAR == true : x.IsLAR == null || x.IsLAR == false)).ToList();

            foreach (var item in reviewDetails)
            {
                EconsentDocumetViwerDto econcentDetails = new EconsentDocumetViwerDto();
                econcentDetails.EconcentReviewDetailsId = item.Id;
                econcentDetails.PatientdigitalSignBase64 = econsentreviewdetails.PatientdigitalSignBase64;
                econcentDetails.FileExtension = econsentreviewdetails.FileExtension;
                _econsentReviewDetailsRepository.ApproveWithDrawPatient(econcentDetails, true);
            }
            _randomizationRepository.ChangeStatustoWithdrawal();
            //detail audit log
            EconsentReviewDetailsAudit audit = new EconsentReviewDetailsAudit();
            audit.EconsentReviewDetailsId = econsentreviewdetails.EconcentReviewDetailsId;
            audit.Activity = ICFAction.Withdraw;
            audit.PateientStatus = randomization.PatientStatusId;
            _econsentReviewDetailsAuditRepository.Add(audit);
            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("GetEconsentReviewDetailsForSubjectManagement/{patientid}")]
        public IActionResult GetEconsentReviewDetailsForSubjectManagement(int patientid)
        {
            return Ok(_econsentReviewDetailsRepository.GetEconsentReviewDetailsForSubjectManagement(patientid));
        }

        [HttpGet]
        [Route("GetEconsentReviewDetailsForPatientDashboard")]
        public IActionResult GetEconsentReviewDetailsForPatientDashboard()
        {
            return Ok(_econsentReviewDetailsRepository.GetEconsentReviewDetailsForPatientDashboard());
        }

        [HttpPut]
        [Route("ApproveRejectEconsentDocument")]
        public IActionResult ApproveRejectEconsentDocument([FromBody] EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            if (econsentReviewDetailsDto.Id <= 0) return BadRequest();

            var randomization = _randomizationRepository.All.FirstOrDefault(x => x.Id == econsentReviewDetailsDto.RandomizationId);
            if (randomization?.LARUserId != null)
            {
                var econsentReviewDetails = _econsentReviewDetailsRepository.All.Where(y => y.RandomizationId == econsentReviewDetailsDto.RandomizationId
                && y.EconsentSetupId == econsentReviewDetailsDto.EconsentSetupId).ToList();

                if (econsentReviewDetails.Exists(c => !c.IsReviewedByPatient))
                {
                    ModelState.AddModelError("Message", "Patient or LAR review doesn't complete.");
                    return BadRequest(ModelState);
                }
            }

            return Ok(_econsentReviewDetailsRepository.ApproveRejectEconsentDocument(econsentReviewDetailsDto));
        }

        [HttpPost]
        [Route("downloadpdf/{id}")]
        public IActionResult downloadpdf(int id)
        {
            return Ok(_econsentReviewDetailsRepository.downloadpdf(id));
        }
    }
}
