using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

namespace onscripter_documentation
{
    class Program
    {
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

            var globalClasses = new HashSet<string>();
            

            List<FunctionEntry> functionEntries = new List<FunctionEntry>(); 
            while (true)
            {
                FunctionEntry ent = ParsingFunctions.ParseOneFunctionEntry(childIter);
                if(ent == null)
                {
                    break;
                }
                else
                {
                    functionEntries.Add(ent);
                }

                //need to separate each of these based class names, then convert to markdown
                //eg for <PRE|ExSource> just surround with ``` ```
                /*foreach (IElement functionDescriptionElement in ent.functionDescription.descriptionElements)
                {
                    globalClasses.UnionWith(Util.GetRecursivelyAsSet((element) => element.TagName + "|" + element.ClassName, functionDescriptionElement));
                }*/
            }

            //Console.WriteLine("Found the following tags in the function description elements:");
            //foreach (string tag in globalClasses)
            //{
            //    Console.WriteLine(tag);
            //}

            //write out function entries as json

            Util.pauseExit(0);
        }
    }
}
