using System;
using System.Collections.Generic;
using System.Linq;
using FileFormatDetection.Core;

namespace FileFormatDetection
{
    public class FileFormatDetectorBase : IFileFormatDetectorBase
    {
        protected readonly int OffSetMin;
        protected readonly int CountOfBytesMax;

        public FileFormatDetectorBase(List<FileFormat> formatsForDetection)
        {
            if (formatsForDetection == null || !formatsForDetection.Any())
            {
                throw new ArgumentNullException(nameof(formatsForDetection));
            }

            FormatsForDetection = formatsForDetection;

            OffSetMin = FormatsForDetection.Min(x => x.FormatSignatures.Min(y => y.Offset));

            var maxOffset = FormatsForDetection.Max(x => x.FormatSignatures.Max(y => y.Offset));
            CountOfBytesMax = maxOffset + FormatsForDetection.SelectMany(x => x.FormatSignatures.Where(y => y.Offset >= maxOffset)).Max(z => z.ToRead);
        }

        public List<FileFormat> FormatsForDetection { get; }

        public List<FileFormat> DetectFromByteBuffer(byte[] buffer, DetectionOptions detectionOptions)
        {
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            var detectedFormats = new List<FileFormat>();

            var offsetToRemove = OffSetMin;
            foreach (var fileFormat in FormatsForDetection)
            {
                var detected = 0;
                foreach (var formatSignature in fileFormat.FormatSignatures)
                {
                    var startPos = formatSignature.Offset - offsetToRemove;
                    if (startPos < 0)
                    {
                        throw new InvalidOperationException("Wrong calculation or wrong formats data");
                    }

                    var signature = BitConverter.ToString(buffer, startPos, formatSignature.ToRead);
                    if (string.Equals(signature, formatSignature.Hex, StringComparison.OrdinalIgnoreCase))
                    {
                        detected++;
                    }
                }

                if (detected == fileFormat.FormatSignatures.Length)
                {
                    detectedFormats.Add(fileFormat);
                    if (detectionOptions == DetectionOptions.ReturnFirstDetected)
                    {
                        return detectedFormats;
                    }
                }
            }

            return detectedFormats;
        }

        public KeyValuePair<Guid, List<FileFormat>> DetectFromInputBuffer(InputBuffer inputBuffer)
        {
            if (inputBuffer == null)
            {
                throw new ArgumentNullException(nameof(inputBuffer));
            }

            var result = DetectFromByteBuffer(inputBuffer.Buffer, inputBuffer.DetectionOptions);

            return new KeyValuePair<Guid, List<FileFormat>>(inputBuffer.Id, result);
        }
    }
}