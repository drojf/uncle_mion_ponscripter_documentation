using System.Collections.Generic;

namespace onscripter_documentation
{
    // The following is guarenteed for all these classes (and corresponding JSON outputs):
    // - all values are non-null
    // - all strings which don't have "HTML", "Pre", or "URL" in their name can be used as is-without filtering, unless otherwise indicated
    // - all strings with the above markers need more processing (eg need to be auto converted to markdown).
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
        public string descriptionHTML; // Description can have indents like: <span class="Indent"> </ span >
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
        /// <summary>
        /// the raw link URL from unclemion's document, including the #
        /// </summary>
        public string linkURL;
        /// <summary>
        /// the link url without the '#'. This id SHOULD match another function's 'id' field, 
        /// but it's not generated from this program - it's taken from unclemion's document.
        /// </summary>
        public string linkId;
    }
}
