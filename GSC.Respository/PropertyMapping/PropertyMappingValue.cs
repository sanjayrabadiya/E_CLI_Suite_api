using System.Collections.Generic;

namespace GSC.Respository.PropertyMapping
{
    public class PropertyMappingValue
    {
        public PropertyMappingValue(IEnumerable<string> destinationProperties,
            bool revert = false)
        {
            DestinationProperties = destinationProperties;
            Revert = revert;
        }

        public IEnumerable<string> DestinationProperties { get; }
        public bool Revert { get; }
    }
}