﻿using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;

namespace GSC.Respository.InformConcent
{
    public class EconsentSectionReferenceRepository : GenericRespository<EconsentSectionReference>, IEconsentSectionReferenceRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEconsentSetupRepository _econsentSetupRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public EconsentSectionReferenceRepository(IGSCContext context, 
            IJwtTokenAccesser jwtTokenAccesser,
            IEconsentSetupRepository econsentSetupRepository,
            IUploadSettingRepository uploadSettingRepository,
            IUnitOfWork uow,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _econsentSetupRepository = econsentSetupRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public void AddEconsentSectionReference(EconsentSectionReferenceDto econsentSectionReferenceDto)
        {
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

                Add(econsentSectionReference);
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
        }

        public List<DropDownDto> GetEconsentDocumentSectionDropDown(int documentId)
        {
            var document = _econsentSetupRepository.Find(documentId);
            var upload = _uploadSettingRepository.GetDocumentPath();
            var FullPath = System.IO.Path.Combine(upload, document.DocumentPath);
            string path = FullPath;
            List<DropDownDto> sectionsHeaders = new List<DropDownDto>();
            if (System.IO.File.Exists(path))
            {
                Stream stream = System.IO.File.OpenRead(path);
                EJ2WordDocument doc = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
                stream.Close();
                doc.Dispose();
                JObject jsonstr = JObject.Parse(json);
                Root jsonobj = JsonConvert.DeserializeObject<Root>(jsonstr.ToString());
                int sectioncount = 1;
                foreach (var e1 in jsonobj.sections)
                {
                    foreach (var e2 in e1.blocks)
                    {
                        if (e2.paragraphFormat != null && e2.paragraphFormat.styleName == "Heading 1")
                        {
                            DropDownDto sectionsHeader = new DropDownDto();
                            sectionsHeader.Id = sectioncount;
                            
                            string headerstring = "";
                            foreach (var e3 in e2.inlines)
                            {
                                if (e3.text != null)
                                {
                                    headerstring = headerstring + e3.text;
                                }
                            }
                            sectionsHeader.Value = "(Section " + sectioncount.ToString() + ") " + headerstring;
                            sectionsHeaders.Add(sectionsHeader);
                            sectioncount++;
                        }
                    }
                }
            }
            return sectionsHeaders;
        }

        public EconsentSectionReferenceDocumentType GetEconsentSectionReferenceDocument(int id)
        {
            var upload = _uploadSettingRepository.GetDocumentPath();
            var Econsentsectiondocument = Find(id);
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
                return econsentSectionReferenceDocument;
            }
            else if (extension == ".pdf")
            {
                var pdfupload = _uploadSettingRepository.GetWebDocumentUrl();
                var pdfFullPath = System.IO.Path.Combine(pdfupload, Econsentsectiondocument.FilePath);
                type = "pdf";
                econsentSectionReferenceDocument.type = type;
                econsentSectionReferenceDocument.data = pdfFullPath;
                return econsentSectionReferenceDocument;
            }
            else
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
                }
                else
                {
                    type = "vid";
                }
                econsentSectionReferenceDocument.type = type;
                econsentSectionReferenceDocument.data = base64;
                return econsentSectionReferenceDocument;
            }

        }
    }
}
