using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Master
{
    public class VariableRepository : GenericRespository<Variable, GscContext>, IVariableRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public VariableRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetVariableDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.VariableName, Code = c.VariableCode})
                .OrderBy(o => o.Value).ToList();
        }

        public List<VariableListDto> GetVariableListByDomainId(int domainId)
        {
            var aaa = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null
                    && (x.DomainId == domainId || x.DomainId == null))
                .Select(c => new VariableListDto
                {
                    VariableId = c.Id, Name = c.VariableName,
                    Type = c.CoreVariableType, CollectionSourcesName = c.CollectionSource.ToString(),
                    DataTypeName = c.DataType.ToString()
                }).OrderBy(o => o.Name).ToList();

            return aaa;
        }

        public string Duplicate(Variable objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.VariableCode == objSave.VariableCode && x.DeletedDate == null))
                return "Duplicate Variable code : " + objSave.VariableCode;

            if (All.Any(x =>
                x.Id != objSave.Id && x.VariableName == objSave.VariableName && x.DomainId == objSave.DomainId &&
                x.AnnotationTypeId == objSave.AnnotationTypeId && x.DeletedDate == null))
                return "Duplicate Variable name and Domain : " + objSave.VariableName;

            if (All.Any(x =>
                x.Id != objSave.Id && x.DomainId == objSave.DomainId && x.Annotation == objSave.Annotation &&
                !string.IsNullOrEmpty(x.Annotation) && x.DeletedDate == null))
                return "Duplicate Variable Annotation: " + objSave.Annotation;

            if (All.Any(x =>
                x.Id != objSave.Id && x.DomainId == objSave.DomainId && x.VariableAlias == objSave.VariableAlias &&
                !string.IsNullOrEmpty(x.VariableAlias) && x.DeletedDate == null))
                return "Duplicate Variable Alias: " + objSave.VariableAlias;

            //if (All.Any(x => x.Id != objSave.Id && x.VariableAlias == objSave.VariableAlias && x.DeletedDate == null))
            //{
            //    return "Duplicate Variable alias : " + objSave.VariableAlias;
            //}

            return "";
        }


        public IList<DropDownDto> GetColumnName(string tableName)
        {
            //var prop = Type.GetType("GSC.Domain.Context.GscContext").GetProperty("Drug");
            var properties = (from t in typeof(DrugDto).GetProperties()
                select t.Name).ToList();

            var dtlist = new List<DropDownDto>();
            for (var i = 0; i < properties.Count; i++)
            {
                var dt = new DropDownDto();

                dt.Value = properties[i];
                dt.Id = i + 1;
                dtlist.Add(dt);
            }

            return dtlist;
        }

        public List<VariableGridDto> GetVariableList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
       ProjectTo<VariableGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}