using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.ProjectRight;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.ProjectRight
{
    public class ProjectDocumentRepository : GenericRespository<ProjectDocument, GscContext>, IProjectDocumentRepository
    {
        public ProjectDocumentRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser) : base(uow, jwtTokenAccesser)
        {
        }

        public string Duplicate(ProjectDocument objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.FileName == objSave.FileName && x.ProjectId == objSave.ProjectId &&
                x.DeletedDate == null)) return "Duplicate Document name : " + objSave.FileName;
            return "";
        }

        public List<ProjectDocumentDto> GetDocument(int id)
        {
            var parentId = Context.Project.Where(p => p.Id == id).Select(p => p.ParentProjectId).FirstOrDefault();
            var result = All.Where(t => t.DeletedDate == null);
            if (parentId > 0)
                result = result.Where(r => r.ProjectId == id || r.ProjectId == parentId);
            else 
                result = result.Where(r => r.ProjectId == id);

           return result.Select(x => new ProjectDocumentDto
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                FileName = x.FileName,
                PathName = x.PathName,
                MimeType = x.MimeType,
                CreatedBy = x.ModifiedBy,
                CreatedByName = x.CreatedByUser.UserName,
                CreatedDate = x.ModifiedDate,
                IsReview = x.IsReview
            }).ToList();
        }
    }
}