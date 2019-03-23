using AngleSharp.Dom;
using System;
using System.Collections.Generic;

namespace onscripter_documentation
{
    class FunctionEntry
    {
        public string id; // 1.
        public H2HeaderInformation headerInformation; // 2.
        public string category; // 3.
        public List<ArgumentInformation> argumentDescriptions; // 4a. and 4b.
        public FunctionDescription functionDescription;
    }
}
