using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.PropertyMapping
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _appUserPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id", new PropertyMappingValue(new List<string> {"Id"})},
                {"UserName", new PropertyMappingValue(new List<string> {"UserName"})},
                {"LastName", new PropertyMappingValue(new List<string> {"UserName"})},
                {"FirstName", new PropertyMappingValue(new List<string> {"UserName"})},
                {"MiddleName", new PropertyMappingValue(new List<string> {"UserName"})},
                {"GenderId", new PropertyMappingValue(new List<string> {"UserName"})},
                {"Email", new PropertyMappingValue(new List<string> {"UserName"})},
                {"DateOfBirth", new PropertyMappingValue(new List<string> {"UserName"})},
                {"ScopeNameId", new PropertyMappingValue(new List<string> {"UserName"})},
                {"Phone", new PropertyMappingValue(new List<string> {"UserName"})},
                {"DepartmentId", new PropertyMappingValue(new List<string> {"UserName"})},
                {"ValidFrom", new PropertyMappingValue(new List<string> {"UserName"})},
                {"ValidTo", new PropertyMappingValue(new List<string> {"UserName"})}
            };


        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<UserDto, User>(_appUserPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping
            <TSource, TDestination>()
        {
            // get matching mapping
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1) return matchingMapping.First().MappingDictionary;

            throw new Exception(
                $"Cannot find exact property mapping instance for <{typeof(TSource)},{typeof(TDestination)}");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields)) return true;

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            // run through the fields clauses
            foreach (var field in fieldsAfterSplit)
            {
                // trim
                var trimmedField = field.Trim();

                // remove everything after the first " " - if the fields 
                // are coming from an orderBy string, this part must be 
                // ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFirstSpace);

                // find the matching property
                if (!propertyMapping.ContainsKey(propertyName)) return false;
            }

            return true;
        }
    }
}