using System.Text.RegularExpressions;

namespace MediaOrganizer
{
    internal class FileAssociation
    {
        public Regex RegexMatch { get; set; }
        public string Tag { get; set; }
    }
}