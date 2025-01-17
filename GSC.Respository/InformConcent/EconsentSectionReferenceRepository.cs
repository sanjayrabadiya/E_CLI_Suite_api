﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.DocIO.DLS;
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
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public EconsentSectionReferenceRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IEconsentSetupRepository econsentSetupRepository,
            IUploadSettingRepository uploadSettingRepository,
            IMapper mapper) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _econsentSetupRepository = econsentSetupRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _mapper = mapper;
        }

        public IList<EconsentSectionReferenceDto> GetSectionReferenceList(bool isDeleted, int documentId)
        {
            var sectionrefrence = All.Where(x => x.EconsentSetupId == documentId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                 ProjectTo<EconsentSectionReferenceDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return sectionrefrence;
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

                doc.OptimizeSfdt = false;
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
                doc.Dispose();

                //string json = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
                stream.Close();
                //doc.Dispose();
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
                            StringBuilder headerStringBuilder = new StringBuilder();
                            foreach (var e3 in e2.inlines.Select(s => s.text))
                            {
                                if (!string.IsNullOrEmpty(e3))
                                {
                                    headerStringBuilder.Append(e3);
                                }
                            }
                            sectionsHeader.Value = "(Section " + sectioncount.ToString() + ") " + headerStringBuilder.ToString();
                            sectionsHeaders.Add(sectionsHeader);
                            sectioncount++;
                        }
                    }
                }
            }
            return sectionsHeaders;
        }

        public EconsentSectionReferenceDocumentType GetEconsentSectionReferenceDocumentNew(int id)
        {
            var upload = _uploadSettingRepository.GetDocumentPath();
            var Econsentsectiondocument = Find(id);
            var FullPath = System.IO.Path.Combine(upload, Econsentsectiondocument.FilePath);
            string path = FullPath;
            if (!System.IO.File.Exists(path))
                return null;
            string extension = System.IO.Path.GetExtension(path);
            string type = "";

            EconsentSectionReferenceDocumentType econsentSectionReferenceDocument = new EconsentSectionReferenceDocumentType();

            if (extension == ".docx" || extension == ".doc")
            {
                Stream stream = System.IO.File.OpenRead(path);
                WordDocument document = new WordDocument();
                if (extension == ".docx")
                    document = new WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
                if (extension == ".doc")
                    document = new WordDocument(stream, Syncfusion.DocIO.FormatType.Doc);
                document.SaveOptions.HtmlExportCssStyleSheetType = CssStyleSheetType.Inline;
                MemoryStream ms = new MemoryStream();
                document.Save(ms, Syncfusion.DocIO.FormatType.Html);
                document.Close();
                ms.Position = 0;
                StreamReader reader = new StreamReader(ms);
                var htmlStringText = reader.ReadToEnd();
                ms.Dispose();
                reader.Dispose();
                stream.Close();
                stream.Dispose();
                type = "doc";
                econsentSectionReferenceDocument.type = type;
                econsentSectionReferenceDocument.data = htmlStringText;
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
                var fileupload = _uploadSettingRepository.GetWebImageUrl();
                var fileFullPath = System.IO.Path.Combine(fileupload, Econsentsectiondocument.FilePath);
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp" || extension == ".gif")
                {
                    type = "img";
                    econsentSectionReferenceDocument.type = type;
                    econsentSectionReferenceDocument.data = fileFullPath;
                    return econsentSectionReferenceDocument;
                }
                else
                {
                    type = "vid";
                    econsentSectionReferenceDocument.type = type;
                    econsentSectionReferenceDocument.data = fileFullPath;
                    return econsentSectionReferenceDocument;
                }
            }

        }

        public List<EconsentSectionReferenceDocument> GetEconsentSectionReferenceDocumentByUser()
        {
            var roleName = _jwtTokenAccesser.RoleName;

            var noneregister = _context.Randomization.Where(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();

            if (roleName == "LAR")
            {
                noneregister = _context.Randomization.Where(x => x.LARUserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            }
            if (noneregister == null) return new List<EconsentSectionReferenceDocument>();
            var result = _context.EconsentReviewDetails.Where(x => x.RandomizationId == noneregister.Id && x.EconsentSetup.DeletedDate == null
               && x.EconsentSetup.LanguageId == noneregister.LanguageId
               && (roleName == "LAR" ? x.IsLAR == true : x.IsLAR == null || x.IsLAR == false)).Select(s => s.EconsentSetupId).ToList();

            var econsentSectionReferenceDocuments = new List<EconsentSectionReferenceDocument>();
            var Econsentsectiondocuments = All.Where(q => result.Contains(q.EconsentSetupId) && q.DeletedDate == null).ToList();

            foreach (var Econsentsectiondocument in Econsentsectiondocuments)
            {
                var econsentSectionReferenceDocument = new EconsentSectionReferenceDocument();
                var upload = _uploadSettingRepository.GetDocumentPath();
                var FullPath = System.IO.Path.Combine(upload, Econsentsectiondocument.FilePath);
                string path = FullPath;
                if (!System.IO.File.Exists(path))
                    return new List<EconsentSectionReferenceDocument>();
                string extension = System.IO.Path.GetExtension(path);
                string type = "";

                if (extension == ".docx" || extension == ".doc")
                {
                    var pdfupload = _uploadSettingRepository.GetWebDocumentUrl();
                    var pdfFullPath = System.IO.Path.Combine(pdfupload, Econsentsectiondocument.FilePath);
                    type = extension.Trim('.');
                    econsentSectionReferenceDocument.type = type;
                    econsentSectionReferenceDocument.data = pdfFullPath.Replace('\\', '/');
                    econsentSectionReferenceDocument.Title = Econsentsectiondocument.ReferenceTitle;
                    econsentSectionReferenceDocuments.Add(econsentSectionReferenceDocument);
                }
                else if (extension == ".pdf")
                {
                    var pdfupload = _uploadSettingRepository.GetWebDocumentUrl();
                    var pdfFullPath = System.IO.Path.Combine(pdfupload, Econsentsectiondocument.FilePath);
                    type = "pdf";
                    econsentSectionReferenceDocument.type = type;
                    econsentSectionReferenceDocument.data = pdfFullPath.Replace('\\', '/');
                    econsentSectionReferenceDocument.Title = Econsentsectiondocument.ReferenceTitle;
                    econsentSectionReferenceDocuments.Add(econsentSectionReferenceDocument);
                }
                else
                {
                    var fileupload = _uploadSettingRepository.GetWebImageUrl();
                    var fileFullPath = System.IO.Path.Combine(fileupload, Econsentsectiondocument.FilePath);
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp" || extension == ".gif")
                    {
                        type = "img";
                        econsentSectionReferenceDocument.type = type;
                        econsentSectionReferenceDocument.data = fileFullPath.Replace('\\', '/');
                        econsentSectionReferenceDocument.Title = Econsentsectiondocument.ReferenceTitle;
                        econsentSectionReferenceDocuments.Add(econsentSectionReferenceDocument);
                    }
                    else
                    {
                        type = "vid";
                        econsentSectionReferenceDocument.type = type;
                        econsentSectionReferenceDocument.data = fileFullPath.Replace('\\', '/');
                        econsentSectionReferenceDocument.Title = Econsentsectiondocument.ReferenceTitle;
                        econsentSectionReferenceDocuments.Add(econsentSectionReferenceDocument);
                    }
                }
            }

            return econsentSectionReferenceDocuments;

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

        public IList<EconcentSectionRefrenceDetailListDto> GetSetionRefefrenceDetailList(int documentId, int sectionNo)
        {
            var sectionRefrence = All.Where(x => x.EconsentSetupId == documentId && x.SectionNo == sectionNo && x.DeletedDate == null).
               ProjectTo<EconcentSectionRefrenceDetailListDto>(_mapper.ConfigurationProvider).ToList();
            return sectionRefrence;
        }
    }
}
