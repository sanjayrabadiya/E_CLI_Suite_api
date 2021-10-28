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
    }
}