﻿using System;
using System.Collections.Generic;
using GSC.Api.Controllers.Common;
using GSC.Respository.Etmf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Syncfusion.EJ2.PdfViewer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Hosting;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class PdfViewerController : BaseController
    {
        public IMemoryCache _cache;
        private IHostingEnvironment _hostingEnvironment;
        private IPdfViewerRepository _pdfViewerRepository;
        public PdfViewerController(IMemoryCache cache, IHostingEnvironment hostingEnvironment,
            IPdfViewerRepository pdfViewerRepository)
        {
            _cache = cache;
            _hostingEnvironment = hostingEnvironment;
            _pdfViewerRepository = pdfViewerRepository;
        }


        [AllowAnonymous]
        [HttpPost("Load")]
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

        [AllowAnonymous]
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

        [AllowAnonymous]
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

        [AllowAnonymous]
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
        [Route("ExportAnnotations")]
        //Post action to export annotations
        public IActionResult ExportAnnotations([FromBody] Dictionary<string, string> jsonObject)
        {
            //PdfRenderer pdfvieweraa = new PdfRenderer(_cache);
            //string jsonResult = pdfvieweraa.GetAnnotations(jsonObject);
            //return Content(jsonResult);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SaveDocument")]
        public ActionResult SaveDocument([FromBody] Dictionary<string, string> jsonObject)
        {
            _pdfViewerRepository.SaveDocument(jsonObject);
            return Ok();
        }
    }
}