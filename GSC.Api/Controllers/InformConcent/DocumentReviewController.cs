using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GSC.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.DocIO.ReaderWriter.DataStreamParser.Escher;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentReviewController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("GetDocumentHeaders")]
        public IActionResult GetDocumentHeaders(int PatientId)
        {
            //var document = _projectWorkplaceArtificatedocumentRepository.Find(id);
            //var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            //var dfdf = System.IO.Path.Combine(upload.DocumentPath, document.DocPath, document.DocumentName);
            //string path = dfdf;
            string path = "C:\\Users\\Shree\\Documents\\ICF_English_A.N.Pharamcia-chlor.docx";
            if (!System.IO.File.Exists(path))
                return null;
            Stream stream = System.IO.File.OpenRead(path);
            EJ2WordDocument document = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
            stream.Close();
            document.Dispose();
            JObject jsonstr = JObject.Parse(json);
            Root jsonobj = JsonConvert.DeserializeObject<Root>(jsonstr.ToString());
            List<SectionsHeader> sectionsHeaders = new List<SectionsHeader>();
            int sectioncount = 1;
            foreach (var e1 in jsonobj.sections)
            {
                foreach (var e2 in e1.blocks)
                {
                    if (e2.paragraphFormat != null && e2.paragraphFormat.styleName == "Heading 1")
                    {
                        SectionsHeader sectionsHeader = new SectionsHeader();
                        sectionsHeader.sectionNo = sectioncount;
                        sectionsHeader.sectionName = "Section " + sectioncount.ToString();
                        string headerstring = "";
                        foreach (var e3 in e2.inlines)
                        {
                            if (e3.text != null)
                            {
                                headerstring = headerstring + e3.text;
                            }
                        }
                        sectionsHeader.header = headerstring;
                        sectionsHeaders.Add(sectionsHeader);
                        sectioncount++;
                    }
                }
            }
            return Ok(sectionsHeaders);
        }
    }
}
