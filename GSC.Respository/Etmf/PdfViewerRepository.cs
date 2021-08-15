using GSC.Common.GenericRespository;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Syncfusion.EJ2.PdfViewer;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace GSC.Respository.Etmf
{
    public class PdfViewerRepository : GenericRespository<ProjectWorkplaceArtificatedocument>, IPdfViewerRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;
        private readonly IProjectSubSecArtificateDocumentHistoryRepository _projectSubSecArtificateDocumentHistoryRepository;
        private readonly IProjectWorkplaceSubSecArtificatedocumentRepository _projectWorkplaceSubSecArtificatedocumentRepository;
        public IMemoryCache _cache;
        public PdfViewerRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
           IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository,
            IMemoryCache cache, 
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IProjectWorkplaceSubSecArtificatedocumentRepository projectWorkplaceSubSecArtificatedocumentRepository,
            IProjectSubSecArtificateDocumentHistoryRepository projectSubSecArtificateDocumentHistoryRepository)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _projectArtificateDocumentHistoryRepository = projectArtificateDocumentHistoryRepository;
            _projectWorkplaceSubSecArtificatedocumentRepository = projectWorkplaceSubSecArtificatedocumentRepository;
            _projectSubSecArtificateDocumentHistoryRepository = projectSubSecArtificateDocumentHistoryRepository;
            _cache = cache;
        }

        public void SaveDocument(Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);

            var docName = jsonObject["documentName"].ToString();
            var fileName = docName.Contains('_') ? docName.Substring(0, docName.LastIndexOf('_')) : docName;
            var docExtendedName = fileName + "_" + DateTime.Now.Ticks + ".pdf";
            string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
            string base64String = documentBase.Split(new string[] { "data:application/pdf;base64," }, StringSplitOptions.None)[1];
            if (base64String != null || base64String != string.Empty)
            {
                byte[] byteArray = Convert.FromBase64String(base64String);

                MemoryStream ms = new MemoryStream(byteArray);
                System.IO.File.WriteAllBytes(jsonObject["documentPath"].ToString() + "/" + docExtendedName, byteArray);
            }

            var version = Convert.ToDouble(jsonObject["level"]);
            if (version == 6)
            {
                var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(Convert.ToInt32(jsonObject["artificateDocumentId"]));
                projectWorkplaceArtificatedocument.DocumentName = docExtendedName;
                _projectWorkplaceArtificatedocumentRepository.Update(projectWorkplaceArtificatedocument);
                if (_context.Save() <= 0) throw new Exception("Updating Document failed on save.");

                if (!Convert.ToBoolean(jsonObject["addHistory"].ToString()))
                    _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, null);
            }
            else if (version == 5.2)
            {
                var projectWorkplaceSubSecArtificatedocument = _context.ProjectWorkplaceSubSecArtificatedocument.Where(x=>x.Id==Convert.ToInt32(jsonObject["artificateDocumentId"])).FirstOrDefault();
                projectWorkplaceSubSecArtificatedocument.DocumentName = docExtendedName;
                _projectWorkplaceSubSecArtificatedocumentRepository.Update(projectWorkplaceSubSecArtificatedocument);
                if (_context.Save() <= 0) throw new Exception("Updating Document failed on save.");

                if (!Convert.ToBoolean(jsonObject["addHistory"].ToString()))
                    _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceSubSecArtificatedocument, null, null);
            }
        }

        public object Load(Dictionary<string, string> jsonData) 
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            MemoryStream stream = new MemoryStream();

            object jsonResult = new object();
            if (jsonData != null && jsonData.ContainsKey("document"))
            {
                if (bool.Parse(jsonData["isFileName"]))
                {
                    string documentPath = jsonData["document"];

                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        byte[] bytes = File.ReadAllBytes(documentPath);
                        stream = new MemoryStream(bytes);
                    }
                    else
                    {
                        string fileName = jsonData["document"].Split(new string[] { "://" }, StringSplitOptions.None)[0];
                        if (fileName == "http" || fileName == "https")
                        {
                            WebClient WebClient = new WebClient();
                            byte[] pdfDoc = WebClient.DownloadData(jsonData["document"]);
                            stream = new MemoryStream(pdfDoc);
                        }
                        //else
                        //{
                        //    return Content(jsonData["document"] + " is not found");
                        //}
                    }
                }
                else
                {
                    byte[] bytes = Convert.FromBase64String(jsonData["document"]);
                    stream = new MemoryStream(bytes);
                }
            }
            jsonResult = pdfviewer.Load(stream, jsonData);
            return jsonResult;
        }

    }
}
