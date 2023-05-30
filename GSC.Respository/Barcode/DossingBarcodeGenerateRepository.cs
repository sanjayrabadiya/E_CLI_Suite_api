﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Barcode
{
    public class DossingBarcodeGenerateRepository : GenericRespository<DossingBarcodeGenerate>, IDossingBarcodeGenerateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IBarcodeDisplayInfoRepository _barcodeDisplayInfoRepository;
        public DossingBarcodeGenerateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IBarcodeDisplayInfoRepository barcodeDisplayInfoRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _barcodeDisplayInfoRepository = barcodeDisplayInfoRepository;
        }

        public List<DossingBarcodeGenerateGridDto> GetBarcodeDetail(int DossingBarcodeId)
        {
            var barcodeGenerator = _context.DossingBarcodeGenerate.Include(x => x.BarcodeConfig).ThenInclude(x => x.BarcodeDisplayInfo)
                .ThenInclude(x => x.TableFieldName)
                .Include(tiers => tiers.BarcodeConfig).ThenInclude(x => x.BarcodeType)
                .Where(x => x.DeletedBy == null && x.DossingBarcodeId == DossingBarcodeId).OrderByDescending(m => m.Id).ToList();

            List<DossingBarcodeGenerateGridDto> lst = new List<DossingBarcodeGenerateGridDto>();

            foreach (var item in barcodeGenerator)
            {
                var sublst = new DossingBarcodeGenerateGridDto
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
                    sublst.BarcodeDisplayInfo[index].DisplayInformation = GetColumnValue(item.DossingBarcodeId, "DossingBarcode", subitem.TableFieldName.FieldName);
                    sublst.BarcodeDisplayInfo[index].OrderNumber = subitem.OrderNumber;
                    sublst.BarcodeDisplayInfo[index].IsSameLine = subitem.IsSameLine;
                    index++;
                }
                lst.Add(sublst);
            }
            return lst;
        }

        public List<DossingBarcodeGenerateGridDto> GetReprintBarcodeGenerateData(int[] Ids)
        {
            var barcodeGenerator = _context.DossingBarcodeGenerate.Include(x => x.BarcodeConfig).ThenInclude(x => x.BarcodeDisplayInfo)
                .ThenInclude(x => x.TableFieldName)
                .Include(tiers => tiers.BarcodeConfig).ThenInclude(x => x.BarcodeType)
                .Where(x => x.DeletedBy == null && Ids.Contains(x.Id)).OrderByDescending(m => m.Id).ToList();

            List<DossingBarcodeGenerateGridDto> lst = new List<DossingBarcodeGenerateGridDto>();

            foreach (var item in barcodeGenerator)
            {
                var sublst = new DossingBarcodeGenerateGridDto
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
                    sublst.BarcodeDisplayInfo[index].DisplayInformation = GetColumnValue(item.DossingBarcodeId, "DossingBarcode", subitem.TableFieldName.FieldName);
                    sublst.BarcodeDisplayInfo[index].OrderNumber = subitem.OrderNumber;
                    sublst.BarcodeDisplayInfo[index].IsSameLine = subitem.IsSameLine;
                    index++;
                }
                lst.Add(sublst);
            }
            return lst;
        }

        string GetColumnValue(int id, string TableName, string ColumnName)
        {
            var tableRepository = _context.DossingBarcode.Where(x => x.Id == id).Select(e => e).FirstOrDefault();

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

        public string GetBarcodeString(DossingBarcode DossingBarcode, int number)
        {
            string barcode = "";
            var volunteer = _context.Volunteer.Find(DossingBarcode.VolunteerId);
            var peroid = _context.ProjectDesignVisit.Where(x => x.Id == DossingBarcode.VisitId).Include(x => x.ProjectDesignPeriod).FirstOrDefault().ProjectDesignPeriod;
            var template = _context.ProjectDesignTemplate.FirstOrDefault(x => x.Id == DossingBarcode.TemplateId);


            var randomization = _context.SupplyManagementUploadFileVisit.Include(x => x.SupplyManagementUploadFileDetail)
              .Where(q => q.ProjectDesignVisitId == DossingBarcode.VisitId &&
              q.SupplyManagementUploadFileDetail.RandomizationNo == Convert.ToInt32(volunteer.RandomizationNumber)).FirstOrDefault();

            if (randomization != null)
            {
                var content = randomization.SupplyManagementUploadFileDetail.TreatmentType;
                if (content.Contains(','))
                {
                    barcode = "DB" + volunteer.RandomizationNumber + peroid.DisplayName + template.TemplateCode + content[0];
                }
                else
                {
                    barcode = "DB" + volunteer.RandomizationNumber + peroid.DisplayName + template.TemplateCode + content;
                }
            }

            return barcode;
        }
    }
}