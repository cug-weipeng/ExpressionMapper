using System.Reflection;

namespace ExpressAll
{
    internal class PropertyDescription
    {
        public PropertyInfo Property { get; set; }
        public string FieldName { get; set; }
        public bool IsKey { get; set; }
        public bool IsAutoincrease { get; set; }
    }
}
