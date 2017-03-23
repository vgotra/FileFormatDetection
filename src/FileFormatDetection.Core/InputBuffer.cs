using System;
using System.Diagnostics;

namespace FileFormatDetection.Core
{
    [DebuggerDisplay("{nameof(InputBuffer)}: Id={Id}, DetectionOptions={DetectionOptions}, Buffer.Length={Buffer?.Length}")]
    public class InputBuffer : InputObject
    {
        public InputBuffer(byte[] buffer, DetectionOptions detectionOptions) : base(detectionOptions)
        {
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            Buffer = buffer;
        }

        public byte[] Buffer { get; }
    }
}