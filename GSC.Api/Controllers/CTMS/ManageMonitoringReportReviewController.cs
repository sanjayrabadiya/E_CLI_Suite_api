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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class ManageMonitoringReportReviewController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IManageMonitoringReportReviewRepository _manageMonitoringReportReviewRepository;

        public ManageMonitoringReportReviewController(IUnitOfWork uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IManageMonitoringReportReviewRepository manageMonitoringReportReviewRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _manageMonitoringReportReviewRepository = manageMonitoringReportReviewRepository;
        }

        /// Get user for send for review
        /// Created By Swati
        [HttpGet]
        [Route("UserRoles/{Id}/{ProjectId}")]
        public IActionResult UserRoles(int Id, int ProjectId)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_manageMonitoringReportReviewRepository.UserRoles(Id, ProjectId));
        }

        //Save user for send for review
        //Created By Swati
        [HttpPost]
        [Route("SaveTemplateReview")]
        public IActionResult SaveTemplateReview([FromBody] List<ManageMonitoringReportReviewDto> manageMonitoringReportReviewDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _manageMonitoringReportReviewRepository.SaveTemplateReview(manageMonitoringReportReviewDto);

            return Ok();
        }

        //Save approver for template
        //Created By Swati
        [HttpPut]
        [Route("ApproveTemplate/{id}")]
        public IActionResult ApproveTemplate(int id)
        {
            var manageMonitoringReportReviewDto = _manageMonitoringReportReviewRepository.FindByInclude(x => x.ManageMonitoringReportId == id
            && x.UserId == _jwtTokenAccesser.UserId && x.ApproveDate == null && x.DeletedDate == null).FirstOrDefault();

            manageMonitoringReportReviewDto.IsApproved = true;
            manageMonitoringReportReviewDto.ApproveDate = _jwtTokenAccesser.GetClientDate();
            var manageMonitoringReportReview = _mapper.Map<ManageMonitoringReportReview>(manageMonitoringReportReviewDto);
            _manageMonitoringReportReviewRepository.Update(manageMonitoringReportReview);

            if (_uow.Save() <= 0) throw new Exception("Updating Approve failed on save.");
            _manageMonitoringReportReviewRepository.SendMailForApproved(manageMonitoringReportReview);

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
                var record = _manageMonitoringReportReviewRepository.Find(item);

                if (record == null)
                    return NotFound();

                _manageMonitoringReportReviewRepository.Delete(record);
            }

            _uow.Save();
            return Ok();
        }

        /// Get user review history By manageMonitoringReportId
        /// Created By Swati
        [Route("GetManageMonitoringReportReviewHistory/{Id}")]
        [HttpGet]
        public IActionResult GetManageMonitoringReportReviewHistory(int Id)
        {
            var History = _manageMonitoringReportReviewRepository.GetManageMonitoringReportReviewHistory(Id);
            return Ok(History);
        }

        [Route("GetManageMonitoringReportReview/{Id}")]
        [HttpGet]
        public IActionResult GetManageMonitoringReportReview(int Id)
        {
            var History = _manageMonitoringReportReviewRepository.GetManageMonitoringReportReview(Id);
            return Ok(History);
        }
    }
}