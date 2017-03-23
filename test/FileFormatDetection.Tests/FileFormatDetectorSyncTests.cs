using System;
using System.Collections.Generic;
using System.IO;
using FileFormatDetection.Core;
using FileFormatDetection.Signatures;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace FileFormatDetection.Tests
{
    [TestClass]
    [DeploymentItem("Audio.json")]
    [DeploymentItem("Samples")]
    public class FileFormatDetectorSyncTests
    {
        private const string PathToSamples = "Samples";
        private string PathToAudioDescriptionFile = "Audio.json";

        public FileFormatDetectorSyncTests()
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
        public void DetectFromFilePath()
        {
            var filePath = Directory.EnumerateFiles(PathToSamples).First();
            var f = GetDetector();

            var result = f.DetectFromFilePath(filePath, DetectionOptions.ReturnAllDetected);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectFromFilePath_WrongFilepath()
        {
            var f = GetDetector();

            f.DetectFromFilePath(" ", DetectionOptions.ReturnAllDetected);
        }

        [TestMethod]
        public void DetectFromStream_DontCloseStream()
        {
            using (var fileStream = new FileStream(Directory.EnumerateFiles(PathToSamples).First(), FileMode.Open, FileAccess.Read))
            {
                var f = GetDetector();

                var result = f.DetectFromStream(fileStream, false, DetectionOptions.ReturnAllDetected);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Any());
                Assert.IsNotNull(fileStream);
                Assert.IsTrue(fileStream.Position > 0);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void DetectFromStream_CloseStream()
        {
            var fileStream = new FileStream(Directory.EnumerateFiles(PathToSamples).First(), FileMode.Open, FileAccess.Read);

            var f = GetDetector();

            var result = f.DetectFromStream(fileStream, true, DetectionOptions.ReturnAllDetected);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsNotNull(fileStream);

            // should throw DisposedException
            fileStream.Seek(0, SeekOrigin.Begin);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectFromStream_NullOrEmptyStream()
        {
            var f = GetDetector();

            var stream = Substitute.For<Stream>();
            stream.Length.Returns(0);

            f.DetectFromStream(stream, true, DetectionOptions.ReturnAllDetected);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DetectFromStream_NotSeekableStream()
        {
            var f = GetDetector();

            var stream = Substitute.For<Stream>();
            stream.Length.Returns(10);
            stream.Position.Returns(1);
            stream.CanRead.Returns(true);
            stream.CanSeek.Returns(false);

            f.DetectFromStream(stream, true, DetectionOptions.ReturnAllDetected);
        }

        [TestMethod]
        public void DetectFromInputFiles()
        {
            var inputFiles = Directory.GetFiles(PathToSamples).Select(x => new InputFile(x, DetectionOptions.ReturnFirstDetected)).ToList();

            var f = GetDetector();

            var result = f.DetectFromInputFiles(inputFiles);

            Assert.AreEqual(inputFiles.Count, result.Count);
            Assert.IsTrue(inputFiles.All(inputFile => result.ContainsKey(inputFile.Id)));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void DetectFromInputFiles_DuplicateInputFiles()
        {
            var inputFiles = Directory.GetFiles(PathToSamples).Select(x => new InputFile(x, DetectionOptions.ReturnFirstDetected)).ToList();
            var existingFile = inputFiles.First();
            inputFiles.Add(existingFile);

            var f = GetDetector();

            f.DetectFromInputFiles(inputFiles);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectFromInputFiles_NullInputFile()
        {
            var f = GetDetector();

            f.DetectFromInputFiles(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DetectFromStream_NotReadableStream()
        {
            var f = GetDetector();

            var stream = Substitute.For<Stream>();
            stream.Length.Returns(10);
            stream.CanRead.Returns(false);

            f.DetectFromStream(stream, true, DetectionOptions.ReturnAllDetected);
        }

        [TestMethod]
        public void DetectFromStream_StreamWithNonZeroPosition()
        {
            using (var fileStream = new FileStream(Directory.EnumerateFiles(PathToSamples).First(), FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(1, SeekOrigin.Begin);

                var f = GetDetector();

                var result = f.DetectFromStream(fileStream, false, DetectionOptions.ReturnAllDetected);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Any());
                Assert.IsTrue(fileStream.Position > 1);
            }
        }

        [TestMethod]
        public void DetectFromInputFile()
        {
            var inputFile = new InputFile(Directory.EnumerateFiles(PathToSamples).First(), DetectionOptions.ReturnFirstDetected);
            var f = GetDetector();

            var result = f.DetectFromInputFile(inputFile);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Key == inputFile.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectFromInputFile_NullInputFile()
        {
            var f = GetDetector();

            f.DetectFromInputFile(null);
        }

        [TestMethod]
        public void DetectFromInputStreams()
        {
            var inputStreams = Directory.EnumerateFiles(PathToSamples)
                .Select(x => new InputStream(new FileStream(x, FileMode.Open, FileAccess.Read), DetectionOptions.ReturnFirstDetected)).ToList();

            var f = GetDetector();

            var result = f.DetectFromInputStreams(inputStreams);

            Assert.AreEqual(inputStreams.Count, result.Count);
            Assert.IsTrue(inputStreams.All(inputStream => result.ContainsKey(inputStream.Id)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectFromInoutStreams_NullStreams()
        {
            var f = GetDetector();

            f.DetectFromInputStreams(null);
        }

        [TestMethod]
        public void DetectFromStream()
        {
            var inputStream = new InputStream(new FileStream(Directory.EnumerateFiles(PathToSamples).First(), FileMode.Open, FileAccess.Read),
                DetectionOptions.ReturnFirstDetected);

            var f = GetDetector();

            var result = f.DetectFromInputStream(inputStream);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Key, inputStream.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectFromStream_NullInputStream()
        {
            var f = GetDetector();

            f.DetectFromInputStream(null);
        }

        [TestMethod]
        public void DetectFromInputBuffers()
        {
            var inputBuffers = Directory.EnumerateFiles(PathToSamples)
                .Select(x => new InputBuffer(File.ReadAllBytes(x), DetectionOptions.ReturnFirstDetected)).ToList();

            var f = GetDetector();

            var result = f.DetectFromInputBuffers(inputBuffers);

            Assert.AreEqual(inputBuffers.Count, result.Count);
            Assert.IsTrue(inputBuffers.All(inputBuffer => result.ContainsKey(inputBuffer.Id)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectFromInputBuffers_Null()
        {
            var f = GetDetector();

            f.DetectFromInputBuffers(null);
        }

        [TestMethod]
        public void DetectFromInputObjects()
        {
            var filePath = Directory.EnumerateFiles(PathToSamples).First();
            var inputBuffer = new InputBuffer(File.ReadAllBytes(filePath), DetectionOptions.ReturnFirstDetected);
            var inputFile = new InputFile(filePath, DetectionOptions.ReturnFirstDetected);
            var inputStream = new InputStream(new FileStream(filePath, FileMode.Open, FileAccess.Read), DetectionOptions.ReturnFirstDetected);
            var inputObjects = new List<InputObject>() { inputBuffer, inputFile, inputStream };

            var f = GetDetector();

            var result = f.DetectFromInputObjects(inputObjects);

            Assert.AreEqual(inputObjects.Count, result.Count);
            Assert.IsTrue(inputObjects.All(io => result.ContainsKey(io.Id)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectFromInputObjects_Null()
        {
            var f = GetDetector();

            f.DetectFromInputObjects(null);
        }

        [TestMethod]
        public void DetectFromInputObjects_EmptyInput()
        {
            var inputObjects = new List<InputObject>();

            var f = GetDetector();

            var result = f.DetectFromInputObjects(inputObjects);

            Assert.AreEqual(inputObjects.Count, result.Count);
        }

        [TestMethod]
        public void DetectFromInputObject()
        {
            var filePath = Directory.EnumerateFiles(PathToSamples).First();
            var inputBuffer = new InputBuffer(File.ReadAllBytes(filePath), DetectionOptions.ReturnFirstDetected);
            var inputFile = new InputFile(filePath, DetectionOptions.ReturnFirstDetected);
            var inputStream = new InputStream(new FileStream(filePath, FileMode.Open, FileAccess.Read), DetectionOptions.ReturnFirstDetected);

            var f = GetDetector();

            var inputBufferResult = f.DetectFromInputObject(inputBuffer);
            var inputFileResult = f.DetectFromInputObject(inputFile);
            var inputStreamResult = f.DetectFromInputObject(inputStream);

            Assert.AreEqual(inputBuffer.Id, inputBufferResult.Key);
            Assert.AreEqual(inputFile.Id, inputFileResult.Key);
            Assert.AreEqual(inputStream.Id, inputStreamResult.Key);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void DetectFromInputObject_WrongInputObject()
        {
            var f = GetDetector();

            f.DetectFromInputObject(null);
        }

        private FileFormatDetector GetDetector()
        {
            var audio = new Audio(File.ReadAllText(PathToAudioDescriptionFile));
            var f = new FileFormatDetector(new List<FileFormat>
            {
                audio.AudioMp3,
                audio.AudioWav,
                audio.AudioOgg
            });
            return f;
        }
    }
}
