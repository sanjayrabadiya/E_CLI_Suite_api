using System;
using System.Collections.Generic;
using GSC.Api.Controllers.Common;
using GSC.Respository.Etmf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Syncfusion.EJ2.PdfViewer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Hosting;
using GSC.Respository.UserMgt;
using GSC.Shared.Configuration;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Redaction;
using Syncfusion.Pdf;
using System.IO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class PdfViewerController : BaseController
    {
        public IMemoryCache _cache;

        private IPdfViewerRepository _pdfViewerRepository;
        private readonly ICentreUserService _centreUserService;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;

        public PdfViewerController(IMemoryCache cache,
            IPdfViewerRepository pdfViewerRepository, ICentreUserService centreUserService, IOptions<EnvironmentSetting> environmentSetting
            )
        {
            _cache = cache;
            _pdfViewerRepository = pdfViewerRepository;
            _centreUserService = centreUserService;
            _environmentSetting = environmentSetting;

        }


        [AllowAnonymous]
        [HttpPost]
        [Route("Load")]
        public IActionResult Load([FromBody] Dictionary<string, string> jsonData)
        {
            var jsonResult = _pdfViewerRepository.Load(jsonData);
            return Ok(JsonConvert.SerializeObject(jsonResult));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("RenderPdfPages")]
        //Post action for processing the PDF documents  
        public IActionResult RenderPdfPages([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object jsonResult = pdfviewer.GetPage(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }


        [HttpPost]
        [Route("Download")]
        //Post action for downloading the PDF documents
        public IActionResult Download([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
            return Content(documentBase);
        }


        [HttpPost]
        [Route("RenderThumbnailImages")]
        //Post action for rendering the ThumbnailImages
        public IActionResult RenderThumbnailImages([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object result = pdfviewer.GetThumbnailImages(jsonObject);
            return Content(JsonConvert.SerializeObject(result));
        }


        [HttpPost]
        [Route("Bookmarks")]
        //Post action for processing the bookmarks from the PDF documents
        public IActionResult Bookmarks([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            var jsonResult = pdfviewer.GetBookmarks(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("RenderAnnotationComments")]
        //Post action for rendering the annotations
        public IActionResult RenderAnnotationComments([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("PrintImages")]
        //Post action for printing the PDF documents
        public IActionResult PrintImages([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object pageImage = pdfviewer.GetPrintImage(jsonObject);
            return Content(JsonConvert.SerializeObject(pageImage));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Unload")]
        //Post action for unloading and disposing the PDF document resources  
        public IActionResult Unload([FromBody] Dictionary<string, string> jsonObject)
        {
            //Initialize the PDF Viewer object with memory cache object
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            pdfviewer.ClearCache(jsonObject);
            return this.Content("Document cache is cleared");
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ImportFormFields")]
        public IActionResult ImportFormFields([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object jsonResult = pdfviewer.ImportFormFields(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("[controller]/ExportFormFields")]
        public IActionResult ExportFormFields([FromBody] Dictionary<string, string> jsonObject)

        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            string jsonResult = pdfviewer.ExportFormFields(jsonObject);
            return Content(jsonResult);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ImportAnnotations")]
        //Post action to import annotations
        public IActionResult ImportAnnotations([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            string jsonResult = string.Empty;
            if (jsonObject != null && jsonObject.ContainsKey("fileName"))
            {
                string documentPath = jsonObject["fileName"];
                if (!string.IsNullOrEmpty(documentPath))
                {
                    jsonResult = System.IO.File.ReadAllText(documentPath);
                }
                else
                {
                    return this.Content(jsonObject["document"] + " is not found");
                }
            }
            return Content(jsonResult);
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("SaveDocument")]
        public async Task<IActionResult> SaveDocument([FromBody] Dictionary<string, string> jsonObject)
        {
            if (!_environmentSetting.Value.IsPremise)
            {
                var userName = Convert.ToString(jsonObject["userName"]);
                var result = await _centreUserService.GetUserDetails($"{_environmentSetting.Value.CentralApi}Login/GetUserDetails/{userName}");
                int CompanyID = Convert.ToInt32(result.CompanyId);
                _pdfViewerRepository.SetDbConnection(result.ConnectionString);
            }
            //await _centreUserService.SentConnectionString(CompanyID, $"{_environmentSetting.Value.CentralApi}Company/GetConnectionDetails/{CompanyID}");
            //_pdfViewerRepository.SaveDocument(jsonObject);

            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
            //return Content(documentBase);
            string base64String = documentBase.Split(new string[] { "data:application/pdf;base64," }, StringSplitOptions.None)[1];

            byte[] byteArray = Convert.FromBase64String(base64String);

            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(byteArray);
            //Get all the pages

            foreach (PdfLoadedPage loadedPage in loadedDocument.Pages)
            {
                List<PdfLoadedAnnotation> removeItems = new List<PdfLoadedAnnotation>();
                //Flatten all the annotations in the page
                foreach (PdfLoadedAnnotation annotation in loadedPage.Annotations)
                {
                    //Check for the circle annotation
                    if (annotation is PdfLoadedRectangleAnnotation)
                    {
                        removeItems.Add(annotation);
                        //Add redaction based on bounds of annotation.
                        PdfRedaction redaction = new PdfRedaction(annotation.Bounds, Syncfusion.Drawing.Color.Black);
                        loadedPage.AddRedaction(redaction);
                    }
                }
                foreach (PdfLoadedAnnotation annotation in removeItems)
                {
                    loadedPage.Annotations.Remove(annotation);
                }
            }
            //Redact the contents from the PDF document

            loadedDocument.Redact();

            //Save the PDF document.

            MemoryStream stream = new MemoryStream();

            //Save the PDF document

            loadedDocument.Save(stream);

            stream.Position = 0;

            //Close the document

            loadedDocument.Close(true);
            byteArray = stream.ToArray();

            _pdfViewerRepository.SaveDocument(jsonObject, byteArray);

            return Ok();

        }
    }
}