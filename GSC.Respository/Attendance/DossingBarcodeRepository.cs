﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Barcode;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Volunteer;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Attendance
{
    public class DossingBarcodeRepository : GenericRespository<DossingBarcode>, IDossingBarcodeRepository
    {
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        public DossingBarcodeRepository(IGSCContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<DossingBarcodeGridDto> GetDossingBarcodeList(bool isDeleted)
        {
            var list = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<DossingBarcodeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            list.ForEach(x =>
            {
                x.isBarcodeGenerated = _context.DossingBarcodeGenerate.Any(t => t.DossingBarcodeId == x.Id && t.DeletedBy == null);
            });

            return list;
        }

        public string Duplicate(DossingBarcodeDto objSave)
        {
            if (All.Any(
                x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId
                && x.VisitId == objSave.VisitId
                && x.TemplateId == objSave.TemplateId
                && x.SiteId == objSave.SiteId
                && x.VolunteerId == objSave.VolunteerId
                && x.DeletedDate == null))
                return "Duplicate dossing";
            return "";
        }

        public string GenerateBarcodeString(DossingBarcode objSave)
        {
            string barcode = "";
            var volunteer = _context.Volunteer.FirstOrDefault(x => x.Id == objSave.VolunteerId);
            var peroid = _context.ProjectDesignVisit.Where(x => x.Id == objSave.VisitId).Include(x => x.ProjectDesignPeriod).First().ProjectDesignPeriod;
            var template = _context.ProjectDesignTemplate.First(x => x.Id == objSave.TemplateId);

            var randomization = _context.SupplyManagementUploadFileVisit.Include(x => x.SupplyManagementUploadFileDetail)
                .Where(q => q.ProjectDesignVisitId == objSave.VisitId &&
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

        public List<DossingBarcodeGridDto> UpdateBarcode(List<int> ids)
        {
            var barcodes = _context.DossingBarcode.Where(x => ids.Contains(x.Id)).ToList();
            List<int> barcodeIds = new List<int>();
            foreach (var barcode in barcodes)
            {
                var strBarcode = GenerateBarcodeString(barcode);
                if (!string.IsNullOrEmpty(strBarcode))
                {
                    barcode.BarcodeString = strBarcode;
                    barcode.BarcodeDate = DateTime.Now;
                    _context.DossingBarcode.Update(barcode);
                    barcodeIds.Add(barcode.Id);
                }
            }

            _context.Save();


            var data = All.Include(x => x.BarcodeType)
                .Include(x => x.Project)
                .Include(x => x.Site.ManageSite)
                .Where(x => barcodeIds.Contains(x.Id));
            var gridDto = _mapper.Map<List<DossingBarcodeGridDto>>(data);
            return gridDto;
        }

        public void BarcodeReprint(List<int> ids)
        {
            var barcodes = _context.DossingBarcode.Where(x => ids.Contains(x.Id));
            foreach (var barcode in barcodes)
            {
                barcode.IsBarcodeReprint = true;

                _context.DossingBarcode.Update(barcode);
            }

            _context.Save();
        }

        public void DeleteBarcode(List<int> ids)
        {
            var barcodes = _context.DossingBarcode.Where(x => ids.Contains(x.Id));
            foreach (var barcode in barcodes)
            {
                barcode.IsBarcodeReprint = false;
                barcode.BarcodeDate = null;

                _context.DossingBarcode.Update(barcode);
            }

            _context.Save();
        }
    }
}
