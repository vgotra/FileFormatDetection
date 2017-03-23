using System;
using System.Collections.Generic;
using System.IO;

namespace FileFormatDetection.Core
{
    public interface IFileFormatDetector : IFileFormatDetectorBase
    {
        List<FileFormat> DetectFromFilePath(string filePath, DetectionOptions detectionOptions);
        List<FileFormat> DetectFromStream(Stream stream, bool closeStream, DetectionOptions detectionOptions);
        Dictionary<Guid, List<FileFormat>> DetectFromInputFiles(IList<InputFile> inputFiles);
        KeyValuePair<Guid, List<FileFormat>> DetectFromInputFile(InputFile inputFile);
        Dictionary<Guid, List<FileFormat>> DetectFromInputStreams(IList<InputStream> inputStreams);
        KeyValuePair<Guid, List<FileFormat>> DetectFromInputStream(InputStream inputStream);
        Dictionary<Guid, List<FileFormat>> DetectFromInputBuffers(IList<InputBuffer> inputBuffers);
        Dictionary<Guid, List<FileFormat>> DetectFromInputObjects(IList<InputObject> inputObjects);
        KeyValuePair<Guid, List<FileFormat>> DetectFromInputObject(InputObject inputObject);
    }
}