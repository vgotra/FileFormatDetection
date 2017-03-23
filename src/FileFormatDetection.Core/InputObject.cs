using System;
using System.Diagnostics;

namespace FileFormatDetection.Core
{
    [DebuggerDisplay("{nameof(InputObject)}: Id={Id}, DetectionOptions={DetectionOptions}")]
    public class InputObject
    {
        protected InputObject(DetectionOptions detectionOptions)
        {
            Id = Guid.NewGuid();
            DetectionOptions = detectionOptions;
        }

        public Guid Id { get; }
        public DetectionOptions DetectionOptions { get; }
    }
}