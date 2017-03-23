using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileFormatDetection.Core;
using System.Threading;

namespace FileFormatDetection
{
    public class FileFormatDetector : FileFormatDetectorBase, IFileFormatDetector
    {
        public FileFormatDetector(List<FileFormat> formatsForDetection) : base(formatsForDetection)
        {
        }

        public List<FileFormat> DetectFromFilePath(string filePath, DetectionOptions detectionOptions)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            return DetectFromStream(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), true, detectionOptions);
        }

        public List<FileFormat> DetectFromStream(Stream stream, bool closeStream, DetectionOptions detectionOptions)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentNullException(nameof(stream));
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
                ss.Wait();
                stream.Seek(0, SeekOrigin.Begin);
                rCount = stream.Read(bytesToDetect, OffSetMin, CountOfBytesMax);
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

        public Dictionary<Guid, List<FileFormat>> DetectFromInputFiles(IList<InputFile> inputFiles)
        {
            if (inputFiles == null)
            {
                throw new ArgumentNullException(nameof(inputFiles));
            }

            if (inputFiles.Any(x => inputFiles.Count(y => y.FilePath.Equals(x.FilePath, StringComparison.OrdinalIgnoreCase)) > 1))
            {
                throw new InvalidDataException("Enumeration of file paths contains duplicates.");
            }

            var tasks = inputFiles.Select(i => new Task<KeyValuePair<Guid, List<FileFormat>>>(() => DetectFromInputFile(i))).ToArray();

            foreach (var task in tasks)
            {
                task.Start();
            }

            var results = Task.WhenAll(tasks).GetAwaiter().GetResult();
            var result = new Dictionary<Guid, List<FileFormat>>();

            foreach (var keyValuePair in results)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return result;
        }

        public KeyValuePair<Guid, List<FileFormat>> DetectFromInputFile(InputFile inputFile)
        {
            if (inputFile == null)
            {
                throw new ArgumentNullException(nameof(inputFile));
            }

            var result = DetectFromStream(new FileStream(inputFile.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read),
                true,
                inputFile.DetectionOptions);

            return new KeyValuePair<Guid, List<FileFormat>>(inputFile.Id, result);
        }

        public Dictionary<Guid, List<FileFormat>> DetectFromInputStreams(IList<InputStream> inputStreams)
        {
            if (inputStreams == null)
            {
                throw new ArgumentNullException(nameof(inputStreams));
            }

            var tasks = inputStreams.Select(x => new Task<KeyValuePair<Guid, List<FileFormat>>>(() => DetectFromInputStream(x))).ToArray();

            foreach (var task in tasks)
            {
                task.Start();
            }

            var results = Task.WhenAll(tasks).GetAwaiter().GetResult();
            var result = new Dictionary<Guid, List<FileFormat>>();
            foreach (var keyValuePair in results)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return result;
        }

        public KeyValuePair<Guid, List<FileFormat>> DetectFromInputStream(InputStream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            var result = DetectFromStream(inputStream.Stream, false, inputStream.DetectionOptions);

            return new KeyValuePair<Guid, List<FileFormat>>(inputStream.Id, result);
        }

        public Dictionary<Guid, List<FileFormat>> DetectFromInputBuffers(IList<InputBuffer> inputBuffers)
        {
            if (inputBuffers == null)
            {
                throw new ArgumentNullException(nameof(inputBuffers));
            }

            var tasks = inputBuffers.Select(i => new Task<KeyValuePair<Guid, List<FileFormat>>>(() => DetectFromInputBuffer(i))).ToArray();

            foreach (var task in tasks)
            {
                task.Start();
            }

            var results = Task.WhenAll(tasks).GetAwaiter().GetResult();
            var result = new Dictionary<Guid, List<FileFormat>>();
            foreach (var keyValuePair in results)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return result;
        }

        public virtual Dictionary<Guid, List<FileFormat>> DetectFromInputObjects(IList<InputObject> inputObjects)
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
                ? new Task<Dictionary<Guid, List<FileFormat>>>(() => DetectFromInputFiles(inputFiles))
                : Task.FromResult(new Dictionary<Guid, List<FileFormat>>());
            var inputStreamsTask = inputStreams.Any()
                ? new Task<Dictionary<Guid, List<FileFormat>>>(() => DetectFromInputStreams(inputStreams))
                : Task.FromResult(new Dictionary<Guid, List<FileFormat>>());
            var inputBuffersTask = inputBuffers.Any()
                ? new Task<Dictionary<Guid, List<FileFormat>>>(() => DetectFromInputBuffers(inputBuffers))
                : Task.FromResult(new Dictionary<Guid, List<FileFormat>>());

            if (inputFilesTask.Status == TaskStatus.Created)
            {
                inputFilesTask.Start();
            }
            if (inputStreamsTask.Status == TaskStatus.Created)
            {
                inputStreamsTask.Start();
            }
            if (inputBuffersTask.Status == TaskStatus.Created)
            {
                inputBuffersTask.Start();
            }

            var results = Task.WhenAll(inputFilesTask, inputStreamsTask, inputBuffersTask).GetAwaiter().GetResult();

            return results.SelectMany(dict => dict).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public virtual KeyValuePair<Guid, List<FileFormat>> DetectFromInputObject(InputObject inputObject)
        {
            var inputFile = inputObject as InputFile;
            if (inputFile != null)
            {
                return DetectFromInputFile(inputFile);
            }

            var inputStream = inputObject as InputStream;
            if (inputStream != null)
            {
                return DetectFromInputStream(inputStream);
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