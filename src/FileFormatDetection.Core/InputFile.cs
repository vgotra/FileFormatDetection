using System;
using System.Diagnostics;
using System.IO;

namespace FileFormatDetection.Core
{
    [DebuggerDisplay("{nameof(InputFile)}: Id={Id}, DetectionOptions={DetectionOptions}, FilePath={FilePath}")]
    public class InputFile : InputObject
    {
        public InputFile(string filePath, DetectionOptions detectionOptions) : base(detectionOptions)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File was not found", filePath);
            }

            FilePath = filePath;
        }

        public string FilePath { get; }
    }
}