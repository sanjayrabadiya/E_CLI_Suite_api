using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;

        public BarcodeConfigRepository(IGSCContext context, IMapper mapper)
            : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<BarcodeConfigGridDto> GetBarcodeConfig(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                  ProjectTo<BarcodeConfigGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public BarcodeConfig GetBarcodeConfig(int barcodeTypeId)
        {
            var barcode = _context.BarcodeConfig.Where(t => t.BarcodeTypeId == barcodeTypeId)
                .AsNoTracking().FirstOrDefault();

            return barcode;
        }

        public List<BarcodeConfigDto> GetBarcodeConfigById(int id)
        {
            var result = All.Include(x => x.AppScreen)
                .Include(x => x.BarcodeDisplayInfo)
                .ThenInclude(x => x.TableFieldName)
                .Where(x => x.Id == id && x.DeletedBy == null)
                .Select(x => new BarcodeConfigDto
                {
                    Id = x.Id,
                    AppScreenId = x.AppScreenId,
                    ModuleName = x.AppScreen.ScreenName,
                    PageId = x.PageId,
                    PageName = "",
                    BarcodeTypeId = x.BarcodeTypeId,
                    BarcodeTypeName = x.BarcodeType.BarcodeTypeName,
                    DisplayValue = x.DisplayValue,
                    FontSize = x.FontSize,
                    DisplayInformationLength = x.DisplayInformationLength,
                    BarcodeCombinationList = x.BarcodeCombination.Where(x => x.DeletedDate == null).Select(s => (int)s.TableFieldNameId).ToList(),                  
                    BarcodeDisplayInfo = x.BarcodeDisplayInfo.Where(t => t.BarcodConfigId == x.Id && t.DeletedBy == null).OrderByDescending(s => s.Id).ToList()
                }).OrderByDescending(x => x.Id).ToList();
            return result;
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
                    DisplayValue = c.DisplayValue,
                    FontSize = c.FontSize,
                }).FirstOrDefault();
        }
    }
}