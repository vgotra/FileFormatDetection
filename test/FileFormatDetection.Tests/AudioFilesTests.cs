using System.Collections.Generic;
using System.IO;
using FileFormatDetection.Core;
using FileFormatDetection.Signatures;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFormatDetection.Tests
{
    [TestClass]
    [DeploymentItem(@"Audio.json")]
    [DeploymentItem(@"Samples")]
    public class AudioFilesTests
    {
        private string PathToAudioDescriptionFile = @"Audio.json";
        private string PathToSamples = "Samples";

        public AudioFilesTests()
        {
            if (!File.Exists(PathToAudioDescriptionFile))
            {
                throw new FileNotFoundException($"Description file was not found {PathToAudioDescriptionFile}", PathToAudioDescriptionFile);
            }
        }

        [TestMethod]
        public void AudioTests_Wav()
        {
            var audio = new Audio(File.ReadAllText(PathToAudioDescriptionFile));
            var expectedType = audio.AudioWav.Name;
            var files = Directory.GetFiles(PathToSamples, $"*.{audio.AudioWav.Name}");
            var f = new FileFormatDetector(new List<FileFormat>
            {
                audio.AudioMp3,
                audio.AudioWav
            });

            var actualCount = 0;
            foreach (var file in files)
            {
                var detected = f.DetectFromFilePath(file, DetectionOptions.ReturnFirstDetected);
                if (detected.Any(x => x.Name == expectedType))
                {
                    actualCount++;
                }
            }

            Assert.AreEqual(files.Length, actualCount);
        }

        [TestMethod]
        public void AudioTests_Ogg()
        {
            var audio = new Audio(File.ReadAllText(PathToAudioDescriptionFile));
            var formats = new List<FileFormat>
            {
                audio.AudioMp3,
                audio.AudioWav,
                audio.AudioOgg
            };

            var expectedType = audio.AudioOgg.Name;
            var files = Directory.GetFiles(PathToSamples, $"*.{audio.AudioOgg.Name}");
            var f = new FileFormatDetector(formats);

            var actualCount = 0;
            foreach (var file in files)
            {
                var detected = f.DetectFromFilePath(file, DetectionOptions.ReturnFirstDetected);
                if (detected.Any(x => x.Name == expectedType))
                {
                    actualCount++;
                }
            }

            Assert.AreEqual(files.Length, actualCount);
        }
    }
}
