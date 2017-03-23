using System;
using System.Linq;
using System.Collections.Generic;
using FileFormatDetection.Core;

namespace FileFormatDetection.Signatures
{
    public class Audio
    {
        public List <FileFormat> FileFormats { get; }

        public Audio(string formatsContent)
        {
            FileFormats = FileFormatsLoader.LoadFromJsonContent(formatsContent);
        }

        public FileFormat AudioMp3 => FileFormats.First(x => x.Name.Equals("Mp3", StringComparison.OrdinalIgnoreCase));

        public FileFormat AudioWav => FileFormats.First(x => x.Name.Equals("Wav", StringComparison.OrdinalIgnoreCase));

        public FileFormat AudioOgg => FileFormats.First(x => x.Name.Equals("Ogg", StringComparison.OrdinalIgnoreCase));

        public FileFormat Audio3gpp => FileFormats.First(x => x.Name.Equals("3gpp", StringComparison.OrdinalIgnoreCase));

        public FileFormat Audio3gpp2 => FileFormats.First(x => x.Name.Equals("3gpp2", StringComparison.OrdinalIgnoreCase));
    }
}