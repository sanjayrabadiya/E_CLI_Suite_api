using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class DocumentNameRepository : GenericRespository<DocumentName, GscContext>, IDocumentNameRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public DocumentNameRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetDocumentDropDown(int documentId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) &&
                    x.DocumentTypeId == documentId && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.Name}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(DocumentName objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.Name == objSave.Name && x.DeletedDate == null))
                return "Duplicate Document name : " + objSave.Name;
            return "";
        }
    }
}