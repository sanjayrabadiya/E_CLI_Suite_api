using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class ResourceTypeRepository : GenericRespository<ResourceType>, IResourceTypeRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public ResourceTypeRepository(IGSCContext context, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }

        public string Duplicate(ResourceType objSave)
        {
            var exists = All.Any(x => x.Id != objSave.Id && x.ResourceCode == objSave.ResourceCode.Trim() && x.DeletedDate == null);
            return exists ? $"Duplicate Resource Code : {objSave.ResourceCode}" : string.Empty;
        }

        public List<ResourceTypeGridDto> GetResourceTypeList(bool isDeleted)
        {
            var resourcetype= All.Where(x => x.DeletedDate != null == isDeleted)
                      .ProjectTo<ResourceTypeGridDto>(_mapper.ConfigurationProvider)
                      .OrderByDescending(x => x.Id)
                      .ToList();
            resourcetype.ForEach(x =>
            {
                x.User = x.ResourceType == "Material" ? "NA" : x.User;
                x.Designation = x.Designation == null ? "NA" : x.Designation;
                x.YersOfExperience = x.YersOfExperience == null ? "NA" : x.YersOfExperience;
                x.Role = x.Role == null ? "NA" : x.Role;
                x.User = x.Role == null ? "NA" : x.Role;
                x.NameOfMaterial = x.NameOfMaterial == "" ? "NA" : x.NameOfMaterial;
                x.OwnerName = x.OwnerName == "" ? "NA" : x.NameOfMaterial;
            }
            );
            return resourcetype;
        }

        public List<DropDownDto> GetUnitTypeDropDown()
        {
            return _context.Unit.Where(x => x.DeletedBy == null)
                                .Select(c => new DropDownDto { Id = c.Id, Value = c.UnitName, IsDeleted = c.DeletedDate != null })
                                .OrderBy(o => o.Value)
                                .ToList();
        }

        public List<DropDownDto> GetDesignationDropDown()
        {
            return _context.Designation.Where(x => x.DeletedBy == null)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.NameOFDesignation + " - " + c.YersOfExperience + " years of Experience", IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetDesignationDropDown(int resourceTypeID, int resourceSubTypeID, int projectId)
        {
            var query = _context.ResourceType.Include(s => s.Designation)
                                             .Where(x => x.DeletedBy == null && (int)x.ResourceTypes == resourceTypeID && (int)x.ResourceSubType == resourceSubTypeID && x.DesignationId != null);

            if (projectId != 0)
            {
                var userAccessData = _context.UserAccess.Include(x => x.UserRole)
                                                        .Where(s => s.ProjectId == projectId && s.DeletedBy == null)
                                                        .Select(y => (int?)y.UserRole.UserId)
                                                        .ToList();

                query = query.Where(x => userAccessData.Contains(x.UserId));
            }

            return query.Select(c => new DropDownDto { Id = c.Designation.Id, Value = c.Designation.NameOFDesignation + " - " + c.Designation.YersOfExperience + " years of Experience", IsDeleted = c.DeletedDate != null })
                        .Distinct()
                        .OrderBy(o => o.Value)
                        .ToList();
        }

        public List<DropDownDto> GetNameOfMaterialDropDown(int resourceTypeID, int resourceSubTypeID)
        {
            return _context.ResourceType.Where(x => x.DeletedBy == null && (int)x.ResourceTypes == resourceTypeID && (int)x.ResourceSubType == resourceSubTypeID)
                                        .Select(c => new DropDownDto { Id = c.Id, Value = c.NameOfMaterial, IsDeleted = c.DeletedDate != null })
                                        .OrderBy(o => o.Value)
                                        .ToList();
        }

        public List<DropDownDto> GetRollUserDropDown(int designationID, int projectId)
        {
            var query = _context.ResourceType.Include(s => s.Designation)
                                             .Where(x => x.DeletedBy == null && x.DesignationId == designationID && x.RoleId != null);

            if (projectId != 0)
            {
                var userAccessData = _context.UserAccess.Include(x => x.UserRole)
                                                        .Where(s => s.ProjectId == projectId && s.DeletedBy == null)
                                                        .Select(y => (int?)y.UserRole.UserId)
                                                        .ToList();

                query = query.Where(x => userAccessData.Contains(x.UserId));
            }

            return query.Select(c => new DropDownDto { Id = c.Id, Value = c.Role.RoleName + " - " + c.User.UserName, IsDeleted = c.DeletedDate != null })
                        .OrderBy(o => o.Value)
                        .ToList();
        }

        public List<DropDownDto> GetCurrencyDropDown()
        {
            return _context.Currency.Where(x => x.DeletedBy == null)
                                    .Select(c => new DropDownDto { Id = c.Id, Value = c.CurrencyName + " - " + c.CurrencySymbol + " - " + c.Country.CountryName, IsDeleted = c.DeletedDate != null })
                                    .OrderBy(o => o.Value)
                                    .ToList();
        }

        public string ResourceWorking(int id)
        {
            var studyPlanResource = _context.StudyPlanResource.Include(i => i.StudyPlanTask)
                                                              .FirstOrDefault(s => s.ResourceTypeId == id && s.DeletedBy == null);

            return studyPlanResource != null ? $"This Resource All Ready Working on this task : {studyPlanResource.StudyPlanTask.TaskName}" : string.Empty;
        }
    }
}
