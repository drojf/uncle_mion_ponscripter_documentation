namespace onscripter_documentation
{
    partial class Program
    {
        class Link
        {
            string linkText;
            string linkId;
            string linkURL;

            public Link(string linkText, string linkURL)
            {
                this.linkText = linkText;
                this.linkURL = linkURL;
                this.linkId = linkURL.TrimStart(new char[] { '#' });
            }
        }
    }
}
