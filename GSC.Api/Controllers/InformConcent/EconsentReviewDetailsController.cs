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

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentReviewDetailsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IProjectRepository _projectRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public EconsentReviewDetailsController(IUnitOfWork uow,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IEmailSenderRespository emailSenderRespository,
            IProjectRepository projectRepository,
            IRandomizationRepository randomizationRepository,
            IJwtTokenAccesser jwtTokenAccesser, IGSCContext context)
        {
            _uow = uow;
            _context = context;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _emailSenderRespository = emailSenderRespository;
            _projectRepository = projectRepository;
            _randomizationRepository = randomizationRepository;
            _jwtTokenAccesser = jwtTokenAccesser;   
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

        [AllowAnonymous]
        [HttpPost]
        [Route("ImportSectionData/{id}/{sectionno}")]
        public string ImportSectionData(int id, int sectionno)
        {
            var jsonnew = _econsentReviewDetailsRepository.ImportSectionData(id, sectionno);
            return jsonnew;
        }

        [HttpPut]
        public IActionResult Put([FromBody] EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            return Ok(_econsentReviewDetailsRepository.UpdateDocument(econsentReviewDetailsDto));
        }

        //[HttpPost]
        //[Route("GetEconsentDocument")]
        //public IActionResult GetEconsentDocument([FromBody] EconsentReviewDetailsDto econsentreviewdetails)
        //{
        //    var json = _econsentReviewDetailsRepository.GetEconsentDocument(econsentreviewdetails);
        //    return Ok(json);
        //}

        [HttpPost]
        [Route("GetEconsentDocument")]
        public IActionResult GetEconsentDocument([FromBody] EconsentDocumetViwerDto econsentreviewdetails)
        {
            var json = _econsentReviewDetailsRepository.GetEconsentDocument(econsentreviewdetails);
            return Ok(json);
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
            return Ok(_econsentReviewDetailsRepository.ApproveRejectEconsentDocument(econsentReviewDetailsDto));
        }

        [HttpPost]
        [Route("downloadpdf/{id}")]
        public  IActionResult downloadpdf(int id)
        {
            return Ok(_econsentReviewDetailsRepository.downloadpdf(id));
        }

    }
}
