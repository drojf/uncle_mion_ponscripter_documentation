using AngleSharp.Dom;

namespace onscripter_documentation
{
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

            return new H2HeaderInformation(sectionsWhereCommandCanBeUsed, versionsWhereCommandCanBeUsed, wordName);
        }
    }
}
