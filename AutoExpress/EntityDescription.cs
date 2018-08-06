using System.Collections.Generic;

namespace ExpressAll
{
    internal class EntityDescription
    {
        public string Name { get; set; }
        public string TableName { get; set; }
        public string Schema { get; set; }
        public List<PropertyDescription> Properties { get; set; }
    }
}
