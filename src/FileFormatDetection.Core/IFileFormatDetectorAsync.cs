using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileFormatDetection.Core
{
    public interface IFileFormatDetectorAsync : IFileFormatDetectorBase
    {
        Task<List<FileFormat>> DetectFromFilePathAsync(string filePath, DetectionOptions detectionOptions);
        Task<List<FileFormat>> DetectFromFilePathAsync(string filePath, DetectionOptions detectionOptions, CancellationToken cancelationToken);
        Task<List<FileFormat>> DetectFromStreamAsync(Stream stream, bool closeStream, DetectionOptions detectionOptions);
        Task<List<FileFormat>> DetectFromStreamAsync(Stream stream, bool closeStream, DetectionOptions detectionOptions, CancellationToken cancelationToken);
        Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputFilesAsync(IList<InputFile> inputFiles);
        Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputFilesAsync(IList<InputFile> inputFiles, CancellationToken cancellationToken);
        Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputFileAsync(InputFile inputFile);
        Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputFileAsync(InputFile inputFile, CancellationToken cancellationToken);
        Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputStreamsAsync(IList<InputStream> inputStreams);
        Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputStreamsAsync(IList<InputStream> inputStreams, CancellationToken cancellationToken);
        Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputStreamAsync(InputStream inputStream);
        Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputStreamAsync(InputStream inputStream, CancellationToken cancellationToken);
        Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputBuffersAsync(IList<InputBuffer> inputBuffers);
        Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputBuffersAsync(IList<InputBuffer> inputBuffers, CancellationToken cancellationToken);
        Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputObjectsAsync(IList<InputObject> inputObjects);
        Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputObjectsAsync(IList<InputObject> inputObjects, CancellationToken cancellationToken);
        Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputObjectAsync(InputObject inputObject);
        Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputObjectAsync(InputObject inputObject, CancellationToken cancellationToken);
    }
}