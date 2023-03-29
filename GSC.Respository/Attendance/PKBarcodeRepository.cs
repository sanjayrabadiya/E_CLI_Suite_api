using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
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
                    barcode = barcode + "PK" + volunteer.RandomizationNumber + peroid.DisplayName + template.DesignOrder + "0" + i + ",";
                }
            }
            else
            {
                barcode = "PK" + volunteer.RandomizationNumber + peroid.DisplayName + template.DesignOrder;
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

        public List<BarcodeDataEntrySubject> GetPkSubjectDetails(int siteId, int templateId)
        {
            var pkDetails = All.Where(r => r.SiteId == siteId && r.TemplateId == templateId).ToList();
          //  var AttendanceDetails = _attendanceRepository.All.Where(r => r.ProjectId == siteId).Select(x=>x.Id).ToList();

           var projectdata= _context.ScreeningTemplate
                                        .Include(x => x.ScreeningVisit)
                                        .ThenInclude(x => x.ScreeningEntry)
                                        .ThenInclude(x => x.Attendance)
                                        .ThenInclude(x=>x.Volunteer)
                                        .Where(x => x.ProjectDesignTemplateId == templateId && x.Status < Helper.ScreeningTemplateStatus.Submitted).Select(x =>
                                      new BarcodeDataEntrySubject
                                      {
                                          VolunteerNo = x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.VolunteerNo +" "+ x.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,
                                          AttendanceId = (int)x.ScreeningVisit.ScreeningEntry.AttendanceId,
                                          ProjectAttendanceBarcodeString = _context.AttendanceBarcodeGenerate.Where(r=>r.AttendanceId== x.ScreeningVisit.ScreeningEntry.AttendanceId && r.DeletedDate==null).FirstOrDefault().BarcodeString,
                                          ProjectDesignTemplateId = x.ProjectDesignTemplateId,
                                          ScreeningTemplateId = x.Id,
                                          Status=x.Status,
                                          ScheduleDate=x.ScheduleDate,
                                          BarcodeString = All.Where(r => r.SiteId == siteId && r.TemplateId == templateId && r.DeletedDate == null && r.VolunteerId == x.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId).FirstOrDefault().BarcodeString //pkDetails.Where(t=>t.VolunteerId == x.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId).FirstOrDefault().BarcodeString,
                                      }).ToList();

            return projectdata.ToList();
        }
    }
}
