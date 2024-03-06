using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EmailSender;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using GSC.Respository.Configuration;
using System;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using HtmlToOpenXml;


namespace GSC.Respository.Master
{
    public class LettersActivityRepository : GenericRespository<LettersActivity>, ILettersActivityRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly ILettersActivityRepository _lettersActivityRepository;
        public LettersActivityRepository(IGSCContext context, IEmailSenderRespository emailSenderRespository,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IUploadSettingRepository uploadSettingRepository,
            ILettersActivityRepository lettersActivityRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _emailSenderRespository = emailSenderRespository;
            _uploadSettingRepository = uploadSettingRepository;
            _lettersActivityRepository = lettersActivityRepository;
        }
        public List<DropDownDto> GetActivityTypeDropDown()
        {
            return _context.CtmsActivity.Where(x =>
                    x.DeletedBy == null && x.CreatedBy != null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ActivityName, Code = c.ActivityCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }
        public List<DropDownDto> GetLettersFormatTypeDropDown()
        {
            return _context.LettersFormate.Where(x =>
                    x.DeletedBy == null && x.CreatedBy != null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.LetterName, Code = c.LetterCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }
        public List<DropDownDto> GetMedicalUserTypeDown(int siteId)
        {
            var medUser = _context.SiteTeam.Where(x => (x.DeletedBy == null && x.CreatedBy != null) && (x.ProjectId == siteId)).ToList();

            return _context.Users.Where(x => medUser.Select(y => y.UserId).Contains(x.Id) && x.DeletedBy == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.UserName, Code = c.UserName, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }
        List<LettersActivityGridDto> ILettersActivityRepository.GetLettersActivityList(bool isDeleted, int? projectId)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId == projectId).
                   ProjectTo<LettersActivityGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public List<LettersActivityDateDropDown> getSelectDateDrop(int projectId, int siteId)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" || x.ActivityCode == "act_004" || x.ActivityCode == "act_005" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();
            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                   .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                   && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            var list = _context.CtmsMonitoring
                       .Include(i => i.Project)
                       .ThenInclude(i => i.ManageSite)
                       .Where(z => z.ProjectId == siteId && StudyLevelForm.Select(y => y.Id).Contains(z.StudyLevelFormId)
                       && (siteId == 0 ? (!z.Project.IsTestSite) : true)
                       && z.DeletedDate == null && z.ScheduleStartDate != null && z.ScheduleEndDate != null)
                       .Select
                       (b => new LettersActivityDateDropDown
                       {
                           Id = b.Id,
                           Value = b.ScheduleStartDate,
                           ActivityType = b.StudyLevelForm.Activity.CtmsActivity.Id,
                           Code = b.Project.ManageSite.City.State.Country.CountryName
                       }).ToList();

            return list;

        }
        public void CreateLettersEmail(LettersFormate lettersFormate, LettersActivityDto lettersActivityDto)
        {
            Guid obj = Guid.NewGuid();
            var adress = "";
            string currentDate = DateTime.Now.ToString("dd MMMM yyyy");

            var projectdata = _context.Project.Include(c => c.Country).Include(s => s.State).Include(c => c.City).Include(a => a.CityArea).Include(m => m.ManageSite).Where(x => x.Id == lettersActivityDto.ProjectId).FirstOrDefault();
            var CtmsMonitoringdata = _context.CtmsMonitoring.Where(x => x.Id == lettersActivityDto.CtmsMonitoringId).FirstOrDefault();
            var studyCode = _context.Project.Where(p => p.Id == projectdata.ParentProjectId && p.ParentProjectId == null).FirstOrDefault();
            var userIntigration = _context.Users.Where(p => p.Id == lettersActivityDto.UserIntigration && p.DeletedBy == null).FirstOrDefault();

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

            lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##CURRENTDATE##", currentDate, RegexOptions.IgnoreCase);
            lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##<strong>CURRENTDATE</strong>##", "<strong>" + currentDate + "</strong>", RegexOptions.IgnoreCase);

            lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##ADDRESS##", adress, RegexOptions.IgnoreCase);
            lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##<strong>ADDRESS</strong>##", "<strong>" + adress + "</strong>", RegexOptions.IgnoreCase);
            if (projectdata != null)
            {
                lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##SITENAME##", projectdata.ProjectCode == null ? projectdata.ManageSite.SiteName : projectdata.ProjectCode, RegexOptions.IgnoreCase);
                lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##<strong>SITENAME</strong>##", "<strong>" + projectdata.ProjectCode == null ? projectdata.ManageSite.SiteName : projectdata.ProjectCode + "</strong>", RegexOptions.IgnoreCase);
            }
            if(studyCode!=null)
            {    
                lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##SUDYCODE##", studyCode.ProjectCode, RegexOptions.IgnoreCase);
                lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##<strong>SUDYCODE</strong>##", "<strong>" + studyCode.ProjectCode + "</strong>", RegexOptions.IgnoreCase);

                lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##SUDYNAME##", studyCode.ProjectName, RegexOptions.IgnoreCase);
                lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##<strong>SUDYNAME</strong>##", "<strong>" + studyCode.ProjectName + "</strong>", RegexOptions.IgnoreCase);
            }
            if (userIntigration != null)
            {
                lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##USERNAME##", userIntigration.UserName, RegexOptions.IgnoreCase);
                lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##<strong>USERNAME</strong>##", "<strong>" + userIntigration.UserName + "</strong>", RegexOptions.IgnoreCase);
            }
            if (CtmsMonitoringdata != null)
            {
                lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##DATE##", CtmsMonitoringdata.ScheduleStartDate.ToString(), RegexOptions.IgnoreCase);
                lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##<strong>DATE</strong>##", "<strong>" + CtmsMonitoringdata.ScheduleStartDate.ToString() + "</strong>", RegexOptions.IgnoreCase);
            }
            lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##CRANAME##", _jwtTokenAccesser.UserName, RegexOptions.IgnoreCase);
            lettersFormate.LetterBody = Regex.Replace(lettersFormate.LetterBody, "##<strong>CRANAME</strong>##", "<strong>" + _jwtTokenAccesser.UserName + "</strong>", RegexOptions.IgnoreCase);

            lettersActivityDto.LetterBody = lettersFormate.LetterBody;

            if (!String.IsNullOrEmpty(lettersActivityDto.FilePath))
            {
                string[] removePaths = { lettersActivityDto.FilePath };
                var removeFullPath = Path.Combine(removePaths);
                if (File.Exists(removeFullPath))
                {
                    File.Delete(Path.Combine(removeFullPath));
                }
            }
            string[] paths = { _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms.ToString(), "Letters-" + obj.ToString() + ".docx" };
            var fullPath = Path.Combine(paths);
            lettersActivityDto.AttachmentPath = fullPath;

            ////docx file save
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(fullPath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();
                mainPart.Document.Append(body);
                HtmlConverter converter = new HtmlConverter(mainPart);
                converter.ParseHtml(lettersFormate.LetterBody);
                wordDocument.Save();
            }

            //docx filepath save in Table
            string[] paths1 = { _uploadSettingRepository.GetWebDocumentUrl(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms.ToString(), "Letters-" + obj.ToString() + ".docx" };
            var fullPath1 = Path.Combine(paths1);
            lettersActivityDto.FilePath = fullPath1;
        }
        public void updateLettersEmail(LettersActivityDto lettersActivityDto)
        {
            Guid obj = Guid.NewGuid();
            if (!String.IsNullOrEmpty(lettersActivityDto.FilePath))
            {
                string[] removePaths = { lettersActivityDto.FilePath };
                var removeFullPath = Path.Combine(removePaths);
                if (File.Exists(removeFullPath))
                {
                    File.Delete(Path.Combine(removeFullPath));
                }
            }

            string[] paths = { _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms.ToString(), "Letters-" + obj.ToString() + ".docx" };
            var fullPath = Path.Combine(paths);
            lettersActivityDto.AttachmentPath = fullPath;
            //docx file save
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(fullPath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();
                mainPart.Document.Append(body);
                HtmlConverter converter = new HtmlConverter(mainPart);
                converter.ParseHtml(lettersActivityDto.LetterBody);
                wordDocument.Save();
            }

            //docx filepath save in table
            string[] paths1 = { _uploadSettingRepository.GetWebDocumentUrl(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms.ToString(), "Letters-" + obj.ToString() + ".docx" };
            var fullPath1 = Path.Combine(paths1);
            lettersActivityDto.FilePath = fullPath1;
        }
        public string Duplicate(LettersActivity objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.Email == objSave.Email.Trim() && x.DeletedDate == null))
                return "Duplicate Letters Activity : " + objSave.Email;
            return "";
        }
        public List<LettersActivityDto> UserRoles(int ProjectId)
        {
            //add by mitul on 30-11-2023 -> CTMS UserAccess wise user get
            var users = _context.UserAccess.Include(x => x.UserRole).ThenInclude(x => x.User).Where(s => s.ProjectId == ProjectId && s.DeletedDate == null && s.UserRole.UserId != _jwtTokenAccesser.UserId)
                .OrderByDescending(s => s.Id).Select(c => new LettersActivityDto
                {
                    UserId = c.UserRole.UserId,
                    Name = c.UserRole.User.UserName,
                    IsSelected = true,
                }).ToList();

            return users;
        }
        public string GetSendMail(SendMailModel sendMailModel)
        {
            var record = _lettersActivityRepository.Find(sendMailModel.Id);
            var data = _context.LettersActivity.Include(c => c.Activity).Include(m => m.CtmsMonitoring).Where(x => x.Id == sendMailModel.Id).FirstOrDefault();
            if (record == null) return "";
            var lettersActivityDto = _mapper.Map<LettersActivityDto>(record);

            if(data!=null)
            {
                if (sendMailModel.Email != null && sendMailModel.Email != "")
                    _emailSenderRespository.SendALettersMailtoInvestigator(lettersActivityDto.AttachmentPath, sendMailModel.Email, sendMailModel.Body, data.Activity.ActivityName, data.CtmsMonitoring.ScheduleStartDate.ToString());

                for (int i = 0; i < sendMailModel.OpstionLists.Count; i++)
                {
                    EmailModel item = sendMailModel.OpstionLists[i];
                    lettersActivityDto.Email = item.Option;
                    if (item.Option != null && item.Option != "")
                        _emailSenderRespository.SendALettersMailtoInvestigator(lettersActivityDto.AttachmentPath, item.Option, sendMailModel.Body, data.Activity.ActivityName, data.CtmsMonitoring.ScheduleStartDate.ToString());
                }

                foreach (var item in sendMailModel.UserModel)
                {
                    var email = _context.Users.Where(x => x.Id == item.userId && x.DeletedBy == null).Select(x => x.Email).FirstOrDefault();
                    _emailSenderRespository.SendALettersMailtoInvestigator(lettersActivityDto.AttachmentPath, email, sendMailModel.Body, data.Activity.ActivityName, data.CtmsMonitoring.ScheduleStartDate.ToString());
                }
            }

            var lettersActivity = _mapper.Map<LettersActivity>(lettersActivityDto);
            _lettersActivityRepository.Update(lettersActivity);
            _context.Save();
            return "";
        }
    }
}