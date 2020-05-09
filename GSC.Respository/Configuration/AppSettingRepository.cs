using System;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Configuration
{
    public class AppSettingRepository : GenericRespository<AppSetting, GscContext>, IAppSettingRepository
    {
        public AppSettingRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }

        public void Save<T>(T settings, int? companyId = null)
        {
            if (settings == null) return;

            var baseName = typeof(T).Name;

            var query = All.Where(t => t.KeyName.StartsWith(baseName)).AsQueryable();

            if (companyId.HasValue)
                query = query.Where(x => x.CompanyId == companyId);

            var existing = query.ToList();
            existing.ForEach(Remove);

            foreach (var prop in settings.GetType().GetProperties())
            {
                var propName = prop.Name;

                var propValue = prop.GetValue(settings);

                var objSave = new AppSetting
                {
                    KeyName = baseName + "." + propName,
                    KeyValue = Convert.ToString(propValue),
                    CompanyId = companyId
                };

                Add(objSave);
            }
        }

        public T Get<T>(int? companyId = null)
        {
            var settings = (T) Activator.CreateInstance(typeof(T));

            var query = All.AsQueryable();

            if (companyId.HasValue)
                query = query.Where(x => x.CompanyId == companyId);

            var propList = query.ToList();

            foreach (var prop in settings.GetType().GetProperties())
            {
                var propName = prop.Name;

                var propValue = propList.Where(x => x.KeyName == typeof(T).Name + "." + propName)
                    .Select(x => x.KeyValue).FirstOrDefault();

                var propertyInfo = settings.GetType().GetProperty(propName);
                if (propertyInfo != null)
                    propertyInfo.SetValue(settings, propValue);
            }

            return settings;
        }
    }
}