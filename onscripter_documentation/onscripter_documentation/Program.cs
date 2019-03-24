using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace onscripter_documentation
{
    class Program
    {
        static void Main(string[] args)
        {
            string htmlPath = @"..\..\..\..\WaybackArchive\NScripter API Reference [compiled by senzogawa, translated_annotated_XML-ized by Mion (incorporates former translation by Seung 'gp32' Park)].html";

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
            }

            //write out function entries as json
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter(@"onscripter_documentation.json"))
            using (JsonWriter writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented } )
            {
                serializer.Serialize(writer, functionEntries);
            }

            Util.pauseExit(0);
        }
    }
}
