using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace onscripter_documentation
{
    partial class Program
    {
        class FunctionDescriptionContent
        {
            //Description can have indents like: <span class="Indent"> </ span >
            string descriptionHTML;
            List<Notice> notices = new List<Notice>();
            List<Example> examples = new List<Example>();

            public FunctionDescriptionContent(List<IElement> contentElements)
            {
                HashSet<string> descriptionAllowedTags = new HashSet<string> { "br", "b", "span" };
                HashSet<string> markdownConvertibleTags = new HashSet<string> { "br", "b" };

                foreach (IElement contentElement in contentElements)
                {
                    switch (contentElement.ClassName)
                    {
                        //Description should be a <p> node containing only text, <br>, and <b> elements.
                        case "Description":
                            Util.ValidateNodeOnlyContainsValidTags(contentElement, descriptionAllowedTags);
                            if (descriptionHTML == null)
                            {
                                descriptionHTML = contentElement.InnerHtml;
                            }
                            else
                            {
                                throw new Exception("Description appears more than once!");
                            }
                            break;

                        //Notice always two sub elements: 'NoticeHead' and 'NoticeBody'
                        case "Notice":
                            var noticeChildren = contentElement.Children.ToList();
                            Util.ExceptionListSize(noticeChildren, 2);

                            var noticeHead = noticeChildren[0];
                            var noticeBody = noticeChildren[1];

                            Util.ValidateElementClass(noticeHead, "NoticeHead");
                            Util.ValidateElementClass(noticeBody, "NoticeBody");

                            Util.ValidateNodeOnlyContainsValidTags(noticeHead, markdownConvertibleTags);
                            Util.ValidateNodeOnlyContainsValidTags(noticeBody, markdownConvertibleTags);

                            notices.Add(new Notice(noticeHead.InnerHtml, noticeBody.InnerHtml));
                            break;

                        //<div class="Example"> always are of the form:
                        // 1. <span class="ExHeading>
                        // 2. <br>
                        // 3. <div class="ExComment">
                        // 4. <pre class="ExSource>
                        case "Example":
                            var exampleChildren = contentElement.Children.ToList();
                            Util.ExceptionListSize(exampleChildren, 4);

                            var exampleHeading = exampleChildren[0];
                            var exampleComment = exampleChildren[2];
                            var exampleSource = exampleChildren[3];

                            Util.ValidateElementClass(exampleHeading, "ExHeading");
                            Util.ValidateElementClass(exampleComment, "ExComment");
                            Util.ValidateElementClass(exampleSource, "ExSource");

                            Util.ValidateNodeOnlyContainsValidTags(exampleHeading, markdownConvertibleTags);
                            Util.ValidateNodeOnlyContainsValidTags(exampleComment, markdownConvertibleTags);
                            Util.ValidateNodeOnlyContainsValidTags(exampleSource, markdownConvertibleTags);

                            examples.Add(new Example(exampleHeading.InnerHtml, exampleComment.InnerHtml, exampleSource.InnerHtml));
                            break;

                        default:
                            throw new Exception($"Got unknown root description node with className: {contentElement.ClassName}");
                    }
                }
            }
        }

        class Notice
        {
            public readonly string NoticeHeadInnerHTML;
            public readonly string NoticeBodyInnerHTML;

            public Notice(string noticeHeadInnerHTML, string noticeBodyInnterHTML)
            {
                NoticeHeadInnerHTML = noticeHeadInnerHTML;
                NoticeBodyInnerHTML = noticeBodyInnterHTML;
            }
        }

        class Example
        {
            public readonly string ExampleHeadingHTML; //not sure if this is always plain text or is sometimes HTML.
            public readonly string ExampleCommentHTML; //not sure if this is always plain text or is sometimes HTML.
            public readonly string ExampleSourcePre;   //always code

            public Example(string exampleHeadingHTML, string exampleCommentHTML, string exampleSourcePre)
            {
                ExampleHeadingHTML = exampleHeadingHTML;
                ExampleCommentHTML = exampleCommentHTML;
                ExampleSourcePre = exampleSourcePre;
            }
        }

        class FunctionDescription
        {
            public readonly FunctionDescriptionContent functionDescriptionContent; // all the elements that make up the description of the function
            public readonly List<Link> relatedFunctionsLinks; // a list of links to related functions

            // The function description consists of:
            // 1. A series of nodes of misc types, describing the function. The first node is (always?) a <p class="Description"> node 
            //   - Followed by a horizontal rule
            // 2. (OPTONAL) A series of <a href="#rubyoff"> rubyoff </a> ... nodes, which links to other related functions.
            //   - Followed by a horizontal rule
            // 3. The links to page top / list / main . This section is always the same and should be ignored
            //
            // Sainty Checks:
            // - that there are exactly two horizontal rules
            // - that the first node is a <p class="Description"> ?
            public FunctionDescription(IElement divContentBody)
            {
                Console.WriteLine("\n\n--------------- Parsing ------------------");
                Console.WriteLine(divContentBody.InnerHtml);

                var enumerator = divContentBody.Children.GetEnumerator();

                List<List<IElement>> nodesGroupedByHr = SplitByPredicate(enumerator, (node) => node.TagName.ToLower() == "hr");

                if(!(nodesGroupedByHr.Count == 2 || nodesGroupedByHr.Count == 3))
                {
                    throw new Exception("The ContentBody div didn't split into 2 or 3 groups, when split by the <hr> tag");
                }

                var contentElements = nodesGroupedByHr[0];
                this.functionDescriptionContent = new FunctionDescriptionContent(contentElements);
                var linksToRelatedFunctionsElements = nodesGroupedByHr.Count == 3 ? nodesGroupedByHr[1] : new List<IElement>();
                this.relatedFunctionsLinks = ParseLinksToRelatedFunctions(linksToRelatedFunctionsElements);
            }

            // Note: like most splitting functions, 
            // - if the enumerator is empty, a list containing one list will be returned
            // - if the enumerator doesn't contain the predicate, a list containing one list will be returned, containing the entire enumerator
            // - if the predicate is the first or last element of the list, a list containing two list will be returned, one of which will be empty
            public static List<List<T>> SplitByPredicate<T>(IEnumerator<T> enumerator, Func<T, bool> splitElementPredicate)
            {
                List<List<T>> retList = new List<List<T>>();
                retList.Add(new List<T>());

                while (enumerator.MoveNext())
                {
                    if (splitElementPredicate(enumerator.Current))
                    {
                        retList.Add(new List<T>());
                    }
                    else
                    {
                        retList.Last().Add(enumerator.Current);
                    }
                }

                return retList;
            }

            // This function parses a list of links to related functions. 
            // It should be a list of <a> elements only, as below:
            //
            // <a href="#btndef">
            //     btndef
            // </a>
            // /
            // <a href="#btntime">
            //     btntime
            // </a>
            // /
            // <a href="#btntime2">
            //     btntime2
            // </a>
            // /
            public static List<Link> ParseLinksToRelatedFunctions(List<IElement> links)
            {
                return links.Select((x) =>
                {
                    if(x.TagName.ToLower() != "a") { throw new Exception("A non hyperreference was found in the 'links to related functions' section!"); }
                    return new Link(x.Text(), x.GetAttribute("href"));
                }).ToList();
            }

        }
    }
}
