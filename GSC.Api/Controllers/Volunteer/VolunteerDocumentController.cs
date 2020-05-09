using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Audit;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Volunteer;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Volunteer
{
    [Route("api/[controller]")]
    public class VolunteerDocumentController : BaseController
    {
        private readonly IAuditTrailRepository _auditTrailRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IVolunteerDocumentRepository _volunteerDocumentRepository;

        public VolunteerDocumentController(IVolunteerDocumentRepository volunteerDocumentRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IUploadSettingRepository uploadSettingRepository,
            IDocumentTypeRepository documentTypeRepository,
            IAuditTrailRepository auditTrailRepository)
        {
            _volunteerDocumentRepository = volunteerDocumentRepository;
            _uow = uow;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
            _documentTypeRepository = documentTypeRepository;
            _auditTrailRepository = auditTrailRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var volunteerDocument = _volunteerDocumentRepository
                .FindByInclude(t => t.VolunteerId == id && t.DeletedDate == null, t => t.DocumentType,
                    t => t.DocumentName).OrderByDescending(x => x.Id);
            volunteerDocument.ForEach(t => t.PathName = documentUrl + t.PathName);
            return Ok(volunteerDocument);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VolunteerDocumentDto volunteerDocumentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            volunteerDocumentDto.Id = 0;
            if (volunteerDocumentDto.FileModel?.Base64?.Length > 0)
            {
                var documentCategory = _documentTypeRepository.Find(volunteerDocumentDto.DocumentTypeId).TypeName;
                volunteerDocumentDto.PathName = DocumentService.SaveDocument(volunteerDocumentDto.FileModel,
                    _uploadSettingRepository.GetDocumentPath(), FolderType.Volunteer, documentCategory);
                volunteerDocumentDto.MimeType = volunteerDocumentDto.FileModel.Extension;
            }


            var volunteerDocument = _mapper.Map<VolunteerDocument>(volunteerDocumentDto);
            _volunteerDocumentRepository.Add(volunteerDocument);
            if (_uow.Save() <= 0) throw new Exception("Creating volunteer document failed on save.");

            _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerDocument, AuditAction.Inserted,
                volunteerDocument.Id, volunteerDocument.VolunteerId, volunteerDocumentDto.Changes);

            return Ok(volunteerDocument.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VolunteerDocumentDto volunteerDocumentDto)
        {
            if (volunteerDocumentDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //added by swati for if document send empty if they cant want to change docuemnt
            var document = _volunteerDocumentRepository.Find(volunteerDocumentDto.Id);
            document.DocumentTypeId = volunteerDocumentDto.DocumentTypeId;
            document.DocumentNameId = volunteerDocumentDto.DocumentNameId;
            document.Note = volunteerDocumentDto.Note;

            if (volunteerDocumentDto.FileModel?.Base64?.Length > 0)
            {
                var documentCategory = _documentTypeRepository.Find(volunteerDocumentDto.DocumentTypeId).TypeName;
                document.PathName = DocumentService.SaveDocument(volunteerDocumentDto.FileModel,
                    _uploadSettingRepository.GetDocumentPath(), FolderType.Volunteer, documentCategory);
                document.MimeType = volunteerDocumentDto.FileModel.Extension;
            }

            var volunteerDocument = _mapper.Map<VolunteerDocument>(document);

            _volunteerDocumentRepository.Update(volunteerDocument);
            if (_uow.Save() <= 0) throw new Exception("Updating volunteer document failed on save.");

            _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerDocument, AuditAction.Updated,
                volunteerDocument.Id, volunteerDocument.VolunteerId, volunteerDocumentDto.Changes);

            return Ok(volunteerDocument.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _volunteerDocumentRepository.Find(id);

            if (record == null)
                return NotFound();

            _volunteerDocumentRepository.Delete(record);
            _uow.Save();

            _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerDocument, AuditAction.Deleted,
                record.Id, record.VolunteerId, null);

            return Ok();
        }
    }
}