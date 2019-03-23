using AngleSharp.Dom;
using System.Collections.Generic;

namespace onscripter_documentation
{
    class ArgumentEntry
    {
        readonly string argType;
        readonly string argMeaning;

        public ArgumentEntry(string argType, string argMeaning)
        {
            this.argType = argType;
            this.argMeaning = argMeaning;
        }

        public override string ToString()
        {
            return $"{argType}: {argMeaning}";
        }
    }

    class ArgumentInformation
    {
        string argumentSignature;
        List<ArgumentEntry> argumentEntries;

        public ArgumentInformation(string argumentSignature, List<ArgumentEntry> argumentEntries)
        {
            this.argumentSignature = argumentSignature;
            this.argumentEntries = argumentEntries;
        }

        public static ArgumentInformation ParseArgumentInformation(IElement H3FunctionSignature, IElement divArguments)
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
                    new ArgumentEntry(
                        Util.ConvertToTextStringWithoutWhitespace(argTypeElement),
                        Util.ConvertToTextStringWithoutWhitespace(argMeaningElement)
                    )
                );
            }

            return new ArgumentInformation(
                Util.ConvertToTextStringWithoutWhitespace(H3FunctionSignature),
                argumentEntries);
        }
    }
}
