using System.Collections.Generic;

namespace GSC.Respository.PropertyMapping
{
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            MappingDictionary = mappingDictionary;
        }

        public Dictionary<string, PropertyMappingValue> MappingDictionary { get; }
    }
}