using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementAllocationRepository : GenericRespository<SupplyManagementAllocation>, ISupplyManagementAllocationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SupplyManagementAllocationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<SupplyManagementAllocationGridDto> GetSupplyAllocationList(bool isDeleted, int ProjectId)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementAllocationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public IList<DropDownDto> GetVisitDropDownByRandomization(int projectId)
        {
            var visits = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
             && x.ProjectDesignPeriod.DeletedDate == null && x.ProjectDesignPeriod.ProjectDesign.DeletedDate == null
             && x.DeletedDate == null)
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.DisplayName,
                }).Distinct().ToList();
            return visits;
        }

        public List<DropDownDto> GetParentProjectDropDownProjectRight()
        {

            var projectList = _context.ProjectDocumentReview.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.Project.ParentProjectId == null
                                             && _context.ProjectRight.Any(a => (a.project.ParentProjectId == x.ProjectId || a.ProjectId == x.ProjectId)
                                                                              && a.UserId == _jwtTokenAccesser.UserId &&
                                                                              a.RoleId == _jwtTokenAccesser.RoleId
                                                                              && a.DeletedDate == null &&
                                                                              a.RollbackReason == null) &&
                                             x.DeletedDate == null)
                .Select(c => new DropDownDto
                {
                    Id = c.ProjectId,
                    Value = c.Project.ProjectCode,
                    Code = c.Project.ProjectCode,
                    ExtraData = c.Project.ParentProjectId
                }).OrderBy(o => o.Value).Distinct().ToList();

            var childParentList = _context.ProjectDocumentReview.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.Project.ParentProjectId != null
                                            && _context.ProjectRight.Any(a => (a.project.ParentProjectId == x.ProjectId || a.ProjectId == x.ProjectId)
                                                                             && a.UserId == _jwtTokenAccesser.UserId &&
                                                                             a.RoleId == _jwtTokenAccesser.RoleId
                                                                             && a.DeletedDate == null &&
                                                                             a.RollbackReason == null) &&
                                            x.DeletedDate == null).Select(c => new DropDownDto
                                            {
                                                Id = (int)c.Project.ParentProjectId,
                                                Value = _context.Project.Where(x => x.Id == c.Project.ParentProjectId).FirstOrDefault().ProjectCode,
                                                ExtraData = c.Project.ParentProjectId
                                            }).OrderBy(o => o.Value).Distinct().ToList();
            projectList.AddRange(childParentList);

            if (projectList == null || projectList.Count == 0) return null;
            var data = projectList.GroupBy(d => d.Id).Select(c => new DropDownDto
            {
                Id = c.FirstOrDefault().Id,
                Value = c.FirstOrDefault().Value,
                Code = c.FirstOrDefault().Code,
                ExtraData = c.FirstOrDefault().ExtraData
            }).OrderBy(o => o.Id).ToList();


            return data;
        }
        public IList<DropDownDto> GetTemplateDropDownByVisitId(int visitId)
        {
            var templates = _context.ProjectDesignTemplate.Where(x => x.ProjectDesignVisitId == visitId && x.DeletedDate == null)
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.TemplateName,
                }).Distinct().ToList();
            return templates;
        }
        public IList<DropDownDto> GetVariableDropDownByTemplateId(int templateId)
        {
            var templates = _context.ProjectDesignVariable.Where(x =>
            x.ProjectDesignTemplateId == templateId
            && x.CollectionSource == CollectionSources.TextBox
            && x.DeletedDate == null)
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.VariableName,
                }).Distinct().ToList();
            return templates;
        }
        public object GetProductTypeByVisit(int visitId)
        {
            var templates = _context.SupplyManagementUploadFileVisit.Where(x =>
            x.ProjectDesignVisitId == visitId
            && x.DeletedDate == null)
                .FirstOrDefault();
            return templates;
        }

        public string CheckDuplicate(SupplyManagementAllocation obj)
        {

            if (obj.Id > 0)
            {
                var data = All.Where(x => x.Id != obj.Id && x.DeletedDate == null
                && x.PharmacyStudyProductTypeId == obj.PharmacyStudyProductTypeId && x.ProjectDesignVariableId == obj.ProjectDesignVariableId).FirstOrDefault();
                if (data != null)
                {
                    return "Record already exist!";
                }
            }
            else
            {
                var data = All.Where(x => x.DeletedDate == null && x.PharmacyStudyProductTypeId == obj.PharmacyStudyProductTypeId && x.ProjectDesignVariableId == obj.ProjectDesignVariableId).FirstOrDefault();
                if (data != null)
                {
                    return "Record already exist!";
                }
            }
            return "";
        }
        public List<DropDownDto> GetPharmacyStudyProductTypeDropDown(int ProjectId)
        {
            return _context.ProductReceipt.Where(c => c.ProjectId == ProjectId && c.DeletedDate == null
            && c.Status == ProductVerificationStatus.Approved).Select(c => new DropDownDto { Id = c.PharmacyStudyProductType.Id, Value = c.PharmacyStudyProductType.ProductType.ProductTypeCode + "-" + c.PharmacyStudyProductType.ProductType.ProductTypeName })
                .OrderBy(o => o.Value).Distinct().ToList();
        }

        public bool CheckRandomizationAssign(SupplyManagementAllocation obj)
        {

            var randomization = _context.Randomization.Include(x => x.Project).Where(x => x.Project.ParentProjectId == obj.ProjectId && x.RandomizationNumber != null).FirstOrDefault();
            if (randomization != null)
                return false;

            return true;
        }

        public List<DropDownDto> GetTreatmentTypeKitSequence(int ProjectId)
        {
            var data = _context.SupplyManagementUploadFileDetail.Where(x => x.DeletedDate == null && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve
                        && x.SupplyManagementUploadFile.ProjectId == ProjectId)
                        .Select(x => new DropDownDto
                        {
                            Code = x.TreatmentType,
                            Value = x.TreatmentType

                        }).OrderBy(o => o.Value).Distinct().ToList();

            return data;
        }
        public List<DropDownDto> GetPharmacyStudyProductTypeDropDownKitSequence(int ProjectId, string TreatmentType, int VisitId)
        {
            List<DropDownDto> list = new List<DropDownDto>();
            var uploaddata = _context.SupplyManagementUploadFileDetail.Include(x => x.SupplyManagementUploadFile)
                .Where(x => x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve && x.DeletedDate == null && x.SupplyManagementUploadFile.ProjectId == ProjectId
                && x.TreatmentType == TreatmentType.Trim()).ToList();

            if (uploaddata != null && uploaddata.Count > 0)
            {
                var visits = _context.SupplyManagementUploadFileVisit.Where(x => x.DeletedDate == null && x.ProjectDesignVisitId == VisitId && uploaddata.Select(s => s.Id).Contains(x.SupplyManagementUploadFileDetailId)).FirstOrDefault();

                if (visits != null)
                {
                    var PharmacyStudyProductType = _context.PharmacyStudyProductType.Include(x => x.ProductType).Where(x => x.DeletedDate == null && x.ProductType.ProductTypeCode == visits.Value && x.ProjectId == ProjectId).FirstOrDefault();

                    if (PharmacyStudyProductType != null)
                    {
                        DropDownDto obj = new DropDownDto();
                        obj.Value = PharmacyStudyProductType.ProductType.ProductTypeCode;
                        obj.Id = PharmacyStudyProductType.Id;
                        list.Add(obj);
                        return list;
                    }
                }
            }
            return new List<DropDownDto>();
        }
    }
}
