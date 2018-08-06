using System.Collections.Generic;

namespace UnitTestProject1
{
    internal class SqlModel
    {
        public string SqlText { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
