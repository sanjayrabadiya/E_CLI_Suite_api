using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementFactorMappingRepository : GenericRespository<SupplyManagementFactorMapping>, ISupplyManagementFactorMappingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SupplyManagementFactorMappingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<SupplyManagementFactorMappingGridDto> GetSupplyFactorMappingList(bool isDeleted, int ProjectId)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementFactorMappingGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public string Validation(SupplyManagementFactorMapping obj)
        {
            if (obj.Id > 0)
            {
                if (All.Any(x => x.Id != obj.Id && x.ProjectId == obj.ProjectId && x.Factor == obj.Factor && x.DeletedDate == null))
                {
                    return "This variable has been already added with same factor ";
                }

                var variable = _context.ProjectDesignVariable.Where(x => x.Id == obj.ProjectDesignVariableId).FirstOrDefault();
                if (variable != null)
                {
                    if (obj.Factor == Fector.Age && variable.CollectionSource != CollectionSources.TextBox)
                    {
                        return "Colection source is not matched with factor collection source";
                    }
                    if (obj.Factor == Fector.BMI && variable.CollectionSource != CollectionSources.TextBox)
                    {
                        return "Colection source is not matched with factor collection source";
                    }
                    if (obj.Factor == Fector.Joint && (variable.CollectionSource != CollectionSources.ComboBox && variable.CollectionSource != CollectionSources.RadioButton))
                    {
                        return "Colection source is not matched with factor collection source";
                    }
                    if (obj.Factor == Fector.Diatory && (variable.CollectionSource != CollectionSources.ComboBox && variable.CollectionSource != CollectionSources.RadioButton))
                    {
                        return "Colection source is not matched with factor collection source";
                    }
                    if (obj.Factor == Fector.Gender && (variable.CollectionSource != CollectionSources.ComboBox && variable.CollectionSource != CollectionSources.RadioButton))
                    {
                        return "Colection source is not matched with factor collection source";
                    }
                    if (obj.Factor == Fector.Eligibility && (variable.CollectionSource != CollectionSources.ComboBox && variable.CollectionSource != CollectionSources.RadioButton))
                    {
                        return "Colection source is not matched with factor collection source";
                    }

                }

            }
            else
            {
                if (All.Any(x => x.ProjectId == obj.ProjectId && x.Factor == obj.Factor && x.DeletedDate == null))
                {
                    return "This variable has been already added with same factor ";
                }
                var variable = _context.ProjectDesignVariable.Where(x => x.Id == obj.ProjectDesignVariableId && x.StudyVersion == null).FirstOrDefault();
                if (variable != null)
                {
                    if (obj.Factor == Fector.Age && variable.CollectionSource != CollectionSources.TextBox)
                    {
                        return "Colection source is not matched with factor collection source";
                    }
                    if (obj.Factor == Fector.BMI && variable.CollectionSource != CollectionSources.TextBox)
                    {
                        return "Colection source is not matched with factor collection source";
                    }
                    if (obj.Factor == Fector.Joint && (variable.CollectionSource != CollectionSources.ComboBox && variable.CollectionSource != CollectionSources.RadioButton))
                    {
                        return "Colection source is not matched with factor collection source";
                    }
                    if (obj.Factor == Fector.Diatory && (variable.CollectionSource != CollectionSources.ComboBox && variable.CollectionSource != CollectionSources.RadioButton))
                    {
                        return "Colection source is not matched with factor collection source";
                    }
                    if (obj.Factor == Fector.Gender && (variable.CollectionSource != CollectionSources.ComboBox && variable.CollectionSource != CollectionSources.RadioButton))
                    {
                        return "Colection source is not matched with factor collection source";
                    }
                    if (obj.Factor == Fector.Eligibility && (variable.CollectionSource != CollectionSources.ComboBox && variable.CollectionSource != CollectionSources.RadioButton))
                    {
                        return "Colection source is not matched with factor collection source";
                    }

                }
            }

            return "";
        }


    }
}
