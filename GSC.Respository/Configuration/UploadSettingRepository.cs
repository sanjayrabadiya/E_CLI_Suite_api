using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Configuration
{
    public class UploadSettingRepository : GenericRespository<UploadSetting, GscContext>, IUploadSettingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public UploadSettingRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public string GetImagePath()
        {
            return FindBy(x => x.CompanyId == _jwtTokenAccesser.CompanyId && x.DeletedDate == null).FirstOrDefault()
                ?.ImagePath;
        }

        public string GetDocumentPath()
        {
            return FindBy(x => x.CompanyId == _jwtTokenAccesser.CompanyId && x.DeletedDate == null).FirstOrDefault()
                ?.DocumentPath;
        }

        public string GetWebImageUrl()
        {
            return FindBy(x => x.CompanyId == _jwtTokenAccesser.CompanyId && x.DeletedDate == null).FirstOrDefault()
                ?.ImageUrl;
        }

        public string GetWebDocumentUrl()
        {
            return FindBy(x => x.CompanyId == _jwtTokenAccesser.CompanyId && x.DeletedDate == null).FirstOrDefault()
                ?.DocumentUrl;
        }
    }
}