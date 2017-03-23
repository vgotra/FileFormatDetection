using System;
using System.Collections.Generic;

namespace FileFormatDetection.Core
{
    public interface IFileFormatDetectorBase
    {
        List<FileFormat> FormatsForDetection { get; }
        List<FileFormat> DetectFromByteBuffer(byte[] buffer, DetectionOptions detectionOptions);
        KeyValuePair<Guid, List<FileFormat>> DetectFromInputBuffer(InputBuffer inputBuffer);
    }
}