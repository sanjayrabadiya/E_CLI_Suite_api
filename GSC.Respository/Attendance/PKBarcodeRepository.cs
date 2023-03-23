using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Attendance;
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
    public class PKBarcodeRepository : GenericRespository<PKBarcode>, IPKBarcodeRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IMapper _mapper;
        public PKBarcodeRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IVolunteerRepository volunteerRepository, IMapper mapper) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _volunteerRepository = volunteerRepository;
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

            barcode = "PK" + volunteer.RandomizationNumber + peroid.DisplayName + template.DesignOrder;
            return barcode;
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
    }
}
