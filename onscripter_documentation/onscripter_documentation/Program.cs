using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace onscripter_documentation
{
    class Program
    {
        static Regex whitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        static Regex H2HeaderRegex = new Regex(@"\[([^]]*)]\s*\(([^\)]*)\)", RegexOptions.Compiled);

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

        private static void ExceptionIfStringEmpty(string s)
        {
            if(s == String.Empty)
            {
                throw new Exception("String is Empty!");
            }
        }

        private static void ExceptionIfListCountNotOne<T>(List<T> l)
        {
            if (l.Count != 1)
            {
                throw new Exception("List not of size 1");
            }
        }

        private static IElement GetFirstElementOfClassOrError(IElement el, string className)
        {
            var childrenWithClassName = el.Children.Where((c) => c.ClassName == className).ToList();
            if(childrenWithClassName.Count != 1)
            {
                throw new Exception("List not of size 1");
            }
            return childrenWithClassName.First();
        }

        private static IElement GetFirstElementOrError(IElement el)
        {
            if (el.ChildElementCount != 1)
            {
                throw new Exception("List not of size 1");
            }

            return el.FirstElementChild;
        }

        private static string ConvertToTextString(INode el)
        {
            return whitespaceRegex.Replace(el.Text(), " ").Trim();
            //StringBuilder sb = new StringBuilder();
            //ConvertToTextStringRec(sb, el);
            //return sb.ToString();
        }

        /*private static void ConvertToTextStringRec(StringBuilder sb, INode el)
        {
            if (el.NodeType == NodeType.Text)
            {
                Console.WriteLine($"[{el.Text()}]");
                sb.Append(el.Text());
            }

            foreach (var c in el.ChildNodes)
            {
                ConvertToTextStringRec(sb, c);
            }
        }*/

        class H2HeaderInformation
        {
            public readonly string sectionsWhereCommandCanBeUsed;
            public readonly string versionsWhereCommandCanBeUsed;
            public readonly string wordName;

            public H2HeaderInformation(string sectionsWhereCommandCanBeUsed, string versionsWhereCommandCanBeUsed, string wordName)
            {
                this.sectionsWhereCommandCanBeUsed = sectionsWhereCommandCanBeUsed;
                this.versionsWhereCommandCanBeUsed = versionsWhereCommandCanBeUsed;
                this.wordName = wordName;
            }

            public override string ToString()
            {
                return $"[{sectionsWhereCommandCanBeUsed}] ({versionsWhereCommandCanBeUsed}): {wordName}";
            }
        }

        class FunctionEntry
        {
            public string id;
            public string category;
            public H2HeaderInformation headerInformation;
        }

        static H2HeaderInformation ParseH2HeaderBlock(IElement h2HeaderElement)
        {
            string asRawText = ConvertToTextString(GetFirstElementOfClassOrError(h2HeaderElement, "Support"));

            string[] splitHeader = asRawText.Split(new char[] { '[', ']', '(', ')' } );
            string sectionsWhereCommandCanBeUsed = splitHeader[1].Trim();
            string versionsWhereCommandCanBeUsed = splitHeader[3].Trim();            
            ExceptionIfStringEmpty(sectionsWhereCommandCanBeUsed);
            ExceptionIfStringEmpty(versionsWhereCommandCanBeUsed);

            string wordName = ConvertToTextString(GetFirstElementOfClassOrError(h2HeaderElement, "WordName"));

            return new H2HeaderInformation(sectionsWhereCommandCanBeUsed, versionsWhereCommandCanBeUsed, wordName);
        }

        //Each block appears as the following:
        // 1. A <a> tag, like <a id="_minus-sign"> </a>. This is not visible, but is the anchor which you visit when you are linked to a specific command
        // 2. A <h2> tag, which contains the light purple part of the header. It contains misc info like which onscripter version it can be used in, and [Definition/Program Block]
        // 3. A <h4> tag, which contains the dark purple part of the header. It contains the Category, like Variable Manipulation/Calculations
        // 4. The argument section. If the function is overloaded, there will be one argument section for each overload. It consists of (all on the root element)
        //   4a. A <h3> tag, which contains the argument summary (type/order of arguments)
        //   4b. A <div class="Arguments"> which contains the a description of each argumens 
        // 5. A <div class="ContentBody">, which contains the command description, AND the links to related commands

        //iterates of the 'MAIN' div of the onscripter documentation
        //returns true if still items to process, false otherwise
        static bool ParseOneFunctionEntry(IEnumerator<IElement> children)
        {
            if(!children.MoveNext())
            {
                Console.WriteLine("ran out of entries");
                return false;
            }

            FunctionEntry functionEntry = new FunctionEntry();

            // 1. Index link  <a id="_minus-sign"> </ a >
            var name = children.Current;
            if (name.TagName.ToLower() != "a")
            {
                Console.WriteLine("out of sync!");
                pauseExit(-1);
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
            functionEntry.category = GetFirstElementOrError(categoryGroup).TextContent.Trim();

            // 4. Argument section. There may be multiple of this type of element if function is overloaded
            while(true)
            {
                children.MoveNext();
                if (children.Current.TagName.ToLower() != "h3")
                {
                    break;
                }

                // 4a. <h3> Contains number/type of arguments (not always regular - see the 'subtract (-)' entry)
                var arguments = children.Current;
                //Console.WriteLine(arguments.InnerHtml);

                // 4b. <div class="Arguments"> Argument descriptions (div Arguments)
                children.MoveNext();
                var argumentDescription = children.Current;
                //Console.WriteLine(argumentDescription.InnerHtml);
            }

            // 6. <div class="ContentBody"> Main Description of the function
            //Movenext not necessary as will have been carried out by previous stage
            var functionDescription = children.Current;

            return true;
        }

        static void Main(string[] args)
        {
            string htmlPath = @"C:\Users\drojf\OneDrive\Drojf\07th_mod\unclemion_backup\NScripter API Reference [compiled by senzogawa, translated_annotated_XML-ized by Mion (incorporates former translation by Seung 'gp32' Park)].html";

            //read in from html file
            string htmltext = File.ReadAllText(htmlPath);

            // Create a new parser front-end (can be re-used)
            var parser = new HtmlParser();

            //Just get the DOM representation
            var document = parser.ParseDocument(htmltext);

            //look for <div id="MAIN">
            var functionDetailedDocumentation = document.GetElementById("MAIN");

            var childIter = functionDetailedDocumentation.Children.GetEnumerator();
            while (ParseOneFunctionEntry(childIter))
            {

            }

            /*            foreach(var child in functionDetailedDocumentation.Children)
                        {


                            Console.Write($"{child.TagName} > ");
                            foreach (var attr in child.Attributes)
                            {
                                Console.Write($"[{attr.Value}] ");
                            }
                            Console.WriteLine();
                        }*/


            //Serialize it back to the console
            //Console.WriteLine(functionDetailedDocumentation.OuterHtml); //document.DocumentElement.OuterHtml);

            pauseExit(0);
        }

        static void pauseExit(int exitCode)
        {
            Console.WriteLine("press any key to exit");
            Console.ReadKey();
            Environment.Exit(exitCode);
        }
    }
}
