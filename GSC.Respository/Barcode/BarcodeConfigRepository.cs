using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Barcode
{
    public class BarcodeConfigRepository : GenericRespository<BarcodeConfig>, IBarcodeConfigRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public BarcodeConfigRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
           _context = context;
        }

        public List<BarcodeConfigDto> GetBarcodeConfig(bool isDeleted)
        {
            var barcodeconfig = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).Select(c => new BarcodeConfigDto
                    {
                        Id = c.Id,
                        IsDeleted = c.DeletedDate != null,
                        BarcodeTypeId = c.BarcodeTypeId,
                        BarcodeTypeName = c.BarcodeType.BarcodeTypeName,
                        SubjectNo = c.SubjectNo,
                        ProjectNo = c.ProjectNo,
                        Period = c.Period,
                        VolunteerId = c.VolunteerId,
                        RandomizationNo = c.RandomizationNo,
                        BarcodeFor = c.BarcodeFor,
                        BarcodeForName = c.BarcodeFor == 0 ? "" : ((BarcodeFor)c.BarcodeFor).GetDescription(),
                        Width = c.Width,
                        Height = c.Height,
                        DisplayValue = c.DisplayValue,
                        FontSize = c.FontSize,
                        TextMargin = c.TextMargin,
                        MarginTop = c.MarginTop,
                        MarginBottom = c.MarginBottom,
                        MarginLeft = c.MarginLeft,
                        MarginRight = c.MarginRight
                    }
                )
                .ToList();
            return barcodeconfig;
        }

        public string GenerateBarcodeString(int barcodeTypeId)
        {
            var result = FindBy(x => x.BarcodeTypeId == barcodeTypeId && x.DeletedDate == null).FirstOrDefault();
            if (result == null)
                return null;

            Update(result);
            return "";
        }

        public BarcodeConfigDto GenerateBarcodeConfig(int barcodeTypeId)
        {
            return FindBy(x => x.BarcodeTypeId == barcodeTypeId && x.DeletedDate == null).Select(c =>
                new BarcodeConfigDto
                {
                    Id = c.Id,
                    IsDeleted = c.DeletedDate != null,
                    BarcodeTypeId = c.BarcodeTypeId,
                    BarcodeTypeName = c.BarcodeType.BarcodeTypeName,
                    SubjectNo = c.SubjectNo,
                    ProjectNo = c.ProjectNo,
                    Period = c.Period,
                    VolunteerId = c.VolunteerId,
                    RandomizationNo = c.RandomizationNo,
                    BarcodeFor = c.BarcodeFor,
                    BarcodeForName = c.BarcodeFor == 0 ? "" : ((BarcodeFor)c.BarcodeFor).GetDescription(),
                    Width = c.Width,
                    Height = c.Height,
                    DisplayValue = c.DisplayValue,
                    FontSize = c.FontSize,
                    TextMargin = c.TextMargin,
                    MarginTop = c.MarginTop,
                    MarginBottom = c.MarginBottom,
                    MarginLeft = c.MarginLeft,
                    MarginRight = c.MarginRight
                }).FirstOrDefault();
        }

        public BarcodeConfig GetBarcodeConfig(int barcodeTypeId)
        {
            var barcode = _context.BarcodeConfig.Where(t => t.BarcodeTypeId == barcodeTypeId)
                .AsNoTracking().FirstOrDefault();

            return barcode;
        }
    }
}