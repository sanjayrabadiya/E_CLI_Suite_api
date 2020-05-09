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
    public class DocumentTypeRepository : GenericRespository<DocumentType, GscContext>, IDocumentTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public DocumentTypeRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetDocumentDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.TypeName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(DocumentType objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.TypeName == objSave.TypeName && x.DeletedDate == null))
                return "Duplicate Document type name : " + objSave.TypeName;

            return "";
        }
    }
}