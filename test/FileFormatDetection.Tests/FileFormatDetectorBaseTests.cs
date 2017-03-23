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
    public class FileFormatDetectorBaseTests
    {
        //TODO: add tests for offsets

        private const string PathToSamples = "Samples";
        private string PathToAudioDescriptionFile = "Audio.json";

        public FileFormatDetectorBaseTests()
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
        public void FileFormatDetectorBase_Ctor()
        {
            var f = GetDetector();
            Assert.IsNotNull(f);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FileFormatDetectorBase_NUllOrEmptyFormats()
        {
            new FileFormatDetectorBase(new List<FileFormat>());
        }

        [TestMethod]
        public void DetectFromByteBuffer()
        {
            var bytesBuffer = File.ReadAllBytes(Directory.EnumerateFiles(PathToSamples).First());
            var f = GetDetector();

            var result = f.DetectFromByteBuffer(bytesBuffer, DetectionOptions.ReturnAllDetected);
            
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectFromByteBuffer_NullOrEmptyBuffer()
        {
            var f = GetDetector();
            f.DetectFromByteBuffer(new byte[0], DetectionOptions.ReturnFirstDetected);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DetectFromByteBuffer_WrongStartPos()
        {
            var audioMp3 = new Audio(File.ReadAllText(PathToAudioDescriptionFile)).AudioMp3;
            var f = new FileFormatDetectorBase(new List<FileFormat> { audioMp3 });

            //change offset
            audioMp3.FormatSignatures.First().Offset = audioMp3.FormatSignatures.First().Offset - 10;

            var bytesBuffer = File.ReadAllBytes(Directory.EnumerateFiles(PathToSamples).First());

            f.DetectFromByteBuffer(bytesBuffer, DetectionOptions.ReturnAllDetected);
        }

        [TestMethod]
        public void DetectFromInputBuffer()
        {
            var inputBuffer = new InputBuffer(File.ReadAllBytes(Directory.EnumerateFiles(PathToSamples).First()), DetectionOptions.ReturnFirstDetected);
            var f = GetDetector();

            var result = f.DetectFromInputBuffer(inputBuffer);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Key == inputBuffer.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectFromInputBuffer_NullInputBuffer()
        {
            var f = GetDetector();
            f.DetectFromInputBuffer(null);
        }

        private FileFormatDetectorBase GetDetector()
        {
            var audio = new Audio(File.ReadAllText(PathToAudioDescriptionFile));
            var f = new FileFormatDetectorBase(new List<FileFormat>
            {
                audio.AudioMp3,
                audio.AudioWav,
                audio.AudioOgg
            });
            return f;
        }
    }
}
