using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceDetailRepository : GenericRespository<ProjectWorkplaceDetail>, IProjectWorkplaceDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public ProjectWorkplaceDetailRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetCountryByWorkplace(int ParentProjectId)
        {
            var data = (from workplace in _context.ProjectWorkplace.Where(x => x.ProjectId == ParentProjectId)
                        join workplacedetail in _context.ProjectWorkplaceDetail.Where(x => x.DeletedDate == null && x.WorkPlaceFolderId == (int)WorkPlaceFolder.Country) on workplace.Id equals workplacedetail.ProjectWorkplaceId
                        join country in _context.Country.Where(x => x.DeletedDate == null) on workplacedetail.ItemId equals country.Id
                        select new DropDownDto
                        {
                            Id = workplacedetail.Id,
                            Value = country.CountryName
                        }).ToList();

            return data;

        }


        public List<DropDownDto> GetSiteByWorkplace(int ParentProjectId)
        {
            var data = (from workplace in _context.ProjectWorkplace.Where(x => x.ProjectId == ParentProjectId)
                        join workplacedetail in _context.ProjectWorkplaceDetail.Where(x => x.DeletedDate == null && x.WorkPlaceFolderId == (int)WorkPlaceFolder.Site) on workplace.Id equals workplacedetail.ProjectWorkplaceId
                        join project in _context.Project.Where(x => x.DeletedDate == null) on workplacedetail.ItemId equals project.Id
                        select new DropDownDto
                        {
                            Id = workplacedetail.Id,
                            Value = project.ProjectCode + " - " + project.ProjectName
                        }).ToList();

            return data;

        }

        public List<EtmfUserPermissionDto> GetByUserId(int UserId, int ProjectId)
        {
            var Worksplace = Enum.GetValues(typeof(WorkPlaceFolder))
                            .Cast<WorkPlaceFolder>().Select(e => new EtmfUserPermissionDto
                            {
                                UserId = UserId,
                                ItemId = Convert.ToInt16(e),
                                ItemName = e.GetDescription(),
                                hasChild = true,
                            }).OrderBy(o => o.ItemId).ToList();

            var ProjectWorkplaceDetail = _context.ProjectWorkplaceDetail.Include(t => t.ProjectWorkplace)
                .Where(t => t.DeletedDate == null && t.ProjectWorkplace.ProjectId == ProjectId)
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

            ProjectWorkplaceDetail.ForEach(t =>
            {
                t.UserId = UserId;
                var p = _context.EtmfUserPermission.Where(s =>
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
                w.IsAdd = ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).All(x => x.IsAdd);
                w.IsEdit = ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).All(x => x.IsEdit);
                w.IsDelete = ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).All(x => x.IsDelete);
                w.IsView = ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).All(x => x.IsView);
                w.IsExport = ProjectWorkplaceDetail.Where(x => x.ParentWorksplaceFolderId == w.ItemId).All(x => x.IsExport);
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
                existing = existing.Select(c => { c.DeletedBy = _jwtTokenAccesser.UserId; c.DeletedDate = DateTime.Now; return c; }).ToList();
                _context.EtmfUserPermission.UpdateRange(existing);
                _context.Save();
            }

            EtmfUserPermission = EtmfUserPermission.Where(t => t.IsAdd || t.IsEdit || t.IsDelete || t.IsView || t.IsExport)
                .ToList();

            _context.EtmfUserPermission.AddRange(EtmfUserPermission);
            _context.Save();
        }

        public void updatePermission(List<EtmfUserPermission> EtmfUserPermission)
        {
            var userId = EtmfUserPermission.First().UserId;

            EtmfUserPermission = EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId > 0).ToList();
            var ProjectWorksplace = EtmfUserPermission.Where(t => t.IsAdd || t.IsEdit || t.IsDelete || t.IsView || t.IsExport)
                .Select(x => x.ProjectWorkplaceDetailId).ToList();

            var existing = _context.EtmfUserPermission.Where(t => t.UserId == userId &&
                                        ProjectWorksplace.Contains(t.ProjectWorkplaceDetailId)).ToList();

            if (existing.Any())
            {
                var reasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                var reasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
                existing = existing.Select(c => { c.DeletedBy = _jwtTokenAccesser.UserId; c.DeletedDate = DateTime.Now; c.AuditReasonId = reasonId; c.RollbackReason = reasonOth; return c; }).ToList();

                _context.EtmfUserPermission.UpdateRange(existing);
                _context.Save();
            }

            EtmfUserPermission = EtmfUserPermission.Where(t => t.IsAdd || t.IsEdit || t.IsDelete || t.IsView || t.IsExport)
                .ToList();
            _context.EtmfUserPermission.UpdateRange(EtmfUserPermission);
            _context.Save();
        }

        public void AddEtmfAccessRights(List<ProjectWorkplaceDetail> ProjectWorkplaceDetail)
        {
            foreach (var item in ProjectWorkplaceDetail)
            {
                _context.EtmfUserPermission.Add(new EtmfUserPermission
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
            var result = _context.EtmfUserPermission
                .Where(x => x.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == ProjectId)
                .Select(y => new EtmfUserPermissionDto
                {
                    Id = y.Id,
                    UserId = y.UserId,
                    UserName = y.User.UserName,
                    CreatedDate = y.CreatedDate,
                    DeletedDate = y.DeletedDate
                }).ToList();

            result = result.GroupBy(x => x.UserId).Select(x => new EtmfUserPermissionDto
            {
                Id = x.FirstOrDefault().Id,
                UserId = x.Key,
                IsRevoke = x.LastOrDefault().DeletedDate == null ? false : true,
                UserName = x.FirstOrDefault().UserName,
                CreatedDate = x.FirstOrDefault().CreatedDate
            }).ToList();

            return result;

        }

        public void SaveProjectRollbackRight(int projectId, int[] userIds)
        {
            foreach (var itemDto in userIds)
            {
                var EtmfUserPermission = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == projectId
                && x.UserId == itemDto && x.DeletedDate == null).ToList();

                foreach (var item in EtmfUserPermission)
                {
                    item.RollbackReason = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                    item.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
                    item.DeletedBy = _jwtTokenAccesser.UserId;
                    item.DeletedDate = DateTime.Now;
                    _context.EtmfUserPermission.Update(item);
                }
            }
        }

        public List<EtmfUserPermissionDto> GetEtmfRightHistoryDetails(int projectId, int userId)
        {
            var result = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetail.ProjectWorkplace.ProjectId == projectId
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
                    AuditReason = x.AuditReason.ReasonName
                }).OrderByDescending(x => x.Id).ToList();

            return result;
        }

        public List<DropDownDto> GetSitesForEtmf(int ProjectId)
        {
            var result = All.Where(x => x.ProjectWorkplace.ProjectId == ProjectId && x.WorkPlaceFolderId == (int)WorkPlaceFolder.Site)
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
    }
}
