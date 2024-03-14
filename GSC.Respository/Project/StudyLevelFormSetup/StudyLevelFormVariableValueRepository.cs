using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.StudyLevelFormSetup
{
    public class StudyLevelFormVariableValueRepository : GenericRespository<StudyLevelFormVariableValue>, IStudyLevelFormVariableValueRepository
    {
        private readonly IMapper _mapper;
        public StudyLevelFormVariableValueRepository(IGSCContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public void UpdateVariableValues(StudyLevelFormVariableDto variableDto, bool CollectionValueDisable)
        {

            if (CollectionValueDisable)
            {
                var deletedisableValues = All.Where(x => x.StudyLevelFormVariableId == variableDto.Id).ToList();
                foreach (var item in deletedisableValues)
                {
                    Delete(item);
                }
            }
            else
            {
                if (variableDto.Values == null || !variableDto.Values.Any()) return;
                int seqNo = 0;
                variableDto.Values.ToList().ForEach(x =>
                {
                    var variableValue = _mapper.Map<StudyLevelFormVariableValue>(x);
                    if (x.Id > 0)
                    {
                        if (x.IsDeleted)
                        {
                            Remove(variableValue);
                        }
                        else
                        {
                            seqNo += 1;
                            variableValue.SeqNo = seqNo;
                            Update(variableValue);
                        }
                    }
                    else if (x.Id == 0 && !x.IsDeleted)
                    {
                        seqNo += 1;
                        variableValue.StudyLevelFormVariableId = variableDto.Id;
                        variableValue.SeqNo = seqNo;
                        Add(variableValue);
                    }
                });
            }
        }

    }
}
