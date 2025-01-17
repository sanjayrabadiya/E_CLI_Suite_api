﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Shared.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementUploadFileRepository : GenericRespository<SupplyManagementUploadFile>, ISupplyManagementUploadFileRepository
    {

        private readonly IMapper _mapper;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly ISupplyManagementUploadFileVisitRepository _supplyManagementUploadFileVisitRepository;
        private readonly ISupplyManagementUploadFileDetailRepository _supplyManagementUploadFileDetailRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IPharmacyStudyProductTypeRepository _pharmacyStudyProductTypeRepository;
        private readonly IGSCContext _context;
        private readonly IEmailSenderRespository _emailSenderRespository;
        public SupplyManagementUploadFileRepository(IGSCContext context,
                IUploadSettingRepository uploadSettingRepository,
                ISupplyManagementUploadFileVisitRepository supplyManagementUploadFileVisitRepository,
                ISupplyManagementUploadFileDetailRepository supplyManagementUploadFileDetailRepository,
                IPharmacyStudyProductTypeRepository pharmacyStudyProductTypeRepository,
             IProjectDesignVisitRepository projectDesignVisitRepository,
             IProjectRepository projectRepository,
             ICountryRepository countryRepository,
             IMapper mapper, IEmailSenderRespository emailSenderRespository)
            : base(context)
        {
            _uploadSettingRepository = uploadSettingRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _supplyManagementUploadFileVisitRepository = supplyManagementUploadFileVisitRepository;
            _supplyManagementUploadFileDetailRepository = supplyManagementUploadFileDetailRepository;
            _pharmacyStudyProductTypeRepository = pharmacyStudyProductTypeRepository;
            _projectRepository = projectRepository;
            _countryRepository = countryRepository;
            _emailSenderRespository = emailSenderRespository;
            _mapper = mapper;
            _context = context;
        }

        public List<SupplyManagementUploadFileGridDto> GetSupplyManagementUploadFileList(bool isDeleted, int ProjectId)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementUploadFileGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }


        // Upload excel data insert into database
        public string InsertExcelDataIntoDatabaseTable(SupplyManagementUploadFile supplyManagementUploadFile, SupplyManagementKitNumberSettings setting)
        {
            // check level for the upload file
            var validate = All.Where(x => x.ProjectId == supplyManagementUploadFile.ProjectId && x.Status != Helper.LabManagementUploadStatus.Reject && x.DeletedDate == null).FirstOrDefault();
            if (validate != null && validate.SupplyManagementUploadFileLevel != supplyManagementUploadFile.SupplyManagementUploadFileLevel)
                return "Please Select Appropriate Level.";

            if (supplyManagementUploadFile.SiteId > 0)
            {
                var project = _context.Project.Where(s => s.Id == supplyManagementUploadFile.SiteId && (s.Status == Helper.MonitoringSiteStatus.CloseOut || s.Status == Helper.MonitoringSiteStatus.Terminated || s.Status == Helper.MonitoringSiteStatus.OnHold || s.Status == Helper.MonitoringSiteStatus.Rejected)).FirstOrDefault();
                if (project != null)
                {
                    return "You can't upload sheet,selected site is " + project.Status.GetDescription() + "!";
                }
            }
            // Read excel to stream reader
            var documentUrl = _uploadSettingRepository.GetDocumentPath();
            string pathname = documentUrl + supplyManagementUploadFile.PathName;
            FileStream streamer = new FileStream(pathname, FileMode.Open);
            IExcelDataReader reader = null;
            if (Path.GetExtension(pathname) == ".xls")
                reader = ExcelReaderFactory.CreateBinaryReader(streamer);
            else
                reader = ExcelReaderFactory.CreateOpenXmlReader(streamer);

            // convert excel to dataset
            DataSet results = reader.AsDataSet();
            if (results == null)
                return "File is not Compatible";
            if (results.Tables.Count > 0 && results.Tables[0].Rows.Count == 0)
            {
                return "File is not Compatible";
            }
            // validate excel file
            var isValid = validateExcel(results, supplyManagementUploadFile, setting);

            if (isValid != "")
                return isValid;

            // Get detail in data table 
            DataTable dt = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).CopyToDataTable();
            if (validate != null)
            {
                // check randomization number with already upload file
                var isValidRandomizationNo = RandomizationNumberValidation(dt, supplyManagementUploadFile.ProjectId, validate, supplyManagementUploadFile.SiteId, supplyManagementUploadFile.CountryId);
                if (isValidRandomizationNo != "")
                    return isValidRandomizationNo;
            }
            else
            {
                // check randomization number for first file uploaded
                var isValidRandomizationNoInSerial = RandomizationNumberCheckSeries(dt, 0);
                if (isValidRandomizationNoInSerial != "")
                    return isValidRandomizationNoInSerial;
            }

            // validate product type
            var productType = validateProductType(dt, supplyManagementUploadFile.ProjectId, setting);
            if (productType != "")
                return productType;

            // insert data into table if excel is valid
            InserFileData(results, supplyManagementUploadFile, setting);

            return "";
        }

        // Generate excecl format for download
        public FileStreamResult DownloadFormat(SupplyManagementUploadFileDto supplyManagementUploadFile, SupplyManagementKitNumberSettings setting)
        {
            var studyCode = GetProjectCode(supplyManagementUploadFile.ProjectId);
            string site = "";
            string country = "";
            if (supplyManagementUploadFile.SiteId != null)
                site = (string)GetProjectCode((int)supplyManagementUploadFile.SiteId);
            if (supplyManagementUploadFile.CountryId != null)
                country = (string)GetCountry((int)supplyManagementUploadFile.CountryId);

            var projectDesignVisits = GetProjectDesignVisit(supplyManagementUploadFile.ProjectId);
            #region Excel Report Design
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Range("A1:B3").Style.Fill.BackgroundColor = XLColor.LightGreen;
                worksheet.Range("A1:B3").Style.Font.SetBold();
                worksheet.Range("A1:B3").Style.Border.SetDiagonalBorder(XLBorderStyleValues.Thin);
                worksheet.Cell(1, 1).Value = "Study Code";
                worksheet.Cell(2, 1).Value = "Country";
                worksheet.Cell(3, 1).Value = "Site Code";
                worksheet.Range("A5:B5").Style.Fill.BackgroundColor = XLColor.LightGreen;
                worksheet.Range("A5:B5").Style.Font.SetBold();
                worksheet.Cell(5, 1).Value = "Randomization No";
                worksheet.Cell(5, 2).Value = "Product Code";

                var j = 0;
                if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == false)
                {
                    worksheet.Row(5).Cell(3).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    worksheet.Row(5).Cell(3).Style.Font.SetBold();
                    worksheet.Cell(5, 3).Value = "Kit No";
                    j = 4;
                }
                else if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                {
                    worksheet.Row(5).Cell(3).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    worksheet.Row(5).Cell(3).Style.Font.SetBold();
                    worksheet.Cell(5, 3).Value = "Kit No";

                    worksheet.Row(5).Cell(4).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    worksheet.Row(5).Cell(4).Style.Font.SetBold();
                    worksheet.Cell(5, 4).Value = "Display Randomization No";
                    j = 5;
                }
                else if (!setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                {
                    worksheet.Row(5).Cell(3).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    worksheet.Row(5).Cell(3).Style.Font.SetBold();
                    worksheet.Cell(5, 3).Value = "Display Randomization No";
                    j = 4;
                }
                else
                {
                    j = 3;
                }

                projectDesignVisits.ForEach(d =>
                {
                    worksheet.Row(5).Cell(j).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    worksheet.Row(5).Cell(j).Style.Font.SetBold();
                    worksheet.Row(5).Cell(j).SetValue(d.DisplayName);
                    j++;
                });

                worksheet.Row(1).Cell(2).SetValue(studyCode.ToString());
                worksheet.Row(2).Cell(2).SetValue(country);
                worksheet.Row(3).Cell(2).SetValue(site);

                MemoryStream memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;
                FileStreamResult fileStreamResult = new FileStreamResult(memoryStream, "application/vnd.ms-excel");
                fileStreamResult.FileDownloadName = "RandomizationUploadExcel.xls";
                return fileStreamResult;
            }
            #endregion
        }

        public object GetProjectCode(int projectId)
        {
            return _projectRepository.Find(projectId).ProjectCode;
        }

        public object GetCountry(int countryId)
        {
            return _countryRepository.Find(countryId).CountryName;
        }

        public List<ProjectDesignVisit> GetProjectDesignVisit(int projectId)
        {
            return _projectDesignVisitRepository.All
                                        .Where(x => x.ProjectDesignPeriod.ProjectDesign.Project.Id == projectId
                                        && x.DeletedDate == null && x.InActiveVersion == null).ToList();
        }

        public string validateExcel(DataSet results, SupplyManagementUploadFile supplyManagementUploadFile, SupplyManagementKitNumberSettings setting)
        {
            var study = results.Tables[0].Rows[0].ItemArray[1].ToString().Trim();
            var country = results.Tables[0].Rows[1].ItemArray[1].ToString().Trim();
            var site = results.Tables[0].Rows[2].ItemArray[1].ToString().Trim();

            if (study.Trim().ToLower() != Convert.ToString(GetProjectCode(supplyManagementUploadFile.ProjectId)).ToLower().Trim())
                return "Please check study code";

            if (supplyManagementUploadFile.SupplyManagementUploadFileLevel == Helper.SupplyManagementUploadFileLevel.Study)
            {
                if (country.Trim() != "" || site.Trim() != "")
                    return "Remove Site Code or Country Name if file uploaded for the Study.";
            }
            else if (supplyManagementUploadFile.SupplyManagementUploadFileLevel == Helper.SupplyManagementUploadFileLevel.Site)
            {
                if (country.Trim() != "")
                    return "Remove country if file uploaded for the site level.";
                if (site.Trim() != "")
                {
                    if (supplyManagementUploadFile.SiteId == null)
                        return "Please select site!";
                    if (site.Trim().ToLower() != Convert.ToString(GetProjectCode((int)supplyManagementUploadFile.SiteId)).ToLower())
                        return "Please check site code";
                }
                else return "Please enter site code";
            }
            else
            {
                if (site.Trim() != "")
                    return "Remove site code if file uploaded for the country level.";
                if (country.Trim() != "")
                {
                    if (country.Trim().ToLower() != Convert.ToString(GetCountry((int)supplyManagementUploadFile.CountryId)).ToLower())
                        return "Please check country name";
                }
                else
                    return "Please enter country name";
            }

            StringBuilder selectQuery = new StringBuilder();
            var projectDesignVisits = GetProjectDesignVisit(supplyManagementUploadFile.ProjectId);
            var j = 0;
            var visitcheck = results;
            var table = visitcheck.Tables[0].Rows[4].ItemArray.Where(x => x.ToString() != "").ToList();
            var duplicates = table.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            if (duplicates.Count > 0)
            {
                return "Visit name should not be duplicate.";
            }


            foreach (var item in results.Tables[0].Rows[4].ItemArray.Where(x => x.ToString() != ""))
            {
                if (setting.IsUploadWithKit || setting.IsStaticRandomizationNo == true)
                {
                    selectQuery.Append("Column" + j + " is null or ");
                    if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                    {
                        if (j > 3)
                        {
                            var r = projectDesignVisits.Exists(x => x.DisplayName.ToLower().Trim() == item.ToString().ToLower().Trim());
                            if (!r)
                                return "Visit name not match with design visit.";
                        }
                        else
                        {
                            if (j == 0 && item.ToString().Trim().ToLower() != "randomization no")
                                return "File is not Compatible!";

                            if (j == 1 && item.ToString().Trim().ToLower() != "product code")
                                return "File is not Compatible!";

                            if (j == 2 && item.ToString().Trim().ToLower() != "kit no")
                                return "File is not Compatible!";

                            if (j == 3 && item.ToString().Trim().ToLower() != "display randomization no")
                                return "File is not Compatible!";
                        }
                    }
                    if (!setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                    {
                        if (j > 2)
                        {
                            var r = projectDesignVisits.Exists(x => x.DisplayName.ToLower().Trim() == item.ToString().ToLower().Trim());
                            if (!r)
                                return "Visit name not match with design visit.";
                        }
                        else
                        {
                            if (j == 0 && item.ToString().Trim().ToLower() != "randomization no")
                                return "File is not Compatible!";

                            if (j == 1 && item.ToString().Trim().ToLower() != "product code")
                                return "File is not Compatible!";

                            if (j == 2 && item.ToString().Trim().ToLower() != "display randomization no")
                                return "File is not Compatible!";
                        }
                    }
                    if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == false)
                    {
                        if (j > 2)
                        {
                            var r = projectDesignVisits.Exists(x => x.DisplayName.ToLower().Trim() == item.ToString().ToLower().Trim());
                            if (!r)
                                return "Visit name not match with design visit.";
                        }
                        else
                        {
                            if (j == 0 && item.ToString().Trim().ToLower() != "randomization no")
                                return "File is not Compatible!";

                            if (j == 1 && item.ToString().Trim().ToLower() != "product code")
                                return "File is not Compatible!";

                            if (j == 2 && item.ToString().Trim().ToLower() != "kit no")
                                return "File is not Compatible!";
                        }
                    }
                    j++;
                }
                else
                {
                    selectQuery.Append("Column" + j + " is null or ");
                    if (j >= 2)
                    {
                        var r = projectDesignVisits.Exists(x => x.DisplayName.ToLower().Trim() == item.ToString().ToLower().Trim());
                        if (!r)
                            return "Visit name not match with design visit.";
                    }
                    else
                    {
                        if (j == 0 && item.ToString().Trim().ToLower() != "randomization no")
                            return "File is not Compatible!";

                        if (j == 1 && item.ToString().Trim().ToLower() != "product code")
                            return "File is not Compatible!";

                    }
                    j++;
                }

            }


            if (results.Tables[0].AsEnumerable().Where((row, index) => index > 4).ToList().Count > 0)
            {
                DataRow[] dr = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).CopyToDataTable().Select(selectQuery.ToString().Substring(0, selectQuery.Length - 3));
                if (dr.Length != 0)
                    return "Please fill required randomization details!";
                else if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                {

                    var kits = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).Select(k => k.Field<string>("Column2")).Distinct().OrderBy(k => k).ToArray();
                    foreach (string item in kits)
                    {
                        var kit = _context.SupplyManagementKITSeries.Where(x => x.DeletedDate == null && x.KitNo == item && x.ProjectId == supplyManagementUploadFile.ProjectId).FirstOrDefault();
                        if (kit != null)
                        {
                            return item + " kit number is already created or assigned";
                        }
                        int i = 0;
                        foreach (DataRow row in results.Tables[0].Rows)
                        {
                            if (row["Column2"].ToString() == item)
                            {
                                if (i > 0)
                                {
                                    return item + " Duplicate kit number found in sheet";
                                }
                                i++;
                            }
                        }
                    }

                    var randomizationNos = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).Select(k => k.Field<string>("Column3")).Distinct().OrderBy(k => k).ToArray();
                    foreach (string item in randomizationNos)
                    {
                        var uploadDetail = _context.SupplyManagementUploadFileDetail.Include(s => s.SupplyManagementUploadFile).Where(x => x.DeletedDate == null
                                  && x.DisplayRandomizationNumber == item && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve
                                  && x.SupplyManagementUploadFile.ProjectId == supplyManagementUploadFile.ProjectId).FirstOrDefault();
                        if (uploadDetail != null)
                        {
                            return item + " Randomization number is already created or assigned";
                        }

                        int i = 0;
                        foreach (DataRow row in results.Tables[0].Rows)
                        {
                            if (row["Column3"].ToString() == item)
                            {
                                if (i > 0)
                                {
                                    return item + " Duplicate display randomization number found in sheet";
                                }
                                i++;
                            }
                        }
                    }
                }
                else if (!setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                {
                    var randomizationNos = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).Select(k => k.Field<string>("Column2")).Distinct().OrderBy(k => k).ToArray();
                    foreach (string item in randomizationNos)
                    {
                        var uploadDetail = _context.SupplyManagementUploadFileDetail.Include(s => s.SupplyManagementUploadFile).Where(x => x.DeletedDate == null
                                  && x.DisplayRandomizationNumber == item && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve
                                  && x.SupplyManagementUploadFile.ProjectId == supplyManagementUploadFile.ProjectId).FirstOrDefault();
                        if (uploadDetail != null)
                        {
                            return item + " Randomization number is already created or assigned";
                        }

                        int i = 0;
                        foreach (DataRow row in results.Tables[0].Rows)
                        {
                            if (row["Column2"].ToString() == item)
                            {
                                if (i > 0)
                                {
                                    return item + " Duplicate display randomization number found in sheet";
                                }
                                i++;
                            }
                        }
                    }
                }
                else if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == false)
                {
                    var kits = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).Select(k => k.Field<string>("Column2")).Distinct().OrderBy(k => k).ToArray();
                    foreach (string item in kits)
                    {
                        var kit = _context.SupplyManagementKITSeries.Where(x => x.DeletedDate == null && x.KitNo == item && x.ProjectId == supplyManagementUploadFile.ProjectId).FirstOrDefault();
                        if (kit != null)
                        {
                            return item + " kit number is already created or assigned";
                        }
                        int i = 0;
                        foreach (DataRow row in results.Tables[0].Rows)
                        {
                            if (row["Column2"].ToString() == item)
                            {
                                if (i > 0)
                                {
                                    return item + " Duplicate kit number found in sheet";
                                }
                                i++;
                            }
                        }
                    }
                }
                return "";
            }
            else
            {
                return "Please fill required randomization details.";
            }
        }

        public string InserFileData(DataSet results, SupplyManagementUploadFile supplyManagementUploadFile, SupplyManagementKitNumberSettings setting)
        {

            DataTable dt = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).CopyToDataTable();
            var visitIds = GetVisitId(results.Tables[0].Rows[4].ItemArray.Where(x => x.ToString() != "").ToList(), supplyManagementUploadFile);
            if (dt.Rows.Count > 0)
            {
                Add(supplyManagementUploadFile);
                _context.Save();
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var supplyManagementUploadFileDetail = new SupplyManagementUploadFileDetail();
                supplyManagementUploadFileDetail.Id = 0;
                supplyManagementUploadFileDetail.SupplyManagementUploadFileId = supplyManagementUploadFile.Id;
                supplyManagementUploadFileDetail.RandomizationNo = Convert.ToInt32(dt.Rows[i][0]);
                supplyManagementUploadFileDetail.TreatmentType = dt.Rows[i][1].ToString();
                if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                {
                    supplyManagementUploadFileDetail.KitNo = dt.Rows[i][2].ToString();
                    supplyManagementUploadFileDetail.DisplayRandomizationNumber = dt.Rows[i][3].ToString();
                }
                if (!setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                {
                    supplyManagementUploadFileDetail.DisplayRandomizationNumber = dt.Rows[i][2].ToString();
                }
                if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == false)
                {
                    supplyManagementUploadFileDetail.KitNo = dt.Rows[i][2].ToString();
                }
                _supplyManagementUploadFileDetailRepository.Add(supplyManagementUploadFileDetail);
                _context.Save();

                supplyManagementUploadFileDetail.Visits = new List<SupplyManagementUploadFileVisit>();
                for (int j = 0; j < visitIds.Length; j++)
                {
                    var supplyManagementUploadFileVisit = new SupplyManagementUploadFileVisit();
                    supplyManagementUploadFileVisit.Id = 0;
                    supplyManagementUploadFileVisit.SupplyManagementUploadFileDetailId = supplyManagementUploadFileDetail.Id;
                    supplyManagementUploadFileVisit.ProjectDesignVisitId = visitIds[j];

                    if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                        supplyManagementUploadFileVisit.Value = dt.Rows[i][j + 4].ToString();
                    else if (!setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                        supplyManagementUploadFileVisit.Value = dt.Rows[i][j + 3].ToString();
                    else if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == false)
                        supplyManagementUploadFileVisit.Value = dt.Rows[i][j + 3].ToString();
                    else
                        supplyManagementUploadFileVisit.Value = dt.Rows[i][j + 2].ToString();

                    if (j == 0)
                    {
                        supplyManagementUploadFileVisit.Isfirstvisit = true;
                    }
                    else
                    {
                        supplyManagementUploadFileVisit.Isfirstvisit = false;
                    }
                    _supplyManagementUploadFileVisitRepository.Add(supplyManagementUploadFileVisit);
                    _context.Save();
                }
            }
            return "";
        }

        public int[] GetVisitId(List<object> dr, SupplyManagementUploadFile supplyManagementUploadFile)
        {
            var visits = GetProjectDesignVisit(supplyManagementUploadFile.ProjectId);
            List<int> visitIds = new List<int>();
            foreach (var item in dr)
            {
                var visit = visits.Find(x => x.DisplayName.ToString().Trim().ToLower() == item.ToString().Trim().ToLower());
                if (visit != null)
                    visitIds.Add(visit.Id);
            }
            return visitIds.ToArray();
        }

        public string RandomizationNumberValidation(DataTable dt, int projectId, SupplyManagementUploadFile supplyManagementUploadFileDetail, int? siteId, int? countryId)
        {
            // get last upload excel file max randomization number
            var maxRandomizationNo = new SupplyManagementUploadFileDetail();

            if (supplyManagementUploadFileDetail.SupplyManagementUploadFileLevel == Helper.SupplyManagementUploadFileLevel.Country)
            {
                // get last upload sheet data
                var supplyManagementUploadFile = All.Where(x => x.ProjectId == projectId && x.CountryId == countryId && x.Status != Helper.LabManagementUploadStatus.Reject && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();
                if (supplyManagementUploadFile == null)
                {
                    // check randomization number is serial no or not
                    var result = RandomizationNumberCheckSeries(dt, 0);
                    if (result != "")
                        return result;
                    else
                        return "";
                }
                maxRandomizationNo = _supplyManagementUploadFileDetailRepository.
                All.Where(x => x.SupplyManagementUploadFileId == supplyManagementUploadFile.Id && x.SupplyManagementUploadFile.CountryId == countryId).OrderByDescending(x => x.Id).FirstOrDefault();
            }
            if (supplyManagementUploadFileDetail.SupplyManagementUploadFileLevel == Helper.SupplyManagementUploadFileLevel.Site)
            {
                // get last upload sheet data
                var supplyManagementUploadFile = All.Where(x => x.ProjectId == projectId && x.SiteId == siteId && x.Status != Helper.LabManagementUploadStatus.Reject && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();
                if (supplyManagementUploadFile == null)
                {
                    // check randomization number is serial no or not
                    var result = RandomizationNumberCheckSeries(dt, 0);
                    if (result != "")
                        return result;
                    else
                        return "";
                }
                maxRandomizationNo = _supplyManagementUploadFileDetailRepository.
                All.Where(x => x.SupplyManagementUploadFileId == supplyManagementUploadFile.Id && x.SupplyManagementUploadFile.SiteId == siteId).OrderByDescending(x => x.Id).FirstOrDefault();
            }
            if (supplyManagementUploadFileDetail.SupplyManagementUploadFileLevel == Helper.SupplyManagementUploadFileLevel.Study)
            {
                // get last upload sheet data
                var supplyManagementUploadFile = All.Where(x => x.ProjectId == projectId && x.Status != Helper.LabManagementUploadStatus.Reject && x.DeletedDate == null).OrderByDescending(x => x.Id).FirstOrDefault();
                if (supplyManagementUploadFile == null)
                {
                    // check randomization number is serial no or not
                    var result = RandomizationNumberCheckSeries(dt, 0);
                    if (result != "")
                        return result;
                    else
                        return "";
                }
                maxRandomizationNo = _supplyManagementUploadFileDetailRepository.
                All.Where(x => x.SupplyManagementUploadFileId == supplyManagementUploadFile.Id).OrderByDescending(x => x.Id).FirstOrDefault();
            }
            int MaxNo = 0;

            if (maxRandomizationNo != null)
                MaxNo = maxRandomizationNo.RandomizationNo;

            // check randomization number is serial no or not
            var checkRandomizationNumber = RandomizationNumberCheckSeries(dt, MaxNo);
            if (checkRandomizationNumber != "")
                return checkRandomizationNumber;
            else
                return "";
        }

        public string RandomizationNumberCheckSeries(DataTable dt, int lastNo)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lastNo += 1;
                if (Convert.ToInt32(dt.Rows[i][0]) != lastNo)
                    return "Randomization Number not in a Proper Format!";
            }
            return "";
        }

        public string validateProductType(DataTable dt, int ProjectId, SupplyManagementKitNumberSettings setting)
        {
            var productTypes = _pharmacyStudyProductTypeRepository.All.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).
                Select(a => new { ProductTypeCode = a.ProductType.ProductTypeCode.ToString().Trim() }).Distinct().ToList();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string[] str = dt.Rows[i][1].ToString().TrimStart(' ').TrimEnd(' ').Trim().Split(',').ToArray();
                if (str.Length > 0)
                {
                    List<string> list = new List<string>();
                    string[] str1 = new string[str.Length];
                    foreach (var item in str)
                    {
                        var t1 = item.Replace("\n", " ");
                        var t2 = t1.TrimStart(' ');
                        var t3 = t2.TrimEnd(' ');
                        if (t3.Contains(@"\n"))
                        {
                            return "Remove space from product code.";
                        }
                        list.Add(t3);
                    }
                    str1 = list.ToArray();
                    var result = str1.ToList().TrueForAll(m => productTypes.Select(x => x.ProductTypeCode).Contains(m));

                    if (!result)
                        return "Product code not match.";

                    if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                    {
                        string[] dataRow = dt.Rows[i].ItemArray.Select(x => x.ToString()).Skip(1).Skip(1).Skip(1).Skip(1).Skip(1).ToArray();
                        var cellResult = dataRow.ToList().TrueForAll(m => str1.Contains(m.Trim()));

                        if (!cellResult)
                            return "Product code not match in cell.";
                    }
                    else if (!setting.IsUploadWithKit && setting.IsStaticRandomizationNo == true)
                    {
                        string[] dataRow = dt.Rows[i].ItemArray.Select(x => x.ToString()).Skip(1).Skip(1).Skip(1).Skip(1).ToArray();
                        var cellResult = dataRow.ToList().TrueForAll(m => str1.Contains(m.Trim()));

                        if (!cellResult)
                            return "Product code not match in cell.";
                    }
                    else if (setting.IsUploadWithKit && setting.IsStaticRandomizationNo == false)
                    {
                        string[] dataRow = dt.Rows[i].ItemArray.Select(x => x.ToString()).Skip(1).Skip(1).Skip(1).Skip(1).ToArray();
                        var cellResult = dataRow.ToList().TrueForAll(m => str1.Contains(m.Trim()));

                        if (!cellResult)
                            return "Product code not match in cell.";
                    }
                    else
                    {
                        string[] dataRow = dt.Rows[i].ItemArray.Select(x => x.ToString()).Skip(1).Skip(1).Skip(1).ToArray();
                        var cellResult = dataRow.ToList().TrueForAll(m => str1.Contains(m.Trim()));

                        if (!cellResult)
                            return "Product code not match in cell.";
                    }
                }

            }

            return "";
        }

        public bool CheckUploadApproalPending(int ProjectId, int SiteId, int CountryId)
        {
            if (SiteId == 0 && CountryId == 0)
                return All.Any(x => x.ProjectId == ProjectId && x.CountryId == CountryId && x.DeletedDate == null && x.Status == Helper.LabManagementUploadStatus.Pending);
            if (SiteId > 0)
                return All.Any(x => x.ProjectId == ProjectId && x.SiteId == SiteId && x.DeletedDate == null && x.Status == Helper.LabManagementUploadStatus.Pending);
            if (CountryId > 0)
                return All.Any(x => x.ProjectId == ProjectId && x.CountryId == CountryId && x.DeletedDate == null && x.Status == Helper.LabManagementUploadStatus.Pending);

            return false;

        }
        public void SendRandomizationUploadSheetEmail(SupplyManagementUploadFile obj)
        {
            SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
            IwrsEmailModel iWRSEmailModel = new IwrsEmailModel();

            var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive && x.ProjectId == obj.ProjectId && x.Triggers == SupplyManagementEmailTriggers.RandomizationSheetApprovedRejected).ToList();
            if (!emailconfiglist.Any())
                return;
            if (emailconfiglist.Count > 0)
            {

                emailconfig = emailconfiglist.FirstOrDefault();

                var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                if (details.Any())
                {
                    var project = _context.Project.Where(x => x.Id == obj.ProjectId).FirstOrDefault();
                    if (project != null)
                        iWRSEmailModel.StudyCode = project.ProjectCode;

                    var site = _context.Project.Where(x => x.Id == obj.SiteId).FirstOrDefault();
                    if (site != null)
                    {
                        iWRSEmailModel.SiteCode = site.ProjectCode;
                        var managesite = _context.ManageSite.Where(x => x.Id == site.ManageSiteId).FirstOrDefault();
                        if (managesite != null)
                        {
                            iWRSEmailModel.SiteName = managesite.SiteName;
                        }
                    }
                    iWRSEmailModel.Status = obj.Status == LabManagementUploadStatus.Approve ? "Approved" : "Rejected";
                    if (obj.CountryId > 0)
                    {
                        var country = _context.Country.Where(x => x.Id == obj.CountryId).FirstOrDefault();
                        if (country != null)
                            iWRSEmailModel.Country = country.CountryName;
                    }

                    _emailSenderRespository.SendforApprovalEmailIWRS(iWRSEmailModel, details.Select(x => x.Users.Email).Distinct().ToList(), emailconfig);
                    foreach (var item in details)
                    {
                        SupplyManagementEmailConfigurationDetailHistory history = new SupplyManagementEmailConfigurationDetailHistory();
                        history.SupplyManagementEmailConfigurationDetailId = item.Id;
                        _context.SupplyManagementEmailConfigurationDetailHistory.Add(history);
                        _context.Save();
                    }
                }
            }

        }
    }
}
