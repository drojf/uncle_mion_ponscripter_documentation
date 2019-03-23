using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace onscripter_documentation
{
    class Util
    {
        static readonly Regex whitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        public static void ExceptionIfStringEmpty(string s)
        {
            if (s == String.Empty)
            {
                throw new Exception("String is Empty!");
            }
        }

        public static void ExceptionListSize<T>(List<T> l, int listSize)
        {
            if (l.Count != listSize)
            {
                throw new Exception($"List size is {l.Count}, should be {listSize}");
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

        public static void ValidateElementClass(IElement el, string className)
        {
            if (el.ClassName != className)
            {
                throw new Exception($"unexpected node class {el.ClassName}");
            }
        }

        public static void _GetTagsRecursively<TResult>(Func<IElement, TResult> selector, HashSet<TResult> allTags, IElement currentNode)
        {
            allTags.Add(selector(currentNode));
            foreach (IElement child in currentNode.Children)
            {
                _GetTagsRecursively(selector, allTags, child);
            }
        }
        public static HashSet<TResult> GetRecursivelyAsSet<TResult>(Func<IElement, TResult> selector, IElement currentNode, bool includeRoot=true)
        {
            HashSet<TResult> tags = new HashSet<TResult>();
            if (includeRoot)
            {
                _GetTagsRecursively(selector, tags, currentNode);
            }
            else
            {
                // if not including root, just call individual on the each child of root
                foreach (IElement childElement in currentNode.Children)
                {
                    _GetTagsRecursively(selector, tags, childElement);
                }
            }
            return tags;
        }

        //Note: root node is not checked for containing valid tags
        public static void ValidateNodeOnlyContainsValidTags(IElement element, HashSet<string> allowedTags)
        {
            HashSet<string> allowedTagsWithEmptyTag = allowedTags; //allowedTags.Union(new HashSet<string>() { "" };
                                                                   //allowedTagsWithEmptyTag = allowedTagsWithEmptyTag.Union(allowedTags);

            //Don't want to include the root element itself, 
            HashSet<string> actualTags = Util.GetRecursivelyAsSet(x => x.TagName.ToLower(), element, includeRoot: false);
            bool isSubset = actualTags.IsSubsetOf(allowedTagsWithEmptyTag);

            if (!isSubset)
            {
                throw new Exception($"The element {element.TagName}'s tags is not a subset of the allowed tags.");
            }
        }

        public static void SetAndThrowIfAlreadySet<T>(T valueToSet, ref T variableToSet)
        {
            if(variableToSet != null)
            {
                throw new Exception("Variable has been set already!");
            }
            else
            {
                variableToSet = valueToSet;
            }
        }



    }
}
