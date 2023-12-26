﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface IProductReceiptRepository : IGenericRepository<ProductReceipt>
    {
        
        List<DropDownDto> GetProductReceipteDropDown(int ProjectId);
        List<ProductReceiptGridDto> GetProductReceiptList(int ProjectId, bool isDeleted);
        string StudyProductTypeAlreadyUse(int PharmacyStudyProductTypeId);

        List<DropDownDto> GetLotBatchList(int ProjectId);

        void GenerateProductRecieptBarcode(ProductReceipt productReceipt);

        List<ProductRecieptBarcodeGenerateGridDto> GetProductReceiptBarcodeDetail(PharmacyBarcodeConfig pharmacyBarcodeConfig, int productReceiptId);
    }
}