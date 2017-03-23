using System;
using System.Diagnostics;
using System.IO;

namespace FileFormatDetection.Core
{
    [DebuggerDisplay("{nameof(InputStream)}: Id={Id}, DetectionOptions={DetectionOptions}, Stream.Type={Stream?.GetType()}, Stream.Length={Stream?.Length}")]
    public class InputStream : InputObject
    {
        public InputStream(Stream stream, DetectionOptions detectionOptions) : base(detectionOptions)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Stream = stream;
        }

        public Stream Stream { get; }
    }
}