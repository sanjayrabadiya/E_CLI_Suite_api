using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.InformConcent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.DocIO.Utilities;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;
using Microsoft.AspNetCore.Cors;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentSectionReferenceController : BaseController
    {
        private readonly IEconsentSectionReferenceRepository _econsentSectionReferenceRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public EconsentSectionReferenceController(IEconsentSectionReferenceRepository econsentSectionReferenceRepository,
                                                IUnitOfWork uow,
                                                IMapper mapper,
                                                IUploadSettingRepository uploadSettingRepository)
        {
            _econsentSectionReferenceRepository = econsentSectionReferenceRepository;
            _uow = uow;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
        }


        [HttpGet]
        [Route("GetSectionReference/{isDeleted}/{documentId}")]
        public IActionResult GetSectionReference(bool isDeleted,int documentId)
        {
            var econsentSectionReferences = _econsentSectionReferenceRepository.FindByInclude(x => x.EconsentSetupId == documentId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).OrderByDescending(x => x.Id).ToList();
            var econsentSectionReferenceDto = _mapper.Map<IEnumerable<EconsentSectionReferenceDto>>(econsentSectionReferences).ToList();
            return Ok(econsentSectionReferenceDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var econsentSectionReference = _econsentSectionReferenceRepository.Find(id);
            var econsentSectionReferenceDto = _mapper.Map<EconsentSectionReferenceDto>(econsentSectionReference);
            return Ok(econsentSectionReferenceDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EconsentSectionReferenceDto econsentSectionReferenceDto)
        {
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            for (int i = 0; i < econsentSectionReferenceDto.FileModel.Count; i++)
            {
                Data.Dto.InformConcent.SaveFileDto obj = new Data.Dto.InformConcent.SaveFileDto();
                obj.Path = _uploadSettingRepository.GetDocumentPath();
                obj.FolderType = FolderType.InformConcent;
                obj.RootName = "EconsentSectionReference";
                obj.FileModel = econsentSectionReferenceDto.FileModel[i];

                econsentSectionReferenceDto.Id = 0;

                if (econsentSectionReferenceDto.FileModel[i]?.Base64?.Length > 0)
                {
                    econsentSectionReferenceDto.FilePath = DocumentService.SaveEconsentSectionReferenceFile(obj.FileModel, obj.Path, obj.FolderType, obj.RootName);
                }

                var econsentSectionReference = _mapper.Map<EconsentSectionReference>(econsentSectionReferenceDto);

                _econsentSectionReferenceRepository.Add(econsentSectionReference);
                string root = Path.Combine(obj.Path, obj.FolderType.ToString(), obj.RootName);
                if (_uow.Save() <= 0)
                {
                    if (Directory.Exists(root))
                    {
                        Directory.Delete(root, true);
                    }
                    throw new Exception($"Creating EConsent File failed on save.");
                }
            }
           
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] EconsentSectionReferenceDto econsentSectionReferenceDto)
        {
            if (econsentSectionReferenceDto.Id <= 0)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var document = _econsentSectionReferenceRepository.Find(econsentSectionReferenceDto.Id);

                if (econsentSectionReferenceDto.FileModel != null && econsentSectionReferenceDto.FileModel[0]?.Base64?.Length > 0)
                {
                    document.FilePath = DocumentService.SaveEconsentSectionReferenceFile(econsentSectionReferenceDto.FileModel[0], _uploadSettingRepository.GetDocumentPath(), FolderType.InformConcent, "EconsentSectionReference");
                }
            
            document.SectionNo = econsentSectionReferenceDto.SectionNo;
            document.ReferenceTitle = econsentSectionReferenceDto.ReferenceTitle;
            document.EconsentSetupId = econsentSectionReferenceDto.EconsentSetupId;
            _econsentSectionReferenceRepository.Update(document);

            if (_uow.Save() <= 0)
            {
                throw new Exception($"Updating Econsent file failed on save.");
            }
            return Ok(document.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _econsentSectionReferenceRepository.Find(id);

            if (record == null)
                return NotFound();

            _econsentSectionReferenceRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _econsentSectionReferenceRepository.Find(id);

            if (record == null)
                return NotFound();

            _econsentSectionReferenceRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetEconsentDocumentSectionDropDown/{documentId}")]
        public IActionResult GetEconsentDocumentSectionDropDown(int documentId)
        {
            return Ok(_econsentSectionReferenceRepository.GetEconsentDocumentSectionDropDown(documentId));
        }

        [HttpGet]
        [Route("GetEconsentDocumentSectionReference/{documentId}/{sectionNo}")]
        public IActionResult GetEconsentDocumentSectionReference(int documentId,int sectionNo)
        {
            var references = _econsentSectionReferenceRepository.FindBy(x => x.EconsentSetupId == documentId && x.SectionNo == sectionNo).ToList();
            return Ok(references);
        }

        [HttpPost]
        [Route("GetEconsentSectionReferenceDocument/{id}")]
        public IActionResult GetEconsentDocument(int id)
        {
            var upload = _uploadSettingRepository.GetDocumentPath();
            var Econsentsectiondocument = _econsentSectionReferenceRepository.Find(id);
            var FullPath = System.IO.Path.Combine(upload, Econsentsectiondocument.FilePath);
            string path = FullPath;
            if (!System.IO.File.Exists(path))
                return null;
            Stream stream = System.IO.File.OpenRead(path);
            string extension = System.IO.Path.GetExtension(path);
            string type = "";
            EconsentSectionReferenceDocumentType econsentSectionReferenceDocument = new EconsentSectionReferenceDocumentType();
           if (extension == ".docx" || extension == ".doc")
            {
                string sfdtText = "";
                EJ2WordDocument wdocument = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
                sfdtText = Newtonsoft.Json.JsonConvert.SerializeObject(wdocument);
                wdocument.Dispose();
                string json = sfdtText;
                stream.Close();
                type = "doc";
                econsentSectionReferenceDocument.type = type;
                econsentSectionReferenceDocument.data = json;
                return Ok(econsentSectionReferenceDocument);
            } else if (extension == ".pdf")
            {
                var pdfupload = _uploadSettingRepository.GetWebDocumentUrl();
                var pdfFullPath = System.IO.Path.Combine(pdfupload, Econsentsectiondocument.FilePath);
                type = "pdf";
                econsentSectionReferenceDocument.type = type;
                econsentSectionReferenceDocument.data = pdfFullPath;
                return Ok(econsentSectionReferenceDocument);
            } else
            {
                byte[] bytesimage;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    bytesimage = memoryStream.ToArray();
                }
                string base64 = Convert.ToBase64String(bytesimage);
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp" || extension == ".gif")
                {
                    type = "img";
                } else
                {
                    type = "vid";
                }
                econsentSectionReferenceDocument.type = type;
                econsentSectionReferenceDocument.data = base64;
                return Ok(econsentSectionReferenceDocument);
            }
            
        }
    }
}
