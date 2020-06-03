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
    public class AnnotationTypeRepository : GenericRespository<AnnotationType, GscContext>, IAnnotationTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public AnnotationTypeRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetAnnotationTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.AnnotationeName, Code = c.AnnotationeCode, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }
    }
}