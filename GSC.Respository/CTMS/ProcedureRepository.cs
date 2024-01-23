using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;

namespace GSC.Respository.CTMS
{
   public  class ProcedureRepository : GenericRespository<Procedure>, IProcedureRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public ProcedureRepository(IGSCContext context,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context=context;
        }
        public string Duplicate(Procedure objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.Name == objSave.Name.Trim() && x.DeletedDate == null))
                return "Duplicate Resource Code : " + objSave.Name;

            return "";
        }
        public List<ProcedureGridDto> GetProcedureList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                  ProjectTo<ProcedureGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }

        //public List<DropDownEnum> GetParentProjectDropDown()
        //{
        //    return All.Where(x => x.DeletedDate ==null )
        //        .Select(c => new DropDownEnum
        //        {
        //            Id = (short)c.Id,
        //            Value = c.Name,
        //        }).ToList();
        //}
    }
}
