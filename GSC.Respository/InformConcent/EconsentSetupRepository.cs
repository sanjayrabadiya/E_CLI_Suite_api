using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Master;
using GSC.Respository.PropertyMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public class EconsentSetupRepository : GenericRespository<EconsentSetup, GscContext>, IEconsentSetupRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IPatientStatusRepository _patientStatusRepository;
        public EconsentSetupRepository(IUnitOfWork<GscContext> uow, 
            IJwtTokenAccesser jwtTokenAccesser,
            IPatientStatusRepository patientStatusRepository) : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _patientStatusRepository = patientStatusRepository;
        }

        public string Duplicate(EconsentSetupDto objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.Version == objSave.Version && x.LanguageId == objSave.LanguageId && x.DeletedDate == null))
            {
                return "Duplicate Dictionary";
            }
            return "";
        }

        public List<DropDownDto> GetEconsentDocumentDropDown(int projectId)
        {
            return All.Where(x =>
                   x.ProjectId == projectId && x.DeletedDate == null)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.DocumentName, IsDeleted = false }).OrderBy(o => o.Value)
               .ToList();
        }

        public List<DropDownDto> GetPatientStatusDropDown()
        {
            return _patientStatusRepository.All.Where(x => (x.Code == 1 || x.Code == 2 || x.Code == 4 || x.Code == 7) && x.DeletedDate == null)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.StatusName, IsDeleted = false }).OrderBy(o => o.Value)
               .ToList();
        }
    }
}
