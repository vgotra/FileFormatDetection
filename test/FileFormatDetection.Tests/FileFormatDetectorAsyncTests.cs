using System;
using System.Collections.Generic;
using System.IO;
using FileFormatDetection.Core;
using FileFormatDetection.Signatures;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace FileFormatDetection.Tests
{
    [TestClass]
    [DeploymentItem("Audio.json")]
    [DeploymentItem("Samples")]
    public class FileFormatDetectorAsyncTests
    {
        //TODO: maybe it's good to replace files with mocked streams

        private const string PathToSamples = "Samples";
        private string PathToAudioDescriptionFile = "Audio.json";

        public FileFormatDetectorAsyncTests()
        {
            if (!Directory.Exists(PathToSamples))
            {
                throw new DirectoryNotFoundException($"Path to samples was not found: {PathToSamples}");
            }

            if (!File.Exists(PathToAudioDescriptionFile))
            {
                throw new FileNotFoundException($"Description file was not found {PathToAudioDescriptionFile}", PathToAudioDescriptionFile);
            }
        }

        [TestMethod]
        public async Task DetectFromFilePathAsync()
        {
            var filePath = Directory.EnumerateFiles(PathToSamples).First();
            var f = GetDetector();

            var result = await f.DetectFromFilePathAsync(filePath, DetectionOptions.ReturnAllDetected);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DetectFromFilePathAsync_WrongFilepath()
        {
            var f = GetDetector();

            await f.DetectFromFilePathAsync(" ", DetectionOptions.ReturnAllDetected);
        }

        [TestMethod]
        public async Task DetectFromStreamAsync_DontCloseStream()
        {
            using (var fileStream = new FileStream(Directory.EnumerateFiles(PathToSamples).First(), FileMode.Open, FileAccess.Read))
            {
                var f = GetDetector();

                var result = await f.DetectFromStreamAsync(fileStream, false, DetectionOptions.ReturnAllDetected);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Any());
                Assert.IsNotNull(fileStream);
                Assert.IsTrue(fileStream.Position > 0);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public async Task DetectFromStreamAsync_CloseStream()
        {
            var fileStream = new FileStream(Directory.EnumerateFiles(PathToSamples).First(), FileMode.Open, FileAccess.Read);

            var f = GetDetector();

            var result = await f.DetectFromStreamAsync(fileStream, true, DetectionOptions.ReturnAllDetected);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsNotNull(fileStream);

            // should throw DisposedException
            fileStream.Seek(0, SeekOrigin.Begin);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DetectFromStreamAsync_NullOrEmptyStream()
        {
            var f = GetDetector();

            var stream = Substitute.For<Stream>();
            stream.Length.Returns(0);

            await f.DetectFromStreamAsync(stream, true, DetectionOptions.ReturnAllDetected);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DetectFromStreamAsync_NotSeekableStream()
        {
            var f = GetDetector();

            var stream = Substitute.For<Stream>();
            stream.Length.Returns(10);
            stream.Position.Returns(1);
            stream.CanRead.Returns(true);
            stream.CanSeek.Returns(false);

            await f.DetectFromStreamAsync(stream, true, DetectionOptions.ReturnAllDetected);
        }

        [TestMethod]
        public async Task DetectFromInputFilesAsync()
        {
            var inputFiles = Directory.GetFiles(PathToSamples).Select(x => new InputFile(x, DetectionOptions.ReturnFirstDetected)).ToList();

            var f = GetDetector();

            var result = await f.DetectFromInputFilesAsync(inputFiles);

            Assert.AreEqual(inputFiles.Count, result.Count);
            Assert.IsTrue(inputFiles.All(inputFile => result.ContainsKey(inputFile.Id)));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task DetectFromInputFilesAsync_DuplicateInputFiles()
        {
            var inputFiles = Directory.GetFiles(PathToSamples).Select(x => new InputFile(x, DetectionOptions.ReturnFirstDetected)).ToList();
            var existingFile = inputFiles.First();
            inputFiles.Add(existingFile);

            var f = GetDetector();

            await f.DetectFromInputFilesAsync(inputFiles);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DetectFromInputFilesAsync_NullInputFile()
        {
            var f = GetDetector();

            await f.DetectFromInputFilesAsync(null);
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DetectFromStreamAsync_NotReadableStream()
        {
            var f = GetDetector();

            var stream = Substitute.For<Stream>();
            stream.Length.Returns(10);
            stream.CanRead.Returns(false);

            await f.DetectFromStreamAsync(stream, true, DetectionOptions.ReturnAllDetected);
        }

        [TestMethod]
        public async Task DetectFromStreamAsync_StreamWithNonZeroPosition()
        {
            using (var fileStream = new FileStream(Directory.EnumerateFiles(PathToSamples).First(), FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(1, SeekOrigin.Begin);

                var f = GetDetector();

                var result = await f.DetectFromStreamAsync(fileStream, false, DetectionOptions.ReturnAllDetected);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Any());
                Assert.IsTrue(fileStream.Position > 1);
            }
        }

        [TestMethod]
        public async Task DetectFromInputFileAsync()
        {
            var inputFile = new InputFile(Directory.EnumerateFiles(PathToSamples).First(), DetectionOptions.ReturnFirstDetected);
            var f = GetDetector();

            var result = await f.DetectFromInputFileAsync(inputFile);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Key == inputFile.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DetectFromInputFileAsync_NullInputFile()
        {
            var f = GetDetector();

            await f.DetectFromInputFileAsync(null);
        }

        [TestMethod]
        public async Task DetectFromInputStreamsAsync()
        {
            var inputStreams = Directory.EnumerateFiles(PathToSamples)
                .Select(x => new InputStream(new FileStream(x, FileMode.Open, FileAccess.Read), DetectionOptions.ReturnFirstDetected)).ToList();

            var f = GetDetector();

            var result = await f.DetectFromInputStreamsAsync(inputStreams);

            Assert.AreEqual(inputStreams.Count, result.Count);
            Assert.IsTrue(inputStreams.All(inputStream => result.ContainsKey(inputStream.Id)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DetectFromInputStreamsAsync_NullStreams()
        {
            var f = GetDetector();

            await f.DetectFromInputStreamsAsync(null);
        }

        [TestMethod]
        public async Task DetectFromStreamAsync()
        {
            var inputStream = new InputStream(new FileStream(Directory.EnumerateFiles(PathToSamples).First(), FileMode.Open, FileAccess.Read),
                DetectionOptions.ReturnFirstDetected);

            var f = GetDetector();

            var result = await f.DetectFromInputStreamAsync(inputStream);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Key, inputStream.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DetectFromStreamAsync_NullInputStream()
        {
            var f = GetDetector();

            await f.DetectFromInputStreamAsync(null);
        }

        [TestMethod]
        public async Task DetectFromInputBuffersAsync()
        {
            var inputBuffers = Directory.EnumerateFiles(PathToSamples)
                .Select(x => new InputBuffer(File.ReadAllBytes(x), DetectionOptions.ReturnFirstDetected)).ToList();

            var f = GetDetector();

            var result = await f.DetectFromInputBuffersAsync(inputBuffers);

            Assert.AreEqual(inputBuffers.Count, result.Count);
            Assert.IsTrue(inputBuffers.All(inputBuffer => result.ContainsKey(inputBuffer.Id)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DetectFromInputBuffersAsync_Null()
        {
            var f = GetDetector();

            await f.DetectFromInputBuffersAsync(null);
        }

        [TestMethod]
        public async Task DetectFromInputObjectsAsync()
        {
            var filePath = Directory.EnumerateFiles(PathToSamples).First();
            var inputBuffer = new InputBuffer(File.ReadAllBytes(filePath), DetectionOptions.ReturnFirstDetected);
            var inputFile = new InputFile(filePath, DetectionOptions.ReturnFirstDetected);
            var inputStream = new InputStream(new FileStream(filePath, FileMode.Open, FileAccess.Read), DetectionOptions.ReturnFirstDetected);
            var inputObjects = new List<InputObject>() {inputBuffer, inputFile, inputStream};

            var f = GetDetector();

            var result = await f.DetectFromInputObjectsAsync(inputObjects);

            Assert.AreEqual(inputObjects.Count, result.Count);
            Assert.IsTrue(inputObjects.All(io => result.ContainsKey(io.Id)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DetectFromInputObjectsAsync_Null()
        {
            var f = GetDetector();

            await f.DetectFromInputObjectsAsync(null);
        }

        [TestMethod]
        public async Task DetectFromInputObjectsAsync_EmptyInput()
        {
            var inputObjects = new List<InputObject>();

            var f = GetDetector();

            var result = await f.DetectFromInputObjectsAsync(inputObjects);

            Assert.AreEqual(inputObjects.Count, result.Count);
        }

        [TestMethod]
        public async Task DetectFromInputObjectAsync()
        {
            var filePath = Directory.EnumerateFiles(PathToSamples).First();
            var inputBuffer = new InputBuffer(File.ReadAllBytes(filePath), DetectionOptions.ReturnFirstDetected);
            var inputFile = new InputFile(filePath, DetectionOptions.ReturnFirstDetected);
            var inputStream = new InputStream(new FileStream(filePath, FileMode.Open, FileAccess.Read), DetectionOptions.ReturnFirstDetected);

            var f = GetDetector();

            var inputBufferResult = await f.DetectFromInputObjectAsync(inputBuffer);
            var inputFileResult = await f.DetectFromInputObjectAsync(inputFile);
            var inputStreamResult = await f.DetectFromInputObjectAsync(inputStream);

            Assert.AreEqual(inputBuffer.Id, inputBufferResult.Key);
            Assert.AreEqual(inputFile.Id, inputFileResult.Key);
            Assert.AreEqual(inputStream.Id, inputStreamResult.Key);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task DetectFromInputObjectAsync_WrongInputObject()
        {
            var f = GetDetector();

            await f.DetectFromInputObjectAsync(null);
        }

        private FileFormatDetectorAsync GetDetector()
        {
            var audio = new Audio(File.ReadAllText(PathToAudioDescriptionFile));
            var f = new FileFormatDetectorAsync(new List<FileFormat>
            {
                audio.AudioMp3,
                audio.AudioWav,
                audio.AudioOgg
            });
            return f;
        }
    }
}
