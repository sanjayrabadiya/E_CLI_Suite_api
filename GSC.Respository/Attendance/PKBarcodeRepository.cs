using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Screening;
using GSC.Respository.Volunteer;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.Attendance
{
    public class PKBarcodeRepository : GenericRespository<PKBarcode>, IPKBarcodeRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IAttendanceRepository _attendanceRepository;


        private readonly IMapper _mapper;
        public PKBarcodeRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IAttendanceRepository attendanceRepository,
            IVolunteerRepository volunteerRepository, IMapper mapper) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _volunteerRepository = volunteerRepository;
            _attendanceRepository = attendanceRepository;
            _mapper = mapper;
        }

        public List<PKBarcodeGridDto> GetPKBarcodeList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<PKBarcodeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(PKBarcodeDto objSave)
        {
            if (All.Any(
                x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId
                && x.VisitId == objSave.VisitId
                && x.TemplateId == objSave.TemplateId
                && x.SiteId == objSave.SiteId
                && x.VolunteerId == objSave.VolunteerId
                && x.DeletedDate == null))
                return "Duplicate PK Barcode";
            return "";
        }

        public string GenerateBarcodeString(PKBarcodeDto objSave)
        {
            string barcode = "";
            var volunteer = _context.Volunteer.FirstOrDefault(x => x.Id == objSave.VolunteerId);
            var peroid = _context.ProjectDesignVisit.Where(x => x.Id == objSave.VisitId).Include(x => x.ProjectDesignPeriod).FirstOrDefault().ProjectDesignPeriod;
            var template = _context.ProjectDesignTemplate.FirstOrDefault(x => x.Id == objSave.TemplateId);
            if (objSave.PKBarcodeOption > 1)
            {
                for (int i = 1; i <= objSave.PKBarcodeOption; i++)
                {
                    barcode = barcode + "PK" + volunteer.RandomizationNumber + peroid.DisplayName + template.TemplateCode + "0" + i + ",";
                }
            }
            else
            {
                barcode = "PK" + volunteer.RandomizationNumber + peroid.DisplayName + template.TemplateCode;
            }
            return barcode.TrimEnd(',');
        }

        public void UpdateBarcode(List<int> ids)
        {
            var barcodes = _context.PKBarcode.Where(x => ids.Contains(x.Id));
            foreach (var barcode in barcodes)
            {
                //barcode.IsBarcodeReprint = true;
                barcode.BarcodeDate = DateTime.Now;

                _context.PKBarcode.Update(barcode);
            }

            _context.Save();
        }

        public void BarcodeReprint(List<int> ids)
        {
            var barcodes = _context.PKBarcode.Where(x => ids.Contains(x.Id));
            foreach (var barcode in barcodes)
            {
                barcode.IsBarcodeReprint = true;

                _context.PKBarcode.Update(barcode);
            }

            _context.Save();
        }

        public void DeleteBarcode(List<int> ids)
        {
            var barcodes = _context.PKBarcode.Where(x => ids.Contains(x.Id));
            foreach (var barcode in barcodes)
            {
                barcode.IsBarcodeReprint = false;
                barcode.BarcodeDate = null;

                _context.PKBarcode.Update(barcode);
            }

            _context.Save();
        }

        public List<BarcodeDataEntrySubject> GetSubjectDetails(int siteId, int templateId, BarcodeGenerationType generationType)
        {

            var project = _context.Project.Find(siteId).ParentProjectId; 
            var projectdata = _context.ScreeningTemplate
                                         .Include(x => x.ScreeningVisit)
                                         .ThenInclude(x => x.ScreeningEntry)
                                         .ThenInclude(x => x.Attendance)
                                         .ThenInclude(x => x.Volunteer)
                                         .Where(x => x.ProjectDesignTemplateId == templateId && x.Status < Helper.ScreeningTemplateStatus.Submitted && x.ScreeningVisit.ScreeningEntry.ProjectId== project && x.DeletedDate == null).Select(x =>
                                       new BarcodeDataEntrySubject
                                       {
                                           VolunteerNo = x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.VolunteerNo + " " + x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,
                                           AttendanceId = (int)x.ScreeningVisit.ScreeningEntry.AttendanceId,
                                           ScreeningEntryId = x.ScreeningVisit.ScreeningEntryId,
                                           ProjectDesignVisitId = x.ScreeningVisit.ProjectDesignVisitId,
                                           ProjectAttendanceBarcodeString = _context.AttendanceBarcodeGenerate.Where(r => r.AttendanceId == x.ScreeningVisit.ScreeningEntry.AttendanceId && r.DeletedDate == null).FirstOrDefault().BarcodeString,
                                           ProjectDesignTemplateId = x.ProjectDesignTemplateId,
                                           ScreeningTemplateId = x.Id,
                                           Status = x.Status,
                                           ScheduleDate = x.ScheduleDate,
                                           VolunteerId= (int)x.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId
                                           //BarcodeString = BarcodeString(siteId, templateId, (int)x.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId, generationType),
                                       }).ToList();

            projectdata.ForEach(x =>
            {
                x.BarcodeString = BarcodeString(siteId, x.ProjectDesignTemplateId, x.VolunteerId, generationType);
                if (generationType == BarcodeGenerationType.PkBarocde)
                    x.PKBarcodeOption = All.Where(r => r.SiteId == siteId && r.TemplateId == x.ProjectDesignTemplateId && r.DeletedDate == null && r.VolunteerId == x.VolunteerId).FirstOrDefault().PKBarcodeOption;
                else if (generationType == BarcodeGenerationType.SampleBarcode)
                {
                    x.PKBarcodeOption = _context.SampleBarcode.Where(r => r.SiteId == siteId && r.TemplateId == x.ProjectDesignTemplateId && r.DeletedDate == null && r.VolunteerId == x.VolunteerId).FirstOrDefault().PKBarcodeOption;
                    x.ProjectAttendanceBarcodeString = _context.PKBarcode.Where(r => r.SiteId == siteId && r.VisitId==x.ProjectDesignVisitId && r.DeletedDate == null && r.VolunteerId == x.VolunteerId).FirstOrDefault().BarcodeString;
                }
                else if (generationType == BarcodeGenerationType.DosingBarcode)
                    x.PKBarcodeOption = _context.DossingBarcode.Where(r => r.SiteId == siteId && r.TemplateId == x.ProjectDesignTemplateId && r.DeletedDate == null && r.VolunteerId == x.VolunteerId).FirstOrDefault().PKBarcodeOption;
            });

            return projectdata.ToList();
        }

        string BarcodeString(int siteId, int templateId, int VolunteerId, BarcodeGenerationType generationType)
        {
            if (generationType == BarcodeGenerationType.PkBarocde)
                return All.Where(r => r.SiteId == siteId && r.TemplateId == templateId && r.DeletedDate == null && r.VolunteerId == VolunteerId).FirstOrDefault().BarcodeString;
            else if (generationType == BarcodeGenerationType.SampleBarcode)
                return _context.SampleBarcode.Where(r => r.SiteId == siteId && r.TemplateId == templateId && r.DeletedDate == null && r.VolunteerId == VolunteerId).FirstOrDefault().BarcodeString;
            else if (generationType == BarcodeGenerationType.DosingBarcode)
                return _context.DossingBarcode.Where(r => r.SiteId == siteId && r.TemplateId == templateId && r.DeletedDate == null && r.VolunteerId == VolunteerId).FirstOrDefault().BarcodeString;
            else
                return "";
        }
    }
}
