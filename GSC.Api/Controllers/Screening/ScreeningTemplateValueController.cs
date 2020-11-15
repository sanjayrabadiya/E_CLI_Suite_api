using System;
using System.Collections.Generic;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningTemplateValueController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IUnitOfWork _uow;

        private readonly IUploadSettingRepository _uploadSettingRepository;

        public ScreeningTemplateValueController(IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IUploadSettingRepository uploadSettingRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var screeningTemplateValue = _screeningTemplateValueRepository.Find(id);

            var screeningTemplateValueDto = _mapper.Map<ScreeningTemplateValueDto>(screeningTemplateValue);
            return Ok(screeningTemplateValueDto);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] ScreeningTemplateValueDto screeningTemplateValueDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var value = _screeningTemplateValueRepository.GetValueForAudit(screeningTemplateValueDto);

            var screeningTemplateValue = _mapper.Map<ScreeningTemplateValue>(screeningTemplateValueDto);
            screeningTemplateValue.Id = 0;
            screeningTemplateValue.Audits = new List<ScreeningTemplateValueAudit>
            {
                new ScreeningTemplateValueAudit
                {
                    Value = screeningTemplateValueDto.IsNa ? "N/A" : value,
                    OldValue = screeningTemplateValueDto.OldValue,
                    TimeZone = screeningTemplateValueDto.TimeZone,
                    UserId = _jwtTokenAccesser.UserId,
                    UserRoleId = _jwtTokenAccesser.RoleId,
                    IpAddress = _jwtTokenAccesser.IpAddress
                }
            };

            _screeningTemplateValueRepository.Add(screeningTemplateValue);

            ScreeningTemplateStatus(screeningTemplateValueDto, screeningTemplateValue.ScreeningTemplateId);

            if (_uow.Save() <= 0) throw new Exception("Creating Screening Value failed on save.");

            var result = _screeningTemplateRepository.ValidateVariableValue(screeningTemplateValue, screeningTemplateValueDto.EditCheckIds, screeningTemplateValueDto.CollectionSource);

            return Ok(result);
        }

        private void ScreeningTemplateStatus(ScreeningTemplateValueDto screeningTemplateValueDto, int screeningTemplateId)
        {
            if (screeningTemplateValueDto.ScreeningStatus == Helper.ScreeningTemplateStatus.Pending)
            {
                var screeningTemplate = _screeningTemplateRepository.Find(screeningTemplateId);
                if (screeningTemplate.Status > Helper.ScreeningTemplateStatus.InProcess) return;
                screeningTemplate.Status = Helper.ScreeningTemplateStatus.InProcess;
                screeningTemplate.IsDisable = false;
                _screeningTemplateRepository.Update(screeningTemplate);
            }
        }


        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] ScreeningTemplateValueDto screeningTemplateValueDto)
        {
            if (screeningTemplateValueDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var value = _screeningTemplateValueRepository.GetValueForAudit(screeningTemplateValueDto);

            var screeningTemplateValue = _mapper.Map<ScreeningTemplateValue>(screeningTemplateValueDto);
            screeningTemplateValue.Audits = new List<ScreeningTemplateValueAudit>
            {
                new ScreeningTemplateValueAudit
                {
                    Value = value,
                    Note = screeningTemplateValueDto.IsDeleted ? "Clear Data" : null,
                    OldValue = screeningTemplateValueDto.OldValue,
                    TimeZone = screeningTemplateValueDto.TimeZone,
                    UserId = _jwtTokenAccesser.UserId,
                    UserRoleId = _jwtTokenAccesser.RoleId,
                    IpAddress = _jwtTokenAccesser.IpAddress
                }
            };

            if (screeningTemplateValueDto.IsDeleted)
                _screeningTemplateValueRepository.DeleteChild(screeningTemplateValue.Id);

            _screeningTemplateValueRepository.Update(screeningTemplateValue);

            ScreeningTemplateStatus(screeningTemplateValueDto, screeningTemplateValue.ScreeningTemplateId);

            if (_uow.Save() <= 0) throw new Exception("Updating Screening Value failed on save.");

            var result = _screeningTemplateRepository.ValidateVariableValue(screeningTemplateValue, screeningTemplateValueDto.EditCheckIds, screeningTemplateValueDto.CollectionSource);

            return Ok(result);
        }


        [HttpPut("UploadDocument")]
        public IActionResult UploadDocument([FromBody] ScreeningTemplateValueDto screeningTemplateValueDto)
        {
            if (screeningTemplateValueDto.Id <= 0) return BadRequest();

            var screeningTemplateValue = _screeningTemplateValueRepository.Find(screeningTemplateValueDto.Id);

            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();

            if (screeningTemplateValueDto.FileModel?.Base64?.Length > 0)
            {
                var documentCategory = "Template";
                screeningTemplateValue.DocPath = DocumentService.SaveDocument(screeningTemplateValueDto.FileModel,
                    documentUrl, FolderType.Screening, documentCategory);
                screeningTemplateValue.MimeType = screeningTemplateValueDto.FileModel.Extension;
            }

            if (_uow.Save() <= 0) throw new Exception("Uploading document failed on save.");

            screeningTemplateValueDto.DocPath = documentUrl + screeningTemplateValue.DocPath;

            return Ok(screeningTemplateValueDto);
        }

        [HttpGet("GetQueryStatusCount/{id}")]
        public IActionResult GetQueryStatusCount(int id)
        {
            return Ok(_screeningTemplateValueRepository.GetQueryStatusCount(id));
        }

        [HttpGet("GetQueryStatusBySubject/{id}")]
        public IActionResult GetQueryStatusBySubject(int id)
        {
            return Ok(_screeningTemplateValueRepository.GetQueryStatusBySubject(id));
        }
    }
}