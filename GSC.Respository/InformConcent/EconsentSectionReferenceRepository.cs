using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Shared;
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
        
        public EconsentSectionReferenceRepository(IGSCContext context, 
            IJwtTokenAccesser jwtTokenAccesser,
            IEconsentSetupRepository econsentSetupRepository,
            IUploadSettingRepository uploadSettingRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _econsentSetupRepository = econsentSetupRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public List<DropDownDto> GetEconsentDocumentSectionDropDown(int documentId)
        {
            var document = _econsentSetupRepository.Find(documentId);
            var upload = _uploadSettingRepository.GetDocumentPath();//_context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
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
    }
}
