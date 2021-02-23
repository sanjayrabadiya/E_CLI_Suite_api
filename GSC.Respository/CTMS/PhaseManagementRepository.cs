using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.CTMS
{
    public class PhaseManagementRepository : GenericRespository<PhaseManagement>, IPhaseManagementRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public PhaseManagementRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }


        public List<DropDownDto> GetPhaseManagementDropDown()
        {
            return All.Select(c => new DropDownDto { Id = c.Id, Value = c.PhaseName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(PhaseManagement objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.PhaseCode == objSave.PhaseCode.Trim() && x.DeletedDate == null))
                return "Duplicate Phase Code : " + objSave.PhaseCode;

            if (All.Any(x => x.Id != objSave.Id && x.PhaseName == objSave.PhaseName.Trim() && x.DeletedDate == null))
                return "Duplicate Phase Name : " + objSave.PhaseName;

            return "";
        }

        public List<PhaseManagementGridDto> GetPhaseManagementList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<PhaseManagementGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }

        //public List<PhaseManagementDto> GetPhaseManagementList(bool isDeleted)
        //{
        //    return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
        //           ProjectTo<PhaseManagementDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        //}

        //List<PhaseManagementGridDto> IPhaseManagementRepository.GetPhaseManagementList(bool isDeleted)
        //{
        //    return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
        //           ProjectTo<PhaseManagementGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        //}
    }



}
