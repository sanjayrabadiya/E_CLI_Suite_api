
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;
using GSC.Shared.JWTAuth;
using GSC.Respository.EditCheckImpact;

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
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IScreeningTemplateValueAuditRepository _screeningTemplateValueAuditRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IScreeningTemplateValueChildRepository _screeningTemplateValueChildRepository;
        private readonly IImpactService _impactService;
        public ScreeningTemplateValueController(IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IUploadSettingRepository uploadSettingRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueAuditRepository screeningTemplateValueAuditRepository,
            IScreeningTemplateValueChildRepository screeningTemplateValueChildRepository,
            IScreeningVisitRepository screeningVisitRepository,
            IImpactService impactService)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningVisitRepository = screeningVisitRepository;
            _screeningTemplateValueAuditRepository = screeningTemplateValueAuditRepository;
            _screeningTemplateValueChildRepository = screeningTemplateValueChildRepository;
            _impactService = impactService;
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

            _screeningTemplateValueRepository.Add(screeningTemplateValue);

            var aduit = new ScreeningTemplateValueAudit
            {
                ScreeningTemplateValue = screeningTemplateValue,
                Value = screeningTemplateValueDto.IsNa ? "N/A" : value,
                OldValue = screeningTemplateValueDto.OldValue,
            };
            _screeningTemplateValueAuditRepository.Save(aduit);

            _screeningTemplateValueChildRepository.Save(screeningTemplateValue);

            ScreeningTemplateStatus(screeningTemplateValueDto, screeningTemplateValue.ScreeningTemplateId);

            _uow.Save();

            var result = _screeningTemplateRepository.ValidateVariableValue(screeningTemplateValue, screeningTemplateValueDto.EditCheckIds, screeningTemplateValueDto.CollectionSource);

            return Ok(result);
        }

        //[HttpPost]
        //[TransactionRequired]
        //public IActionResult MobilePost([FromBody] ScreeningTemplateValueMobileDto screeningTemplateValueMobileDto)
        //{
        //    if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

        //    var screeningTemplateValueDto = _mapper.Map<ScreeningTemplateValueDto>(screeningTemplateValueMobileDto);
        //    var value = _screeningTemplateValueRepository.GetValueForAudit(screeningTemplateValueDto);

        //    var screeningTemplateValue = _mapper.Map<ScreeningTemplateValue>(screeningTemplateValueDto);
        //    screeningTemplateValue.Id = 0;

        //    _screeningTemplateValueRepository.Add(screeningTemplateValue);

        //    var aduit = new ScreeningTemplateValueAudit
        //    {
        //        ScreeningTemplateValue = screeningTemplateValue,
        //        Value = screeningTemplateValueDto.IsNa ? "N/A" : value,
        //        OldValue = screeningTemplateValueDto.OldValue,
        //        TimeZone = screeningTemplateValueDto.TimeZone,
        //        UserId = _jwtTokenAccesser.UserId,
        //        UserRoleId = _jwtTokenAccesser.RoleId,
        //        IpAddress = _jwtTokenAccesser.IpAddress
        //    };
        //    _screeningTemplateValueAuditRepository.Add(aduit);

        //    ScreeningTemplateStatus(screeningTemplateValueDto, screeningTemplateValue.ScreeningTemplateId);

        //    _uow.Save();

        //    var result = _screeningTemplateRepository.ValidateVariableValue(screeningTemplateValue, screeningTemplateValueDto.EditCheckIds, screeningTemplateValueDto.CollectionSource);

        //    return Ok(result);
        //}

        private void ScreeningTemplateStatus(ScreeningTemplateValueDto screeningTemplateValueDto, int screeningTemplateId)
        {
            if (screeningTemplateValueDto.ScreeningStatus == Helper.ScreeningTemplateStatus.Pending)
            {
                var screeningTemplate = _screeningTemplateRepository.Find(screeningTemplateId);
                if (screeningTemplate.Status > Helper.ScreeningTemplateStatus.InProcess) return;
                screeningTemplate.Status = Helper.ScreeningTemplateStatus.InProcess;
                screeningTemplate.IsDisable = false;
                _screeningTemplateRepository.Update(screeningTemplate);

                var screeningVisit = _screeningVisitRepository.Find(screeningTemplate.ScreeningVisitId);
                if (screeningVisit != null)
                {
                    screeningVisit.Status = ScreeningVisitStatus.InProgress;
                    _screeningVisitRepository.Update(screeningVisit);
                }
            }


        }


        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] ScreeningTemplateValueDto screeningTemplateValueDto)
        {
            if (screeningTemplateValueDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (screeningTemplateValueDto.IsDeleted && (
                screeningTemplateValueDto.CollectionSource == CollectionSources.Date ||
                screeningTemplateValueDto.CollectionSource == CollectionSources.DateTime ||
                screeningTemplateValueDto.CollectionSource == CollectionSources.Time))
            {
                if (!_screeningTemplateRepository.IsRepated(screeningTemplateValueDto.ScreeningTemplateId) && _impactService.CheckReferenceVariable(screeningTemplateValueDto.ProjectDesignVariableId))
                {
                    ModelState.AddModelError("Message", "Reference schedule date can't clear!");
                    return BadRequest(ModelState);
                }
            }


            var value = _screeningTemplateValueRepository.GetValueForAudit(screeningTemplateValueDto);

            var screeningTemplateValue = _mapper.Map<ScreeningTemplateValue>(screeningTemplateValueDto);

            var aduit = new ScreeningTemplateValueAudit
            {
                ScreeningTemplateValueId = screeningTemplateValue.Id,
                Value = value,
                Note = screeningTemplateValueDto.IsDeleted ? "Clear Data" : null,
                OldValue = screeningTemplateValueDto.OldValue
            };
            _screeningTemplateValueAuditRepository.Save(aduit);

            if (screeningTemplateValueDto.IsDeleted)
                _screeningTemplateValueRepository.DeleteChild(screeningTemplateValue.Id);


            _screeningTemplateValueChildRepository.Save(screeningTemplateValue);

            _screeningTemplateValueRepository.Update(screeningTemplateValue);

            ScreeningTemplateStatus(screeningTemplateValueDto, screeningTemplateValue.ScreeningTemplateId);

            _uow.Save();

            var result = _screeningTemplateRepository.ValidateVariableValue(screeningTemplateValue, screeningTemplateValueDto.EditCheckIds, screeningTemplateValueDto.CollectionSource);

            return Ok(result);
        }



        [HttpPut("UploadDocument")]
        public IActionResult UploadDocument([FromBody] ScreeningTemplateValueDto screeningTemplateValueDto)
        {
            if (screeningTemplateValueDto.Id <= 0) return BadRequest();

            var screeningTemplateValue = _screeningTemplateValueRepository.Find(screeningTemplateValueDto.Id);

            var documentPath = _uploadSettingRepository.GetDocumentPath();

            if (screeningTemplateValueDto.FileModel?.Base64?.Length > 0)
            {
                var documentCategory = "Template";
                screeningTemplateValue.DocPath = DocumentService.SaveDocument(screeningTemplateValueDto.FileModel,
                    documentPath, FolderType.Screening, documentCategory);
                screeningTemplateValue.MimeType = screeningTemplateValueDto.FileModel.Extension;
            }

            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            screeningTemplateValueDto.DocPath = screeningTemplateValue.DocPath;
            screeningTemplateValueDto.DocFullPath = documentUrl + screeningTemplateValue.DocPath;
            screeningTemplateValue.DocPath = screeningTemplateValue.DocPath;

            _screeningTemplateValueRepository.Update(screeningTemplateValue);
            _uow.Save();

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