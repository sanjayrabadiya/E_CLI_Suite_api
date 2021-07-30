using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class ProductVerificationRepository : GenericRespository<ProductVerification>, IProductVerificationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public ProductVerificationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<ProductVerificationDto> GetProductVerificationList(int productReceiptId)
        {
            return All.Where(x => (x.DeletedDate == null) && x.ProductReceiptId == productReceiptId).
                   ProjectTo<ProductVerificationDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<ProductVerificationGridDto> GetProductVerification(int ProjectId, bool isDeleted)
        {
            var dtolist = (from productverification in _context.ProductVerification.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null)
                           join productReceipt in _context.ProductReceipt.Where(t => t.DeletedDate == null && t.ProjectId == ProjectId) on productverification.ProductReceiptId equals productReceipt.Id
                           join productverificationDetail in _context.ProductVerificationDetail.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null) on productverification.Id equals productverificationDetail.ProductVerificationId
                           select new ProductVerificationGridDto
                           {
                               Id = productverification.Id,
                               ProductReceiptId = productverification.ProductReceiptId,
                               StudyCode = productReceipt.Project.ProjectCode,
                               StorageArea = productReceipt.CentralDepot.StorageArea,
                               ProductType = productReceipt.PharmacyStudyProductType.ProductType.ProductTypeName,
                               ProductName = productReceipt.ProductName,
                               BatchLot = productverification.BatchLotId.GetDescription(),
                               BatchLotNumber = productverification.BatchLotNumber,
                               ManufactureBy = productverification.ManufactureBy,
                               MarketedBy = productverification.MarketedBy,
                               LabelClaim = productverification.LabelClaim,
                               DistributedBy = productverification.DistributedBy,
                               PackDesc = productverification.PackDesc,
                               MarketAuthorization = productverification.MarketAuthorization,
                               MfgDate = productverification.MfgDate,
                               RetestExpiry = productverification.RetestExpiryId.GetDescription(),
                               RetestExpiryDate = productverification.RetestExpiryDate,
                               IsDeleted = productverification.DeletedDate != null,
                               CreatedDate = productverification.CreatedDate,
                               ModifiedDate = productverification.ModifiedDate,
                               DeletedDate = productverification.DeletedDate,
                               CreatedByUser = productverification.CreatedByUser.UserName,
                               ModifiedByUser = productverification.ModifiedByUser.UserName,
                               DeletedByUser = productverification.DeletedByUser.UserName,
                               QuantityVerification = productverificationDetail.QuantityVerification,
                               Description = productverificationDetail.Description,
                               Remarks = productverificationDetail.Remarks,
                               IsAssayRequirement = productverificationDetail.IsAssayRequirement,
                               IsRetentionConfirm = productverificationDetail.IsRetentionConfirm,
                               IsSodiumVaporLamp = productverificationDetail.IsSodiumVaporLamp,
                               IsProductDescription = productverificationDetail.IsProductDescription,
                               NumberOfBox = productverificationDetail.NumberOfBox,
                               NumberOfQty = productverificationDetail.NumberOfQty,
                               ReceivedQty = productverificationDetail.ReceivedQty,
                               IsConditionProduct = productverificationDetail.IsConditionProduct

                           }).ToList().OrderByDescending(x => x.Id).ToList();

            return dtolist;
        }

        public List<ProductVerificationGridDto> GetProductVerificationSummary(int productReceiptId)
        {
            var dtolist = (from productverification in _context.ProductVerification.Where(x => x.DeletedDate == null)
                           join productReceipt in _context.ProductReceipt.Where(t => t.DeletedDate == null) on productverification.ProductReceiptId equals productReceipt.Id
                           join productverificationDetail in _context.ProductVerificationDetail.Where(x => x.DeletedDate == null) on productverification.Id equals productverificationDetail.ProductVerificationId
                           join project in _context.Project.Where(t => t.DeletedDate == null) on productReceipt.ProjectId equals project.Id
                           where productverification.ProductReceiptId == productReceiptId
                           select new ProductVerificationGridDto
                           {
                               Id = productverification.Id,
                               ClientId = project.ClientId,
                               ProductReceiptId = productverification.ProductReceiptId,
                               StudyCode = productReceipt.Project.ProjectCode,
                               StorageArea = productReceipt.CentralDepot.StorageArea,
                               ProductType = productReceipt.PharmacyStudyProductType.ProductType.ProductTypeName,
                               ProductName = productReceipt.ProductName,
                               BatchLot = productverification.BatchLotId.GetDescription(),
                               BatchLotNumber = productverification.BatchLotNumber,
                               ManufactureBy = productverification.ManufactureBy,
                               MarketedBy = productverification.MarketedBy,
                               LabelClaim = productverification.LabelClaim,
                               DistributedBy = productverification.DistributedBy,
                               PackDesc = productverification.PackDesc,
                               MarketAuthorization = productverification.MarketAuthorization,
                               MfgDate = productverification.MfgDate,
                               RetestExpiry = productverification.RetestExpiryId.GetDescription(),
                               RetestExpiryDate = productverification.RetestExpiryDate,
                               IsDeleted = productverification.DeletedDate != null,
                               CreatedDate = productverification.CreatedDate,
                               ModifiedDate = productverification.ModifiedDate,
                               DeletedDate = productverification.DeletedDate,
                               CreatedByUser = productverification.CreatedByUser.UserName,
                               ModifiedByUser = productverification.ModifiedByUser.UserName,
                               DeletedByUser = productverification.DeletedByUser.UserName,
                               QuantityVerification = productverificationDetail.QuantityVerification,
                               Description = productverificationDetail.Description,
                               Remarks = productverificationDetail.Remarks,
                               IsAssayRequirement = productverificationDetail.IsAssayRequirement,
                               IsRetentionConfirm = productverificationDetail.IsRetentionConfirm,
                               IsSodiumVaporLamp = productverificationDetail.IsSodiumVaporLamp,
                               IsProductDescription = productverificationDetail.IsProductDescription,
                               NumberOfBox = productverificationDetail.NumberOfBox,
                               NumberOfQty = productverificationDetail.NumberOfQty,
                               ReceivedQty = productverificationDetail.ReceivedQty,
                               IsConditionProduct = productverificationDetail.IsConditionProduct

                           }).ToList().OrderByDescending(x => x.Id).ToList();

            return dtolist;
        }

    }
}
