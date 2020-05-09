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
            var document =
                (from projectdoc in Context.ProjectDocument.Where(t => t.ProjectId == id && t.DeletedDate == null)
                    //join user in Context.Users.Where(t => t.DeletedDate == null) on projectdoc.CreatedBy equals user.Id
                    join usermodified in Context.Users.Where(t => t.DeletedDate == null) on projectdoc.ModifiedBy equals
                        usermodified.Id
                    select new ProjectDocumentDto
                    {
                        Id = projectdoc.Id,
                        ProjectId = projectdoc.ProjectId,
                        FileName = projectdoc.FileName,
                        PathName = projectdoc.PathName,
                        MimeType = projectdoc.MimeType,
                        CreatedBy = projectdoc.ModifiedBy,
                        CreatedByName = usermodified.UserName,
                        CreatedDate = projectdoc.ModifiedDate,
                        IsReview = projectdoc.IsReview
                    }).OrderByDescending(t => t.Id).ToList();

            return document;
        }
    }
}