using ClosedXML.Excel;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Report;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GSC.Respository.Reports
{
    public class PharmacyReportRepository : GenericRespository<JobMonitoring>, IPharmacyReportRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public PharmacyReportRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public FileStreamResult GetRandomizationKitReport(RandomizationIWRSReport randomizationIWRSReport)
        {
            List<RandomizationIWRSReportData> list = new List<RandomizationIWRSReportData>();
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == randomizationIWRSReport.ProjectId).FirstOrDefault();

            if (setting.KitCreationType == KitCreationType.SequenceWise)
            {
                list = _context.SupplyManagementKITSeriesDetail.Include(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Include(x => x.ProjectDesignVisit).Include(x => x.SupplyManagementKITSeries).ThenInclude(x => x.Project).Include(x => x.Randomization).
                       Where(x => x.DeletedDate == null && x.SupplyManagementKITSeries.DeletedDate == null && x.SupplyManagementKITSeries.RandomizationId != null && x.SupplyManagementKITSeries.Randomization != null
                       && x.SupplyManagementKITSeries.ProjectId == randomizationIWRSReport.ProjectId && x.RandomizationId != null).Select(x => new RandomizationIWRSReportData
                       {
                           ProjectCode = x.SupplyManagementKITSeries.Project.ProjectCode,
                           SiteCode = x.SupplyManagementKITSeries.Randomization.Project.ProjectCode,
                           KitNo = x.SupplyManagementKITSeries.KitNo,
                           Visit = x.ProjectDesignVisit.DisplayName,
                           Treatment = x.PharmacyStudyProductType.ProductType.ProductTypeCode,
                           ScreeningNo = x.SupplyManagementKITSeries.Randomization.ScreeningNumber,
                           RandomizationNumber = x.SupplyManagementKITSeries.Randomization.RandomizationNumber,
                           RandomizationDate = x.SupplyManagementKITSeries.Randomization.DateOfRandomization,
                           ProjectId = x.SupplyManagementKITSeries.ProjectId,
                           SiteId = x.SupplyManagementKITSeries.Randomization.ProjectId,
                           VisitId = x.ProjectDesignVisitId,
                           RandomizationId = x.RandomizationId
                       }).ToList();
                if (list.Count > 0)
                {
                    list.ForEach(s =>
                    {
                        var Allocationdetail = _context.SupplyManagementVisitKITSequenceDetail.Where(d => d.DeletedDate == null && d.ProjectDesignVisitId == s.VisitId && d.RandomizationId == s.RandomizationId).FirstOrDefault();
                        if (Allocationdetail != null)
                        {
                            s.AllocatedBy = _context.Users.Where(a => a.Id == Allocationdetail.CreatedBy && a.DeletedDate == null).FirstOrDefault().UserName;
                            s.Allocatedate = Allocationdetail.CreatedDate;
                        }
                    });
                }
            }
            if (setting.KitCreationType == KitCreationType.KitWise)
            {
                list = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).ThenInclude(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Include(x => x.SupplyManagementKIT).ThenInclude(x => x.Project).
                       Where(x => x.DeletedDate == null && x.SupplyManagementKIT.DeletedDate == null && x.RandomizationId != null
                       && x.SupplyManagementKIT.ProjectId == randomizationIWRSReport.ProjectId).Select(x => new RandomizationIWRSReportData
                       {
                           ProjectCode = x.SupplyManagementKIT.Project.ProjectCode,
                           KitNo = x.KitNo,
                           Treatment = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode,
                           ProjectId = x.SupplyManagementKIT.ProjectId,
                           VisitId = x.SupplyManagementKIT.ProjectDesignVisitId,
                           RandomizationId = x.RandomizationId
                       }).ToList();
                if (list != null && list.Count > 0)
                {
                    list.ForEach(x =>
                    {
                        if (x.RandomizationId > 0)
                        {
                            var randomization = _context.Randomization.Include(s => s.Project).Where(z => z.Id == x.RandomizationId).FirstOrDefault();
                            if (randomization != null)
                            {
                                x.SiteCode = randomization.Project.ProjectCode;
                                x.SiteId = randomization.ProjectId;
                                x.RandomizationNumber = randomization.RandomizationNumber;
                                x.RandomizationDate = randomization.DateOfRandomization;
                                x.ScreeningNo = randomization.ScreeningNumber;
                            }
                        }
                        if (x.VisitId > 0)
                        {
                            var visit = _context.ProjectDesignVisit.Where(a => a.Id == x.VisitId).FirstOrDefault();
                            if (visit != null)
                            {
                                x.Visit = visit.DisplayName;
                            }
                        }
                        var Allocationdetail = _context.SupplyManagementVisitKITSequenceDetail.Where(d => d.DeletedDate == null && d.ProjectDesignVisitId == x.VisitId && d.RandomizationId == x.RandomizationId).FirstOrDefault();
                        if (Allocationdetail != null)
                        {
                            x.AllocatedBy = _context.Users.Where(a => a.Id == Allocationdetail.CreatedBy && a.DeletedDate == null).FirstOrDefault().UserName;
                            x.Allocatedate = Allocationdetail.CreatedDate;
                        }

                    });
                }
            }
            if (randomizationIWRSReport.SiteId > 0)
            {
                list = list.Where(x => x.SiteId == randomizationIWRSReport.SiteId).ToList();
            }

            if (randomizationIWRSReport.VisitIds != null && randomizationIWRSReport.VisitIds.Length > 0)
            {
                list = list.Where(x => randomizationIWRSReport.VisitIds.Contains((int)x.VisitId)).ToList();
            }



            #region Excel Report Design
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Cell(1, 1).Value = "Study";
                worksheet.Cell(1, 2).Value = "Site";
                worksheet.Cell(1, 3).Value = "Visit";
                worksheet.Cell(1, 4).Value = "Treatment";
                worksheet.Cell(1, 5).Value = "Kit No.";
                worksheet.Cell(1, 6).Value = "Screening No.";
                worksheet.Cell(1, 7).Value = "Randomization No.";
                worksheet.Cell(1, 8).Value = "Randomization Date";
                worksheet.Cell(1, 9).Value = "Allocation By";
                worksheet.Cell(1, 10).Value = "Allocation Date";
                var j = 2;

                list.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.ProjectCode);
                    worksheet.Row(j).Cell(2).SetValue(d.SiteCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Visit);
                    worksheet.Row(j).Cell(4).SetValue(d.Treatment);
                    worksheet.Row(j).Cell(5).SetValue(d.KitNo);
                    worksheet.Row(j).Cell(6).SetValue(d.ScreeningNo);
                    worksheet.Row(j).Cell(7).SetValue(d.RandomizationNumber);
                    worksheet.Row(j).Cell(8).SetValue(Convert.ToDateTime(d.RandomizationDate).ToString("dddd, dd MMMM yyyy"));
                    worksheet.Row(j).Cell(9).SetValue(d.AllocatedBy);
                    worksheet.Row(j).Cell(10).SetValue(Convert.ToDateTime(d.Allocatedate).ToString("dddd, dd MMMM yyyy"));
                    j++;
                });

                #endregion ProjectDesignPeriod sheet

                MemoryStream memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;
                FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/vnd.ms-excel");
                fileStreamResult.FileDownloadName = "RandomizationKitReportExcel.xls";
                return fileStreamResult;
            }

        }

        public FileStreamResult GetProductAccountabilityCentralReport(ProductAccountabilityCentralReportSearch randomizationIWRSReport)
        {
            List<ProductAccountabilityCentralReport> list = new List<ProductAccountabilityCentralReport>();
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == randomizationIWRSReport.ProjectId).FirstOrDefault();

            var productreceipt = _context.ProductReceipt.Include(x => x.CentralDepot).Include(x => x.Project).Include(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType)
                                .Where(x => x.DeletedDate == null && (x.Status == ProductVerificationStatus.Quarantine || x.Status == ProductVerificationStatus.SentForApproval
                                 || x.Status == ProductVerificationStatus.Approved) && x.ProjectId == randomizationIWRSReport.ProjectId).ToList();
            if (productreceipt.Count > 0)
            {
                productreceipt.ForEach(x =>
                {
                    ProductAccountabilityCentralReport productAccountabilityCentralReport = new ProductAccountabilityCentralReport();
                    productAccountabilityCentralReport.ProjectCode = x.Project.ProjectCode;
                    productAccountabilityCentralReport.StorageLocation = x.CentralDepot.StorageArea;
                    if (x.CentralDepot.MinTemp > 0 && x.CentralDepot.MaxTemp > 0)
                        productAccountabilityCentralReport.StorageConditionTemprature = x.CentralDepot.MinTemp + "/" + x.CentralDepot.MaxTemp;
                    if (x.CentralDepot.MinTemp > 0)
                        productAccountabilityCentralReport.StorageConditionTemprature = x.CentralDepot.MinTemp.ToString();
                    if (x.CentralDepot.MaxTemp > 0)
                        productAccountabilityCentralReport.StorageConditionTemprature = x.CentralDepot.MaxTemp.ToString();
                    productAccountabilityCentralReport.ActionName = "Product Reciept";
                    productAccountabilityCentralReport.ActionBy = _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                    productAccountabilityCentralReport.ActionDate = x.CreatedDate;
                    productAccountabilityCentralReport.ProductTypeCode = x.PharmacyStudyProductType.ProductType.ProductTypeCode;
                    productAccountabilityCentralReport.ReceiptStatus = x.Status.ToString();
                    productAccountabilityCentralReport.StudyProductTypeId = x.PharmacyStudyProductTypeId;
                    var verification = _context.ProductVerification.Where(s => s.ProductReceiptId == x.Id && s.DeletedDate == null).FirstOrDefault();
                    if (verification != null)
                    {
                        productAccountabilityCentralReport.LotBatchNo = verification.BatchLotNumber;
                        if (verification.RetestExpiryId == ReTestExpiry.Expiry)
                        {
                            productAccountabilityCentralReport.RetestExpiryDate = verification.RetestExpiryDate;
                        }
                    }
                    var verificationdetail = _context.ProductVerificationDetail.Where(s => s.ProductReceiptId == x.Id && s.DeletedDate == null).FirstOrDefault();
                    if (verificationdetail != null)
                    {
                        productAccountabilityCentralReport.NoofBoxorBottle = (int)verificationdetail.NumberOfBox;
                        productAccountabilityCentralReport.Noofimp = (int)verificationdetail.NumberOfQty;
                        productAccountabilityCentralReport.TotalIMP = ((int)verificationdetail.NumberOfQty * (int)verificationdetail.NumberOfBox);
                    }
                    list.Add(productAccountabilityCentralReport);
                });
            }
            var verificationdetail = _context.ProductReceipt.Include(x => x.CentralDepot).Include(x => x.Project).Include(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType)
                               .Where(x => x.DeletedDate == null && x.Status == ProductVerificationStatus.Approved && x.ProjectId == randomizationIWRSReport.ProjectId).ToList();
            if (verificationdetail.Count > 0)
            {
                verificationdetail.ForEach(x =>
                {
                    ProductAccountabilityCentralReport productAccountabilityCentralReport = new ProductAccountabilityCentralReport();
                    productAccountabilityCentralReport.ProjectCode = x.Project.ProjectCode;
                    productAccountabilityCentralReport.StorageLocation = x.CentralDepot.StorageArea;
                    if (x.CentralDepot.MinTemp > 0 && x.CentralDepot.MaxTemp > 0)
                        productAccountabilityCentralReport.StorageConditionTemprature = x.CentralDepot.MinTemp + "/" + x.CentralDepot.MaxTemp;
                    if (x.CentralDepot.MinTemp > 0)
                        productAccountabilityCentralReport.StorageConditionTemprature = x.CentralDepot.MinTemp.ToString();
                    if (x.CentralDepot.MaxTemp > 0)
                        productAccountabilityCentralReport.StorageConditionTemprature = x.CentralDepot.MaxTemp.ToString();
                    productAccountabilityCentralReport.ActionName = "Verification";
                    productAccountabilityCentralReport.ActionBy = _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                    productAccountabilityCentralReport.ActionDate = x.CreatedDate;
                    productAccountabilityCentralReport.ProductTypeCode = x.PharmacyStudyProductType.ProductType.ProductTypeCode;
                    productAccountabilityCentralReport.StudyProductTypeId = x.PharmacyStudyProductTypeId;
                    productAccountabilityCentralReport.ReceiptStatus = x.Status.ToString();

                    var verification = _context.ProductVerification.Where(s => s.ProductReceiptId == x.Id && s.DeletedDate == null).FirstOrDefault();

                    var verificationdetail = _context.ProductVerificationDetail.Where(s => s.ProductReceiptId == x.Id && s.DeletedDate == null).FirstOrDefault();
                    if (verificationdetail != null && verification != null)
                    {
                        productAccountabilityCentralReport.LotBatchNo = verification.BatchLotNumber;
                        if (verification.RetestExpiryId == ReTestExpiry.Expiry)
                        {
                            productAccountabilityCentralReport.RetestExpiryDate = verification.RetestExpiryDate;
                        }
                        productAccountabilityCentralReport.NoofBoxorBottle = (int)verificationdetail.NumberOfBox;
                        productAccountabilityCentralReport.Noofimp = (int)verificationdetail.NumberOfQty;
                        productAccountabilityCentralReport.TotalIMP = ((int)verificationdetail.NumberOfQty * (int)verificationdetail.NumberOfBox);
                        productAccountabilityCentralReport.UsedVerificationQty = (int)verificationdetail.QuantityVerification;
                        productAccountabilityCentralReport.RetentionQty = (int)verificationdetail.RetentionSampleQty;
                        list.Add(productAccountabilityCentralReport);
                    }
                });
            }
            if (randomizationIWRSReport.productTypeId > 0)
            {
                list = list.Where(x => x.StudyProductTypeId == randomizationIWRSReport.productTypeId).ToList();
            }
            if (!string.IsNullOrEmpty(randomizationIWRSReport.LotNo))
            {
                list = list.Where(x => x.LotBatchNo == randomizationIWRSReport.LotNo).ToList();
            }
            if (setting.KitCreationType == KitCreationType.SequenceWise)
            {
                var kitpack = _context.SupplyManagementKITSeries.Include(x => x.Project).Where(x => x.DeletedDate == null && x.ProjectId == randomizationIWRSReport.ProjectId).ToList();
                if (randomizationIWRSReport.SiteId > 0)
                {
                    kitpack = kitpack.Where(x => x.ToSiteId > 0 ? x.ToSiteId == randomizationIWRSReport.SiteId : x.SiteId == randomizationIWRSReport.SiteId).ToList();
                }
                if (kitpack.Count > 0)
                {
                    kitpack.ForEach(x =>
                    {
                        ProductAccountabilityCentralReport productAccountabilityCentralReport = new ProductAccountabilityCentralReport();
                        productAccountabilityCentralReport.ProjectCode = x.Project.ProjectCode;
                        if (x.ToSiteId > 0)
                        {
                            productAccountabilityCentralReport.SiteCode = _context.Project.Where(z => z.Id == x.ToSiteId).FirstOrDefault().ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.ToSiteId;
                        }
                        else if (x.SiteId > 0)
                        {
                            productAccountabilityCentralReport.SiteCode = _context.Project.Where(z => z.Id == x.SiteId).FirstOrDefault().ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.SiteId;
                        }
                        productAccountabilityCentralReport.ProductTypeCode = x.TreatmentType;
                        productAccountabilityCentralReport.ActionName = "KitPack";
                        productAccountabilityCentralReport.ActionBy = _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                        productAccountabilityCentralReport.ActionDate = x.CreatedDate;


                        if (randomizationIWRSReport.productTypeId > 0)
                        {
                            var visits = _context.SupplyManagementKITSeriesDetail.Include(z => z.ProjectDesignVisit)
                               .Where(s => s.SupplyManagementKITSeriesId == x.Id && s.DeletedDate == null && s.PharmacyStudyProductTypeId == randomizationIWRSReport.productTypeId).Select(z => z.ProjectDesignVisit.DisplayName).ToList();
                            if (visits.Count > 0)
                                productAccountabilityCentralReport.VisitName = string.Join(",", visits.Distinct());


                            var noofimp = _context.SupplyManagementKITSeriesDetail
                               .Where(s => s.SupplyManagementKITSeriesId == x.Id && s.DeletedDate == null && s.PharmacyStudyProductTypeId == randomizationIWRSReport.productTypeId).Select(z => z.NoOfImp).Sum();
                            productAccountabilityCentralReport.TotalIMP = (noofimp * 1);
                            productAccountabilityCentralReport.Noofimp = noofimp;
                        }
                        else
                        {
                            var visits = _context.SupplyManagementKITSeriesDetail.Include(z => z.ProjectDesignVisit)
                               .Where(s => s.SupplyManagementKITSeriesId == x.Id && s.DeletedDate == null).Select(z => z.ProjectDesignVisit.DisplayName).ToList();
                            if (visits.Count > 0)
                                productAccountabilityCentralReport.VisitName = string.Join(",", visits.Distinct());

                            var noofimp = _context.SupplyManagementKITSeriesDetail
                               .Where(s => s.SupplyManagementKITSeriesId == x.Id && s.DeletedDate == null).Select(z => z.NoOfImp).Sum();
                            productAccountabilityCentralReport.TotalIMP = (noofimp * 1);
                            productAccountabilityCentralReport.Noofimp = noofimp;
                        }
                        productAccountabilityCentralReport.NoofBoxorBottle = 1;
                        list.Add(productAccountabilityCentralReport);
                    });
                }
            }
            if (setting.KitCreationType == KitCreationType.KitWise)
            {
                var kitpack = _context.SupplyManagementKIT.Include(x => x.ProjectDesignVisit).Include(x => x.Project).Include(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Where(x => x.DeletedDate == null && x.ProjectId == randomizationIWRSReport.ProjectId).ToList();
                if (randomizationIWRSReport.productTypeId > 0)
                {
                    kitpack = kitpack.Where(x => x.PharmacyStudyProductTypeId == randomizationIWRSReport.productTypeId).ToList();
                }
                if (randomizationIWRSReport.SiteId > 0)
                {
                    kitpack = kitpack.Where(x => x.ToSiteId > 0 ? x.ToSiteId == randomizationIWRSReport.SiteId : x.SiteId == randomizationIWRSReport.SiteId).ToList();
                }
                if (kitpack.Count > 0)
                {
                    kitpack.ForEach(x =>
                    {
                        ProductAccountabilityCentralReport productAccountabilityCentralReport = new ProductAccountabilityCentralReport();
                        productAccountabilityCentralReport.ProjectCode = x.Project.ProjectCode;
                        if (x.ToSiteId > 0)
                        {
                            productAccountabilityCentralReport.SiteCode = _context.Project.Where(z => z.Id == x.ToSiteId).FirstOrDefault().ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.ToSiteId;
                        }
                        else if (x.SiteId > 0)
                        {
                            productAccountabilityCentralReport.SiteCode = _context.Project.Where(z => z.Id == x.SiteId).FirstOrDefault().ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.SiteId;
                        }
                        productAccountabilityCentralReport.ProductTypeCode = x.PharmacyStudyProductType.ProductType.ProductTypeCode;
                        productAccountabilityCentralReport.ActionName = "Kit";
                        productAccountabilityCentralReport.ActionBy = _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                        productAccountabilityCentralReport.ActionDate = x.CreatedDate;
                        productAccountabilityCentralReport.VisitName = x.ProjectDesignVisit.DisplayName;
                        productAccountabilityCentralReport.NoofBoxorBottle = x.NoofPatient;
                        productAccountabilityCentralReport.Noofimp = x.NoOfImp;
                        productAccountabilityCentralReport.TotalIMP = (x.NoOfImp * x.NoofPatient);
                        list.Add(productAccountabilityCentralReport);
                    });
                }
            }

            list = list.OrderBy(x => x.ActionDate).ToList();
           
            if (randomizationIWRSReport.ActionType == ProductAccountabilityActions.ProductReciept)
            {
                list = list.Where(x => x.ActionName == "Product Reciept").ToList();
            }
            if (randomizationIWRSReport.ActionType == ProductAccountabilityActions.ProductVerification)
            {
                list = list.Where(x => x.ActionName == "Verification").ToList();
            }
            if (randomizationIWRSReport.ActionType == ProductAccountabilityActions.KitPack)
            {
                list = list.Where(x => x.ActionName == "KitPack").ToList();
            }
            if (randomizationIWRSReport.ActionType == ProductAccountabilityActions.Kit)
            {
                list = list.Where(x => x.ActionName == "Kit").ToList();
            }
            #region Excel Report Design
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Cell(1, 1).Value = "Study";
                worksheet.Cell(1, 2).Value = "Site";
                worksheet.Cell(1, 3).Value = "Product Type";
                worksheet.Cell(1, 4).Value = "Action";
                worksheet.Cell(1, 5).Value = "No of Boxes/kit";
                worksheet.Cell(1, 6).Value = "No of IMP/box/kit";
                worksheet.Cell(1, 7).Value = "Visit";
                worksheet.Cell(1, 8).Value = "Storage Condition";
                worksheet.Cell(1, 9).Value = "Storage location";
                worksheet.Cell(1, 10).Value = "Retention";
                worksheet.Cell(1, 11).Value = "Lot/Batch No";
                worksheet.Cell(1, 12).Value = "Expiry Date";
                worksheet.Cell(1, 13).Value = "Unused";
                worksheet.Cell(1, 14).Value = "Total IMP remaining";
                worksheet.Cell(1, 15).Value = "Action By";
                worksheet.Cell(1, 16).Value = "Action On";

                var j = 2;

                list.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.ProjectCode);
                    worksheet.Row(j).Cell(2).SetValue(d.SiteCode);
                    worksheet.Row(j).Cell(3).SetValue(d.ProductTypeCode);
                    worksheet.Row(j).Cell(4).SetValue(d.ActionName);
                    worksheet.Row(j).Cell(5).SetValue(d.NoofBoxorBottle);
                    worksheet.Row(j).Cell(6).SetValue(d.Noofimp);
                    worksheet.Row(j).Cell(7).SetValue(d.VisitName);
                    worksheet.Row(j).Cell(8).SetValue(d.StorageConditionTemprature);
                    worksheet.Row(j).Cell(9).SetValue(d.StorageLocation);
                    worksheet.Row(j).Cell(10).SetValue(d.RetentionQty);
                    worksheet.Row(j).Cell(11).SetValue(d.LotBatchNo);
                    worksheet.Row(j).Cell(12).SetValue(d.RetestExpiryDate != null ? Convert.ToDateTime(d.RetestExpiryDate).ToString("dddd, dd MMMM yyyy") : "");
                    worksheet.Row(j).Cell(13).SetValue(d.UsedVerificationQty);
                    worksheet.Row(j).Cell(14).SetValue(d.TotalIMP);
                    worksheet.Row(j).Cell(15).SetValue(d.ActionBy);
                    worksheet.Row(j).Cell(16).SetValue(Convert.ToDateTime(d.ActionDate).ToString("dddd, dd MMMM yyyy"));
                    j++;
                });

                worksheet.Cell(list.Count + 3, 13).Value = "Under quarentine";
                worksheet.Cell(list.Count + 4, 13).Value = "Verified Qty for dispensing";
                worksheet.Cell(list.Count + 5, 13).Value = "Remaining Qty";

                var underQuarentine = list.Where(x => x.ReceiptStatus == "Quarantine" || x.ReceiptStatus == "SentForApproval").Sum(x => x.TotalIMP);
                var verifiedQty = list.Where(x => x.ReceiptStatus == "Approved" && x.ActionName == "Verification").Sum(x => x.TotalIMP);
                var kits = list.Where(x => x.ActionName == "KitPack" || x.ActionName == "Kit").Sum(x => x.TotalIMP);

                worksheet.Row(list.Count + 3).Cell(14).SetValue(underQuarentine);
                worksheet.Row(list.Count + 4).Cell(14).SetValue(verifiedQty);
                worksheet.Row(list.Count + 5).Cell(14).SetValue(verifiedQty - kits);



                #endregion ProjectDesignPeriod sheet

                MemoryStream memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;
                FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/vnd.ms-excel");
                fileStreamResult.FileDownloadName = "ProductAccountabilityCentralReportExcel.xls";
                return fileStreamResult;
            }

        }
    }
}