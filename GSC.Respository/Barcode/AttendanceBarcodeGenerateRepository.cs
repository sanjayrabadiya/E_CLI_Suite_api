using GSC.Common.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Barcode
{
    public class AttendanceBarcodeGenerateRepository : GenericRespository<AttendanceBarcodeGenerate>, IAttendanceBarcodeGenerateRepository
    {
        private readonly IGSCContext _context;
        public AttendanceBarcodeGenerateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IBarcodeDisplayInfoRepository barcodeDisplayInfoRepository)
            : base(context)
        {
            _context = context;
        }

        public List<AttendanceBarcodeGenerateGridDto> GetBarcodeDetail(int attendanceId)
        {
            var barcodeGenerator = _context.AttendanceBarcodeGenerate.Include(x => x.BarcodeConfig).ThenInclude(x => x.BarcodeDisplayInfo)
                .ThenInclude(x => x.TableFieldName)
                .Include(tiers => tiers.BarcodeConfig).ThenInclude(x => x.BarcodeType)
                .Where(x => x.DeletedBy == null && x.AttendanceId == attendanceId).OrderByDescending(m => m.Id).ToList();

            List<AttendanceBarcodeGenerateGridDto> lst = new List<AttendanceBarcodeGenerateGridDto>();

            foreach (var item in barcodeGenerator)
            {
                var sublst = new AttendanceBarcodeGenerateGridDto
                {
                    BarcodeString = item.BarcodeString,
                    IsRePrint = item.IsRePrint,
                    BarcodeType = item.BarcodeConfig.BarcodeType.BarcodeTypeName,
                    DisplayValue = item.BarcodeConfig.DisplayValue,
                    DisplayInformationLength = item.BarcodeConfig.DisplayInformationLength,
                    FontSize = item.BarcodeConfig.FontSize,
                    BarcodeDisplayInfo = new BarcodeDisplayInfo[item.BarcodeConfig.BarcodeDisplayInfo.Count()]
                };
                int index = 0;
                foreach (var subitem in item.BarcodeConfig.BarcodeDisplayInfo)
                {
                    sublst.BarcodeDisplayInfo[index] = new BarcodeDisplayInfo();
                    sublst.BarcodeDisplayInfo[index].Alignment = subitem.Alignment;
                    sublst.BarcodeDisplayInfo[index].DisplayInformation = GetColumnValue(item.AttendanceId, subitem.TableFieldName.FieldName);
                    sublst.BarcodeDisplayInfo[index].OrderNumber = subitem.OrderNumber;
                    sublst.BarcodeDisplayInfo[index].IsSameLine = subitem.IsSameLine;
                    index++;
                }
                lst.Add(sublst);
            }
            return lst;
        }

        public List<AttendanceBarcodeGenerateGridDto> GetReprintBarcodeGenerateData(int[] Ids)
        {
            var barcodeGenerator = _context.AttendanceBarcodeGenerate.Include(x => x.BarcodeConfig).ThenInclude(x => x.BarcodeDisplayInfo)
                .ThenInclude(x => x.TableFieldName)
                .Include(tiers => tiers.BarcodeConfig).ThenInclude(x => x.BarcodeType)
                .Where(x => x.DeletedBy == null && Ids.Contains(x.Id)).OrderByDescending(m => m.Id).ToList();

            List<AttendanceBarcodeGenerateGridDto> lst = new List<AttendanceBarcodeGenerateGridDto>();
            
            foreach (var item in barcodeGenerator)
            {
                var sublst = new AttendanceBarcodeGenerateGridDto
                {
                    BarcodeString = item.BarcodeString,
                    IsRePrint = item.IsRePrint,
                    BarcodeType = item.BarcodeConfig.BarcodeType.BarcodeTypeName.ToString(),
                    DisplayValue = item.BarcodeConfig.DisplayValue,
                    DisplayInformationLength = item.BarcodeConfig.DisplayInformationLength,
                    FontSize = item.BarcodeConfig.FontSize,
                    BarcodeDisplayInfo = new BarcodeDisplayInfo[item.BarcodeConfig.BarcodeDisplayInfo.Count()]
                };
                int index = 0;
                foreach (var subitem in item.BarcodeConfig.BarcodeDisplayInfo)
                {
                    sublst.BarcodeDisplayInfo[index] = new BarcodeDisplayInfo();
                    sublst.BarcodeDisplayInfo[index].Alignment = subitem.Alignment;
                    sublst.BarcodeDisplayInfo[index].DisplayInformation = GetColumnValue(item.AttendanceId, subitem.TableFieldName.FieldName);
                    sublst.BarcodeDisplayInfo[index].OrderNumber = subitem.OrderNumber;
                    sublst.BarcodeDisplayInfo[index].IsSameLine = subitem.IsSameLine;
                    index++;
                }
                lst.Add(sublst);
            }
            return lst;
        }

        string GetColumnValue(int id, string ColumnName)
        {
            var tableRepository = _context.Attendance.Where(x => x.Id == id).Select(e => e).FirstOrDefault();

            if (tableRepository == null) return "";


            if (ColumnName == "ProjectId")
                return _context.Project.Find(tableRepository.ProjectId).ProjectCode;

            if (ColumnName == "SiteId")
            {
                return _context.Project.Find(tableRepository.SiteId).ProjectCode;
            }

            if (ColumnName == "VolunteerId")
                return _context.Volunteer.Find(tableRepository.VolunteerId).VolunteerNo;

            var _value = tableRepository.GetType().GetProperties().Where(a => a.Name == ColumnName).Select(p => p.GetValue(tableRepository, null)).FirstOrDefault();

            return _value != null ? _value.ToString() : "";
        }

        public string GetBarcodeString(int id)
        {
            var attendanceData = _context.Attendance.Where(x => x.Id == id).FirstOrDefault();
            if (attendanceData == null) return "";
            var volunteerNo = _context.Volunteer.Find(attendanceData.VolunteerId).VolunteerNo;
            var projectCode = _context.Project.Find(attendanceData.ProjectId).ProjectCode;
            
            return projectCode+volunteerNo;
        }

    }
}
