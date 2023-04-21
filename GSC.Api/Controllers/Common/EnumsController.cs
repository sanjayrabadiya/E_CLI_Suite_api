using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Data.Dto.Master;
using GSC.Helper;
using GSC.Shared.Extension;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Common
{
    [Route("api/[controller]")]
    public class EnumsController : BaseController
    {
        [HttpGet]
        [Route("GetGender")]
        public IActionResult GetGender()
        {
            var genders = Enum.GetValues(typeof(Gender))
                .Cast<Gender>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();

            return Ok(genders);
        }

        [HttpGet]
        [Route("GetFreezerType")]
        public IList<DropDownEnum> GetFreezerType()
        {
            return Enum.GetValues(typeof(FreezerType))
                .Cast<FreezerType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetUserType")]
        public IList<DropDownEnum> GetUserType()
        {
            var result = Enum.GetValues(typeof(UserType))
                .Cast<UserType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
            return result;
        }

        //[HttpGet]
        //[Route("GetRegulatoryType")]
        //public IList<DropDownEnum> GetRegulatoryType()
        //{
        //    return Enum.GetValues(typeof(RegulatoryType))
        //        .Cast<RegulatoryType>().Select(e => new DropDownEnum
        //        {
        //            Id = Convert.ToInt16(e),
        //            Value = e.GetDescription()
        //        }).OrderBy(o => o.Value).ToList();
        //}

        [HttpGet]
        [Route("GetCoreVariableType")]
        public IList<DropDownEnum> GetCoreVariableType()
        {
            return Enum.GetValues(typeof(CoreVariableType))
                .Cast<CoreVariableType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetRoleVariableType")]
        public IList<DropDownEnum> GetRoleVariableType()
        {
            return Enum.GetValues(typeof(RoleVariableType))
                .Cast<RoleVariableType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetPrintType")]
        public IList<DropDownEnum> GetPrintType()
        {
            return Enum.GetValues(typeof(PrintType))
                .Cast<PrintType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetDataType")]
        public IList<DropDownEnum> GetDataType()
        {
            return Enum.GetValues(typeof(DataType))
                .Cast<DataType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetValidationType")]
        public IList<DropDownEnum> GetValidationType()
        {
            return Enum.GetValues(typeof(ValidationType))
                .Cast<ValidationType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetCollectionSources")]
        public IList<DropDownEnum> GetCollectionSources()
        {
            return Enum.GetValues(typeof(CollectionSources))
                .Cast<CollectionSources>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetModules")]
        public IList<DropDownEnum> GetModules()
        {
            return Enum.GetValues(typeof(AuditModule))
                .Cast<AuditModule>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("ActivityMode")]
        public IList<DropDownEnum> ActivityMode()
        {
            return Enum.GetValues(typeof(ActivityMode))
                .Cast<ActivityMode>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("DateFormats")]
        public IList<DropDownEnum> DateFormats()
        {
            return Enum.GetValues(typeof(DateFormats))
                .Cast<DateFormats>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).ToList();
        }

        [HttpGet]
        [Route("TimeFormats")]
        public IList<DropDownEnum> TimeFormats()
        {
            return Enum.GetValues(typeof(TimeFormats))
                .Cast<TimeFormats>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).ToList();
        }

        [HttpGet]
        [Route("GetEditTypes")]
        public IList<DropDownEnum> GetEditTypes()
        {
            return Enum.GetValues(typeof(EditType))
                .Cast<EditType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("EditCheckValidations")]


        [HttpGet]
        [Route("ComparisonList")]
        public IList<DropDownEnum> ComparisonList()
        {
            return Enum.GetValues(typeof(Operator))
                .Cast<Operator>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("ComparisonWitohutCheckList")]
        public IList<DropDownEnum> ComparisonWitohutCheckList()
        {
            return Enum.GetValues(typeof(Operator))
                .Cast<Operator>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("EditCheckRuleBy")]
        public IList<DropDownEnum> EditCheckRuleBy()
        {
            return Enum.GetValues(typeof(EditCheckRuleBy))
                .Cast<EditCheckRuleBy>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("QueryStatus")]
        public IList<DropDownEnum> QueryStatus()
        {
            return Enum.GetValues(typeof(QueryStatus))
                .Cast<QueryStatus>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("ScreeningStatus")]
        public IList<DropDownEnum> ScreeningStatus()
        {
            return Enum.GetValues(typeof(ScreeningTemplateStatus))
                .Cast<ScreeningTemplateStatus>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("BlockndLotNo")]
        public IList<DropDownEnum> BlockndLotNo()
        {
            return Enum.GetValues(typeof(BlockndLotNo))
                .Cast<BlockndLotNo>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("ReceiptDateExpRet")]
        public IList<DropDownEnum> ReceiptDateExpRet()
        {
            return Enum.GetValues(typeof(ReceiptDateExpRet))
                .Cast<ReceiptDateExpRet>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetTrainingType")]
        public IActionResult GetTrainingType()
        {
            var traningTypes = Enum.GetValues(typeof(TrainigType))
                .Cast<TrainigType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();

            return Ok(traningTypes);
        }

        [HttpGet]
        [Route("GetFormType")]
        public IList<DropDownEnum> GetFormType()
        {
            return Enum.GetValues(typeof(FormType))
                .Cast<FormType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetBarcodeFor")]
        public IList<DropDownEnum> GetBarcodeFor()
        {
            return Enum.GetValues(typeof(BarcodeFor))
                .Cast<BarcodeFor>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetDateValidateType")]
        public IList<DropDownEnum> GetDateValidateType()
        {
            return Enum.GetValues(typeof(DateValidateType))
                .Cast<DateValidateType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("ProejctScheduleOperator")]
        public IList<DropDownEnum> ProejctScheduleOperator()
        {
            return Enum.GetValues(typeof(ProjectScheduleOperator))
                .Cast<ProjectScheduleOperator>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("DossierPdfType")]
        public IList<DropDownEnum> DossierPdfType()
        {
            return Enum.GetValues(typeof(DossierPdfType))
                .Cast<DossierPdfType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("DossierPdfStatus")]
        public IList<DropDownEnum> DossierPdfStatus()
        {
            return Enum.GetValues(typeof(DossierPdfStatus))
                .Cast<DossierPdfStatus>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("ScreeningPdfStatus")]
        public IList<DropDownEnum> ScreeningPdfStatus()
        {
            return Enum.GetValues(typeof(ScreeningPdfStatus))
                .Cast<ScreeningPdfStatus>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("CodedType")]
        public IList<DropDownEnum> CodedType()
        {
            return Enum.GetValues(typeof(CodedType))
                .Cast<CodedType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("ETMFMaterLibraryColumn")]
        public IList<DropDownEnum> ETMFMaterLibraryColumn()
        {
            return Enum.GetValues(typeof(ETMFMaterLibraryColumn))
                .Cast<ETMFMaterLibraryColumn>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription(),
                }).OrderBy(o => o.Id).ToList();
        }

        [HttpGet]
        [Route("WorkplaceFolderList")]
        public IList<DropDownEnum> WorkplaceFolderList()
        {
            return Enum.GetValues(typeof(WorkPlaceFolder))
                .Cast<WorkPlaceFolder>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription(),
                }).OrderBy(o => o.Id).ToList();
        }

        [HttpGet]
        [Route("WorkplaceStatus")]
        public IList<DropDownEnum> WorkplaceStatus()
        {
            return Enum.GetValues(typeof(WorkplaceStatus))
                .Cast<WorkplaceStatus>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription(),
                }).OrderBy(o => o.Id).ToList();
        }

        [HttpGet]
        [Route("HolidayType")]
        public IList<DropDownEnum> HolidayType()
        {
            return Enum.GetValues(typeof(HolidayType))
                .Cast<HolidayType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        //For Meddra
        [HttpGet]
        [Route("CommentStatus")]
        public IList<DropDownEnum> CommentStatus()
        {
            return Enum.GetValues(typeof(CommentStatus))
                .Cast<CommentStatus>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetRelationship")]
        public IList<DropDownEnum> GetRelationship()
        {
            return Enum.GetValues(typeof(Relationship))
                .Cast<Relationship>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("DBDSReportFilter")]
        public IList<DropDownEnum> DBDSReportFilter()
        {
            return Enum.GetValues(typeof(DBDSReportFilter))
                .Cast<DBDSReportFilter>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetAlignment")]
        public IList<DropDownEnum> GetAlignment()
        {
            return Enum.GetValues(typeof(Alignment))
                .Cast<Alignment>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetActivityType")]
        public IActionResult GetActivityType()
        {
            var refrencetype = Enum.GetValues(typeof(ActivityType))
                .Cast<ActivityType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();

            return Ok(refrencetype);
        }

        [HttpGet]
        [Route("GetRefrenceType")]
        public IActionResult GetRefrenceType()
        {
            var refrencetype = Enum.GetValues(typeof(RefrenceType))
                .Cast<RefrenceType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(refrencetype);
        }

        [HttpGet]
        [Route("GetDepotType")]
        public IActionResult GetDepotType()
        {
            var refrencetype = Enum.GetValues(typeof(DepotType))
                .Cast<DepotType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(refrencetype);
        }

        [HttpGet]
        [Route("GetDayType")]
        public IActionResult GetDayType()
        {
            var refrencetype = Enum.GetValues(typeof(DayType))
                .Cast<DayType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(refrencetype);
        }

        [HttpGet]
        [Route("GetFrequencyType")]
        public IActionResult GetFrequencyType()
        {
            var refrencetype = Enum.GetValues(typeof(FrequencyType))
                .Cast<FrequencyType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(refrencetype);
        }

        [HttpGet]
        [Route("GetDbdsReportType")]
        public IActionResult GetDbdsReportType()
        {
            var refrencetype = Enum.GetValues(typeof(DbdsReportType))
                .Cast<DbdsReportType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(refrencetype);
        }

        [HttpGet]
        [Route("GetProductUnitType")]
        public IActionResult GetProductUnitType()
        {
            var refrencetype = Enum.GetValues(typeof(ProductUnitType))
                .Cast<ProductUnitType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(refrencetype);
        }

        [HttpGet]
        [Route("GetUploadLimitType")]
        public IActionResult GetUploadLimit()
        {
            var uploadlimit = Enum.GetValues(typeof(UploadLimitType))
                .Cast<UploadLimitType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(uploadlimit);
        }


        [HttpGet]
        [Route("GetPatientStatus")]
        public IActionResult GetPatientStatus()
        {
            var uploadlimit = Enum.GetValues(typeof(ScreeningPatientStatus))
                .Cast<ScreeningPatientStatus>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(uploadlimit);
        }

        [HttpGet]
        [Route("GetICFAction")]
        public IActionResult GetICFAction()
        {
            var result = Enum.GetValues(typeof(ICFAction))
                .Cast<ICFAction>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("LabManagementExcelFileColumn")]
        public IList<DropDownEnum> LabManagementExcelFileColumn()
        {
            return Enum.GetValues(typeof(LabManagementExcelFileColumn))
                .Cast<LabManagementExcelFileColumn>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription(),
                }).OrderBy(o => o.Id).ToList();
        }

        [HttpGet]
        [Route("GetMonitoringSiteStatus")]
        public IList<DropDownEnum> GetMonitoringSiteStatus()
        {
            return Enum.GetValues(typeof(MonitoringSiteStatus))
                .Cast<MonitoringSiteStatus>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription(),
                }).OrderBy(o => o.Id).ToList();
        }

        [HttpGet]
        [Route("CRFTypes")]
        public IList<DropDownEnum> CRFTypes()
        {
            return Enum.GetValues(typeof(CRFTypes))
                .Cast<CRFTypes>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("PDFLayoutList")]
        public IList<DropDownEnum> PDFLayoutList()
        {
            return Enum.GetValues(typeof(PdfLayouts))
                .Cast<PdfLayouts>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetTableCollectionSource")]
        public IList<DropDownEnum> GetTableCollectionSource()
        {
            return Enum.GetValues(typeof(TableCollectionSource))
                .Cast<TableCollectionSource>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetScaleType")]
        public IList<DropDownEnum> GetScaleType()
        {
            return Enum.GetValues(typeof(ScaleType))
                .Cast<ScaleType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }

        [HttpGet]
        [Route("GetQueryTypes")]
        public IList<DropDownEnum> GetQueryTypes()
        {
            return Enum.GetValues(typeof(QueryTypes))
                .Cast<QueryTypes>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
        }
        [HttpGet]
        [Route("GetShipmentType")]
        public IActionResult GetShipmentType()
        {
            var refrencetype = Enum.GetValues(typeof(ShipmentType))
                .Cast<ShipmentType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(refrencetype);
        }
        [HttpGet]
        [Route("GetKitsStatusTypeDropdown")]
        public IActionResult GetKitsStatusTypeDropdown()
        {
            var refrencetype = Enum.GetValues(typeof(KitStatus))
                .Cast<KitStatus>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).Where(x => x.Id == 4 || x.Id == 5).OrderBy(o => o.Id).ToList();

            return Ok(refrencetype);
        }

        [HttpGet]
        [Route("GetScreeningReportType")]
        public IActionResult GetScreeningReportType()
        {
            var refrencetype = Enum.GetValues(typeof(ScreeningReport))
                .Cast<ScreeningReport>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(refrencetype);
        }

        [HttpGet]
        [Route("SupplyManagementAllocationType")]
        public IActionResult SupplyManagementAllocationType()
        {
            var refrencetype = Enum.GetValues(typeof(SupplyManagementAllocationType))
                .Cast<SupplyManagementAllocationType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(refrencetype);
        }

        [HttpGet]
        [Route("GetProjectStatus")]
        public IActionResult GetProjectStatus()
        {
            var refrencetype = Enum.GetValues(typeof(ProjectStatusEnum))
                .Cast<ProjectStatusEnum>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(refrencetype);
        }

        [HttpGet]
        [Route("GetFactors")]
        public IActionResult GetFactors()
        {
            var fectore = Enum.GetValues(typeof(Fector))
                .Cast<Fector>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(fectore);
        }
        [HttpGet]
        [Route("GetGenderFactors")]
        public IActionResult GetGenderFactors()
        {
            var fectore = Enum.GetValues(typeof(Gender))
                .Cast<Gender>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).Where(x => x.Id != 3).OrderBy(o => o.Id).ToList();

            return Ok(fectore);
        }

        [HttpGet]
        [Route("GetDaitoryFactors")]
        public IActionResult GetDaitoryFactors()
        {
            var fectore = Enum.GetValues(typeof(DaitoryFector))
                .Cast<DaitoryFector>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(fectore);
        }
        [HttpGet]
        [Route("GetJointfactor")]
        public IActionResult GetJointfactor()
        {
            var fectore = Enum.GetValues(typeof(Jointfactor))
                .Cast<Jointfactor>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(fectore);
        }
        [HttpGet]
        [Route("GetELigibilityfactor")]
        public IActionResult GetELigibilityfactor()
        {
            var fectore = Enum.GetValues(typeof(Eligibilityfactor))
                .Cast<Eligibilityfactor>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(fectore);
        }
        [HttpGet]
        [Route("GetFactorsOperators/{id}")]
        public IActionResult GetFactorsOperators(int id)
        {
            if (id == 1 || id == 2 || id == 5 || id == 6)
            {
                var fectore = Enum.GetValues(typeof(FectorOperator))
                    .Cast<FectorOperator>().Select(e => new DropDownEnum
                    {
                        Id = Convert.ToInt16(e),
                        Value = e.GetDescription()
                    }).Where(x => x.Id == 1).OrderBy(o => o.Id).ToList();

                return Ok(fectore);
            }
            else
            {
                var fectore = Enum.GetValues(typeof(FectorOperator))
                   .Cast<FectorOperator>().Select(e => new DropDownEnum
                   {
                       Id = Convert.ToInt16(e),
                       Value = e.GetDescription()
                   }).OrderBy(o => o.Id).ToList();

                return Ok(fectore);
            }

        }
        [HttpGet]
        [Route("GetFactorsTypes")]
        public IActionResult GetFactorsTypes()
        {
            var fectore = Enum.GetValues(typeof(FectoreType))
                .Cast<FectoreType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(fectore);
        }
        [HttpGet]
        [Route("GetKitStatusRandomization")]
        public IActionResult GetKitStatusRandomization()
        {
            var fectore = Enum.GetValues(typeof(KitStatusRandomization))
                .Cast<KitStatusRandomization>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).Where(x => x.Id == 1 || x.Id == 2 || x.Id == 3 || x.Id == 4 || x.Id == 7 || x.Id == 8 || x.Id == 9 || x.Id == 10 || x.Id == 11 || x.Id == 12).OrderBy(o => o.Id).ToList();

            return Ok(fectore);
        }
        [HttpGet]
        [Route("GetKitStatusDiscard")]
        public IActionResult GetKitStatusDiscard()
        {
            var fectore = Enum.GetValues(typeof(KitStatusRandomization))
                .Cast<KitStatusRandomization>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).Where(x => x.Id == 1 || x.Id == 2 || x.Id == 3 || x.Id == 5 || x.Id == 6 || x.Id == 7).OrderBy(o => o.Id).ToList();

            return Ok(fectore);
        }
        [HttpGet]
        [Route("GetSupplyManagementEmailTriggers")]
        public IActionResult GetSupplyManagementEmailTriggers()
        {
            var emailtrigger = Enum.GetValues(typeof(SupplyManagementEmailTriggers))
                .Cast<SupplyManagementEmailTriggers>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(emailtrigger);
        }
        [HttpGet]
        [Route("GetKitStatusForReturn/{statusName}")]
        public IActionResult GetKitStatusForReturn(string statusName)
        {
            if (statusName == "Missing")
            {
                var status = Enum.GetValues(typeof(KitStatus))
                    .Cast<KitStatus>().Select(e => new DropDownEnum
                    {
                        Id = Convert.ToInt16(e),
                        Value = e.GetDescription()
                    }).Where(x => x.Id == 12 || x.Id == 13).OrderBy(o => o.Id).ToList();

                return Ok(status);
            }
            if (statusName == "Damaged" || statusName == "Used")
            {
                var status = Enum.GetValues(typeof(KitStatus))
                    .Cast<KitStatus>().Select(e => new DropDownEnum
                    {
                        Id = Convert.ToInt16(e),
                        Value = e.GetDescription()
                    }).Where(x => x.Id == 11).OrderBy(o => o.Id).ToList();

                return Ok(status);
            }
            if (statusName == "Unused")
            {
                var status = Enum.GetValues(typeof(KitStatus))
                    .Cast<KitStatus>().Select(e => new DropDownEnum
                    {
                        Id = Convert.ToInt16(e),
                        Value = e.GetDescription()
                    }).Where(x => x.Id == 12 || x.Id == 13 || x.Id == 14 || x.Id == 15).OrderBy(o => o.Id).ToList();

                return Ok(status);
            }
            return Ok();
        }

        [HttpGet]
        [Route("GetKitCreationType")]
        public IActionResult GetKitCreationType()
        {
            var fectore = Enum.GetValues(typeof(KitCreationType))
                .Cast<KitCreationType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(fectore);
        }


        [HttpGet]
        [Route("GetPKBarcodeOption")]
        public IActionResult GetPKBarcodeOption()
        {
            var pkenum = Enum.GetValues(typeof(PKBarcodeOption))
                .Cast<PKBarcodeOption>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(pkenum);
        }

        [HttpGet]
        [Route("GetBarcodeGenerationType")]
        public IActionResult GetBarcodeGenerationType()
        {
            var pkenum = Enum.GetValues(typeof(BarcodeGenerationType))
                .Cast<BarcodeGenerationType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(pkenum);
        }
        [HttpGet]
        [Route("GetProductAccountabilityActions")]
        public IActionResult ProductAccountabilityActions()
        {
            var pkenum = Enum.GetValues(typeof(ProductAccountabilityActions))
                .Cast<ProductAccountabilityActions>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(pkenum);
        }

        [HttpGet]
        [Route("GetCentrifugationFilter")]
        public IActionResult GetCentrifugationFilter()
        {
            var centri = Enum.GetValues(typeof(CentrifugationFilter))
                .Cast<CentrifugationFilter>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Id).ToList();

            return Ok(centri);
        }

    }
}