using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html;

namespace onscripter_documentation
{
    class Program
    {
        static IElement GenerateElementFromFunctionEntry(IHtmlDocument doc, FunctionEntry fe)
        {
            IElement functionEntryRoot = doc.CreateElement("div");
            functionEntryRoot.ClassName = "FunctionEntry";

            //Create function link
            var idLink = doc.CreateElement("a");
            idLink.Id = fe.id;
            functionEntryRoot.AppendChild(idLink);

            {
                var titleRow = doc.CreateElement("div");
                titleRow.ClassName = "TitleRow";

                //Create function name
                var functionName = doc.CreateElement("span");
                functionName.ClassName = "FunctionName";

                functionName.TextContent = $"[{fe.headerInformation.wordName}] {fe.category}";
                titleRow.AppendChild(functionName);

                // Add misc Information about function in smaller size
                {
                    var metadataHolder = doc.CreateElement("span");
                    metadataHolder.ClassName = "MetadataHolder";

                    var functionCanBeUsedIn = doc.CreateElement("span");
                    functionCanBeUsedIn.ClassName = "FunctionCanBeUsedIn";
                    functionCanBeUsedIn.TextContent = $"{fe.headerInformation.sectionsWhereCommandCanBeUsed}";
                    metadataHolder.AppendChild(functionCanBeUsedIn);

                    var versionsCanBeUsedIn = doc.CreateElement("span");
                    versionsCanBeUsedIn.ClassName = "VersionsCanBeUsedIn";
                    versionsCanBeUsedIn.TextContent = $"{fe.headerInformation.versionsWhereCommandCanBeUsed}";
                    metadataHolder.AppendChild(versionsCanBeUsedIn);

                    titleRow.AppendChild(metadataHolder);
                }

                functionEntryRoot.AppendChild(titleRow);
            }

            //Create function description
            var functionDescription = doc.CreateElement("div");
            functionDescription.ClassName = "FunctionDescription";
            functionDescription.InnerHtml = fe.functionDescription.descriptionHTML;
            functionEntryRoot.AppendChild(functionDescription);

            return functionEntryRoot;
        }

        static string GenerateDocument(string templateFilePath, List<FunctionEntry> functionEntries)
        {
            var document = new HtmlParser().ParseDocument(File.ReadAllText(templateFilePath, Encoding.UTF8));

            // Insert generated document header information here
            var p = document.CreateElement("p");
            p.TextContent = "Insert generated document header information here";
            document.Body.AppendChild(p);

            foreach (FunctionEntry functionEntry in functionEntries)
            {
                document.Body.AppendChild(GenerateElementFromFunctionEntry(document, functionEntry));
            }

            var sw = new StringWriter();
            document.ToHtml(sw, new PrettyMarkupFormatter());
            return sw.ToString();
        }

        /// <summary>
        /// Dump a list documenting each function and its arguments
        ///
        /// NOTE: Since the argument list is directly copied from the onscripter documentation
        /// so it will redundantly include the function name in the argument list.
        ///
        /// NOTE: As some functions in Ponscripter are overloaded, some functions
        /// have multiple argument lists (one for each overloaded variant)
        ///
        /// The format is a list, separated by "alert/bell" character '\a':
        /// [command_name, arg_list1, arg_list2, arg_list3 ...]
        /// </summary>
        /// <param name="functionEntries">List of FunctionEntry objects to be dumped</param>
        static void DumpFunctionList(List<FunctionEntry> functionEntries)
        {
            // Write out expected arguments of each function
            StringBuilder sb = new StringBuilder();
            foreach (FunctionEntry fe in functionEntries)
            {
                List<string> functionArgDump = new List<string>();

                //FunctionArgDump fad = new FunctionArgDump(fe.id);
                functionArgDump.Add(fe.id);

                foreach (ArgumentInformation arg_info in fe.argumentDescriptions)
                {
                    functionArgDump.Add(arg_info.argumentSignature);
                }

                sb.AppendLine(string.Join("\a", functionArgDump));
            }

            File.WriteAllText("FunctionList.txt", sb.ToString());
        }

        static void Main(string[] args)
        {
            string templateFilePath = "entire_document_template.html";

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

            DumpFunctionList(functionEntries);

            string output = GenerateDocument(templateFilePath, functionEntries);
            Console.WriteLine(output);
            File.WriteAllText("GeneratedDocumentation.html", output);

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
