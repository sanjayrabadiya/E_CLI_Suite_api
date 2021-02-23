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
                    s.ProjectWorkplaceDetailId == t.ProjectWorkplaceDetailId && s.UserId == UserId).FirstOrDefault();
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

            return ProjectWorkplaceDetail;
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
                _context.EtmfUserPermission.RemoveRange(existing);
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

            var existing = _context.EtmfUserPermission.Where(t => t.UserId == userId && ProjectWorksplace.Contains(t.ProjectWorkplaceDetailId)).ToList();
            if (existing.Any())
            {
                _context.EtmfUserPermission.RemoveRange(existing);
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
    }
}
