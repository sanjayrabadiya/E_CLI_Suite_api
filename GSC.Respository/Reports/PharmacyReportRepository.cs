using ClosedXML.Excel;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Report;
using GSC.Data.Entities.SupplyManagement;
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
                    worksheet.Row(j).Cell(10).SetValue(Convert.ToDateTime(d.Allocatedate).ToString("dddd, dd MMMM yyyy hh:mm"));
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

        public List<RandomizationIWRSReportData> GetRandomizationKitReportData(RandomizationIWRSReport randomizationIWRSReport)
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


            return list;
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
                        productAccountabilityCentralReport.RetestExpiryId = verification.RetestExpiryId;
                        productAccountabilityCentralReport.RetestExpiryDate = verification.RetestExpiryDate;

                    }
                    var verificationdetail = _context.ProductVerificationDetail.Where(s => s.ProductReceiptId == x.Id && s.DeletedDate == null).FirstOrDefault();
                    if (verificationdetail != null)
                    {
                        productAccountabilityCentralReport.NoofBoxorBottle = (int)verificationdetail.NumberOfBox;
                        productAccountabilityCentralReport.Noofimp = (int)verificationdetail.NumberOfQty;
                        productAccountabilityCentralReport.TotalIMP = ((int)verificationdetail.NumberOfQty * (int)verificationdetail.NumberOfBox);
                        productAccountabilityCentralReport.UsedVerificationQty = verificationdetail.QuantityVerification == null ? 0 : (int)verificationdetail.QuantityVerification;
                        productAccountabilityCentralReport.RetentionQty = verificationdetail.RetentionSampleQty == null ? 0 : (int)verificationdetail.RetentionSampleQty;
                        var tempqty = (productAccountabilityCentralReport.UsedVerificationQty + productAccountabilityCentralReport.RetentionQty);
                        productAccountabilityCentralReport.TotalIMP = (productAccountabilityCentralReport.TotalIMP - tempqty);
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
                        productAccountabilityCentralReport.RetestExpiryDate = verification.RetestExpiryDate;
                        productAccountabilityCentralReport.RetestExpiryId = verification.RetestExpiryId;
                        productAccountabilityCentralReport.NoofBoxorBottle = (int)verificationdetail.NumberOfBox;
                        productAccountabilityCentralReport.Noofimp = (int)verificationdetail.NumberOfQty;
                        productAccountabilityCentralReport.TotalIMP = ((int)verificationdetail.NumberOfQty * (int)verificationdetail.NumberOfBox);
                        productAccountabilityCentralReport.UsedVerificationQty = verificationdetail.QuantityVerification == null ? 0 : (int)verificationdetail.QuantityVerification;
                        productAccountabilityCentralReport.RetentionQty = verificationdetail.RetentionSampleQty == null ? 0 : (int)verificationdetail.RetentionSampleQty;
                        var tempqty = (productAccountabilityCentralReport.UsedVerificationQty + productAccountabilityCentralReport.RetentionQty);
                        productAccountabilityCentralReport.TotalIMP = (productAccountabilityCentralReport.TotalIMP - tempqty);
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
                var kitpack = _context.SupplyManagementKITSeries.
                    Include(x => x.SupplyManagementShipment).
                    ThenInclude(s => s.SupplyManagementRequest)
                    .ThenInclude(x => x.FromProject)
                    .Include(x => x.Project).Where(x => x.DeletedDate == null && x.ProjectId == randomizationIWRSReport.ProjectId).ToList();
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
                        if (x.SupplyManagementShipmentId > 0)
                        {
                            productAccountabilityCentralReport.SiteCode = x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId;
                        }
                        else if (x.SiteId > 0)
                        {
                            productAccountabilityCentralReport.SiteCode = _context.Project.Where(z => z.Id == x.SiteId).FirstOrDefault().ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.SiteId;
                        }
                        productAccountabilityCentralReport.ProductTypeCode = x.TreatmentType;
                        productAccountabilityCentralReport.ActionName = "KitPack";
                      

                        productAccountabilityCentralReport.ActionBy = x.ModifiedDate != null && x.ModifiedBy > 0 ? _context.Users.Where(d => d.Id == x.ModifiedBy).FirstOrDefault().UserName : _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                        productAccountabilityCentralReport.ActionDate = x.ModifiedDate != null && x.ModifiedBy > 0 ? x.ModifiedDate : x.CreatedDate;

                        productAccountabilityCentralReport.KitStatus = x.Status.GetDescription();
                        productAccountabilityCentralReport.Status = x.Status;

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
                            productAccountabilityCentralReport.NoofBoxorBottle = 1;
                            if (visits.Count > 0)
                                list.Add(productAccountabilityCentralReport);
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
                            productAccountabilityCentralReport.NoofBoxorBottle = 1;
                            list.Add(productAccountabilityCentralReport);
                        }

                    });
                }
            }
            if (setting.KitCreationType == KitCreationType.KitWise)
            {
                var kitpack = _context.SupplyManagementKITDetail.
                    Include(x => x.SupplyManagementShipment).
                    ThenInclude(s => s.SupplyManagementRequest)
                    .ThenInclude(x => x.FromProject)
                    .Include(x => x.SupplyManagementKIT).
                    ThenInclude(x => x.ProjectDesignVisit).
                    Include(x => x.SupplyManagementKIT).
                    ThenInclude(x => x.Project).
                    Include(x => x.SupplyManagementKIT).
                    ThenInclude(x => x.PharmacyStudyProductType).
                    ThenInclude(x => x.ProductType).Where(x => x.DeletedDate == null && x.SupplyManagementKIT.ProjectId == randomizationIWRSReport.ProjectId).ToList();
                if (randomizationIWRSReport.productTypeId > 0)
                {
                    kitpack = kitpack.Where(x => x.SupplyManagementKIT.PharmacyStudyProductTypeId == randomizationIWRSReport.productTypeId).ToList();
                }
                if (randomizationIWRSReport.SiteId > 0)
                {
                    kitpack = kitpack.Where(x => x.SupplyManagementKIT.ToSiteId > 0 ? x.SupplyManagementKIT.ToSiteId == randomizationIWRSReport.SiteId : x.SupplyManagementKIT.SiteId == randomizationIWRSReport.SiteId).ToList();
                }
                if (kitpack.Count > 0)
                {
                    kitpack.ForEach(x =>
                    {
                        ProductAccountabilityCentralReport productAccountabilityCentralReport = new ProductAccountabilityCentralReport();
                        productAccountabilityCentralReport.ProjectCode = x.SupplyManagementKIT.Project.ProjectCode;
                        if (x.SupplyManagementShipmentId > 0)
                        {
                            productAccountabilityCentralReport.SiteCode = x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId;
                        }
                        else if (x.SupplyManagementKIT.SiteId > 0)
                        {
                            productAccountabilityCentralReport.SiteCode = _context.Project.Where(z => z.Id == x.SupplyManagementKIT.SiteId).FirstOrDefault().ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.SupplyManagementKIT.SiteId;
                        }
                        productAccountabilityCentralReport.ProductTypeCode = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode;
                        productAccountabilityCentralReport.ActionName = "Kit";
                        productAccountabilityCentralReport.ActionBy = x.ModifiedDate != null && x.ModifiedBy > 0 ? _context.Users.Where(d => d.Id == x.ModifiedBy).FirstOrDefault().UserName : _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                        productAccountabilityCentralReport.ActionDate = x.ModifiedDate != null && x.ModifiedBy > 0 ? x.ModifiedDate : x.CreatedDate;
                        productAccountabilityCentralReport.VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName;
                        productAccountabilityCentralReport.KitStatus = x.Status.GetDescription();
                        productAccountabilityCentralReport.Status = x.Status;
                        productAccountabilityCentralReport.NoofBoxorBottle = 1;
                        productAccountabilityCentralReport.Noofimp = (int)x.NoOfImp;
                        productAccountabilityCentralReport.TotalIMP = ((int)x.NoOfImp * 1);
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
            if (randomizationIWRSReport.ActionType == ProductAccountabilityActions.Individual)
            {
                list = list.Where(x => x.ActionName == "Individual").ToList();
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
                worksheet.Cell(1, 8).Value = "Status";
                worksheet.Cell(1, 9).Value = "Storage Condition";
                worksheet.Cell(1, 10).Value = "Storage location";
                worksheet.Cell(1, 11).Value = "Retention";
                worksheet.Cell(1, 12).Value = "Lot/Batch No";
                worksheet.Cell(1, 13).Value = "Retest/Expiry Date";
                worksheet.Cell(1, 14).Value = "Qty used for Verification";
                worksheet.Cell(1, 15).Value = "Total IMP remaining";
                worksheet.Cell(1, 16).Value = "Action By";
                worksheet.Cell(1, 17).Value = "Action On";

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
                    worksheet.Row(j).Cell(8).SetValue(d.KitStatus);
                    worksheet.Row(j).Cell(9).SetValue(d.StorageConditionTemprature);
                    worksheet.Row(j).Cell(10).SetValue(d.StorageLocation);
                    worksheet.Row(j).Cell(11).SetValue(d.RetentionQty);
                    worksheet.Row(j).Cell(12).SetValue(d.LotBatchNo);
                    if (d.RetestExpiryDate != null && d.RetestExpiryId == ReTestExpiry.ReTest)
                    {
                        worksheet.Row(j).Cell(13).SetValue("ReTest - " + Convert.ToDateTime(d.RetestExpiryDate).ToString("dddd, dd MMMM yyyy"));
                    }
                    else if (d.RetestExpiryDate != null && d.RetestExpiryId == ReTestExpiry.Expiry)
                    {
                        worksheet.Row(j).Cell(13).SetValue("Expiry - " + Convert.ToDateTime(d.RetestExpiryDate).ToString("dddd, dd MMMM yyyy"));
                    }
                    else
                        worksheet.Row(j).Cell(13).SetValue("");
                    worksheet.Row(j).Cell(14).SetValue(d.UsedVerificationQty);
                    worksheet.Row(j).Cell(15).SetValue(d.TotalIMP);
                    worksheet.Row(j).Cell(16).SetValue(d.ActionBy);
                    worksheet.Row(j).Cell(17).SetValue(Convert.ToDateTime(d.ActionDate).ToString("dddd, dd MMMM yyyy hh:mm"));
                    j++;
                });

                worksheet.Cell(list.Count + 3, 14).Value = "Under quarentine";
                worksheet.Cell(list.Count + 4, 14).Value = "Verified Qty for dispensing";
                worksheet.Cell(list.Count + 5, 14).Value = "Qty Used in Kit";
                worksheet.Cell(list.Count + 6, 14).Value = "Remaining Qty";
                worksheet.Cell(list.Count + 7, 14).Value = "Available Kit";

                var underQuarentine = list.Where(x => x.ReceiptStatus == "Quarantine" || x.ReceiptStatus == "SentForApproval").Sum(x => x.TotalIMP);
                var verifiedQty = list.Where(x => x.ReceiptStatus == "Approved" && x.ActionName == "Verification").Sum(x => x.TotalIMP);
                var kits = list.Where(x => x.ActionName == "KitPack" || x.ActionName == "Kit").Sum(x => x.TotalIMP);
                var avaialblekits = list.Count(x => x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue);

                worksheet.Row(list.Count + 3).Cell(15).SetValue(underQuarentine);
                worksheet.Row(list.Count + 4).Cell(15).SetValue(verifiedQty);
                worksheet.Row(list.Count + 5).Cell(15).SetValue(kits);
                worksheet.Row(list.Count + 6).Cell(15).SetValue(verifiedQty > 0 ? verifiedQty - kits : 0);
                worksheet.Row(list.Count + 7).Cell(15).SetValue(avaialblekits);


                #endregion ProjectDesignPeriod sheet

                MemoryStream memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;
                FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/vnd.ms-excel");
                fileStreamResult.FileDownloadName = "ProductAccountabilityCentralReportExcel.xls";
                return fileStreamResult;
            }

        }

        public FileStreamResult GetProductAccountabilitySiteReport(ProductAccountabilityCentralReportSearch randomizationIWRSReport)
        {
            List<ProductAccountabilityCentralReport> list = new List<ProductAccountabilityCentralReport>();
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == randomizationIWRSReport.ProjectId).FirstOrDefault();


            if (randomizationIWRSReport.ActionType == ProductAccountabilityActions.KitPack)
            {
                var kitpack = _context.SupplyManagementKITSeries.
                    Include(x => x.Project).
                    Include(x => x.SupplyManagementShipment).
                    ThenInclude(x => x.SupplyManagementRequest).
                    ThenInclude(x => x.FromProject).
                    Where(x => x.DeletedDate == null && x.ProjectId == randomizationIWRSReport.ProjectId && x.SupplyManagementShipmentId != null
                    && x.Status != KitStatus.ReturnReceive && x.Status != KitStatus.ReturnReceiveWithIssue
                    && x.Status != KitStatus.ReturnReceiveWithoutIssue && x.Status != KitStatus.ReturnReceiveDamaged && x.Status != KitStatus.ReturnReceiveMissing).ToList();
                if (randomizationIWRSReport.SiteId > 0)
                {
                    kitpack = kitpack.Where(x => x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == randomizationIWRSReport.SiteId).ToList();
                }
                if (kitpack.Count > 0)
                {
                    kitpack.ForEach(x =>
                    {
                        ProductAccountabilityCentralReport productAccountabilityCentralReport = new ProductAccountabilityCentralReport();
                        productAccountabilityCentralReport.ProjectCode = x.Project.ProjectCode;
                        productAccountabilityCentralReport.Id = x.Id;
                        productAccountabilityCentralReport.KitNo = x.KitNo;
                        if (x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId > 0)
                        {
                            productAccountabilityCentralReport.RequestedFrom = x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId;
                            if (x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId > 0)
                                productAccountabilityCentralReport.ToSiteId = (int)x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId;
                        }
                        if (x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId > 0)
                        {
                            productAccountabilityCentralReport.RequestedTo = _context.Project.Where(z => z.Id == x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId).FirstOrDefault().ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId;
                            if (x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId > 0)
                                productAccountabilityCentralReport.ToSiteId = (int)x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId;
                        }
                        else
                        {
                            productAccountabilityCentralReport.RequestedTo = x.Project.ProjectCode;
                        }
                        productAccountabilityCentralReport.ProductTypeCode = x.TreatmentType;
                        productAccountabilityCentralReport.ActionBy = x.ModifiedDate != null && x.ModifiedBy > 0 ? _context.Users.Where(d => d.Id == x.ModifiedBy).FirstOrDefault().UserName : _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                        productAccountabilityCentralReport.ActionDate = x.ModifiedDate != null && x.ModifiedBy > 0 ? x.ModifiedDate : x.CreatedDate;
                        productAccountabilityCentralReport.KitStatus = x.Status.GetDescription();
                        productAccountabilityCentralReport.Status = x.Status;
                        productAccountabilityCentralReport.PreStatus = x.PrevStatus;

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
                            productAccountabilityCentralReport.NoofBoxorBottle = 1;
                            if (visits.Count > 0)
                                list.Add(productAccountabilityCentralReport);
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
                            productAccountabilityCentralReport.NoofBoxorBottle = 1;
                            list.Add(productAccountabilityCentralReport);
                        }

                    });
                }
            }
            if (setting.KitCreationType == KitCreationType.KitWise)
            {
                var kitpack = _context.SupplyManagementKITDetail.
                    Include(x => x.SupplyManagementKIT).
                    ThenInclude(x => x.ProjectDesignVisit).
                    Include(x => x.SupplyManagementKIT).
                    ThenInclude(x => x.Project).
                    Include(x => x.SupplyManagementKIT).
                    ThenInclude(x => x.PharmacyStudyProductType).
                    ThenInclude(x => x.ProductType).
                    Include(x => x.SupplyManagementShipment).
                    ThenInclude(x => x.SupplyManagementRequest).
                    ThenInclude(x => x.FromProject).
                    Where(x => x.DeletedDate == null && x.SupplyManagementKIT.ProjectId == randomizationIWRSReport.ProjectId && x.SupplyManagementShipmentId > 0
                    && x.Status != KitStatus.ReturnReceive && x.Status != KitStatus.ReturnReceiveWithIssue
                    && x.Status != KitStatus.ReturnReceiveWithoutIssue && x.Status != KitStatus.ReturnReceiveDamaged && x.Status != KitStatus.ReturnReceiveMissing).ToList();
                if (randomizationIWRSReport.productTypeId > 0)
                {
                    kitpack = kitpack.Where(x => x.SupplyManagementKIT.PharmacyStudyProductTypeId == randomizationIWRSReport.productTypeId).ToList();
                }
                if (randomizationIWRSReport.SiteId > 0)
                {
                    kitpack = kitpack.Where(x => x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == randomizationIWRSReport.SiteId).ToList();
                }
                if (randomizationIWRSReport.VisitId > 0)
                {
                    kitpack = kitpack.Where(x => x.SupplyManagementKIT.ProjectDesignVisitId == randomizationIWRSReport.VisitId).ToList();
                }
                if (kitpack.Count > 0)
                {
                    kitpack.ForEach(x =>
                    {
                        ProductAccountabilityCentralReport productAccountabilityCentralReport = new ProductAccountabilityCentralReport();
                        productAccountabilityCentralReport.ProjectCode = x.SupplyManagementKIT.Project.ProjectCode;
                        productAccountabilityCentralReport.Id = x.Id;
                        productAccountabilityCentralReport.KitNo = x.KitNo;
                        if (x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId > 0)
                        {
                            productAccountabilityCentralReport.RequestedFrom = x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId;
                            if (x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId > 0)
                                productAccountabilityCentralReport.ToSiteId = (int)x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId;
                        }
                        if (x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId > 0)
                        {
                            productAccountabilityCentralReport.RequestedTo = _context.Project.Where(z => z.Id == x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId).FirstOrDefault().ProjectCode;
                            productAccountabilityCentralReport.SiteId = (int)x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId;
                            if (x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId > 0)
                                productAccountabilityCentralReport.ToSiteId = (int)x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId;
                        }
                        else
                        {
                            productAccountabilityCentralReport.RequestedTo = x.SupplyManagementKIT.Project.ProjectCode;
                        }
                        productAccountabilityCentralReport.ProductTypeCode = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode;
                        productAccountabilityCentralReport.KitStatus = x.Status.GetDescription();
                        productAccountabilityCentralReport.Status = x.Status;
                        productAccountabilityCentralReport.PreStatus = x.PrevStatus;
                        productAccountabilityCentralReport.ActionBy = x.ModifiedDate != null && x.ModifiedBy > 0  ? _context.Users.Where(d => d.Id == x.ModifiedBy).FirstOrDefault().UserName : _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                        productAccountabilityCentralReport.ActionDate = x.ModifiedDate != null && x.ModifiedBy > 0  ? x.ModifiedDate : x.CreatedDate;
                        productAccountabilityCentralReport.VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName;
                        productAccountabilityCentralReport.NoofBoxorBottle = 1;
                        productAccountabilityCentralReport.Noofimp = (int)x.NoOfImp;
                        productAccountabilityCentralReport.TotalIMP = ((int)x.NoOfImp * 1);
                        list.Add(productAccountabilityCentralReport);
                    });
                }
            }

            list = list.OrderBy(x => x.ActionDate).ToList();


            #region Excel Report SIte Level
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Cell(1, 1).Value = "Study";
                worksheet.Cell(1, 2).Value = "Requested from";
                worksheet.Cell(1, 3).Value = "Requested to";
                worksheet.Cell(1, 4).Value = "Product Type";
                worksheet.Cell(1, 5).Value = "Visit";
                worksheet.Cell(1, 6).Value = "Kit No";
                worksheet.Cell(1, 7).Value = "Status";
                worksheet.Cell(1, 8).Value = "No of pack/kit";
                worksheet.Cell(1, 9).Value = "No of IMP/box/kit";
                worksheet.Cell(1, 10).Value = "Total IMP";
                worksheet.Cell(1, 11).Value = "Action By";
                worksheet.Cell(1, 12).Value = "Action On";

                var j = 2;

                list.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.ProjectCode);
                    worksheet.Row(j).Cell(2).SetValue(d.RequestedFrom);
                    worksheet.Row(j).Cell(3).SetValue(d.RequestedTo);
                    worksheet.Row(j).Cell(4).SetValue(d.ProductTypeCode);
                    worksheet.Row(j).Cell(5).SetValue(d.VisitName);
                    worksheet.Row(j).Cell(6).SetValue(d.KitNo);
                    worksheet.Row(j).Cell(7).SetValue(d.KitStatus);
                    worksheet.Row(j).Cell(8).SetValue(d.NoofBoxorBottle);
                    worksheet.Row(j).Cell(9).SetValue(d.Noofimp);
                    worksheet.Row(j).Cell(10).SetValue(d.TotalIMP);
                    worksheet.Row(j).Cell(11).SetValue(d.ActionBy);
                    worksheet.Row(j).Cell(12).SetValue(Convert.ToDateTime(d.ActionDate).ToString("dddd, dd MMMM yyyy hh:mm"));
                    j++;
                });

                worksheet.Cell(list.Count + 3, 7).Value = "No of kits";
                worksheet.Cell(list.Count + 4, 7).Value = "No of Imp";
                worksheet.Cell(list.Count + 5, 7).Value = "No of kit with issue/without issue";
                worksheet.Cell(list.Count + 6, 7).Value = "No of damage";
                worksheet.Cell(list.Count + 7, 7).Value = "No of missing";
                worksheet.Cell(list.Count + 8, 7).Value = "No of discard";
                worksheet.Cell(list.Count + 9, 7).Value = "No of used/Allocated";
                worksheet.Cell(list.Count + 10, 7).Value = "Returned";
                //worksheet.Cell(list.Count + 12, 7).Value = "Return receive";
                //worksheet.Cell(list.Count + 13, 7).Value = "Return receive missing";
                //worksheet.Cell(list.Count + 14, 7).Value = "Return receive damaged";
                //worksheet.Cell(list.Count + 15, 7).Value = "Return only unused";
                //worksheet.Cell(list.Count + 11, 7).Value = "No of shipped to other site(unused only)";
                worksheet.Cell(list.Count + 11, 7).Value = "Send to sponsor";
                worksheet.Cell(list.Count + 12, 7).Value = "Shipped";
                worksheet.Cell(list.Count + 13, 7).Value = "Total kit";
                worksheet.Cell(list.Count + 14, 7).Value = "Total IMP";

                var noofkis = list.Sum(x => x.NoofBoxorBottle);
                var noofimp = list.Sum(x => x.Noofimp);
                //var kitcreated = list.Count(x => x.Status == KitStatus.AllocationPending);
                var withwithoutissue = list.Count(x => x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue);
                var damaged = list.Count(x => x.Status == KitStatus.Damaged);
                var missiing = list.Count(x => x.Status == KitStatus.Missing);
                var discard = list.Count(x => x.Status == KitStatus.Discard);
                var sendtosponser = list.Count(x => x.Status == KitStatus.Sendtosponser);
                var allocated = list.Count(x => x.Status == KitStatus.Allocated);
                var returns = list.Count(x => x.Status == KitStatus.Returned);
                //var returnreceive = list.Count(x => x.Status == KitStatus.ReturnReceive);
                //var returnreceivemissing = list.Count(x => x.Status == KitStatus.ReturnReceiveMissing);
                //var returnreceivedamage = list.Count(x => x.Status == KitStatus.ReturnReceiveDamaged);
                //var returnkit = list.Count(x => x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue);
                //var shiipedtoothersite = list.Count(x => x.ToSiteId != null && x.ToSiteId != x.SiteId && (x.PreStatus == KitStatus.ReturnReceiveWithIssue || x.PreStatus == KitStatus.ReturnReceiveWithoutIssue));
                var shipped = list.Count(x => x.Status == KitStatus.Shipped);
                //var totalkit = withwithoutissue + kitcreated;
                var totalkit = withwithoutissue;
                var totalimp = list.Where(x => x.Status == KitStatus.AllocationPending || x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue).Sum(x => x.TotalIMP);
                worksheet.Row(list.Count + 3).Cell(8).SetValue(noofkis);
                worksheet.Row(list.Count + 4).Cell(8).SetValue(noofimp);
                //worksheet.Row(list.Count + 5).Cell(8).SetValue(kitcreated);
                worksheet.Row(list.Count + 5).Cell(8).SetValue(withwithoutissue);
                worksheet.Row(list.Count + 6).Cell(8).SetValue(damaged);
                worksheet.Row(list.Count + 7).Cell(8).SetValue(missiing);
                worksheet.Row(list.Count + 8).Cell(8).SetValue(discard);
                worksheet.Row(list.Count + 9).Cell(8).SetValue(allocated);
                worksheet.Row(list.Count + 10).Cell(8).SetValue(returns);
                //worksheet.Row(list.Count + 12).Cell(8).SetValue(returnreceive);
                //worksheet.Row(list.Count + 13).Cell(8).SetValue(returnreceivemissing);
                //worksheet.Row(list.Count + 14).Cell(8).SetValue(returnreceivedamage);
                //worksheet.Row(list.Count + 15).Cell(8).SetValue(returnkit);
                //worksheet.Row(list.Count + 11).Cell(8).SetValue(shiipedtoothersite);
                worksheet.Row(list.Count + 11).Cell(8).SetValue(sendtosponser);
                worksheet.Row(list.Count + 12).Cell(8).SetValue(shipped);
                worksheet.Row(list.Count + 13).Cell(8).SetValue(totalkit);
                worksheet.Row(list.Count + 14).Cell(8).SetValue(totalimp);



                #endregion

                MemoryStream memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;
                FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/vnd.ms-excel");
                fileStreamResult.FileDownloadName = "ProductAccountabilityCentralReportExcel.xls";
                return fileStreamResult;
            }

        }

        public FileStreamResult GetProductShipmentReport(ProductAccountabilityCentralReportSearch randomizationIWRSReport)
        {
            List<ProductAccountabilityCentralReport> list = new List<ProductAccountabilityCentralReport>();
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == randomizationIWRSReport.ProjectId).FirstOrDefault();

            var request = _context.SupplyManagementRequest.Include(x => x.ProjectDesignVisit).Include(x => x.FromProject).Where(x => x.DeletedDate == null && (x.FromProjectId == randomizationIWRSReport.SiteId || x.ToProjectId == randomizationIWRSReport.SiteId)).OrderBy(x => x.Id).ToList();

            if (request.Count > 0)
            {
                request.ForEach(x =>
                {
                    ProductAccountabilityCentralReport requestobj = new ProductAccountabilityCentralReport();
                    PharmacyStudyProductType type = new PharmacyStudyProductType();
                    var project = _context.Project.Where(z => z.Id == x.FromProject.ParentProjectId).FirstOrDefault();
                    var toproject = _context.Project.Where(z => z.Id == x.ToProjectId).FirstOrDefault();
                    requestobj.ProjectCode = project.ProjectCode;
                    requestobj.RequestedFrom = x.FromProject.ProjectCode;
                    if (x.IsSiteRequest)
                    {
                        requestobj.RequestedTo = toproject != null ? toproject.ProjectCode : "";
                    }
                    else
                    {
                        requestobj.RequestedTo = project.ProjectCode;
                    }

                    if (setting != null && setting.KitCreationType == KitCreationType.SequenceWise)
                    {
                        requestobj.Type = "Kit pack";
                    }
                    if (setting != null && setting.KitCreationType == KitCreationType.KitWise)
                    {
                        if (x.StudyProductTypeId > 0)
                        {
                            type = _context.PharmacyStudyProductType.Include(k => k.ProductType).Where(s => s.Id == x.StudyProductTypeId).FirstOrDefault();
                            if (type != null && type.ProductUnitType == ProductUnitType.Kit)
                            {
                                requestobj.Type = "Kit";
                            }
                            else
                            {
                                requestobj.Type = "Individual";
                            }
                            if (type != null && type.ProductType != null)
                            {
                                requestobj.ProductTypeCode = type.ProductType.ProductTypeCode;
                            }
                        }
                    }
                    requestobj.ActionName = "Request";
                    if (x.ProjectDesignVisit != null)
                    {
                        requestobj.VisitName = x.ProjectDesignVisit.DisplayName;
                    }
                    requestobj.KitNo = x.RequestQty.ToString();
                    requestobj.ActionDate = x.CreatedDate;
                    requestobj.ActionBy = _context.Users.Where(s => s.Id == x.CreatedBy).FirstOrDefault().UserName;

                    var shipment = _context.SupplyManagementShipment.Include(a => a.SupplyManagementRequest).Where(s => s.SupplyManagementRequestId == x.Id).FirstOrDefault();
                    if (shipment != null)
                    {
                        requestobj.TrackingNumber = shipment.CourierTrackingNo;
                        requestobj.KitStatus = shipment.Status.GetDescription();
                    }

                    list.Add(requestobj);


                    if (shipment != null)
                    {
                        ProductAccountabilityCentralReport shipmentobj = new ProductAccountabilityCentralReport();
                        shipmentobj.ProjectCode = project.ProjectCode;
                        shipmentobj.RequestedFrom = x.FromProject.ProjectCode;
                        if (x.IsSiteRequest)
                        {
                            shipmentobj.RequestedTo = toproject != null ? toproject.ProjectCode : "";
                        }
                        else
                        {
                            shipmentobj.RequestedTo = project.ProjectCode;
                        }

                        if (setting != null && setting.KitCreationType == KitCreationType.SequenceWise)
                        {
                            shipmentobj.Type = "Kit pack";
                        }
                        if (setting != null && setting.KitCreationType == KitCreationType.KitWise)
                        {
                            if (x.StudyProductTypeId > 0)
                            {
                                type = _context.PharmacyStudyProductType.Include(x => x.ProductType).Where(s => s.Id == x.StudyProductTypeId).FirstOrDefault();
                                if (type != null && type.ProductUnitType == ProductUnitType.Kit)
                                {
                                    shipmentobj.Type = "Kit";
                                }
                                else
                                {
                                    shipmentobj.Type = "Individual";
                                }
                                if (type != null && type.ProductType != null)
                                {
                                    shipmentobj.ProductTypeCode = type.ProductType.ProductTypeCode;
                                }
                            }
                        }

                        if (x.ProjectDesignVisit != null)
                        {
                            shipmentobj.VisitName = x.ProjectDesignVisit.DisplayName;
                        }
                        shipmentobj.KitNo = shipment.ApprovedQty.ToString();
                        shipmentobj.ActionDate = shipment.CreatedDate;
                        shipmentobj.ActionBy = _context.Users.Where(s => s.Id == shipment.CreatedBy).FirstOrDefault().UserName;
                        shipmentobj.ActionName = "Shipment";
                        shipmentobj.CourierName = shipment.CourierName;
                        shipmentobj.TrackingNumber = shipment.CourierTrackingNo;
                        shipmentobj.KitStatus = shipment.Status.GetDescription();

                        list.Add(shipmentobj);

                        var receipt = _context.SupplyManagementReceipt.Where(s => s.SupplyManagementShipmentId == shipment.Id).FirstOrDefault();
                        if (receipt != null)
                        {
                            if (setting != null && setting.KitCreationType == KitCreationType.SequenceWise)
                            {
                                var kitpack = _context.SupplyManagementKITSeries.Where(s => s.DeletedDate == null && s.SupplyManagementShipmentId == shipment.Id).ToList();

                                if (kitpack.Count > 0)
                                {
                                    kitpack.ForEach(s =>
                                    {
                                        ProductAccountabilityCentralReport recieptobj = new ProductAccountabilityCentralReport();
                                        recieptobj.ProjectCode = project.ProjectCode;
                                        recieptobj.RequestedFrom = x.FromProject.ProjectCode;
                                        if (x.IsSiteRequest)
                                        {
                                            recieptobj.RequestedTo = toproject != null ? toproject.ProjectCode : "";
                                        }
                                        else
                                        {
                                            recieptobj.RequestedTo = project.ProjectCode;
                                        }

                                        if (setting != null && setting.KitCreationType == KitCreationType.SequenceWise)
                                        {
                                            recieptobj.Type = "Kit pack";
                                        }
                                        if (setting != null && setting.KitCreationType == KitCreationType.KitWise)
                                        {
                                            if (x.StudyProductTypeId > 0)
                                            {
                                                type = _context.PharmacyStudyProductType.Include(x => x.ProductType).Where(s => s.Id == x.StudyProductTypeId).FirstOrDefault();
                                                if (type != null && type.ProductUnitType == ProductUnitType.Kit)
                                                {
                                                    recieptobj.Type = "Kit";
                                                }
                                                else
                                                {
                                                    recieptobj.Type = "Individual";
                                                }
                                            }
                                        }

                                        recieptobj.ActionBy = x.ModifiedDate != null && x.ModifiedBy > 0 ? _context.Users.Where(d => d.Id == x.ModifiedBy).FirstOrDefault().UserName : _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                                        recieptobj.ActionDate = x.ModifiedDate != null && x.ModifiedBy > 0 ? x.ModifiedDate : x.CreatedDate;

                                        recieptobj.ActionName = "Receipt";
                                        recieptobj.CourierName = shipment.CourierName;
                                        recieptobj.TrackingNumber = shipment.CourierTrackingNo;
                                        recieptobj.KitNo = s.KitNo;
                                        if (s.Status == KitStatus.ReturnReceive || s.Status == KitStatus.ReturnReceiveDamaged || s.Status == KitStatus.ReturnReceiveMissing || s.Status == KitStatus.ReturnReceiveWithIssue || s.Status == KitStatus.ReturnReceiveWithoutIssue)
                                        {
                                            recieptobj.KitStatus = s.Status.GetDescription();
                                        }
                                        else
                                        {
                                            recieptobj.KitStatus = s.Status.GetDescription();
                                        }
                                        recieptobj.ProductTypeCode = s.TreatmentType;
                                        recieptobj.Comments = s.Comments;
                                        var visits = _context.SupplyManagementKITSeriesDetail.Include(z => z.ProjectDesignVisit)
                                         .Where(d => d.SupplyManagementKITSeriesId == s.Id && s.DeletedDate == null).Select(z => z.ProjectDesignVisit.DisplayName).ToList();
                                        if (visits.Count > 0)
                                            recieptobj.VisitName = string.Join(",", visits.Distinct());

                                        list.Add(recieptobj);
                                    });
                                }
                            }
                            if (setting != null && setting.KitCreationType == KitCreationType.KitWise)
                            {
                                var kit = _context.SupplyManagementKITDetail.Include(a => a.SupplyManagementKIT).ThenInclude(a => a.PharmacyStudyProductType).ThenInclude(z => z.ProductType).Where(s => s.DeletedDate == null && s.SupplyManagementShipmentId == shipment.Id).ToList();
                                if (kit.Count > 0)
                                {
                                    kit.ForEach(s =>
                                    {
                                        ProductAccountabilityCentralReport recieptobj = new ProductAccountabilityCentralReport();
                                        recieptobj.ProjectCode = project.ProjectCode;
                                        recieptobj.RequestedFrom = x.FromProject.ProjectCode;
                                        if (x.IsSiteRequest)
                                        {
                                            recieptobj.RequestedTo = toproject != null ? toproject.ProjectCode : "";
                                        }
                                        else
                                        {
                                            recieptobj.RequestedTo = project.ProjectCode;
                                        }

                                        if (setting != null && setting.KitCreationType == KitCreationType.SequenceWise)
                                        {
                                            recieptobj.Type = "Kit pack";
                                        }
                                        if (setting != null && setting.KitCreationType == KitCreationType.KitWise)
                                        {
                                            if (x.StudyProductTypeId > 0)
                                            {
                                                type = _context.PharmacyStudyProductType.Include(x => x.ProductType).Where(s => s.Id == x.StudyProductTypeId).FirstOrDefault();
                                                if (type != null && type.ProductUnitType == ProductUnitType.Kit)
                                                {
                                                    recieptobj.Type = "Kit";
                                                }
                                                else
                                                {
                                                    recieptobj.Type = "Individual";
                                                }
                                            }
                                        }
                                        if (x.ProjectDesignVisit != null)
                                        {
                                            recieptobj.VisitName = x.ProjectDesignVisit.DisplayName;
                                        }
                                        recieptobj.ActionBy = x.ModifiedDate != null && x.ModifiedBy > 0 ? _context.Users.Where(d => d.Id == x.ModifiedBy).FirstOrDefault().UserName : _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                                        recieptobj.ActionDate = x.ModifiedDate != null && x.ModifiedBy > 0 ? x.ModifiedDate : x.CreatedDate;
                                        recieptobj.ActionName = "Receipt";
                                        recieptobj.CourierName = shipment.CourierName;
                                        recieptobj.TrackingNumber = shipment.CourierTrackingNo;
                                        recieptobj.KitNo = s.KitNo;
                                        if (s.Status == KitStatus.ReturnReceive || s.Status == KitStatus.ReturnReceiveDamaged || s.Status == KitStatus.ReturnReceiveMissing || s.Status == KitStatus.ReturnReceiveWithIssue || s.Status == KitStatus.ReturnReceiveWithoutIssue)
                                        {
                                            recieptobj.KitStatus = s.Status.GetDescription();
                                        }
                                        else
                                        {
                                            recieptobj.KitStatus = s.Status.GetDescription();
                                        }

                                        recieptobj.ProductTypeCode = s.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode;
                                        recieptobj.Comments = s.Comments;
                                        list.Add(recieptobj);
                                    });
                                }
                            }
                        }
                    }


                });
            }

            #region Excel Report SHipment
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Cell(1, 1).Value = "Study";
                worksheet.Cell(1, 2).Value = "Requested from";
                worksheet.Cell(1, 3).Value = "Requested to";
                worksheet.Cell(1, 4).Value = "Action";
                worksheet.Cell(1, 5).Value = "Tracking Number";
                worksheet.Cell(1, 6).Value = "Type";
                worksheet.Cell(1, 7).Value = "Product Type";
                worksheet.Cell(1, 8).Value = "Visit";
                worksheet.Cell(1, 9).Value = "Status";
                worksheet.Cell(1, 10).Value = "Requested/Approved Imp/Kit";
                worksheet.Cell(1, 11).Value = "Comments";
                worksheet.Cell(1, 12).Value = "Courier Details";
                worksheet.Cell(1, 13).Value = "Expiry";
                worksheet.Cell(1, 14).Value = "Lot No.";
                worksheet.Cell(1, 15).Value = "Action By";
                worksheet.Cell(1, 16).Value = "Action On";

                var j = 2;

                list.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.ProjectCode);
                    worksheet.Row(j).Cell(2).SetValue(d.RequestedFrom);
                    worksheet.Row(j).Cell(3).SetValue(d.RequestedTo);
                    worksheet.Row(j).Cell(4).SetValue(d.ActionName);
                    worksheet.Row(j).Cell(5).SetValue(d.TrackingNumber);
                    worksheet.Row(j).Cell(6).SetValue(d.Type);
                    worksheet.Row(j).Cell(7).SetValue(d.ProductTypeCode);
                    worksheet.Row(j).Cell(8).SetValue(d.VisitName);
                    worksheet.Row(j).Cell(9).SetValue(d.KitStatus);
                    worksheet.Row(j).Cell(10).SetValue(d.KitNo);
                    worksheet.Row(j).Cell(11).SetValue(d.Comments);
                    worksheet.Row(j).Cell(12).SetValue(d.CourierName);
                    worksheet.Row(j).Cell(13).SetValue("");
                    worksheet.Row(j).Cell(14).SetValue("");
                    worksheet.Row(j).Cell(15).SetValue(d.ActionBy);
                    worksheet.Row(j).Cell(16).SetValue(Convert.ToDateTime(d.ActionDate).ToString("dddd, dd MMMM yyyy hh:mm"));
                    j++;
                });





                #endregion

                MemoryStream memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;
                FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/vnd.ms-excel");
                fileStreamResult.FileDownloadName = "ProductAccountabilityCentralReportExcel.xls";
                return fileStreamResult;
            }

        }

        public List<ProductAccountabilityCentralReport> GetProductShipmentReportData(ProductAccountabilityCentralReportSearch randomizationIWRSReport)
        {
            List<ProductAccountabilityCentralReport> list = new List<ProductAccountabilityCentralReport>();
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == randomizationIWRSReport.ProjectId).FirstOrDefault();

            var request = _context.SupplyManagementRequest.Include(x => x.ProjectDesignVisit).Include(x => x.FromProject).Where(x => x.DeletedDate == null && (x.FromProjectId == randomizationIWRSReport.SiteId || x.ToProjectId == randomizationIWRSReport.SiteId)).OrderBy(x => x.Id).ToList();

            if (request.Count > 0)
            {
                request.ForEach(x =>
                {
                    ProductAccountabilityCentralReport requestobj = new ProductAccountabilityCentralReport();
                    PharmacyStudyProductType type = new PharmacyStudyProductType();
                    var project = _context.Project.Where(z => z.Id == x.FromProject.ParentProjectId).FirstOrDefault();
                    var toproject = _context.Project.Where(z => z.Id == x.ToProjectId).FirstOrDefault();
                    requestobj.ProjectCode = project.ProjectCode;
                    requestobj.RequestedFrom = x.FromProject.ProjectCode;
                    if (x.IsSiteRequest)
                    {
                        requestobj.RequestedTo = toproject != null ? toproject.ProjectCode : "";
                    }
                    else
                    {
                        requestobj.RequestedTo = project.ProjectCode;
                    }

                    if (setting != null && setting.KitCreationType == KitCreationType.SequenceWise)
                    {
                        requestobj.Type = "Kit pack";
                    }
                    if (setting != null && setting.KitCreationType == KitCreationType.KitWise)
                    {
                        if (x.StudyProductTypeId > 0)
                        {
                            type = _context.PharmacyStudyProductType.Include(k => k.ProductType).Where(s => s.Id == x.StudyProductTypeId).FirstOrDefault();
                            if (type != null && type.ProductUnitType == ProductUnitType.Kit)
                            {
                                requestobj.Type = "Kit";
                            }
                            else
                            {
                                requestobj.Type = "Individual";
                            }
                            if (type != null && type.ProductType != null)
                            {
                                requestobj.ProductTypeCode = type.ProductType.ProductTypeCode;
                            }
                        }
                    }
                    requestobj.ActionName = "Request";
                    if (x.ProjectDesignVisit != null)
                    {
                        requestobj.VisitName = x.ProjectDesignVisit.DisplayName;
                    }
                    requestobj.KitNo = x.RequestQty.ToString();
                    requestobj.ActionDate = x.CreatedDate;
                    requestobj.ActionBy = _context.Users.Where(s => s.Id == x.CreatedBy).FirstOrDefault().UserName;

                    var shipment = _context.SupplyManagementShipment.Include(a => a.SupplyManagementRequest).Where(s => s.SupplyManagementRequestId == x.Id).FirstOrDefault();
                    if (shipment != null)
                    {
                        requestobj.TrackingNumber = shipment.CourierTrackingNo;
                        requestobj.KitStatus = shipment.Status.GetDescription();
                    }

                    list.Add(requestobj);


                    if (shipment != null)
                    {
                        ProductAccountabilityCentralReport shipmentobj = new ProductAccountabilityCentralReport();
                        shipmentobj.ProjectCode = project.ProjectCode;
                        shipmentobj.RequestedFrom = x.FromProject.ProjectCode;
                        if (x.IsSiteRequest)
                        {
                            shipmentobj.RequestedTo = toproject != null ? toproject.ProjectCode : "";
                        }
                        else
                        {
                            shipmentobj.RequestedTo = project.ProjectCode;
                        }

                        if (setting != null && setting.KitCreationType == KitCreationType.SequenceWise)
                        {
                            shipmentobj.Type = "Kit pack";
                        }
                        if (setting != null && setting.KitCreationType == KitCreationType.KitWise)
                        {
                            if (x.StudyProductTypeId > 0)
                            {
                                type = _context.PharmacyStudyProductType.Include(x => x.ProductType).Where(s => s.Id == x.StudyProductTypeId).FirstOrDefault();
                                if (type != null && type.ProductUnitType == ProductUnitType.Kit)
                                {
                                    shipmentobj.Type = "Kit";
                                }
                                else
                                {
                                    shipmentobj.Type = "Individual";
                                }
                                if (type != null && type.ProductType != null)
                                {
                                    shipmentobj.ProductTypeCode = type.ProductType.ProductTypeCode;
                                }
                            }
                        }

                        if (x.ProjectDesignVisit != null)
                        {
                            shipmentobj.VisitName = x.ProjectDesignVisit.DisplayName;
                        }
                        shipmentobj.KitNo = shipment.ApprovedQty.ToString();
                        shipmentobj.ActionDate = shipment.CreatedDate;
                        shipmentobj.ActionBy = _context.Users.Where(s => s.Id == shipment.CreatedBy).FirstOrDefault().UserName;
                        shipmentobj.ActionName = "Shipment";
                        shipmentobj.CourierName = shipment.CourierName;
                        shipmentobj.TrackingNumber = shipment.CourierTrackingNo;
                        shipmentobj.KitStatus = shipment.Status.GetDescription();

                        list.Add(shipmentobj);

                        var receipt = _context.SupplyManagementReceipt.Where(s => s.SupplyManagementShipmentId == shipment.Id).FirstOrDefault();
                        if (receipt != null)
                        {
                            if (setting != null && setting.KitCreationType == KitCreationType.SequenceWise)
                            {
                                var kitpack = _context.SupplyManagementKITSeries.Where(s => s.DeletedDate == null && s.SupplyManagementShipmentId == shipment.Id).ToList();

                                if (kitpack.Count > 0)
                                {
                                    kitpack.ForEach(s =>
                                    {
                                        ProductAccountabilityCentralReport recieptobj = new ProductAccountabilityCentralReport();
                                        recieptobj.ProjectCode = project.ProjectCode;
                                        recieptobj.RequestedFrom = x.FromProject.ProjectCode;
                                        if (x.IsSiteRequest)
                                        {
                                            recieptobj.RequestedTo = toproject != null ? toproject.ProjectCode : "";
                                        }
                                        else
                                        {
                                            recieptobj.RequestedTo = project.ProjectCode;
                                        }

                                        if (setting != null && setting.KitCreationType == KitCreationType.SequenceWise)
                                        {
                                            recieptobj.Type = "Kit pack";
                                        }
                                        if (setting != null && setting.KitCreationType == KitCreationType.KitWise)
                                        {
                                            if (x.StudyProductTypeId > 0)
                                            {
                                                type = _context.PharmacyStudyProductType.Include(x => x.ProductType).Where(s => s.Id == x.StudyProductTypeId).FirstOrDefault();
                                                if (type != null && type.ProductUnitType == ProductUnitType.Kit)
                                                {
                                                    recieptobj.Type = "Kit";
                                                }
                                                else
                                                {
                                                    recieptobj.Type = "Individual";
                                                }
                                            }
                                        }
                                        recieptobj.ActionBy = x.ModifiedDate != null && x.ModifiedBy > 0 ? _context.Users.Where(d => d.Id == x.ModifiedBy).FirstOrDefault().UserName : _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                                        recieptobj.ActionDate = x.ModifiedDate != null && x.ModifiedBy > 0 ? x.ModifiedDate : x.CreatedDate;
                                        recieptobj.ActionName = "Receipt";
                                        recieptobj.CourierName = shipment.CourierName;
                                        recieptobj.TrackingNumber = shipment.CourierTrackingNo;
                                        recieptobj.KitNo = s.KitNo;
                                        if (s.Status == KitStatus.ReturnReceive || s.Status == KitStatus.ReturnReceiveDamaged || s.Status == KitStatus.ReturnReceiveMissing || s.Status == KitStatus.ReturnReceiveWithIssue || s.Status == KitStatus.ReturnReceiveWithoutIssue)
                                        {
                                            recieptobj.KitStatus = s.Status.GetDescription();
                                        }
                                        else
                                        {
                                            recieptobj.KitStatus = s.Status.GetDescription();
                                        }
                                        recieptobj.ProductTypeCode = s.TreatmentType;
                                        recieptobj.Comments = s.Comments;
                                        var visits = _context.SupplyManagementKITSeriesDetail.Include(z => z.ProjectDesignVisit)
                                         .Where(d => d.SupplyManagementKITSeriesId == s.Id && s.DeletedDate == null).Select(z => z.ProjectDesignVisit.DisplayName).ToList();
                                        if (visits.Count > 0)
                                            recieptobj.VisitName = string.Join(",", visits.Distinct());

                                        list.Add(recieptobj);
                                    });
                                }
                            }
                            if (setting != null && setting.KitCreationType == KitCreationType.KitWise)
                            {
                                var kit = _context.SupplyManagementKITDetail.Include(a => a.SupplyManagementKIT).ThenInclude(a => a.PharmacyStudyProductType).ThenInclude(z => z.ProductType).Where(s => s.DeletedDate == null && s.SupplyManagementShipmentId == shipment.Id).ToList();
                                if (kit.Count > 0)
                                {
                                    kit.ForEach(s =>
                                    {
                                        ProductAccountabilityCentralReport recieptobj = new ProductAccountabilityCentralReport();
                                        recieptobj.ProjectCode = project.ProjectCode;
                                        recieptobj.RequestedFrom = x.FromProject.ProjectCode;
                                        if (x.IsSiteRequest)
                                        {
                                            recieptobj.RequestedTo = toproject != null ? toproject.ProjectCode : "";
                                        }
                                        else
                                        {
                                            recieptobj.RequestedTo = project.ProjectCode;
                                        }

                                        if (setting != null && setting.KitCreationType == KitCreationType.SequenceWise)
                                        {
                                            recieptobj.Type = "Kit pack";
                                        }
                                        if (setting != null && setting.KitCreationType == KitCreationType.KitWise)
                                        {
                                            if (x.StudyProductTypeId > 0)
                                            {
                                                type = _context.PharmacyStudyProductType.Include(x => x.ProductType).Where(s => s.Id == x.StudyProductTypeId).FirstOrDefault();
                                                if (type != null && type.ProductUnitType == ProductUnitType.Kit)
                                                {
                                                    recieptobj.Type = "Kit";
                                                }
                                                else
                                                {
                                                    recieptobj.Type = "Individual";
                                                }
                                            }
                                        }
                                        if (x.ProjectDesignVisit != null)
                                        {
                                            recieptobj.VisitName = x.ProjectDesignVisit.DisplayName;
                                        }
                                        recieptobj.ActionBy = x.ModifiedDate != null && x.ModifiedBy > 0 ? _context.Users.Where(d => d.Id == x.ModifiedBy).FirstOrDefault().UserName : _context.Users.Where(d => d.Id == x.CreatedBy).FirstOrDefault().UserName;
                                        recieptobj.ActionDate = x.ModifiedDate != null && x.ModifiedBy > 0 ? x.ModifiedDate : x.CreatedDate;
                                        recieptobj.ActionName = "Receipt";
                                        recieptobj.CourierName = shipment.CourierName;
                                        recieptobj.TrackingNumber = shipment.CourierTrackingNo;
                                        recieptobj.KitNo = s.KitNo;
                                        if (s.Status == KitStatus.ReturnReceive || s.Status == KitStatus.ReturnReceiveDamaged || s.Status == KitStatus.ReturnReceiveMissing || s.Status == KitStatus.ReturnReceiveWithIssue || s.Status == KitStatus.ReturnReceiveWithoutIssue)
                                        {
                                            recieptobj.KitStatus = s.Status.GetDescription();
                                        }
                                        else
                                        {
                                            recieptobj.KitStatus = s.Status.GetDescription();
                                        }

                                        recieptobj.ProductTypeCode = s.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode;
                                        recieptobj.Comments = s.Comments;
                                        list.Add(recieptobj);
                                    });
                                }
                            }
                        }
                    }


                });
            }

            return list;
        }

        public List<DropDownDto> GetPharmacyStudyProductTypeDropDownPharmacyReport(int ProjectId)
        {
            return _context.ProductReceipt.Where(c => c.ProjectId == ProjectId && c.DeletedDate == null).Select(c => new DropDownDto { Id = c.PharmacyStudyProductType.Id, Value = c.PharmacyStudyProductType.ProductType.ProductTypeCode + "-" + c.PharmacyStudyProductType.ProductType.ProductTypeName })
                .OrderBy(o => o.Value).Distinct().ToList();
        }
    }
}