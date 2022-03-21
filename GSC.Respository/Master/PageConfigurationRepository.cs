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
    public class PageConfigurationRepository : GenericRespository<PageConfiguration>, IPageConfigurationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public PageConfigurationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }
        public string Duplicate(PageConfiguration objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ActualField == objSave.ActualField && x.DeletedDate == null))
                return "Duplicate Actual Field : " + objSave.ActualField;
            return "";
        }

        public List<DropDownDto> GetPageConfigurationDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.DisplayField, Code = c.ActualField.ToString(), IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value)
                .ToList();
        }

        public List<PageConfigurationGridDto> GetPageConfigurationList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                  ProjectTo<PageConfigurationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<PageConfigurationGridDto> GetPageConfigurationListByScreen(int screenId, bool isDeleted)
        {
            return All.Include(i => i.PageConfigurationFields).Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.PageConfigurationFields.AppScreenId == screenId).
                  ProjectTo<PageConfigurationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public PageConfigurationDto GetById(int id)
        {
            var pageConfig = All.SingleOrDefault(q => q.Id == id);
            return _mapper.Map<PageConfigurationDto>(pageConfig);
        }

        public List<PageConfigurationDto> GetPageConfigurationByAppScreen(int screenId)
        {
            var configs = All.Include(x => x.PageConfigurationFields.AppScreens).Where(a => a.PageConfigurationFields.AppScreens.Id == screenId && a.DeletedDate == null)
                .ProjectTo<PageConfigurationDto>(_mapper.ConfigurationProvider).ToList();
            return configs;
        }

        public List<PageConfigurationCommon> GetPageConfigurationByAppScreen(string screenCode)
        {
            var configs = All.Include(x => x.PageConfigurationFields.AppScreens).Where(a => a.PageConfigurationFields.AppScreens.ScreenCode == screenCode && a.DeletedDate == null)
                .ProjectTo<PageConfigurationCommon>(_mapper.ConfigurationProvider).ToList();
            return configs;
        }
    }
}
