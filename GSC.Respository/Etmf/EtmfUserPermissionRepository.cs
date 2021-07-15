using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.ProjectRight;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class EtmfUserPermissionRepository : GenericRespository<EtmfUserPermission>, IEtmfUserPermissionRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IProjectRightRepository _projectRightRepository;
        public EtmfUserPermissionRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IProjectRightRepository projectRightRepository)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
        }

        public List<EtmfUserPermissionDto> GetByUserId(int UserId, int ProjectId, int? ParentProject)
        {
            // Get workplace folder name
            var Worksplace = Enum.GetValues(typeof(WorkPlaceFolder))
                            .Cast<WorkPlaceFolder>().Select(e => new EtmfUserPermissionDto
                            {
                                UserId = UserId,
                                ItemId = Convert.ToInt16(e),
                                ItemName = e.GetDescription(),
                                hasChild = true,
                            }).Where(x => ParentProject != null ? x.ItemId == 2 : x.ItemId == 1 || x.ItemId == 3).OrderBy(o => o.ItemId).ToList();

            // Get child of workplace folder
            var ProjectWorkplaceDetail = _context.ProjectWorkplaceDetail.Include(t => t.ProjectWorkplace)
                .Where(t => t.DeletedDate == null && (ParentProject == null ? t.ProjectWorkplace.ProjectId == ProjectId : t.ItemId == ProjectId))
                .Select(t => new EtmfUserPermissionDto
                {
                    ParentWorksplaceFolderId = t.WorkPlaceFolderId,
                    ProjectWorkplaceDetailId = t.Id,
                    WorkPlaceFolderId = t.WorkPlaceFolderId,
                    WorkPlaceFolder = ((WorkPlaceFolder)t.WorkPlaceFolderId).GetDescription(),
                    ItemId = t.ItemId,
                    ItemName = t.ItemName,
                    hasChild = false,
                }).ToList();

            // update isadd, isdelete .. etc field by etmfuserpermission table
            ProjectWorkplaceDetail.ForEach(t =>
            {
                t.UserId = UserId;
                var p = All.Where(s =>
                    s.ProjectWorkplaceDetailId == t.ProjectWorkplaceDetailId && s.UserId == UserId && s.DeletedDate == null).FirstOrDefault();
                if (p == null) return;
                t.IsAdd = p.IsAdd;
                t.EtmfUserPermissionId = p.Id;
                t.IsDelete = p.IsDelete;
                t.IsEdit = p.IsEdit;
                t.IsExport = p.IsExport;
                t.IsView = p.IsView;
                t.IsAll = p.IsAdd && p.IsDelete && p.IsEdit && p.IsExport && p.IsView;
            });

            Worksplace.ForEach(w =>
            {
                w.IsAdd = ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).Count() == 0 ? false : ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).All(x => x.IsAdd);
                w.IsEdit = ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).Count() == 0 ? false : ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).All(x => x.IsEdit);
                w.IsDelete = ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).Count() == 0 ? false : ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).All(x => x.IsDelete);
                w.IsView = ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).Count() == 0 ? false : ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).All(x => x.IsView);
                w.IsExport = ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).Count() == 0 ? false : ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).All(x => x.IsExport);
                w.IsAll = w.IsAdd && w.IsEdit && w.IsDelete && w.IsView && w.IsExport;
            });

            ProjectWorkplaceDetail.AddRange(Worksplace);

            return ProjectWorkplaceDetail.OrderBy(item => item.ItemId == 3 ? 1 : item.ItemId == 1 ? 2 : 2).ToList();
        }

        public void Save(List<EtmfUserPermission> EtmfUserPermission)
        {
            var userId = EtmfUserPermission.First().UserId;

            EtmfUserPermission = EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId > 0).ToList();
            var ProjectWorksplace = EtmfUserPermission.Where(t => t.IsAdd || t.IsEdit || t.IsDelete || t.IsView || t.IsExport)
                .Select(x => x.ProjectWorkplaceDetailId).ToList();

            var existing = _context.EtmfUserPermission.Where(t => t.UserId == userId && ProjectWorksplace.Contains(t.ProjectWorkplaceDetailId)).ToList();
            if (existing.Any())
            {
                existing = existing.Select(c => { c.DeletedBy = _jwtTokenAccesser.UserId; c.DeletedDate = _jwtTokenAccesser.GetClientDate(); return c; }).ToList();
                _context.EtmfUserPermission.UpdateRange(existing);
                _context.Save();
            }

            EtmfUserPermission = EtmfUserPermission.Where(t => t.IsAdd || t.IsEdit || t.IsDelete || t.IsView || t.IsExport)
                .ToList();

            _context.EtmfUserPermission.AddRange(EtmfUserPermission);
            _context.Save();
        }

        public void updatePermission(List<EtmfUserPermissionDto> EtmfUserPermissionDto)
        {
            var userId = EtmfUserPermissionDto.First().UserId;

            var DeleteAll = EtmfUserPermissionDto.Where(x => x.ProjectWorkplaceDetailId > 0 && (!x.IsAdd && !x.IsEdit && !x.IsDelete && !x.IsView && !x.IsExport)).ToList();
            var UpdateDeleteAll = DeleteAll.Where(t => t.EtmfUserPermissionId > 0).ToList();

            foreach (var item in UpdateDeleteAll)
            {
                var existing = All.Where(t => t.DeletedDate == null && t.UserId == userId && t.ProjectWorkplaceDetailId == item.ProjectWorkplaceDetailId).FirstOrDefault();
                if (existing.IsAdd == item.IsAdd && existing.IsEdit == item.IsEdit && existing.IsDelete == item.IsDelete
                    && existing.IsView == item.IsView && existing.IsExport == item.IsExport) { }
                else
                {
                    Delete(existing);
                    _context.Save();
                }
            }

            EtmfUserPermissionDto = EtmfUserPermissionDto.Where(x => x.ProjectWorkplaceDetailId > 0 && (x.IsAdd || x.IsEdit || x.IsDelete || x.IsView || x.IsExport)).ToList();

            var ToAdd = EtmfUserPermissionDto.Where(x => x.EtmfUserPermissionId == null).ToList();
            foreach (var item in ToAdd)
            {
                EtmfUserPermission obj = new EtmfUserPermission();
                obj.UserId = userId;
                obj.ProjectWorkplaceDetailId = item.ProjectWorkplaceDetailId;
                obj.IsAdd = item.IsAdd;
                obj.IsDelete = item.IsDelete;
                obj.IsView = item.IsView;
                obj.IsEdit = item.IsEdit;
                obj.IsExport = item.IsExport;
                _context.EtmfUserPermission.AddRange(obj);
            }
            _context.Save();

            var ToUpdate = EtmfUserPermissionDto.Where(t => t.EtmfUserPermissionId > 0).ToList();
            foreach (var item in ToUpdate)
            {
                var existing = All.Where(t => t.DeletedDate == null && t.UserId == userId && t.ProjectWorkplaceDetailId == item.ProjectWorkplaceDetailId).FirstOrDefault();
                if (existing.IsAdd == item.IsAdd && existing.IsEdit == item.IsEdit && existing.IsDelete == item.IsDelete
                    && existing.IsView == item.IsView && existing.IsExport == item.IsExport) { }
                else
                {
                    // Delete old record & add same data to table
                    //existing.DeletedBy = _jwtTokenAccesser.UserId;
                    //existing.DeletedDate = DateTime.Now;
                    Delete(existing);
                    _context.Save();

                    existing.Id = 0;
                    existing.DeletedBy = null;
                    existing.DeletedDate = null;
                    existing.IsAdd = item.IsAdd;
                    existing.IsDelete = item.IsDelete;
                    existing.IsView = item.IsView;
                    existing.IsEdit = item.IsEdit;
                    existing.IsExport = item.IsExport;
                    existing.ModifiedAuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
                    existing.ModifiedRollbackReason = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                    Add(existing);
                    _context.Save();
                }
            }
        }

        public void AddEtmfAccessRights(List<ProjectWorkplaceDetail> ProjectWorkplaceDetail)
        {
            // add rights when worksplace created
            foreach (var item in ProjectWorkplaceDetail)
            {
                Add(new EtmfUserPermission
                {
                    UserId = _jwtTokenAccesser.UserId,
                    ProjectWorkplaceDetailId = item.Id,
                    IsAdd = false,
                    IsEdit = false,
                    IsDelete = false,
                    IsExport = false,
                    IsView = false,
                });
            }
            _context.Save();
        }

        public List<EtmfUserPermissionDto> GetEtmfPermissionData(int ProjectId)
        {
            var ParentProject = _context.Project.Where(x => x.Id == ProjectId).FirstOrDefault().ParentProjectId;
            // get etmf rights list
            var result = All
                .Where(x => (ParentProject == null ? x.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == ProjectId && (x.ProjectWorkplaceDetail.WorkPlaceFolderId == 3 || x.ProjectWorkplaceDetail.WorkPlaceFolderId == 1) 
                : x.ProjectWorkplaceDetail.ItemId == ProjectId))
                .Select(y => new EtmfUserPermissionDto
                {
                    Id = y.Id,
                    UserId = y.UserId,
                    UserName = y.User.UserName,
                    CreatedDate = y.CreatedDate,
                    DeletedDate = y.DeletedDate
                }).ToList();

            result = result.OrderByDescending(x => x.Id).GroupBy(x => x.UserId).Select(x => new EtmfUserPermissionDto
            {
                Id = x.FirstOrDefault().Id,
                UserId = x.Key,
                IsRevoke = x.All(y => y.DeletedDate != null) ? true : false,
                UserName = x.FirstOrDefault().UserName,
                CreatedDate = x.FirstOrDefault().CreatedDate
            }).OrderByDescending(x => x.Id).ToList();

            return result;

        }

        public void SaveProjectRollbackRight(int projectId, int[] userIds)
        {
            // save rollback rights
            foreach (var itemDto in userIds)
            {
                var EtmfUserPermission = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == projectId
                && x.UserId == itemDto && x.DeletedDate == null).ToList();

                foreach (var item in EtmfUserPermission)
                {
                    item.IsRevoked = true;
                    item.RollbackReason = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                    item.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
                    item.DeletedBy = _jwtTokenAccesser.UserId;
                    item.DeletedDate = _jwtTokenAccesser.GetClientDate();
                    Update(item);
                }
            }
        }

        public List<EtmfUserPermissionDto> GetEtmfRightHistoryDetails(int projectId, int userId)
        {
            // Get etmf rights history details
            var result = All.Where(x => x.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == projectId
               && x.UserId == userId)
                .Select(x => new EtmfUserPermissionDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User.UserName,
                    ProjectWorkplaceDetailId = x.ProjectWorkplaceDetailId,
                    IsAdd = x.IsAdd,
                    IsEdit = x.IsEdit,
                    IsDelete = x.IsDelete,
                    IsView = x.IsView,
                    IsExport = x.IsExport,
                    CreatedDate = x.CreatedDate,
                    CreatedByUser = x.CreatedByUser.UserName,
                    ModifiedDate = x.ModifiedDate,
                    ModifiedByUser = x.ModifiedByUser.UserName,
                    DeletedDate = x.DeletedDate,
                    DeletedByUser = x.DeletedByUser.UserName,
                    WorkPlaceFolder = ((WorkPlaceFolder)x.ProjectWorkplaceDetail.WorkPlaceFolderId).GetDescription(),
                    ItemName = x.ProjectWorkplaceDetail.ItemName,
                    RollbackReason = x.RollbackReason,
                    AuditReasonId = x.AuditReasonId,
                    AuditReason = x.AuditReason.ReasonName,
                    IsRevoked = x.IsRevoked,
                    ModifiedAuditReason = _context.AuditReason.Where(t => t.Id == x.ModifiedAuditReasonId).FirstOrDefault().ReasonName,
                    ModifiedRollbackReason = x.ModifiedRollbackReason,
                }).OrderByDescending(x => x.Id).ToList();

            return result;
        }

        public List<DropDownDto> GetSitesForEtmf(int ProjectId)
        {
            // Get site for add after worksplace created
            var result = _context.ProjectWorkplaceDetail.Where(x => x.ProjectWorkplace.ProjectId == ProjectId && x.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
                        .Select(x => x.ItemId).ToList();

            var project = _context.Project.Where(x => x.ParentProjectId == ProjectId && x.DeletedDate == null && !result.Contains(x.Id))
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.ProjectCode,
                    ExtraData = x.ProjectName
                }).ToList();

            return project;

        }

        public List<DropDownDto> GetUsersByEtmfRights(int ProjectId, int ProjectDetailsId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && (x.IsReviewDone == true || x.CreatedBy == x.UserId) && x.DeletedDate == null).ToList();
            var latestProjectRight = projectListbyId.OrderByDescending(x => x.Id)
                .GroupBy(c => new { c.UserId }, (key, group) => group.First());

            var users = latestProjectRight.Where(x => x.DeletedDate == null)
                .Select(c => new ProjectArtificateDocumentReviewDto
                {
                    UserId = c.UserId,
                    Name = _context.Users.Where(p => p.Id == c.UserId).Select(r => r.UserName).FirstOrDefault(),
                }).ToList();

            var result = new List<DropDownDto>();
            users.ForEach(x =>
            {
                var etmfUserPermissions = All.Include(y => y.ProjectWorkplaceDetail)
                                         .Where(y => y.ProjectWorkplaceDetailId == ProjectDetailsId && y.DeletedDate == null && y.UserId == x.UserId)
                                         .OrderByDescending(x => x.Id).FirstOrDefault();
                if (etmfUserPermissions != null)
                {
                    var obj = new DropDownDto();
                    //obj.Id = etmfUserPermissions.Id;
                    obj.Id = x.UserId;
                    obj.Value = x.Name;
                    obj.ExtraData = etmfUserPermissions != null ? etmfUserPermissions.IsDelete || etmfUserPermissions.IsView : false;
                    result.Add(obj);
                }
            });

            return result.Where(x => Convert.ToBoolean(x.ExtraData) == true).ToList();
        }
    }
}
