using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Client;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Configuration;

namespace GSC.Respository.Client
{
    public class ClientRepository : GenericRespository<Data.Entities.Client.Client, GscContext>, IClientRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public ClientRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IUploadSettingRepository uploadSettingRepository)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public List<DropDownDto> GetClientDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.ClientName}).OrderBy(o => o.Value).ToList();
        }

        public string DuplicateClient(Data.Entities.Client.Client objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ClientCode == objSave.ClientCode && x.DeletedDate == null))
                return "Duplicate Client code : " + objSave.ClientCode;

            return "";
        }
    }
}