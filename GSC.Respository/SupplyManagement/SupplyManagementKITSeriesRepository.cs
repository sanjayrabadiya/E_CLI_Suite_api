﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementKITSeriesRepository : GenericRespository<SupplyManagementKITSeries>, ISupplyManagementKITSeriesRepository
    {

        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public SupplyManagementKITSeriesRepository(IGSCContext context,
        IMapper mapper, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {


            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;

        }

        public void AddKitSeriesVisitDetail(SupplyManagementKITSeriesDto data)
        {
            if (data.SupplyManagementKITSeriesDetail != null && data.SupplyManagementKITSeriesDetail.Count() > 0)
            {
                foreach (var item in data.SupplyManagementKITSeriesDetail)
                {
                    SupplyManagementKITSeriesDetail obj = new SupplyManagementKITSeriesDetail();
                    obj.SupplyManagementKITSeriesId = data.Id;
                    obj.NoOfImp = item.NoOfImp;
                    obj.NoofPatient = data.NoofPatient;
                    obj.ProjectDesignVisitId = item.ProjectDesignVisitId;
                    obj.PharmacyStudyProductTypeId = item.PharmacyStudyProductTypeId;
                    obj.Days = item.Days;
                    obj.ProductReceiptId = item.ProductReceiptId;
                    obj.TotalUnits = (item.NoOfImp * data.NoofPatient);
                    _context.SupplyManagementKITSeriesDetail.Add(obj);

                }
                SupplyManagementKITSeriesDetailHistory history = new SupplyManagementKITSeriesDetailHistory();
                history.SupplyManagementKITSeriesId = data.Id;
                history.Status = KitStatus.AllocationPending;
                history.RoleId = _jwtTokenAccesser.RoleId;
                _context.SupplyManagementKITSeriesDetailHistory.Add(history);

                var filedetail = _context.SupplyManagementUploadFileDetail.Include(x => x.SupplyManagementUploadFile).Where(x => x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve &&
                    x.SupplyManagementUploadFile.ProjectId == data.ProjectId && x.KitNo == data.KitNo && x.SupplyManagementKITSeriesId == null).FirstOrDefault();
                if (filedetail != null)
                {
                    filedetail.SupplyManagementKITSeriesId = data.Id;
                    _context.SupplyManagementUploadFileDetail.Update(filedetail);

                }

                //_context.Save();
            }
        }

        public string GenerateKitSequenceNo(SupplyManagementKitNumberSettings kitsettings, int noseriese, SupplyManagementKITSeriesDto supplyManagementKITSeriesDto)
        {
            var isnotexist = false;
            if (kitsettings.IsUploadWithKit)
            {
                var uploadedkits = _context.SupplyManagementUploadFileDetail.Include(s => s.SupplyManagementUploadFile).Where(s => s.SupplyManagementUploadFile.ProjectId == supplyManagementKITSeriesDto.ProjectId
                                    && s.TreatmentType.ToLower() == supplyManagementKITSeriesDto.TreatmentType.ToLower() && s.SupplyManagementKITSeriesId == null && s.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve && s.DeletedDate == null)
                    .OrderBy(x => x.Id).FirstOrDefault();
                if (uploadedkits != null)
                {
                    return uploadedkits.KitNo;
                }
            }
            else
            {
                string kitno1 = string.Empty;
                while (!isnotexist)
                {
                    var kitno = kitsettings.Prefix + kitsettings.KitNoseries.ToString().PadLeft((int)kitsettings.KitNumberLength, '0');
                    if (!string.IsNullOrEmpty(kitno))
                    {
                        ++kitsettings.KitNoseries;
                        _context.SupplyManagementKitNumberSettings.Update(kitsettings);
                        _context.Save();
                        var data = _context.SupplyManagementKITSeries.Where(x => x.KitNo == kitno && x.DeletedDate == null).FirstOrDefault();
                        if (data == null)
                        {
                            isnotexist = true;
                            kitno1 = kitno;
                            break;

                        }
                    }
                }
                return kitno1;
            }

            return "";
        }
        public string CheckExpiryDateSequenceWise(SupplyManagementKITSeriesDto supplyManagementKITSeriesDto)
        {
            if (supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail != null && supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail.Count() > 0)
            {
                var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt)
                    .ToList().Where(x => supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail.Any(z => z.ProductReceiptId == x.ProductReceiptId)).OrderBy(a => a.RetestExpiryDate).FirstOrDefault();
                if (productreciept == null)
                    return "Product receipt not found";

                var days = supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail.Sum(x => x.Days);
                var currentdate = DateTime.Now.Date;
                var date = currentdate.AddDays(days);
                if (Convert.ToDateTime(productreciept.RetestExpiryDate).Date < date.Date)
                {
                    return "Product is expired";
                }
            }
            return "";
        }
        public DateTime GetExpiryDateSequenceWise(SupplyManagementKITSeriesDto supplyManagementKITSeriesDto)
        {
            if (supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail != null && supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail.Count() > 0)
            {
                var days = supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail.Sum(x => x.Days);
                var currentdate = DateTime.Now.Date;
                var date = currentdate.AddDays(days);
                return date;
            }
            return new DateTime();
        }
    }
}
