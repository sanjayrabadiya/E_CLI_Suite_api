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
using GSC.Shared.Generic;
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
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IProjectSubSecArtificateDocumentReviewRepository _projectSubSecArtificateDocumentReviewRepository;
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        private readonly IProjectSubSecArtificateDocumentApproverRepository _projectSubSecArtificateDocumentApproverRepository;
        public EtmfUserPermissionRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IProjectRightRepository projectRightRepository,
           IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
           IProjectSubSecArtificateDocumentReviewRepository projectSubSecArtificateDocumentReviewRepository,
           IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
           IProjectSubSecArtificateDocumentApproverRepository projectSubSecArtificateDocumentApproverRepository)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _projectSubSecArtificateDocumentReviewRepository = projectSubSecArtificateDocumentReviewRepository;
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _projectSubSecArtificateDocumentApproverRepository = projectSubSecArtificateDocumentApproverRepository;
        }

        public List<EtmfUserPermissionDto> GetByUserId(int UserId, int RoleId, int ProjectId, int? ParentProject)
        {
            // Get workplace folder name
            var Worksplace = Enum.GetValues(typeof(WorkPlaceFolder))
                            .Cast<WorkPlaceFolder>().Select(e => new EtmfUserPermissionDto
                            {
                                UserId = UserId,
                                RoleId = RoleId,
                                ItemId = Convert.ToInt16(e),
                                ItemName = e.GetDescription(),
                                hasChild = true,
                            }).Where(x => ParentProject != null ? x.ItemId == 2 : x.ItemId == 1 || x.ItemId == 3).OrderBy(o => o.ItemId).ToList();

            // Get child of workplace folder
            var ProjectWorkplaceDetail = _context.EtmfProjectWorkPlace
                .Where(t => t.DeletedDate == null && (ParentProject == null ? t.ProjectId == ProjectId : t.ItemId == ProjectId) && t.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail)
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
                t.RoleId = RoleId;
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
                var workplaceDetails = ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId);
                w.IsAdd = workplaceDetails.All(x => x.IsAdd);
                w.IsEdit = workplaceDetails.All(x => x.IsEdit);
                w.IsDelete = workplaceDetails.All(x => x.IsDelete);
                w.IsView = workplaceDetails.All(x => x.IsView);
                w.IsExport = workplaceDetails.All(x => x.IsExport);
                w.IsAll = w.IsAdd && w.IsEdit && w.IsDelete && w.IsView && w.IsExport;
            });

            ProjectWorkplaceDetail.AddRange(Worksplace);

            return ProjectWorkplaceDetail.OrderBy(item => item.ItemId == 3 ? 1 : 2).ToList();
        }

        public int Save(List<EtmfUserPermission> EtmfUserPermission)
        {
            var userId = EtmfUserPermission[0].UserId;

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
            return _context.Save();
        }

        public void updatePermission(List<EtmfUserPermissionDto> EtmfUserPermissionDto)
        {
            var backupPermission = EtmfUserPermissionDto.Where(x => x.ProjectWorkplaceDetailId > 0).ToList();
            var userId = EtmfUserPermissionDto[0].UserId;

            var DeleteAll = EtmfUserPermissionDto.Where(x => x.ProjectWorkplaceDetailId > 0 && (!x.IsAdd && !x.IsEdit && !x.IsDelete && !x.IsView && !x.IsExport)).ToList();
            var UpdateDeleteAll = DeleteAll.Where(t => t.EtmfUserPermissionId > 0).ToList();

            foreach (var item in UpdateDeleteAll)
            {
                var existing = All.FirstOrDefault(t => t.DeletedDate == null && t.UserId == userId && t.ProjectWorkplaceDetailId == item.ProjectWorkplaceDetailId);

                if (existing != null && existing.IsAdd == item.IsAdd && existing.IsEdit == item.IsEdit && existing.IsDelete == item.IsDelete
                    && existing.IsView == item.IsView && existing.IsExport == item.IsExport)
                {
                    // No action needed
                }
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
                obj.RoleId = item.RoleId;
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
                var existing = All.First(t => t.DeletedDate == null && t.UserId == userId && t.ProjectWorkplaceDetailId == item.ProjectWorkplaceDetailId);
                if (existing.IsAdd == item.IsAdd && existing.IsEdit == item.IsEdit && existing.IsDelete == item.IsDelete
                    && existing.IsView == item.IsView && existing.IsExport == item.IsExport)
                {
                    // No action needed
                }
                else
                {
                    Delete(existing);
                    _context.Save();
                    var ToAddPermission = new EtmfUserPermission();
                    ToAddPermission.Id = 0;
                    ToAddPermission.UserId = existing.UserId;
                    ToAddPermission.RoleId = existing.RoleId;
                    ToAddPermission.ProjectWorkplaceDetailId = existing.ProjectWorkplaceDetailId;
                    ToAddPermission.DeletedBy = null;
                    ToAddPermission.DeletedDate = null;
                    ToAddPermission.IsAdd = item.IsAdd;
                    ToAddPermission.IsDelete = item.IsDelete;
                    ToAddPermission.IsView = item.IsView;
                    ToAddPermission.IsEdit = item.IsEdit;
                    ToAddPermission.IsExport = item.IsExport;
                    ToAddPermission.ModifiedAuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
                    ToAddPermission.ModifiedRollbackReason = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                    Add(ToAddPermission);
                    _context.Save();
                }
            }

            DeleteEtmfAccessUsers(backupPermission);
        }



        private void DeleteEtmfAccessUsers(List<EtmfUserPermissionDto> EtmfUserPermissionDto)
        {
            foreach (var permission in EtmfUserPermissionDto)
            {
                if (!permission.IsAll)
                {
                    var reviewers = _projectWorkplaceArtificateDocumentReviewRepository.All
                        .Include(x => x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                        .Where(x => x.UserId == permission.UserId && !x.IsReviewed
                        && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace
                        .Id == permission.ProjectWorkplaceDetailId && x.DeletedDate == null).ToList();

                    foreach (var user in reviewers)
                    {
                        _projectWorkplaceArtificateDocumentReviewRepository.Delete(user);
                    }

                    var subDocumentReviewers = _projectSubSecArtificateDocumentReviewRepository.All
                       .Include(x => x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                       .Where(x => x.UserId == permission.UserId && !x.IsReviewed
                       && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace
                       .Id == permission.ProjectWorkplaceDetailId && x.DeletedDate == null).ToList();

                    foreach (var user in subDocumentReviewers)
                    {
                        _projectSubSecArtificateDocumentReviewRepository.Delete(user);
                    }

                    var approvers = _projectArtificateDocumentApproverRepository.All
                      .Include(x => x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                      .Where(x => x.UserId == permission.UserId && (x.IsApproved == false || x.IsApproved == null)
                      && x.ProjectWorkplaceArtificatedDocument.ProjectWorkplaceArtificate.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace
                      .Id == permission.ProjectWorkplaceDetailId && x.DeletedDate == null).ToList();

                    foreach (var user in approvers)
                    {
                        _projectArtificateDocumentApproverRepository.Delete(user);
                    }

                    var subDocumentApprovers = _projectSubSecArtificateDocumentApproverRepository.All
                       .Include(x => x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace)
                       .Where(x => x.UserId == permission.UserId && (x.IsApproved == false || x.IsApproved == null)
                       && x.ProjectWorkplaceSubSecArtificateDocument.ProjectWorkplaceSubSectionArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace
                       .Id == permission.ProjectWorkplaceDetailId && x.DeletedDate == null).ToList();

                    foreach (var user in subDocumentApprovers)
                    {
                        _projectSubSecArtificateDocumentApproverRepository.Delete(user);
                    }
                }

                _context.Save();
            }
        }

        public void AddEtmfAccessRights(List<EtmfProjectWorkPlace> ProjectWorkplaceDetail)
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
            var ParentProject = _context.Project.Where(x => x.Id == ProjectId).First().ParentProjectId;
            // get etmf rights list
            var result = All
                .Where(x => (ParentProject == null ? x.ProjectWorkplaceDetail.ProjectId == ProjectId && (x.ProjectWorkplaceDetail.WorkPlaceFolderId == 3 || x.ProjectWorkplaceDetail.WorkPlaceFolderId == 1)
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
                Id = x.First().Id,
                UserId = x.Key,
                IsRevoke = x.All(y => y.DeletedDate != null),
                UserName = x.First().UserName,
                CreatedDate = x.First().CreatedDate
            }).OrderByDescending(x => x.Id).ToList();

            return result;

        }

        public void SaveProjectRollbackRight(int projectId, int[] userIds)
        {
            // save rollback rights
            foreach (var itemDto in userIds)
            {
                var EtmfUserPermission = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetail.ProjectId == projectId
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
            var result = All.Where(x => x.ProjectWorkplaceDetail.ProjectId == projectId
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
            var result = _context.EtmfProjectWorkPlace.Where(x => x.ProjectId == ProjectId && x.WorkPlaceFolderId == (int)WorkPlaceFolder.Site && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceDetail)
                        .Select(x => x.ItemId).ToList();

            var project = _context.Project.Where(x => x.ParentProjectId == ProjectId && x.DeletedDate == null && !result.Contains(x.Id))
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = x.ProjectCode + " (" + x.ProjectName + ")",
                    ExtraData = x.ProjectName
                }).ToList();

            return project;

        }

        public List<DropDownDto> GetUsersByEtmfRights(int ProjectId, int ProjectDetailsId)
        {
            var projectListbyId = _projectRightRepository.FindByInclude(x => x.ProjectId == ProjectId && (x.IsReviewDone || x.CreatedBy == x.UserId) && x.DeletedDate == null).ToList();
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
                    obj.Id = x.UserId;
                    obj.Value = x.Name;
                    obj.ExtraData = (etmfUserPermissions.IsDelete || etmfUserPermissions.IsView);
                    result.Add(obj);
                }
            });

            return result.Where(x => Convert.ToBoolean(x.ExtraData)).ToList();
        }
    }
}
