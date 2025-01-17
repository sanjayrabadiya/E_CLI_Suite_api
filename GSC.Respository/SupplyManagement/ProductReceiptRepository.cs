﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Shared.Extension;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;


namespace GSC.Respository.SupplyManagement
{
    public class ProductReceiptRepository : GenericRespository<ProductReceipt>, IProductReceiptRepository
    {
        private readonly ICentralDepotRepository _centralDepotRepository;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ProductReceiptRepository(IGSCContext context, IMapper mapper, ICentralDepotRepository centralDepotRepository, IUploadSettingRepository uploadSettingRepository)
            : base(context)
        {

            _mapper = mapper;
            _context = context;
            _centralDepotRepository = centralDepotRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public List<DropDownDto> GetProductReceipteDropDown(int ProjectId)
        {
            return All.Where(c => c.ProjectId == ProjectId).Select(c => new DropDownDto { Id = c.Id, Value = c.Project.ProjectCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        public List<ProductReceiptGridDto> GetProductReceiptList(int ProjectId, bool isDeleted)
        {
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var data = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == ProjectId).
                   ProjectTo<ProductReceiptGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(t =>
            {
                t.PathName = t.PathName == null ? "" : documentUrl + t.PathName;
                t.ProductVerificationDetaild = _context.ProductVerificationDetail.Where(x => x.ProductReceiptId == t.Id).FirstOrDefault() != null ?
                _context.ProductVerificationDetail.Where(x => x.ProductReceiptId == t.Id).Select(x => x.Id).FirstOrDefault() : 0;
                var verification = _context.ProductVerification.Where(x => x.ProductReceiptId == t.Id && x.DeletedDate == null).FirstOrDefault();
                if (verification != null)
                {
                    var unit = _context.Unit.Where(s => s.Id == verification.UnitId).FirstOrDefault();
                    t.PacketTypeName = verification.PacketTypeId.GetDescription();
                    t.Dose = verification.Dose;
                    if (unit != null)
                        t.UnitName = unit.UnitName;
                }
            });

            return data;
        }

        // If Study Product Type already use than not delete and Edit
        public string StudyProductTypeAlreadyUse(int PharmacyStudyProductTypeId)
        {
            if (All.Any(x => x.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId && x.DeletedDate == null))
                return "Study Product is in use. Cannot edit or delete!";
            return "";
        }
        public List<DropDownDto> GetLotBatchList(int ProjectId)
        {
            return _context.ProductVerification.Include(x => x.ProductReceipt).Where(c => c.ProductReceipt.ProjectId == ProjectId && c.DeletedDate == null && c.ProductReceipt.DeletedDate == null
              && c.ProductReceipt.Status == ProductVerificationStatus.Approved).Select(c => new DropDownDto { Value = c.BatchLotNumber, Code = c.BatchLotNumber })
                .OrderBy(o => o.Value).Distinct().ToList();
        }

        public void GenerateProductRecieptBarcode(ProductReceipt productReceipt)
        {
            var pharmacyBarcodeConfig = _context.PharmacyBarcodeConfig.Where(s => s.DeletedDate == null && s.ProjectId == productReceipt.ProjectId && s.BarcodeModuleType == BarcodeModuleType.Verification).FirstOrDefault();
            if (pharmacyBarcodeConfig != null)
            {
                var project = _context.Project.Where(s => s.Id == productReceipt.ProjectId).FirstOrDefault();
                var pharmacyStudyProductType = _context.PharmacyStudyProductType.Include(s => s.ProductType).Where(s => s.Id == productReceipt.PharmacyStudyProductTypeId).FirstOrDefault();
                if (project != null && pharmacyStudyProductType != null && pharmacyStudyProductType.ProductType != null)
                {
                    productReceipt.Barcode = project.ProjectCode + pharmacyStudyProductType.ProductType.ProductTypeCode;
                    Update(productReceipt);
                    _context.Save();
                }
            }
        }

        public List<ProductRecieptBarcodeGenerateGridDto> GetProductReceiptBarcodeDetail(PharmacyBarcodeConfig pharmacyBarcodeConfig, int productReceiptId)
        {
            var productReciept = All.Where(s => s.Id == productReceiptId).FirstOrDefault();
            List<ProductRecieptBarcodeGenerateGridDto> lst = new List<ProductRecieptBarcodeGenerateGridDto>();

            var sublst = new ProductRecieptBarcodeGenerateGridDto
            {
                BarcodeString = productReciept != null ? productReciept.Barcode : "",
                BarcodeType = pharmacyBarcodeConfig.BarcodeType.GetDescription(),
                DisplayValue = pharmacyBarcodeConfig.DisplayValue,
                DisplayInformationLength = pharmacyBarcodeConfig.DisplayInformationLength,
                FontSize = pharmacyBarcodeConfig.FontSize,
                BarcodeDisplayInfo = new PharmacyBarcodeDisplayInfo[pharmacyBarcodeConfig.BarcodeDisplayInfo.Count]
            };
            int index = 0;
            foreach (var subitem in pharmacyBarcodeConfig.BarcodeDisplayInfo)
            {
                var tablefieldName = _context.TableFieldName.Where(s => s.Id == subitem.TableFieldNameId).FirstOrDefault();
                if (tablefieldName != null)
                {
                    sublst.BarcodeDisplayInfo[index] = new PharmacyBarcodeDisplayInfo();
                    sublst.BarcodeDisplayInfo[index].Alignment = subitem.Alignment;
                    sublst.BarcodeDisplayInfo[index].DisplayInformation = GetProductRecieptColumnValue(productReceiptId, tablefieldName.FieldName);
                    sublst.BarcodeDisplayInfo[index].OrderNumber = subitem.OrderNumber;
                    sublst.BarcodeDisplayInfo[index].IsSameLine = subitem.IsSameLine;
                    index++;
                }
            }
            lst.Add(sublst);

            return lst;
        }
        string GetProductRecieptColumnValue(int id, string ColumnName)
        {
            var tableRepository = _context.ProductReceipt.Include(s => s.CentralDepot).ThenInclude(s => s.SupplyLocation).Include(s => s.Project).Include(s => s.PharmacyStudyProductType).ThenInclude(s => s.ProductType)
                                 .Where(x => x.Id == id).Select(e => e).FirstOrDefault();
            if (tableRepository == null) return "";

            var productVerification = _context.ProductVerification.Where(s => s.ProductReceiptId == id).FirstOrDefault();

            if (ColumnName == "ProjectId")
                return _context.Project.Find(tableRepository.ProjectId).ProjectCode;

            if (ColumnName == "TransporterName")
                return tableRepository.TransporterName;

            if (ColumnName == "ConditionOfPackReceived")
                return tableRepository.ConditionOfPackReceived;

            if (ColumnName == "ShipmentNo")
                return tableRepository.ShipmentNo;

            if (ColumnName == "BatchLotNumber" && productVerification != null)
            {
                return productVerification.BatchLotNumber;
            }
            if (ColumnName == "ProductName")
                return tableRepository.ProductName;

            if (ColumnName == "PharmacyStudyProductTypeId")
                return tableRepository.PharmacyStudyProductType.ProductType.ProductTypeCode;

            if (ColumnName == "CentralDepotId")
                return tableRepository.CentralDepot.StorageArea;

            if (ColumnName == "ReceiptDate")
                return Convert.ToDateTime(tableRepository.ReceiptDate).ToString("dd/MMM/yyyy hh:mm");

            if (ColumnName == "ReceivedFromLocation")
                return tableRepository.ReceivedFromLocation;


            return "";
        }

        public bool IsCentralExists(int ProjectId)
        {
            return _centralDepotRepository.IsCentralExists(ProjectId);
        }

        public PharmacyBarcodeConfig ProductRecieptViewBarcodeValidate(int productReceiptId)
        {
            var productReciept = All.Where(x => x.Id == productReceiptId).FirstOrDefault();
            if (productReciept != null)
            {
                var barcodeConfig = _context.PharmacyBarcodeConfig.Include(s => s.BarcodeDisplayInfo).Where(x => x.ProjectId == productReciept.ProjectId && x.BarcodeModuleType == BarcodeModuleType.Verification && x.DeletedBy == null).FirstOrDefault();
                return barcodeConfig;
            }
            return new PharmacyBarcodeConfig();
        }

        public DepotType? GetDepotType(int centralDepotId)
        {
            var central = _context.CentralDepot.Where(s => s.Id == centralDepotId).FirstOrDefault();
            if (central != null)
            {
                return central.DepotType;
            }

            return null;
        }

    }
}
