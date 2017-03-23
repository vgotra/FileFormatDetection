using System.Linq;

namespace FileFormatDetection.Core
{
    public class FileFormat
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string[] Extensions { get; set; }
        public FormatSignature[] FormatSignatures { get; set; }
        public override string ToString()
        {
            return $"{Name} - {{{FormatSignatures.First().Hex}}}";
        }
    }
}
