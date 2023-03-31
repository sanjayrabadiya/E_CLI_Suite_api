using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Barcode;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Respository.ProjectRight;
using GSC.Respository.Volunteer;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Attendance
{
    public class SampleBarcodeRepository : GenericRespository<SampleBarcode>, ISampleBarcodeRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IMapper _mapper;
        public SampleBarcodeRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IVolunteerRepository volunteerRepository, IMapper mapper, IProjectRightRepository projectRightRepository) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _volunteerRepository = volunteerRepository;
            _mapper = mapper;
            _projectRightRepository = projectRightRepository;
        }

        public List<SampleBarcodeGridDto> GetSampleBarcodeList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<SampleBarcodeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(SampleBarcodeDto objSave)
        {
            if (All.Any(
               x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId
               && x.VisitId == objSave.VisitId
               && x.TemplateId == objSave.TemplateId
               && x.SiteId == objSave.SiteId
               && x.VolunteerId == objSave.VolunteerId
               && x.DeletedDate == null))
                return "Duplicate sample barcode";
            return "";
        }

        public string GenerateBarcodeString(SampleBarcodeDto objSave)
        {
            string barcode = "";
            var volunteer = _context.Volunteer.FirstOrDefault(x => x.Id == objSave.VolunteerId);
            var peroid = _context.ProjectDesignVisit.Where(x => x.Id == objSave.VisitId).Include(x => x.ProjectDesignPeriod).FirstOrDefault().ProjectDesignPeriod;
            var template = _context.ProjectDesignTemplate.FirstOrDefault(x => x.Id == objSave.TemplateId);

            if (objSave.PKBarcodeOption > 1)
            {
                for (int i = 1; i <= objSave.PKBarcodeOption; i++)
                {
                    barcode = barcode + "SS" + volunteer.RandomizationNumber + peroid.DisplayName + template.DesignOrder + "0" + i + ",";
                }
            }
            else
            {
                barcode = "SS" + volunteer.RandomizationNumber + peroid.DisplayName + template.DesignOrder;
            }
            return barcode.TrimEnd(',');
        }

        public List<ProjectDropDown> GetProjectDropdown()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            var project = _context.PKBarcode.Include(x => x.Project)
                .Where(x => x.DeletedDate == null)
                 .Select(c => new ProjectDropDown
                 {
                     Id = c.ProjectId.Value,
                     Value = c.Project.ProjectCode + " - " + c.Project.ProjectName,
                     //Code = c.Project.ProjectCode,
                     //IsStatic = c.Project.IsStatic,
                     //IsDeleted = c.DeletedDate != null,
                     //ParentProjectId = c.Project.ParentProjectId ?? c.ProjectId ?? 0,
                     //AttendanceLimit = c.Project.AttendanceLimit ?? 0
                 }).Distinct().OrderBy(o => o.Value).ToList();
            return project;
        }

        public List<ProjectDropDown> GetChildProjectDropDown(int parentProjectId)
        {
            var project = _context.PKBarcode.Include(x => x.Project).Include(x => x.Site)
                .Where(x => x.DeletedDate == null && x.ProjectId == parentProjectId)
                 .Select(c => new ProjectDropDown
                 {
                     Id = c.SiteId ?? 0,
                     Value = c.Site.ProjectCode == null ? c.Site.ManageSite.SiteName : c.Site.ProjectCode + " - " + c.Site.ManageSite.SiteName,
                     //CountryId = c.Site.ManageSite != null && c.Site.ManageSite.City != null && c.Site.ManageSite.City.State != null ? c.Site.ManageSite.City.State.CountryId : 0,
                     //Code = c.Site.ProjectCode,
                     //IsStatic = c.Site.IsStatic,
                     //IsTestSite = c.Site.IsTestSite,
                     //ParentProjectId = c.Site.ParentProjectId ?? 0,
                     //AttendanceLimit = c.Site.AttendanceLimit ?? 0,
                 }).Distinct().OrderBy(o => o.Value).ToList();

            return project;
        }

        public List<DropDownDto> GetVisitList(int projectId, int siteId)
        {
            var visitList = _context.PKBarcode.Include(x => x.ProjectDesignVisit)
                .Where(x => x.DeletedDate == null && x.ProjectId == projectId && x.SiteId == siteId)
             .Select(t => new DropDownDto
             {
                 Id = t.VisitId ?? 0,
                 Value = t.ProjectDesignVisit.DisplayName,
                 //Code = t.ProjectDesignVisit.StudyVersion != null || t.ProjectDesignVisit.InActiveVersion != null ?
                 //   "( V : " + t.ProjectDesignVisit.StudyVersion + (t.ProjectDesignVisit.StudyVersion != null && t.ProjectDesignVisit.InActiveVersion != null ? " - " : "" + t.ProjectDesignVisit.InActiveVersion) + ")" : "",
                 //ExtraData = t.ProjectDesignVisit.IsNonCRF,
                 //InActive = t.ProjectDesignVisit.InActiveVersion != null
             }).Distinct().ToList();

            return visitList;
        }

        public List<DropDownDto> GetTemplateList(int projectId, int siteId, int visitId)
        {
            var templateList = _context.PKBarcode.Include(x => x.ProjectDesignTemplate)
                .Where(x => x.DeletedDate == null && x.ProjectId == projectId && x.SiteId == siteId && x.VisitId == visitId)
                .Select(t => new DropDownDto
                {
                    Id = t.TemplateId ?? 0,
                    Value = t.ProjectDesignTemplate.TemplateName,
                    //Code = _context.ProjectScheduleTemplate.Any(x => x.ProjectDesignTemplateId == t.Id) ? "Used" : "",
                    //InActive = t.ProjectDesignTemplate.InActiveVersion != null
                }).Distinct().ToList();

            return templateList;
        }

        public List<DropDownDto> GetVolunteerList(int siteId)
        {
            var subjectList = _context.PKBarcode.Where(x => x.SiteId == siteId && x.DeletedDate == null).Select(x => x.VolunteerId).Distinct().ToList();
            return _context.Volunteer.Where(x => x.RandomizationNumber != null && x.DeletedDate == null &&
            subjectList.Contains(x.Id) &&
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.VolunteerNo + " " + c.FirstName + " " + c.MiddleName + " " + c.LastName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();

            //var templateList = _context.PKBarcode.Include(x => x.Volunteer)
            //    .Where(x => x.DeletedDate == null && x.SiteId == siteId && x.BarcodeDate != null)
            //    .Select(t => new DropDownDto
            //    {
            //        Id = t.VolunteerId ?? 0,
            //        Code = "",
            //        Value = t.Volunteer.FirstName + " " + t.Volunteer.LastName
            //    }).Distinct().ToList();

            //return templateList;
        }

        public void UpdateBarcode(List<int> ids)
        {
            var barcodes = _context.SampleBarcode.Where(x => ids.Contains(x.Id));
            foreach (var barcode in barcodes)
            {
                //barcode.IsBarcodeReprint = true;
                barcode.BarcodeDate = DateTime.Now;

                _context.SampleBarcode.Update(barcode);
            }

            _context.Save();
        }

        public void BarcodeReprint(List<int> ids)
        {
            var barcodes = _context.SampleBarcode.Where(x => ids.Contains(x.Id));
            foreach (var barcode in barcodes)
            {
                barcode.IsBarcodeReprint = true;

                _context.SampleBarcode.Update(barcode);
            }

            _context.Save();
        }

        public void DeleteBarcode(List<int> ids)
        {
            var barcodes = _context.SampleBarcode.Where(x => ids.Contains(x.Id));
            foreach (var barcode in barcodes)
            {
                barcode.IsBarcodeReprint = false;
                barcode.BarcodeDate = null;

                _context.SampleBarcode.Update(barcode);
            }

            _context.Save();
        }
    }
}

