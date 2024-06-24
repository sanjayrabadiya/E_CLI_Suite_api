using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GSC.Shared.Extension;
using GSC.Helper;


namespace GSC.Respository.CTMS
{
    public class StudyPlanResourceRepository : GenericRespository<StudyPlanResource>, IStudyPlanResourceRepository
    {
        private readonly IGSCContext _context;

        public StudyPlanResourceRepository(IGSCContext context) : base(context)
        {
            _context = context;
        }
        public dynamic GetTaskResourceList(bool isDeleted, int studyPlanTaskId)
        {
            var gridResource = _context.StudyPlanResource.Include(r => r.ResourceType).Where(x => x.StudyPlanTaskId == studyPlanTaskId && x.DeletedDate == null && x.ResourceType.ResourceTypes == ResourceTypeEnum.Manpower)
               .Select(c => new ResourceTypeGridDto
               {
                   Id = c.Id,
                   ResourceType = c.ResourceType.ResourceTypes.GetDescription(),
                   ResourceSubType = c.ResourceType.ResourceSubType.GetDescription(),
                   Role = c.ResourceType.Role.RoleName,
                   User = c.ResourceType.User.UserName,
                   UserId = c.ResourceType.UserId,
                   SecurityRoleId = c.ResourceType.RoleId,
                   NameOfMaterial = c.ResourceType.NameOfMaterial != "" ? c.ResourceType.NameOfMaterial : " - ",
                   CreatedDate = c.CreatedDate,
                   CreatedByUser = c.CreatedByUser.UserName,
               }).ToList();

            return gridResource;
        }
        public string Duplicate(StudyPlanResource objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.StudyPlanTaskId == objSave.StudyPlanTaskId && x.ResourceTypeId == objSave.ResourceTypeId && x.DeletedDate == null))
                return "Duplicate Resource ";
            return "";
        }
        public dynamic ResourceById(int id)
        {
            var gridResource = _context.StudyPlanResource.Include(r => r.ResourceType).ThenInclude(d => d.Designation).Where(x => x.Id == id && x.DeletedDate == null)
               .Select(c => new ResourceByEdit
               {
                   resourceId = (c.ResourceType.ResourceTypes.GetDescription() == "Manpower") ? 1 : 2,
                   subresource = (c.ResourceType.ResourceSubType.GetDescription() == "Permanent") ? 1 : c.ResourceType.ResourceSubType.GetDescription() == "Contract" ? 2 : c.ResourceType.ResourceSubType.GetDescription() == "Consumable" ? 3 : 4,
                   designation = c.ResourceType.Designation.Id,
                   nameOfMaterial = c.ResourceType.Id,
                   rollUser = c.ResourceType.Id,
                   Cost = c.ResourceType.Cost,
                   NoOfUnit = c.NoOfUnit,
                   TotalCost = c.TotalCost,
                   Unit = c.ResourceType.Unit.UnitName,
                   ConvertTotalCost = c.ConvertTotalCost,
                   Discount = c.Discount,
                   ResourceUnit = c.ResourceType.Unit.UnitName,
                   GlobalCurrency = _context.Currency.Where(s => s.Id == _context.StudyPlan.Where(s => s.Id == c.StudyPlanTask.StudyPlanId && s.DeletedBy == null).Select(s => s.CurrencyId).FirstOrDefault()).Select(r => r.CurrencyName + " - " + r.CurrencySymbol).FirstOrDefault(),
               }).FirstOrDefault();

            return gridResource;

        }
        public string ValidationCurrency(int resourceId, int studyplanId)
        {
            var resourceCurrency = _context.ResourceType.Include(r => r.Currency).Where(s => s.Id == resourceId && s.DeletedBy == null).FirstOrDefault();
            var globalCurrencyId = _context.StudyPlan.Where(s => s.Id == studyplanId && s.DeletedBy == null).Select(d => d.CurrencyId).FirstOrDefault();

            if(resourceCurrency != null && globalCurrencyId != null)
            {
                if (!_context.CurrencyRate.Where(s => s.StudyPlanId == studyplanId && s.CurrencyId == resourceCurrency.CurrencyId && s.DeletedBy == null).Any() && resourceCurrency.CurrencyId != globalCurrencyId)
                {
                    return resourceCurrency.Currency.CurrencyName + " - " + resourceCurrency.Currency.CurrencySymbol + " Is Currency And Rate Added in Study plan. ";
                }
            }
            return "";
        }
        public dynamic GetResourceInf(int studyPlantaskId, int resourceId)
        {
            //var studyPlanData = _context.StudyPlanTask.Include(x => x.StudyPlan).Where(s => s.Id == studyPlantaskId && s.DeletedBy == null).FirstOrDefault();
            //var ResourceType = _context.ResourceType.Include(s => s.Unit).Where(x => x.Id == resourceId && x.DeletedDate == null)
            //   .Select(c => new ResourceTypeGridDto
            //   {
            //       Id = c.Id,
            //       Unit = c.Unit.UnitName,
            //       Cost = c.Cost,
            //       NumberOfUnit = c.NumberOfUnit,
            //       ResourceType = c.ResourceTypes.GetDescription(),
            //       CurrencyType = c.Currency.CurrencyName + " - " + c.Currency.CurrencySymbol,
            //       GlobalCurrency = _context.Currency.Where(s => s.Id == studyPlanData.StudyPlan.CurrencyId && s.DeletedBy == null).Select(d => d.CurrencyName + " - " + d.CurrencySymbol).FirstOrDefault(),
            //       LocalCurrencySymbol = c.Currency.CurrencySymbol,
            //       GlobalCurrencySymbol = _context.Currency.Where(s => s.Id == studyPlanData.StudyPlan.CurrencyId && s.DeletedBy == null).Select(d => d.CurrencySymbol).FirstOrDefault(),
            //       LocalCurrencyRate = _context.CurrencyRate.Where(s => s.StudyPlanId == studyPlanData.StudyPlanId && s.CurrencyId == c.CurrencyId && s.DeletedBy == null).Select(r => r.LocalCurrencyRate).FirstOrDefault(),
            //   }).FirstOrDefault();

            var studyPlanData = _context.StudyPlan.Where(s => s.Id == studyPlantaskId && s.DeletedBy == null).FirstOrDefault();
            var ResourceType = _context.ResourceType.Include(s => s.Unit).Where(x => x.Id == resourceId && x.DeletedDate == null)
               .Select(c => new ResourceTypeGridDto
               {
                   Id = c.Id,
                   Unit = c.Unit.UnitName,
                   Cost = c.Cost,
                   NumberOfUnit = c.NumberOfUnit,
                   ResourceType = c.ResourceTypes.GetDescription(),
                   CurrencyType = c.Currency.CurrencyName + " - " + c.Currency.CurrencySymbol,
                   GlobalCurrency = _context.Currency.Where(s => s.Id == studyPlanData.CurrencyId && s.DeletedBy == null).Select(d => d.CurrencyName + " - " + d.CurrencySymbol).FirstOrDefault(),
                   LocalCurrencySymbol = c.Currency.CurrencySymbol,
                   GlobalCurrencySymbol = _context.Currency.Where(s => s.Id == studyPlanData.CurrencyId && s.DeletedBy == null).Select(d => d.CurrencySymbol).FirstOrDefault(),
                   LocalCurrencyRate = _context.CurrencyRate.Where(s => s.StudyPlanId == studyPlanData.Id && s.CurrencyId == c.CurrencyId && s.DeletedBy == null).Select(r => r.LocalCurrencyRate).FirstOrDefault(),
               }).FirstOrDefault();

            return ResourceType;
        }

        public void TotalCostUpdate(StudyPlanResource studyPlanResource)
        {
            var TotalCost = _context.StudyPlanResource.Where(s => s.StudyPlanTaskId == studyPlanResource.StudyPlanTaskId && s.DeletedBy == null).Sum(d => d.ConvertTotalCost);
            var StudyPlanTaskData = _context.StudyPlanTask.Where(s => s.Id == studyPlanResource.StudyPlanTaskId && s.DeletedBy == null).FirstOrDefault();
            if (StudyPlanTaskData != null)
            {
                StudyPlanTaskData.TotalCost = TotalCost;
                _context.StudyPlanTask.UpdateRange(StudyPlanTaskData);
                _context.Save();
            }
        }

    }
}

