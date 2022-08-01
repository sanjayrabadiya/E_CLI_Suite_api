using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Master
{
    public class PageConfigurationFieldsRepository : GenericRespository<PageConfigurationFields>, IPageConfigurationFieldsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public PageConfigurationFieldsRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public string Duplicate(PageConfigurationFields objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.FieldName == objSave.FieldName && x.AppScreenId==objSave.AppScreenId && x.DeletedDate == null))
                return "Duplicate Actual Field : " + objSave.FieldName;
            return "";
        }

        public List<PageConfigurationFieldsGridDto> GetPageConfigurationList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                 ProjectTo<PageConfigurationFieldsGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public List<DropDownDto> GetPageConfigurationFieldsDropDown(int screenId)
        {
            return All.Include(x => x.AppScreens).Where(x => x.AppScreens.Id == screenId &&
                      (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ActualFieldName, Code = c.DisplayLable, IsDeleted = c.DeletedDate != null, ExtraData = c.Dependent }).OrderBy(o => o.Value)
                .ToList();
        }
    }
}
