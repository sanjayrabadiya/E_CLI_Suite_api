using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class LettersFormateRepository : GenericRespository<LettersFormate>, ILettersFormateRepository
    {
        private readonly IMapper _mapper;

        public LettersFormateRepository(IGSCContext context, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
        }

        List<LettersFormateGridDto> ILettersFormateRepository.GetlettersFormateList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<LettersFormateGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public string Duplicate(LettersFormate objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.LetterName == objSave.LetterName.Trim() && x.DeletedDate == null))
                return "Duplicate Letter Name : " + objSave.LetterName;
            return "";
        }
    }
}