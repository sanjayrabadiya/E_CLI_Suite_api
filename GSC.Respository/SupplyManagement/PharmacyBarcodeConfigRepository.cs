using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Shared.Extension;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Barcode
{
    public class PharmacyBarcodeConfigRepository : GenericRespository<PharmacyBarcodeConfig>, IPharmacyBarcodeConfigRepository
    {
      
        private readonly IMapper _mapper;

        public PharmacyBarcodeConfigRepository(IGSCContext context,IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
        }

        public List<PharmacyBarcodeConfigGridDto> GetBarcodeConfig(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                  ProjectTo<PharmacyBarcodeConfigGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<PharmacyBarcodeConfigDto> GetBarcodeConfigById(int id)
        {

            var result = All
                .Include(x => x.BarcodeDisplayInfo)
                .ThenInclude(x => x.TableFieldName)
                .Where(x => x.Id == id && x.DeletedBy == null)
                .Select(x => new PharmacyBarcodeConfigDto
                {
                    Id = x.Id,
                    ProjectId = x.ProjectId,
                    SiteId = x.SiteId,
                    BarcodeModuleType = x.BarcodeModuleType,
                    BarcodeType = x.BarcodeType,
                    BarcodeTypeName = x.BarcodeType.GetDescription(),
                    DisplayValue = x.DisplayValue,
                    FontSize = x.FontSize,
                    DisplayInformationLength = x.DisplayInformationLength,
                    BarcodeDisplayInfo = x.BarcodeDisplayInfo.Where(t => t.PharmacyBarcodeConfigId == x.Id && t.DeletedBy == null).OrderByDescending(s => s.Id).ToList(),

                }).OrderByDescending(x => x.Id).ToList();
            return result;
        }

        public string GenerateBarcodeString(int barcodeTypeId)
        {
            var result = FindBy(x => (int)x.BarcodeType == barcodeTypeId && x.DeletedDate == null).FirstOrDefault();
            if (result == null)
                return null;

            Update(result);
            return "";
        }

        public PharmacyBarcodeConfigDto GenerateBarcodeConfig(int barcodeTypeId)
        {
            return FindBy(x => (int)x.BarcodeType == barcodeTypeId && x.DeletedDate == null).Select(c =>
                new PharmacyBarcodeConfigDto
                {
                    Id = c.Id,
                    IsDeleted = c.DeletedDate != null,
                    BarcodeType = c.BarcodeType,
                    BarcodeTypeName = c.BarcodeType.GetDescription(),
                    DisplayValue = c.DisplayValue,
                    FontSize = c.FontSize,
                }).FirstOrDefault();
        }

        public string ValidateBarcodeConfig(PharmacyBarcodeConfig pharmacyBarcodeConfig)
        {
            if (pharmacyBarcodeConfig.Id > 0)
            {
                if (All.Any(s => s.DeletedDate == null && s.Id != pharmacyBarcodeConfig.Id && s.BarcodeModuleType == pharmacyBarcodeConfig.BarcodeModuleType && s.ProjectId == pharmacyBarcodeConfig.ProjectId))
                {
                    return "record already exist!";
                }
            }
            else
            {
                if (All.Any(s => s.DeletedDate == null && s.BarcodeModuleType == pharmacyBarcodeConfig.BarcodeModuleType && s.ProjectId == pharmacyBarcodeConfig.ProjectId))
                {
                    return "record already added!";
                }
            }

            return "";
        }
    }
}