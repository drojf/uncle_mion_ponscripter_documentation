using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace onscripter_documentation
{
    class ParsingFunctions
    {
        //Each block appears as the following:
        // 1. A <a> tag, like <a id="_minus-sign"> </a>. This is not visible, but is the anchor which you visit when you are linked to a specific command
        // 2. A <h2> tag, which contains the light purple part of the header. It contains misc info like which onscripter version it can be used in, and [Definition/Program Block]
        // 3. A <h4> tag, which contains the dark purple part of the header. It contains the Category, like Variable Manipulation/Calculations
        // 4. The argument section. If the function is overloaded, there will be one argument section for each overload. It consists of (all on the root element):
        //   4a. A <h3> tag, which contains the argument signature (type/order of arguments)
        //   4b. A <div class="Arguments"> which contains the a series of (<span class="ArgType"> <span class="ArgMeaning"> <div class="Space">) which each describe one argument
        // 5. A <div class="ContentBody">, which contains the command description, AND the links to related commands

        //iterates of the 'MAIN' div of the onscripter documentation
        //returns true if still items to process, false otherwise
        public static FunctionEntry ParseOneFunctionEntry(IEnumerator<IElement> children)
        {
            if (!children.MoveNext())
            {
                Console.WriteLine("ran out of entries");
                return null;
            }

            FunctionEntry functionEntry = new FunctionEntry();

            // 1. Index link  <a id="_minus-sign"> </ a >
            var name = children.Current;
            if (name.TagName.ToLower() != "a")
            {
                Console.WriteLine("out of sync!");
                Util.pauseExit(-1);
            }
            else
            {
                functionEntry.id = name.Id;
                Console.WriteLine($"Got Definition for: {name.Id}");
            }

            // 2. Info like onscripter version it can be used in, and [Definition/Program Block]
            children.MoveNext();
            var miscinfo = children.Current;
            functionEntry.headerInformation = ParseH2HeaderBlock(miscinfo);

            // 3. <h4> tag - Category, like Variable Manipulation/Calculations
            // <a class="WordCategory" href="#category_tag"> Pretext Tags </a>
            children.MoveNext();
            var categoryGroup = children.Current; //<h4> tag
            functionEntry.category = Util.GetFirstElementOrError(categoryGroup).TextContent.Trim();

            // 4. Argument section. There may be multiple of this type of element if function is overloaded
            List<ArgumentInformation> allArgumentsInformation = new List<ArgumentInformation>();
            while (true)
            {
                // 4a. <h3> Contains the function signature (number/type of arguments) (not always regular - see the 'subtract (-)' entry)
                children.MoveNext();
                if (children.Current.TagName.ToLower() != "h3")
                {
                    break;
                }
                var H3ArgumentSignature = children.Current;
                //Console.WriteLine(arguments.InnerHtml);

                // 4b. <div class="Arguments"> Argument descriptions (div Arguments)
                children.MoveNext();
                var DivArgumentDescriptions = children.Current;

                // Parse the argument signature and argument descriptions into an argument information object
                allArgumentsInformation.Add(ParseArgumentInformation(H3ArgumentSignature, DivArgumentDescriptions));
            }
            functionEntry.argumentDescriptions = allArgumentsInformation;

            // 6. <div class="ContentBody"> Main Description of the function
            //Movenext not necessary as will have been carried out by previous stage
            var functionDescription = children.Current;
            Console.WriteLine(functionDescription.InnerHtml);
            functionEntry.functionDescription = ParseFunctionDescription(functionDescription);

            return functionEntry;
        }

        //Example of H2 Header Block:
        //
        // <div class="Support">
        //     <span class="WordVersion">
        //         Ver.(undoc)
        //     </span>
        //     <span class="WordField">
        //         <span class="NoBr">
        //             [Definition/Program Block]
        //         </span>
        //     </span>
        //     <span class="EngineField">
        //         <span class="NoBr">
        //             ( NScr
        //             <span class="EngineVersion">
        //                 2.03
        //             </span>
        //         </span>
        //         ,
        //         <span class="NoBr">
        //             ONScr-EN
        //             <span class="EngineVersion">
        //                 20091010
        //             </span>
        //             )
        //         </span>
        //     </span>
        // </div>
        // <div class="WordName">
        //     humanpos
        // </div>
        public static H2HeaderInformation ParseH2HeaderBlock(IElement h2HeaderElement)
        {
            string asRawText = Util.ConvertToTextStringWithoutWhitespace(Util.GetFirstElementOfClassOrError(h2HeaderElement, "Support"));

            string[] splitHeader = asRawText.Split(new char[] { '[', ']', '(', ')' });
            string sectionsWhereCommandCanBeUsed = splitHeader[1].Trim();
            string versionsWhereCommandCanBeUsed = splitHeader[3].Trim();
            Util.ExceptionIfStringEmpty(sectionsWhereCommandCanBeUsed);
            Util.ExceptionIfStringEmpty(versionsWhereCommandCanBeUsed);

            string wordName = Util.ConvertToTextStringWithoutWhitespace(Util.GetFirstElementOfClassOrError(h2HeaderElement, "WordName"));

            return new H2HeaderInformation() {
                sectionsWhereCommandCanBeUsed = sectionsWhereCommandCanBeUsed, 
                versionsWhereCommandCanBeUsed = versionsWhereCommandCanBeUsed, 
                wordName = wordName,
            };
        }

        private static ArgumentInformation ParseArgumentInformation(IElement H3FunctionSignature, IElement divArguments)
        {
            List<ArgumentEntry> argumentEntries = new List<ArgumentEntry>();
            var children = divArguments.Children.GetEnumerator();
            while (children.MoveNext())
            {
                IElement argTypeElement = children.Current;
                Util.ValidateElementClass(argTypeElement, "ArgType");

                children.MoveNext();
                IElement argMeaningElement = children.Current;
                Util.ValidateElementClass(argMeaningElement, "ArgMeaning");

                children.MoveNext();
                IElement space = children.Current;
                Util.ValidateElementClass(space, "Space");

                argumentEntries.Add(
                    new ArgumentEntry()
                    {
                        argType = Util.ConvertToTextStringWithoutWhitespace(argTypeElement),
                        argMeaning = Util.ConvertToTextStringWithoutWhitespace(argMeaningElement)
                    }
                );
            }

            return new ArgumentInformation()
            {
                argumentSignature = Util.ConvertToTextStringWithoutWhitespace(H3FunctionSignature),
                argumentEntries = argumentEntries,
            };
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
        private static FunctionDescription ParseFunctionDescription(IElement divContentBody)
        {
            // Note: like most splitting functions, 
            // - if the enumerator is empty, a list containing one list will be returned
            // - if the enumerator doesn't contain the predicate, a list containing one list will be returned, containing the entire enumerator
            // - if the predicate is the first or last element of the list, a list containing two list will be returned, one of which will be empty
            List<List<T>> SplitByPredicate<T>(IEnumerator<T> enumeratorToSplit, Func<T, bool> splitElementPredicate)
            {
                List<List<T>> retList = new List<List<T>>();
                retList.Add(new List<T>());

                while (enumeratorToSplit.MoveNext())
                {
                    if (splitElementPredicate(enumeratorToSplit.Current))
                    {
                        retList.Add(new List<T>());
                    }
                    else
                    {
                        retList.Last().Add(enumeratorToSplit.Current);
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
            List<Link> ParseLinksToRelatedFunctions(List<IElement> links)
            {
                return links.Select((x) =>
                {
                    if (x.TagName.ToLower() != "a") { throw new Exception("A non hyperreference was found in the 'links to related functions' section!"); }
                    return new Link() {
                        linkText = x.Text(),
                        linkURL = x.GetAttribute("href"),
                    };
                }).ToList();
            }

            Console.WriteLine("\n\n--------------- Parsing ------------------");
            Console.WriteLine(divContentBody.InnerHtml);

            var enumerator = divContentBody.Children.GetEnumerator();

            List<List<IElement>> nodesGroupedByHr = SplitByPredicate(enumerator, (node) => node.TagName.ToLower() == "hr");

            if (!(nodesGroupedByHr.Count == 2 || nodesGroupedByHr.Count == 3))
            {
                throw new Exception("The ContentBody div didn't split into 2 or 3 groups, when split by the <hr> tag");
            }

            var contentElements = nodesGroupedByHr[0];
            var linksToRelatedFunctionsElements = nodesGroupedByHr.Count == 3 ? nodesGroupedByHr[1] : new List<IElement>();

            FunctionDescriptionContent functionDescriptionContent = ParseFunctionDescriptionContent(contentElements);

            return new FunctionDescription()
            {
                descriptionHTML = functionDescriptionContent.descriptionHTML,
                notices = functionDescriptionContent.notices,
                examples = functionDescriptionContent.examples,
                relatedFunctionsLinks = ParseLinksToRelatedFunctions(linksToRelatedFunctionsElements),
            };
        }

        /// <summary>
        /// This class is only used to pass multiple parameters from ParseFunctionDescriptionContent->ParseFunctionDescription
        /// </summary>
        private class FunctionDescriptionContent
        {
            //Description can have indents like: <span class="Indent"> </ span >
            public string descriptionHTML;
            public List<Notice> notices = new List<Notice>();
            public List<Example> examples = new List<Example>();
        }

        //Parses a <div class="ContentBody"> node
        // 1. Always contains a <p class="Description"> node
        // 2. Can contain 0 or more <div class="Notice"> nodes
        // 3. can contain 0 or more <div class="Example"> nodes
        private static FunctionDescriptionContent ParseFunctionDescriptionContent(List<IElement> contentElements)
        {
            FunctionDescriptionContent retVal = new FunctionDescriptionContent();

            HashSet<string> descriptionAllowedTags = new HashSet<string> { "br", "b", "span" };
            HashSet<string> markdownConvertibleTags = new HashSet<string> { "br", "b" };

            foreach (IElement contentElement in contentElements)
            {
                switch (contentElement.ClassName)
                {
                    //Description should be a <p> node containing only text, <br>, and <b> elements.
                    case "Description":
                        Util.ValidateNodeOnlyContainsValidTags(contentElement, descriptionAllowedTags);
                        if (retVal.descriptionHTML == null)
                        {
                            retVal.descriptionHTML = contentElement.InnerHtml;
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

                        retVal.notices.Add(new Notice()
                        {
                            NoticeBodyInnerHTML = noticeBody.InnerHtml,
                            NoticeHeadInnerHTML = noticeHead.InnerHtml,
                        });
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

                        retVal.examples.Add(new Example()
                        {
                            ExampleHeadingHTML = exampleHeading.InnerHtml,
                            ExampleCommentHTML = exampleComment.InnerHtml,
                            ExampleSourcePre = exampleSource.InnerHtml,
                        });
                        break;

                    default:
                        throw new Exception($"Got unknown root description node with className: {contentElement.ClassName}");
                }
            }

            if (retVal.descriptionHTML == null)
            {
                throw new Exception("Function content is missing description");
            }

            return retVal;
        }

    }
}
