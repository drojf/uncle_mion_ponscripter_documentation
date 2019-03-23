using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace onscripter_documentation
{
    class Notice
    {
        public string NoticeHeadInnerHTML;
        public string NoticeBodyInnerHTML;
    }

    class Example
    {
        public string ExampleHeadingHTML; //not sure if this is always plain text or is sometimes HTML.
        public string ExampleCommentHTML; //not sure if this is always plain text or is sometimes HTML.
        public string ExampleSourcePre;   //always code
    }

    class FunctionDescription
    {
        //Description can have indents like: <span class="Indent"> </ span >
        public string descriptionHTML;
        public List<Notice> notices = new List<Notice>();
        public List<Example> examples = new List<Example>();
        public List<Link> relatedFunctionsLinks; // a list of links to related functions
    }
}
