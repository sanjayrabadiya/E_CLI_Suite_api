using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.PropertyMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MedraVersionRepository : GenericRespository<MedraVersion, GscContext>, IMedraVersionRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public MedraVersionRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(uow, jwtTokenAccesser)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public string Duplicate(MedraVersion objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.DictionaryId == objSave.DictionaryId && x.Version == objSave.Version && x.DeletedDate == null))
            {
                return "Duplicate Dictionary and version name : " + objSave.Version;
            }
            return "";
        }

        public List<DropDownDto> GetMedraVersionDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.Dictionary.DictionaryName + "-" + c.Version }).OrderBy(o => o.Value).ToList();
        }
    }
}
