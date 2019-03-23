using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onscripter_documentation
{
    class Program
    {
        //iterates of the 'MAIN' div of the onscripter documentation
        //returns true if still items to process, false otherwise
        static bool ParseOneFunctionEntry(IEnumerator<IElement> children)
        {
            if(!children.MoveNext())
            {
                Console.WriteLine("ran out of entries");
                return false;
            }

            //First element is index link  <a id="_minus-sign"> </ a >
            var name = children.Current;
            if (name.TagName.ToLower() != "a")
            {
                Console.WriteLine("out of sync!");
                pauseExit(-1);
            }
            else
            {
                Console.WriteLine($"Got Definition for: {name.Id}");            
            }

            //Second elemnt is misc info like which onscripter version it can be used in, and [Definition/Program Block]
            children.MoveNext();
            var miscinfo = children.Current;

            //Third element is category, like Variable Manipulation/Calculations
            children.MoveNext();
            var categoryGroup = children.Current; //<h4> tag
            foreach (var category in categoryGroup.Children)
            {
                // <a class="WordCategory" href="#category_tag"> Pretext Tags </a>
                string categoryString = category.TextContent.Trim();
                Console.WriteLine(categoryString);
            }

            //Fourth element is number of arguments (not always regular - see the 'subtract (-)' entry)
            //There may be multiple of this type of element if function is overloaded
            while(true)
            {
                children.MoveNext();
                if (children.Current.TagName.ToLower() != "h3")
                {
                    break;
                }
                
                //Argument name/order list
                var arguments = children.Current;
                //Console.WriteLine(arguments.InnerHtml);

                //Argument descriptions (div Arguments)
                children.MoveNext();
                var argumentDescription = children.Current;
                //Console.WriteLine(argumentDescription.InnerHtml);
            }

            //Sixth element is Description (div ContentBody)
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
