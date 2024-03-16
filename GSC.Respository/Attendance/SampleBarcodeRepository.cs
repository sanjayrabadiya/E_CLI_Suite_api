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
        private readonly IMapper _mapper;
        public SampleBarcodeRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<SampleBarcodeGridDto> GetSampleBarcodeList(bool isDeleted)
        {
            var list = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<SampleBarcodeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            list.ForEach(x =>
            {
                x.isBarcodeGenerated = _context.SampleBarcodeGenerate.Any(t => t.SampleBarcodeId == x.Id && t.DeletedBy == null);
            });

            return list;
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
            var volunteer = _context.Volunteer.FirstOrDefault(x => x.Id == objSave.VolunteerId);
            var peroid = _context.ProjectDesignVisit.Where(x => x.Id == objSave.VisitId).Include(x => x.ProjectDesignPeriod).First().ProjectDesignPeriod;
            var template = _context.ProjectDesignTemplate.FirstOrDefault(x => x.Id == objSave.TemplateId);

            var stringBuilder = new StringBuilder();
            if (objSave.PKBarcodeOption > 1)
            {
                for (int i = 1; i <= objSave.PKBarcodeOption; i++)
                {
                    stringBuilder.Append($"SS{volunteer?.RandomizationNumber}{peroid.DisplayName}{template?.TemplateCode}0{i},");
                }
            }
            else
            {
                stringBuilder.Append($"SS{volunteer?.RandomizationNumber}{peroid.DisplayName}{template?.TemplateCode}");
            }

            var barcode = stringBuilder.ToString();

            return barcode.TrimEnd(',');
        }

        public List<ProjectDropDown> GetProjectDropdown()
        {
            var project = _context.PKBarcode.Include(x => x.Project)
                .Where(x => x.DeletedDate == null)
                 .Select(c => new ProjectDropDown
                 {
                     Id = c.ProjectId.Value,
                     Value = c.Project.ProjectCode + " - " + c.Project.ProjectName,
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
                     Value = c.Site.ProjectCode == null ? c.Site.ManageSite.SiteName : c.Site.ProjectCode + " - " + c.Site.ManageSite.SiteName
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
                 Value = t.ProjectDesignVisit.DisplayName
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
                    Value = t.ProjectDesignTemplate.TemplateName
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
        }

        public void UpdateBarcode(List<int> ids)
        {
            var barcodes = _context.SampleBarcode.Where(x => ids.Contains(x.Id));
            foreach (var barcode in barcodes)
            {
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

