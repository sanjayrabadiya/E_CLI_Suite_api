
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementFectorDetailRepository : GenericRespository<SupplyManagementFectorDetail>, ISupplyManagementFectorDetailRepository
    {

        private readonly IGSCContext _context;

        public SupplyManagementFectorDetailRepository(IGSCContext context)
            : base(context)
        {

            _context = context;
        }
        public SupplyManagementFectorDto GetDetailList(int id)
        {
            var data = _context.SupplyManagementFector.Include(x => x.FectorDetailList).Where(x => x.Id == id).Select(x => new SupplyManagementFectorDto
            {
                Id = x.Id,
                SourceFormula = x.SourceFormula,
                CheckFormula = x.CheckFormula,
                ErrorMessage = x.ErrorMessage,
                SampleResult = x.SampleResult,
                Children = x.FectorDetailList.Where(x => x.DeletedDate == null).Select(z => new SupplyManagementFectorDetailDto
                {
                    Id = z.Id,
                    SupplyManagementFectorId = z.SupplyManagementFectorId,
                    ProductTypeCode = z.ProductTypeCode,
                    Fector = z.Fector,
                    Operator = z.Operator,
                    CollectionValue = z.CollectionValue,
                    LogicalOperator = z.LogicalOperator,
                    Ratio = z.Ratio,
                    FactoreName = z.Fector.GetDescription(),
                    FactoreOperatorName = z.Operator.GetDescription(),
                    collectionValueName = GetCollectionValue(z.Fector, z.CollectionValue),
                    Type = z.Type,
                    TypeName = z.Type.GetDescription(),
                    IsDeleted = z.DeletedDate != null,
                    ProjectCode = z.SupplyManagementFector.Project.ProjectCode,
                    StartParens = z.StartParens,
                    EndParens = z.EndParens
                }).ToList()
            }).FirstOrDefault();
            return data;
        }
        public SupplyManagementFectorDetailDto GetDetail(int id)
        {

            var data = All.Where(x => x.Id == id).Select(z => new SupplyManagementFectorDetailDto
            {
                Id = z.Id,
                SupplyManagementFectorId = z.SupplyManagementFectorId,
                ProductTypeCode = z.ProductTypeCode,
                Fector = z.Fector,
                Operator = z.Operator,
                CollectionValue = z.CollectionValue,
                LogicalOperator = z.LogicalOperator,
                Ratio = z.Ratio,
                FactoreName = z.Fector.GetDescription(),
                FactoreOperatorName = z.Operator.ToString(),
                collectionValueName = GetCollectionValue(z.Fector, z.CollectionValue),
                Type = z.Type,
                TypeName = z.Type.GetDescription(),
                StartParens = z.StartParens,
                EndParens = z.EndParens
            }).FirstOrDefault();
            return data;
        }

        public static string GetCollectionValue(Fector fector, string Collectionavalue)
        {
            if (fector == Fector.Gender && !string.IsNullOrEmpty(Collectionavalue))
            {
                if (Collectionavalue == "1")
                {
                    return "Male";
                }
                if (Collectionavalue == "2")
                {
                    return "Female";
                }
            }
            if (fector == Fector.Diatory && !string.IsNullOrEmpty(Collectionavalue))
            {
                if (Collectionavalue == "1")
                {
                    return "Veg";
                }
                if (Collectionavalue == "2")
                {
                    return "Non-veg";
                }
            }
            if (fector == Fector.Joint && !string.IsNullOrEmpty(Collectionavalue))
            {
                if (Collectionavalue == "1")
                {
                    return "Knee";
                }
                if (Collectionavalue == "2")
                {
                    return "Low Back";
                }
            }
            if (fector == Fector.Eligibility && !string.IsNullOrEmpty(Collectionavalue))
            {
                if (Collectionavalue == "1")
                {
                    return "Yes";
                }
                if (Collectionavalue == "2")
                {
                    return "No";
                }
            }
            if (fector == Fector.BMI || fector == Fector.Age || fector == Fector.Weight || fector == Fector.Dose)
            {
                return Collectionavalue;
            }
            return "";

        }

        public bool CheckType(SupplyManagementFectorDetailDto supplyManagementFectorDetailDto)
        {
            var data = _context.SupplyManagementFectorDetail.Where(x => x.SupplyManagementFectorId == supplyManagementFectorDetailDto.SupplyManagementFectorId).FirstOrDefault();
            if (data != null && data.Type != supplyManagementFectorDetailDto.Type)
            {
                return false;
            }
            return true;
        }
        public bool CheckrandomizationStarted(int id)
        {
            var SupplyManagementFector = _context.SupplyManagementFector.Where(x => x.Id == id).FirstOrDefault();
            var randomization = _context.Randomization.Where(x => x.Project.ParentProjectId == SupplyManagementFector.ProjectId
            && x.RandomizationNumber != null).FirstOrDefault();

            if (randomization != null)
            {
                return false;
            }
            return true;
        }

        public bool CheckUploadRandomizationsheet(SupplyManagementFectorDetailDto supplyManagementFectorDetailDto)
        {
            var randomization = _context.SupplyManagementUploadFile.Where(x => x.ProjectId == supplyManagementFectorDetailDto.ProjectId
            && x.Status == LabManagementUploadStatus.Approve && x.DeletedDate == null).FirstOrDefault();
            if (randomization != null && supplyManagementFectorDetailDto.Type == FectoreType.StudyLevel && randomization.SupplyManagementUploadFileLevel != SupplyManagementUploadFileLevel.Study)
            {
                return false;
            }
            if (randomization != null && supplyManagementFectorDetailDto.Type == FectoreType.SiteLevel && randomization.SupplyManagementUploadFileLevel != SupplyManagementUploadFileLevel.Site)
            {
                return false;
            }
            if (randomization != null && supplyManagementFectorDetailDto.Type == FectoreType.CountryLevel && randomization.SupplyManagementUploadFileLevel != SupplyManagementUploadFileLevel.Country)
            {
                return false;
            }
            return true;
        }
    }
}
