using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;
using GSC.Shared.JWTAuth;
using GSC.Respository.CTMS;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsMonitoringReportReviewController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly ICtmsMonitoringReportReviewRepository _ctmsMonitoringReportReviewRepository;
        private readonly ICtmsMonitoringReportRepository _ctmsMonitoringReportRepository;
        private readonly ICtmsMonitoringRepository _ctmsMonitoringRepository;

        public CtmsMonitoringReportReviewController(IUnitOfWork uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            ICtmsMonitoringReportReviewRepository ctmsMonitoringReportReviewRepository,
            ICtmsMonitoringReportRepository ctmsMonitoringReportRepository,
            ICtmsMonitoringRepository ctmsMonitoringRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _ctmsMonitoringReportReviewRepository = ctmsMonitoringReportReviewRepository;
            _ctmsMonitoringReportRepository = ctmsMonitoringReportRepository;
            _ctmsMonitoringRepository = ctmsMonitoringRepository;
        }

        /// Get user for send for review
        /// Created By Swati
        [HttpGet]
        [Route("UserRoles/{Id}/{ProjectId}")]
        public IActionResult UserRoles(int Id, int ProjectId)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_ctmsMonitoringReportReviewRepository.UserRoles(Id, ProjectId));
        }

        //Save user for send for review
        //Created By Swati
        [HttpPost]
        [Route("SaveTemplateReview")]
        public IActionResult SaveTemplateReview([FromBody] List<CtmsMonitoringReportReviewDto> ctmsMonitoringReportReviewDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var review = _ctmsMonitoringReportReviewRepository.FindByInclude(x => x.CtmsMonitoringReportId == ctmsMonitoringReportReviewDto[0].CtmsMonitoringReportId && x.DeletedDate == null && x.IsApproved == false, x => x.User).FirstOrDefault();
            if (review != null)
            {
                ModelState.AddModelError("Message", "Form is already sent for review to " + review.User.UserName);
                return BadRequest(ModelState);
            }
            _ctmsMonitoringReportReviewRepository.SaveTemplateReview(ctmsMonitoringReportReviewDto);

            var manageMonitoringReport = _ctmsMonitoringReportRepository.Find(ctmsMonitoringReportReviewDto[0].CtmsMonitoringReportId);
            manageMonitoringReport.ReportStatus = MonitoringReportStatus.SendForReview;
            _ctmsMonitoringReportRepository.Update(manageMonitoringReport);

            if (_uow.Save() <= 0) throw new Exception("Updating status failed on save.");
            return Ok();
        }

        //Save approver for template
        //Created By Swati
        [HttpPut]
        [Route("ApproveTemplate/{id}")]
        public IActionResult ApproveTemplate(int id)
        {
            var ctmsMonitoringReportReviewDto = _ctmsMonitoringReportReviewRepository.FindByInclude(x => x.CtmsMonitoringReportId == id
            && x.UserId == _jwtTokenAccesser.UserId && x.ApproveDate == null && x.DeletedDate == null).FirstOrDefault();

            ctmsMonitoringReportReviewDto.IsApproved = true;
            ctmsMonitoringReportReviewDto.ApproveDate = _jwtTokenAccesser.GetClientDate();
            var ctmsMonitoringReportReview = _mapper.Map<CtmsMonitoringReportReview>(ctmsMonitoringReportReviewDto);
            _ctmsMonitoringReportReviewRepository.Update(ctmsMonitoringReportReview);

            var ctmsMonitoringReport = _ctmsMonitoringReportRepository.Find(ctmsMonitoringReportReviewDto.CtmsMonitoringReportId);
            ctmsMonitoringReport.ReportStatus = MonitoringReportStatus.FormApproved;
            _ctmsMonitoringReportRepository.Update(ctmsMonitoringReport);

            var ctmsMonitoring = _ctmsMonitoringRepository.Find(ctmsMonitoringReport.CtmsMonitoringId);
            ctmsMonitoring.ActualEndDate = _jwtTokenAccesser.GetClientDate();
            _ctmsMonitoringRepository.Update(ctmsMonitoring);

            if (_uow.Save() <= 0) throw new Exception("Updating Approve failed on save.");
            _ctmsMonitoringReportReviewRepository.SendMailForApproved(ctmsMonitoringReportReview);

            return Ok();
        }

        //Delete review
        //Created By Swati
        [HttpPost]
        [Route("DeleteTemplateReview")]
        public IActionResult DeleteTemplateReview([FromBody] List<int> Data)
        {
            foreach (var item in Data)
            {
                var record = _ctmsMonitoringReportReviewRepository.Find(item);

                if (record == null)
                    return NotFound();

                _ctmsMonitoringReportReviewRepository.Delete(record);
            }

            _uow.Save();
            return Ok();
        }

        /// Get user review history By CtmsMonitoringReportId
        /// Created By Swati
        [Route("GetCtmsMonitoringReportReviewHistory/{Id}")]
        [HttpGet]
        public IActionResult GetCtmsMonitoringReportReviewHistory(int Id)
        {
            var History = _ctmsMonitoringReportReviewRepository.GetCtmsMonitoringReportReviewHistory(Id);
            return Ok(History);
        }

        [Route("GetCtmsMonitoringReportReview/{Id}")]
        [HttpGet]
        public IActionResult GetCtmsMonitoringReportReview(int Id)
        {
            var History = _ctmsMonitoringReportReviewRepository.GetCtmsMonitoringReportReview(Id);
            return Ok(History);
        }

        [Route("isAnyReportReviewer/{Id}")]
        [HttpGet]
        public IActionResult isAnyReportReviewer(int Id)
        {
            var result = _ctmsMonitoringReportReviewRepository.isAnyReportReviewer(Id);
            return Ok(result);
        }

        [Route("GetReviewSendToAnyone/{CtmsMonitoringReportId}")]
        [HttpGet]
        public IActionResult GetReviewSendToAnyone(int CtmsMonitoringReportId)
        {
            var result = _ctmsMonitoringReportReviewRepository.GetReviewSendToAnyone(CtmsMonitoringReportId);
            return Ok(result);
        }
    }
}