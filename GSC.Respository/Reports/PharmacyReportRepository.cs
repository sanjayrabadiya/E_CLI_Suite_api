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
                       && x.SupplyManagementKITSeries.ProjectId == randomizationIWRSReport.ProjectId).Select(x => new RandomizationIWRSReportData
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
                           VisitId = x.ProjectDesignVisitId
                       }).ToList();
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

    }
}