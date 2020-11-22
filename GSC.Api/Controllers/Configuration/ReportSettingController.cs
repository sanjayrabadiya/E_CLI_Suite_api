using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    public class ReportSettingController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IReportSettingRepository _reportSettingRepository;
        private readonly IUnitOfWork _uow;

        public ReportSettingController(IReportSettingRepository reportSettingRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _reportSettingRepository = reportSettingRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var reportSettig = _reportSettingRepository.All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && isDeleted ? x.DeletedDate != null : x.DeletedDate == null)
                .Select(x => new ReportSettingDto
                {
                    Id = x.Id,
                    IsClientLogo = x.IsClientLogo,
                    IsCompanyLogo = x.IsCompanyLogo,
                    IsInitial = x.IsInitial,
                    IsSponsorNumber = x.IsSponsorNumber,
                    IsScreenNumber = x.IsScreenNumber,
                    IsSubjectNumber = x.IsSubjectNumber,
                    LeftMargin = x.LeftMargin,
                    RightMargin = x.RightMargin,
                    TopMargin = x.TopMargin,
                    BottomMargin = x.BottomMargin,
                    IsDeleted = x.DeletedDate != null
                }).OrderByDescending(x => x.Id).ToList();
            return Ok(reportSettig);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var reportSetting = _reportSettingRepository.Find(id);
            var reportSettingDto = _mapper.Map<ReportSettingDto>(reportSetting);
            return Ok(reportSettingDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ReportSettingDto reportsettingDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            reportsettingDto.Id = 0;
            var reportSetting = _mapper.Map<ReportSetting>(reportsettingDto);
            var reportSettingForm = _reportSettingRepository.All
                .Where(x => x.CompanyId == reportSetting.CompanyId && x.DeletedBy == null).FirstOrDefault();
            if (reportSettingForm == null)
            {
                _reportSettingRepository.Add(reportSetting);
            }
            else
            {
                reportSettingForm.IsClientLogo = reportsettingDto.IsClientLogo;
                reportSettingForm.IsCompanyLogo = reportsettingDto.IsCompanyLogo;
                reportSettingForm.IsInitial = reportsettingDto.IsInitial;
                reportSettingForm.IsScreenNumber = reportsettingDto.IsScreenNumber;
                reportSettingForm.IsSponsorNumber = reportsettingDto.IsSponsorNumber;
                reportSettingForm.IsSubjectNumber = reportsettingDto.IsSubjectNumber;
                reportSettingForm.LeftMargin = reportsettingDto.LeftMargin;
                reportSettingForm.RightMargin = reportsettingDto.RightMargin;
                reportSettingForm.TopMargin = reportsettingDto.TopMargin;
                reportSettingForm.BottomMargin = reportsettingDto.BottomMargin;
                var reportSettingUpdate = _mapper.Map<ReportSetting>(reportSettingForm);
                _reportSettingRepository.Update(reportSettingUpdate);
            }

            if (_uow.Save() <= 0) throw new Exception("Creating Report Setting failed on save.");

            return Ok(reportSetting.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ReportSettingDto reportSettingDto)
        {
            if (reportSettingDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var reportSetting = _mapper.Map<ReportSetting>(reportSettingDto);

            _reportSettingRepository.Update(reportSetting);
            if (_uow.Save() <= 0) throw new Exception("Updating Report Setting failed on save.");
            return Ok(reportSetting.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _reportSettingRepository.Find(id);

            if (record == null)
                return NotFound();

            _reportSettingRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _reportSettingRepository.Find(id);

            if (record == null)
                return NotFound();
            _reportSettingRepository.Active(record);
            _uow.Save();
            return Ok();
        }
    }
}