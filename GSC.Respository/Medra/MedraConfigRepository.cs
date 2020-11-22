using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Respository.PropertyMapping;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MedraConfigRepository : GenericRespository<MedraConfig>, IMedraConfigRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public MedraConfigRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(context)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public string Duplicate(MedraConfigDto objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.MedraVersionId == objSave.MedraVersionId && x.LanguageId == objSave.LanguageId && x.DeletedDate == null))
            {
                return "Duplicate Dictionary";
            }
            return "";
        }


        public List<DropDownDto> GetMedraVesrionByDictionaryDropDown(int DictionaryId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.MedraVersionId == DictionaryId && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.MedraVersion.Dictionary.DictionaryName + "-" + c.MedraVersion.Version }).OrderBy(o => o.Value).ToList();

        }

        public List<DropDownDto> GetMedraLanguageVersionDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.MedraVersion.Dictionary.DictionaryName + "-" + c.Language.LanguageName + "-" + c.MedraVersion.Version }).OrderBy(o => o.Value).ToList();
        }

        public DropDownDto GetDetailByMeddraConfigId(int MeddraConfigId)
        {
            var result = All.Where(x =>
                   (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null && x.Id == MeddraConfigId)
                .Select(c => new DropDownDto { Value = c.MedraVersion.Dictionary.DictionaryName + "-" + c.Language.LanguageName + "-" + c.MedraVersion.Version }).OrderBy(o => o.Value).FirstOrDefault();

            return result;
        }

        public MedraConfig GetActiveDictionaryId()
        {
            return All.Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null && x.IsActive).FirstOrDefault();
        }
    }
}
