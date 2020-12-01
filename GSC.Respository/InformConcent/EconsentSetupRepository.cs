using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Master;
using GSC.Respository.PropertyMapping;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public class EconsentSetupRepository : GenericRespository<EconsentSetup>, IEconsentSetupRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IPatientStatusRepository _patientStatusRepository;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public EconsentSetupRepository(IGSCContext context, 
            IJwtTokenAccesser jwtTokenAccesser,
            IPatientStatusRepository patientStatusRepository,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _patientStatusRepository = patientStatusRepository;
            _mapper = mapper;
            _context = context;
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

        public List<EconsentSetupGridDto> GetEconsentSetupList(int projectid, bool isDeleted)
        {
            IList<int> intList = new List<int>();
            intList = _context.Project.Where(x => x.ParentProjectId == projectid).Select(y => y.Id).ToList();
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && intList.Contains(x.ProjectId)).
                   ProjectTo<EconsentSetupGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<DropDownDto> GetPatientStatusDropDown()
        {
            IList<int> intList = new List<int>() { (int)ScreeningPatientStatus.PreScreening, (int)ScreeningPatientStatus.Screening, (int)ScreeningPatientStatus.ConsentCompleted, (int)ScreeningPatientStatus.OnTrial };
            return _patientStatusRepository.All.Where(x => intList.Contains(x.Id) && x.DeletedDate == null)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.StatusName, IsDeleted = false }).OrderBy(o => o.Value)
               .ToList();
            //return _patientStatusRepository.All.Where(x => (x.Code == 1 || x.Code == 2 || x.Code == 4 || x.Code == 7) && x.DeletedDate == null)
            //   .Select(c => new DropDownDto { Id = c.Id, Value = c.StatusName, IsDeleted = false }).OrderBy(o => o.Value)
            //   .ToList();
        }
    }
}
