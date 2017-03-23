using System;
using System.Linq;
using System.Collections.Generic;
using FileFormatDetection.Core;

namespace FileFormatDetection.Signatures
{
    public class Video
    {
        public List <FileFormat> FileFormats { get; }

        public Video(string formatsContent)
        {
            FileFormats = FileFormatsLoader.LoadFromJsonContent(formatsContent);
        }

        public FileFormat VideoMp4 => FileFormats.First(x => x.Name.Equals("Mp4", StringComparison.OrdinalIgnoreCase));

        public FileFormat Video3gpp => FileFormats.First(x => x.Name.Equals("3gpp", StringComparison.OrdinalIgnoreCase));

        public FileFormat Video3gpp2 => FileFormats.First(x => x.Name.Equals("3gpp2", StringComparison.OrdinalIgnoreCase));
    }
}