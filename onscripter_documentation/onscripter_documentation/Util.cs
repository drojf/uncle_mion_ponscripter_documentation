using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace onscripter_documentation
{
    class Util
    {
        static Regex whitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        static Regex H2HeaderRegex = new Regex(@"\[([^]]*)]\s*\(([^\)]*)\)", RegexOptions.Compiled);

        public static void ExceptionIfStringEmpty(string s)
        {
            if (s == String.Empty)
            {
                throw new Exception("String is Empty!");
            }
        }

        public static void ExceptionIfListCountNotOne<T>(List<T> l)
        {
            if (l.Count != 1)
            {
                throw new Exception("List not of size 1");
            }
        }

        public static IElement GetFirstElementOfClassOrError(IElement el, string className)
        {
            var childrenWithClassName = el.Children.Where((c) => c.ClassName == className).ToList();
            if (childrenWithClassName.Count != 1)
            {
                throw new Exception("List not of size 1");
            }
            return childrenWithClassName.First();
        }

        public static IElement GetFirstElementOrError(IElement el)
        {
            if (el.ChildElementCount != 1)
            {
                throw new Exception("List not of size 1");
            }

            return el.FirstElementChild;
        }

        public static string ConvertToTextStringWithoutWhitespace(INode el)
        {
            return whitespaceRegex.Replace(el.Text(), " ").Trim();
        }
    }
}
