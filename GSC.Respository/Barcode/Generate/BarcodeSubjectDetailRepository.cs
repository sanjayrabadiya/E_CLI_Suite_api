using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode.Generate;
using GSC.Data.Entities.Barcode.Generate;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Barcode.Generate
{
    public class BarcodeSubjectDetailRepository : GenericRespository<BarcodeSubjectDetail>,
        IBarcodeSubjectDetailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public BarcodeSubjectDetailRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<BarcodeSubjectDetailDto> GetBarcodeSubjectDetail(bool isDeleted)
        {
            var barcodeSubjectDetail = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).Select(c => new BarcodeSubjectDetailDto
                    {
                        Id = c.Id,
                        IsDeleted = c.DeletedDate != null
                    }
                )
                .ToList();
            return barcodeSubjectDetail;
        }
    }
}