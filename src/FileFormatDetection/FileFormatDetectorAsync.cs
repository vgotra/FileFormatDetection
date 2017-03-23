using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileFormatDetection.Core;

namespace FileFormatDetection
{
    public class FileFormatDetectorAsync : FileFormatDetectorBase, IFileFormatDetectorAsync
    {
        //TODO: Check return formats

        public FileFormatDetectorAsync(List<FileFormat> formatsForDetection) : base(formatsForDetection)
        {
        }

        public async Task<List<FileFormat>> DetectFromFilePathAsync(string filePath, DetectionOptions detectionOptions)
        {
            return await DetectFromFilePathAsync(filePath, detectionOptions, CancellationToken.None);
        }

        public async Task<List<FileFormat>> DetectFromFilePathAsync(string filePath, DetectionOptions detectionOptions, CancellationToken cancelationToken)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            return await DetectFromStreamAsync(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), true,
                detectionOptions, cancelationToken);
        }

        public async Task<List<FileFormat>> DetectFromStreamAsync(Stream stream, bool closeStream, DetectionOptions detectionOptions)
        {
            return await DetectFromStreamAsync(stream, closeStream, detectionOptions, CancellationToken.None);
        }

        public async Task<List<FileFormat>> DetectFromStreamAsync(Stream stream, bool closeStream, DetectionOptions detectionOptions, CancellationToken cancelationToken)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentNullException(nameof(stream), "Stream should not be null or empty");
            }

            if (!stream.CanRead)
            {
                throw new InvalidOperationException("Stream cannot be processed because it does not support reading");
            }

            //TODO think about possible cases with Network streams (buffer/buffer/buffer)
            if (stream.Position != 0 && !stream.CanSeek)
            {
                throw new InvalidOperationException("Stream cannot be processed because position is not on zero index and it does not support seek");
            }

            var detectedFormats = new List<FileFormat>();

            var countBytesToRead = CountOfBytesMax - OffSetMin;
            var bytesToDetect = new byte[countBytesToRead];

            var rCount = 0;

            var ss = new SemaphoreSlim(1);

            try
            {
                await ss.WaitAsync(cancelationToken);
                stream.Seek(0, SeekOrigin.Begin);
                rCount = await stream.ReadAsync(bytesToDetect, OffSetMin, CountOfBytesMax, cancelationToken);
            }
            finally
            {
                ss.Release();
            }

            if (rCount > 0)
            {
                if (closeStream)
                {
                    using (stream)
                    {
                        detectedFormats = DetectFromByteBuffer(bytesToDetect, detectionOptions);
                        bytesToDetect = null;
                    }
                }
                else
                {
                    detectedFormats = DetectFromByteBuffer(bytesToDetect, detectionOptions);
                    bytesToDetect = null;
                }
            }

            return detectedFormats;
        }

        public async Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputFilesAsync(IList<InputFile> inputFiles)
        {
            return await DetectFromInputFilesAsync(inputFiles, CancellationToken.None);
        }

        public async Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputFilesAsync(IList<InputFile> inputFiles, CancellationToken cancellationToken)
        {
            if (inputFiles == null)
            {
                throw new ArgumentNullException(nameof(inputFiles));
            }

            if (inputFiles.Any(x => inputFiles.Count(y => y.FilePath.Equals(x.FilePath, StringComparison.OrdinalIgnoreCase)) > 1))
            {
                throw new InvalidDataException("Enumeration of file paths contains duplicates.");
            }

            var tasks = inputFiles.Select(DetectFromInputFileAsync).ToArray();

            var results = await Task.WhenAll(tasks);
            var result = new Dictionary<Guid, List<FileFormat>>();

            foreach (var keyValuePair in results)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return result;
        }

        public async Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputFileAsync(InputFile inputFile)
        {
            return await DetectFromInputFileAsync(inputFile, CancellationToken.None);
        }

        public async Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputFileAsync(InputFile inputFile, CancellationToken cancellationToken)
        {
            if (inputFile == null)
            {
                throw new ArgumentNullException(nameof(inputFile));
            }

            var result = await DetectFromStreamAsync(new FileStream(inputFile.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read),
                true, inputFile.DetectionOptions, cancellationToken);

            return new KeyValuePair<Guid, List<FileFormat>>(inputFile.Id, result);
        }

        public async Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputStreamsAsync(IList<InputStream> inputStreams)
        {
            return await DetectFromInputStreamsAsync(inputStreams, CancellationToken.None);
        }

        public async Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputStreamsAsync(IList<InputStream> inputStreams, CancellationToken cancellationToken)
        {
            if (inputStreams == null)
            {
                throw new ArgumentNullException(nameof(inputStreams));
            }

            var tasks = inputStreams.Select(DetectFromInputStreamAsync).ToArray();

            var results = await Task.WhenAll(tasks);
            var result = new Dictionary<Guid, List<FileFormat>>();
            foreach (var keyValuePair in results)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return result;
        }

        public async Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputStreamAsync(InputStream inputStream)
        {
            return await DetectFromInputStreamAsync(inputStream, CancellationToken.None);
        }

        public async Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputStreamAsync(InputStream inputStream, CancellationToken cancellationToken)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            var result = await DetectFromStreamAsync(inputStream.Stream, false, inputStream.DetectionOptions, cancellationToken);

            return new KeyValuePair<Guid, List<FileFormat>>(inputStream.Id, result);
        }

        public async Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputBuffersAsync(IList<InputBuffer> inputBuffers)
        {
            return await DetectFromInputBuffersAsync(inputBuffers, CancellationToken.None);
        }

        public async Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputBuffersAsync(IList<InputBuffer> inputBuffers, CancellationToken cancellationToken)
        {
            if (inputBuffers == null)
            {
                throw new ArgumentNullException(nameof(inputBuffers));
            }

            var tasks = inputBuffers.Select(x => new Task<KeyValuePair<Guid, List<FileFormat>>>(() => DetectFromInputBuffer(x))).ToArray();

            foreach (var task in tasks)
            {
                task.Start();
            }

            var results = await Task.WhenAll(tasks);

            var result = new Dictionary<Guid, List<FileFormat>>();
            foreach (var keyValuePair in results)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return result;
        }

        public async Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputObjectsAsync(IList<InputObject> inputObjects)
        {
            return await DetectFromInputObjectsAsync(inputObjects, CancellationToken.None);
        }

        public virtual async Task<Dictionary<Guid, List<FileFormat>>> DetectFromInputObjectsAsync(IList<InputObject> inputObjects, CancellationToken cancellationToken)
        {
            if (inputObjects == null)
            {
                throw new ArgumentNullException(nameof(inputObjects));
            }

            var inputFiles = new List<InputFile>();
            var inputStreams = new List<InputStream>();
            var inputBuffers = new List<InputBuffer>();

            foreach (var inputObject in inputObjects)
            {
                var inputFile = inputObject as InputFile;
                if (inputFile != null)
                {
                    inputFiles.Add(inputFile);
                }

                var inputStream = inputObject as InputStream;
                if (inputStream != null)
                {
                    inputStreams.Add(inputStream);
                }

                var inputBuffer = inputObject as InputBuffer;
                if (inputBuffer != null)
                {
                    inputBuffers.Add(inputBuffer);
                }
            }

            var inputFilesTask = inputFiles.Any()
                ? DetectFromInputFilesAsync(inputFiles, cancellationToken)
                : Task.FromResult(new Dictionary<Guid, List<FileFormat>>());
            var inputStreamsTask = inputStreams.Any()
                ? DetectFromInputStreamsAsync(inputStreams, cancellationToken)
                : Task.FromResult(new Dictionary<Guid, List<FileFormat>>());
            var inputBuffersTask = inputBuffers.Any()
                ? DetectFromInputBuffersAsync(inputBuffers, cancellationToken)
                : Task.FromResult(new Dictionary<Guid, List<FileFormat>>());

            var results = await Task.WhenAll(inputFilesTask, inputStreamsTask, inputBuffersTask);

            return results.SelectMany(dict => dict).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public async Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputObjectAsync(InputObject inputObject)
        {
            return await DetectFromInputObjectAsync(inputObject, CancellationToken.None);
        }

        public virtual async Task<KeyValuePair<Guid, List<FileFormat>>> DetectFromInputObjectAsync(InputObject inputObject, CancellationToken cancellationToken)
        {
            var inputFile = inputObject as InputFile;
            if (inputFile != null)
            {
                return await DetectFromInputFileAsync(inputFile, cancellationToken);
            }

            var inputStream = inputObject as InputStream;
            if (inputStream != null)
            {
                return await DetectFromInputStreamAsync(inputStream, cancellationToken);
            }

            var inputBuffer = inputObject as InputBuffer;
            if (inputBuffer != null)
            {
                return DetectFromInputBuffer(inputBuffer);
            }

            throw new InvalidDataException("Cannot detect a supported input object type");
        }
    }
}