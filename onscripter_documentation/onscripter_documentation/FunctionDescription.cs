using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace onscripter_documentation
{
    partial class Program
    {
        class FunctionDescription
        {
            List<IElement> descriptionElements; // all the elements that make up the description of the function
            List<Link> relatedFunctionsLinks; // a list of links to related functions

            public FunctionDescription(List<IElement> descriptionElements, List<Link> relatedFunctionsLinks)
            {
                this.descriptionElements = descriptionElements;
                this.relatedFunctionsLinks = relatedFunctionsLinks;
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
                    if(splitElementPredicate(enumerator.Current))
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
            public static FunctionDescription ParseContentBody(IElement divContentBody)
            {
                Console.WriteLine("\n\n--------------- Parsing ------------------");
                Console.WriteLine(divContentBody.InnerHtml);

                var enumerator = divContentBody.Children.GetEnumerator();

                List<List<IElement>> nodesGroupedByHr = SplitByPredicate(enumerator, (node) => node.TagName.ToLower() == "hr");

                if(!(nodesGroupedByHr.Count == 2 || nodesGroupedByHr.Count == 3))
                {
                    throw new Exception("The ContentBody div didn't split into 2 or 3 groups, when split by the <hr> tag");
                }

                var descriptionElements = nodesGroupedByHr[0];
                var linksToRelatedFunctionsElements = nodesGroupedByHr.Count == 3 ? nodesGroupedByHr[1] : new List<IElement>();

                return new FunctionDescription(descriptionElements, ParseLinksToRelatedFunctions(linksToRelatedFunctionsElements));
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
