using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementUploadFileRepository : GenericRespository<SupplyManagementUploadFile>, ISupplyManagementUploadFileRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly ISupplyManagementUploadFileVisitRepository _supplyManagementUploadFileVisitRepository;
        private readonly ISupplyManagementUploadFileDetailRepository _supplyManagementUploadFileDetailRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IPharmacyStudyProductTypeRepository _pharmacyStudyProductTypeRepository;

        public SupplyManagementUploadFileRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IUploadSettingRepository uploadSettingRepository,
            ISupplyManagementUploadFileVisitRepository supplyManagementUploadFileVisitRepository,
            ISupplyManagementUploadFileDetailRepository supplyManagementUploadFileDetailRepository,
            IPharmacyStudyProductTypeRepository pharmacyStudyProductTypeRepository,
        IProjectDesignVisitRepository projectDesignVisitRepository,
             IProjectRepository projectRepository,
         ICountryRepository countryRepository,
        IMapper mapper)
            : base(context)
        {
            _uploadSettingRepository = uploadSettingRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _supplyManagementUploadFileVisitRepository = supplyManagementUploadFileVisitRepository;
            _supplyManagementUploadFileDetailRepository = supplyManagementUploadFileDetailRepository;
            _pharmacyStudyProductTypeRepository = pharmacyStudyProductTypeRepository;
            _projectRepository = projectRepository;
            _countryRepository = countryRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<SupplyManagementUploadFileGridDto> GetSupplyManagementUploadFileList(bool isDeleted, int ProjectId)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementUploadFileGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }


        // Upload excel data insert into database
        public string InsertExcelDataIntoDatabaseTable(SupplyManagementUploadFile supplyManagementUploadFile)
        {
            // check level for the upload file
            var validate = All.Where(x => x.ProjectId == supplyManagementUploadFile.ProjectId && x.Status != Helper.LabManagementUploadStatus.Reject && x.DeletedDate == null).FirstOrDefault();
            if (validate != null)
                if (validate.SupplyManagementUploadFileLevel != supplyManagementUploadFile.SupplyManagementUploadFileLevel)
                    return "Please Select Appropriate Level.";

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
            if (results != null && results.Tables.Count > 0 && results.Tables[0].Rows.Count == 0)
            {
                return "File is not Compatible";
            }
            // validate excel file
            var isValid = validateExcel(results, supplyManagementUploadFile);

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

                // check visit with older upload excel sheet
                var visitMatch = matchVisitWithOlderUpload(results, supplyManagementUploadFile.ProjectId, validate);
                if (visitMatch != "")
                    return visitMatch;
            }
            else
            {
                // check randomization number for first file uploaded
                var isValidRandomizationNoInSerial = RandomizationNumberCheckSeries(dt, 0);
                if (isValidRandomizationNoInSerial != "")
                    return isValidRandomizationNoInSerial;
            }

            // validate product type
            var productType = validateProductType(dt, supplyManagementUploadFile.ProjectId);
            if (productType != "")
                return productType;

            // insert data into table if excel is valid
            InserFileData(results, supplyManagementUploadFile);

            return "";
        }

        // Generate excecl format for download
        public FileStreamResult DownloadFormat(SupplyManagementUploadFileDto supplyManagementUploadFile)
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

                var j = 3;
                projectDesignVisits.ForEach(d =>
                {
                    worksheet.Row(5).Cell(j).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    worksheet.Row(5).Cell(j).Style.Font.SetBold();
                    worksheet.Row(5).Cell(j).SetValue(d.DisplayName);
                    j++;
                });

                worksheet.Row(1).Cell(2).SetValue(studyCode);
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
                                        && x.DeletedDate == null).ToList();
        }

        public string validateExcel(DataSet results, SupplyManagementUploadFile supplyManagementUploadFile)
        {
            var study = results.Tables[0].Rows[0].ItemArray[1].ToString().Trim();
            var country = results.Tables[0].Rows[1].ItemArray[1].ToString().Trim();
            var site = results.Tables[0].Rows[2].ItemArray[1].ToString().Trim();

            if (study.Trim().ToLower() != Convert.ToString(GetProjectCode(supplyManagementUploadFile.ProjectId)).ToLower())
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

            string selectQuery = "";
            var projectDesignVisits = GetProjectDesignVisit(supplyManagementUploadFile.ProjectId);
            var j = 0;
            var visitcheck = results;
            var table = visitcheck.Tables[0].Rows[4].ItemArray.Where(x => x.ToString() != "").ToList();
            var duplicates = table.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            if (duplicates.Count > 0)
            {
                return "Visit name should not be duplicate.";
            }
            //var visitcount = visitcheck.Tables[0].Rows[4].ItemArray.Where(x => x.ToString() != "").Count();
            //if (projectDesignVisits.Count() != (visitcount - 2))
            //    return "Visit name not match with design visit.";

            foreach (var item in results.Tables[0].Rows[4].ItemArray.Where(x => x.ToString() != ""))
            {
                selectQuery += "Column" + j + " is null or ";
                if (j >= 2)
                {
                    var r = projectDesignVisits.Any(x => x.DisplayName.ToLower().Trim() == item.ToString().ToLower().Trim());
                    if (!r)
                        return "Visit name not match with design visit.";
                }
                else
                {
                    if (j == 0)
                        if (item.ToString().Trim().ToLower() != "randomization no")
                            return "File is not Compatible!";
                    if (j == 1)
                        if (item.ToString().Trim().ToLower() != "product code")
                            return "File is not Compatible!";
                }
                j++;
            }


            if (results.Tables[0].AsEnumerable().Where((row, index) => index > 4).ToList().Count > 0)
            {
                DataRow[] dr = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).CopyToDataTable().Select(selectQuery.Substring(0, selectQuery.Length - 3));
                if (dr.Length != 0)
                    return "Please fill required randomization details!";
                else
                    return "";
                //else
                //{
                //    for (int i = 5; i < results.Tables[0].Rows.Count; i++)
                //    {
                //        if (Convert.ToString(results.Tables[0].Rows[i][1]).Contains(','))
                //        {
                //            var arr2 = Convert.ToString(results.Tables[0].Rows[i][1]).Split(',').ToArray();
                //            var arr1 = results.Tables[0].Rows[i].ItemArray;
                //            var arr3 = results.Tables[0].Rows[i].ItemArray.Count();

                //            if (arr2.Length != (arr3 - 2))
                //            {
                //                return "Product code with visit sequence not matched";

                //            }

                //            for (var k = 0; k < arr2.Length; k++)
                //            {
                //                if (arr1[2 + k].ToString().ToLower() != arr2[k].ToLower())
                //                {
                //                    return "Product code with visit sequence not matched at row no " + (i + 1).ToString();
                //                }
                //            }


                //        }

                //    }
                //    return "";
                //}
            }
            else
                return "Please fill required randomization details.";




        }

        public string InserFileData(DataSet results, SupplyManagementUploadFile supplyManagementUploadFile)
        {
            DataTable dt = results.Tables[0].AsEnumerable().Where((row, index) => index > 4).CopyToDataTable();
            var visitIds = GetVisitId(results.Tables[0].Rows[4].ItemArray.Where(x => x.ToString() != "").ToList(), supplyManagementUploadFile);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var supplyManagementUploadFileDetail = new SupplyManagementUploadFileDetail();
                supplyManagementUploadFileDetail.Id = 0;
                supplyManagementUploadFileDetail.RandomizationNo = Convert.ToInt32(dt.Rows[i][0]);
                supplyManagementUploadFileDetail.TreatmentType = dt.Rows[i][1].ToString();

                _supplyManagementUploadFileDetailRepository.Add(supplyManagementUploadFileDetail);
                supplyManagementUploadFile.Details.Add(supplyManagementUploadFileDetail);

                supplyManagementUploadFileDetail.Visits = new List<SupplyManagementUploadFileVisit>();
                for (int j = 0; j < visitIds.Length; j++)
                {
                    var supplyManagementUploadFileVisit = new SupplyManagementUploadFileVisit();
                    supplyManagementUploadFileVisit.Id = 0;
                    supplyManagementUploadFileVisit.ProjectDesignVisitId = visitIds[j];
                    supplyManagementUploadFileVisit.Value = dt.Rows[i][j + 2].ToString();
                    _supplyManagementUploadFileVisitRepository.Add(supplyManagementUploadFileVisit);
                    supplyManagementUploadFileDetail.Visits.Add(supplyManagementUploadFileVisit);
                }
            }

            //Add supply Management Upload file Data
            Add(supplyManagementUploadFile);
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

        public string validateProductType(DataTable dt, int ProjectId)
        {
            var productTypes = _pharmacyStudyProductTypeRepository.All.Where(x => x.ProjectId == ProjectId).
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
                    var result = str1.All(m => productTypes.Select(x => x.ProductTypeCode).Contains(m));
                    var columns = dt.Columns.Count;

                    if (!result)
                        return "Product code not match.";

                    string[] dataRow = dt.Rows[i].ItemArray.Select(x => x.ToString()).Skip(1).Skip(1).ToArray();
                    var cellResult = dataRow.All(m => str1.Contains(m.Trim()));

                    if (!cellResult)
                        return "Product code not match in cell.";
                }
               
            }

            return "";
        }

        public string matchVisitWithOlderUpload(DataSet results, int projectId, SupplyManagementUploadFile supplyManagementUploadFile)
        {
            // get last upload sheet data
            //var supplyManagementUploadDetail = _supplyManagementUploadFileDetailRepository.All.Where(x => x.SupplyManagementUploadFileId == supplyManagementUploadFile.Id)
            //.OrderByDescending(x => x.Id).FirstOrDefault();

            //var supplyManagementUploadDetailVisit = _supplyManagementUploadFileVisitRepository.All.Include(x => x.ProjectDesignVisit).Where(x => x.SupplyManagementUploadFileDetailId == supplyManagementUploadDetail.Id)
            //    .Select(x => x.ProjectDesignVisit.DisplayName)
            //.ToList();

            //if (supplyManagementUploadDetailVisit.Count() != results.Tables[0].Rows[4].ItemArray.Where(x => x.ToString() != "").Count() - 2)
            //    return "visit not match with previous upload file.";

            //var j = 0;
            //foreach (var item in results.Tables[0].Rows[4].ItemArray.Where(x => x.ToString() != ""))
            //{
            //    if (j >= 2)
            //    {
            //        if (!supplyManagementUploadDetailVisit.Contains(item.ToString(), StringComparer.InvariantCultureIgnoreCase))
            //            return "visit not match with previous upload file.";
            //    }
            //    j++;
            //}
            return "";
        }
    }
}
