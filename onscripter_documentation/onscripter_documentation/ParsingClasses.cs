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

    class H2HeaderInformation
    {
        public string sectionsWhereCommandCanBeUsed;
        public string versionsWhereCommandCanBeUsed;
        public string wordName;
        public override string ToString()
        {
            return $"[{sectionsWhereCommandCanBeUsed}] ({versionsWhereCommandCanBeUsed}): {wordName}";
        }
    }
    class ArgumentInformation
    {
        public string argumentSignature;
        public List<ArgumentEntry> argumentEntries;
    }

    class ArgumentEntry
    {
        public string argType;
        public string argMeaning;

        public override string ToString()
        {
            return $"{argType}: {argMeaning}";
        }
    }

    class FunctionDescription
    {
        //Description can have indents like: <span class="Indent"> </ span >
        public string descriptionHTML;
        public List<Notice> notices = new List<Notice>();
        public List<Example> examples = new List<Example>();
        public List<Link> relatedFunctionsLinks; // a list of links to related functions
    }

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

    class Link
    {
        public string linkText;
        public string linkURL;
    }
}
