using System.Collections.Generic;

namespace ExpressAll
{
    internal class SqlModel
    {
        public string SqlText { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
