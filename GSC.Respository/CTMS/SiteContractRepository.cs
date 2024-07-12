using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using HtmlToOpenXml;
using Microsoft.EntityFrameworkCore;
using GSC.Shared.JWTAuth;
using System.IO;
using GSC.Respository.Configuration;

namespace GSC.Respository.Master
{
    public class SiteContractRepository : GenericRespository<SiteContract>, ISiteContractRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SiteContractRepository(IGSCContext context, IMapper mapper)
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public SiteContractRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IUploadSettingRepository uploadSettingRepository,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public IList<SiteContractGridDto> GetSiteContractList(bool isDeleted, int studyId, int siteId)
        {
            var query = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == studyId);

            if (siteId != 0)
            {
                query = query.Where(x => x.SiteId == siteId);
            }

            var siteContractGridData = query.ProjectTo<SiteContractGridDto>(_mapper.ConfigurationProvider)
                                            .OrderByDescending(x => x.Id)
                                            .ToList();

            foreach (var item in siteContractGridData)
            {
                item.SiteName = _context.Project
                                        .Include(s => s.ManageSite)
                                        .Where(w => w.Id == item.SiteId)
                                        .Select(d => d.ProjectCode ?? d.ManageSite.SiteName)
                                        .FirstOrDefault();
            }

            return siteContractGridData;
        }
        public string Duplicate(SiteContractDto SiteContractDto)
        {
            if (All.Any(x =>( x.Id != SiteContractDto.Id && x.ProjectId == SiteContractDto.ProjectId && x.SiteId == SiteContractDto.SiteId && x.DeletedDate == null) ||( x.ContractCode == SiteContractDto.ContractCode && x.DeletedDate == null)))
            {
                return "Duplicate this Site Contract";
            }
            return "";
        }
        public void CreateContractTemplateFormat(ContractTemplateFormat contractTemplateFormat, SiteContractDto siteContractDto)
        {
            Guid obj = Guid.NewGuid();
            var adress = "";
            string currentDate = DateTime.Now.ToString("dd MMMM yyyy");

            var projectdata = _context.Project.Include(c => c.Country).Include(s => s.State).Include(c => c.City).Include(a => a.CityArea).Include(m => m.ManageSite).Where(x => x.Id == siteContractDto.SiteId).FirstOrDefault();
            var studyCode = _context.Project.Where(p => p.Id == projectdata.ParentProjectId && p.ParentProjectId == null).FirstOrDefault();

            if (projectdata != null)
            {
                if (projectdata.CityArea != null)
                    adress += projectdata.CityArea.AreaName + ", ";
                if (projectdata.City != null)
                    adress += projectdata.City.CityName + ", ";
                if (projectdata.State != null)
                    adress += projectdata.State.StateName + ", ";
                if (projectdata.Country != null)
                    adress += projectdata.Country.CountryName + "-";
                if (projectdata.PinCode != null)
                    adress += projectdata.PinCode;
            }

            contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##CURRENTDATE##", currentDate, RegexOptions.IgnoreCase);
            contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##<strong>CURRENTDATE</strong>##", "<strong>" + currentDate + "</strong>", RegexOptions.IgnoreCase);

            contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##ADDRESS##", adress, RegexOptions.IgnoreCase);
            contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##<strong>ADDRESS</strong>##", "<strong>" + adress + "</strong>", RegexOptions.IgnoreCase);
            if (projectdata != null)
            {
                contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##SITENAME##", projectdata.ProjectCode == null ? projectdata.ManageSite.SiteName : projectdata.ProjectCode, RegexOptions.IgnoreCase);
                contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##<strong>SITENAME</strong>##", "<strong>" + projectdata.ProjectCode == null ? projectdata.ManageSite.SiteName : projectdata.ProjectCode + "</strong>", RegexOptions.IgnoreCase);
            }
            if (studyCode != null)
            {
                contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##SUDYCODE##", studyCode.ProjectCode, RegexOptions.IgnoreCase);
                contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##<strong>SUDYCODE</strong>##", "<strong>" + studyCode.ProjectCode + "</strong>", RegexOptions.IgnoreCase);

                contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##SUDYNAME##", studyCode.ProjectName, RegexOptions.IgnoreCase);
                contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##<strong>SUDYNAME</strong>##", "<strong>" + studyCode.ProjectName + "</strong>", RegexOptions.IgnoreCase);
            }

            contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##CRANAME##", _jwtTokenAccesser.UserName, RegexOptions.IgnoreCase);
            contractTemplateFormat.TemplateFormat = Regex.Replace(contractTemplateFormat.TemplateFormat, "##<strong>CRANAME</strong>##", "<strong>" + _jwtTokenAccesser.UserName + "</strong>", RegexOptions.IgnoreCase);

            siteContractDto.FormatBody = contractTemplateFormat.TemplateFormat;

            if (!String.IsNullOrEmpty(siteContractDto.ContractDocumentPath))
            {
                string[] removePaths = { siteContractDto.ContractDocumentPath };
                var removeFullPath = Path.Combine(removePaths);
                if (File.Exists(removeFullPath))
                {
                    File.Delete(Path.Combine(removeFullPath));
                }
            }
            var fileName = "Site-Contract-" + obj.ToString() + ".docx";
            string[] paths = { _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms.ToString(), fileName };
            var fullPath = Path.Combine(paths);
            //lettersActivityDto.AttachmentPath = fullPath;


            ////docx file save
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(fullPath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();
                mainPart.Document.Append(body);
                HtmlConverter converter = new HtmlConverter(mainPart);
                converter.ParseHtml(contractTemplateFormat.TemplateFormat);
                wordDocument.Save();
            }

            //docx filepath save in Table
            string[] paths1 = {_jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms.ToString(), fileName };
            var fullPath1 = Path.Combine(paths1);
            siteContractDto.ContractDocumentPath = fullPath1;
            siteContractDto.ContractFileName = fileName;
        }
    }
}
