using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFormatDetection.Core.Tests
{
    [TestClass]
    public class FileFormatsLoaderTests
    {
        [TestMethod]
        [DeploymentItem(@"AudioCorrect.json")]
        public void CanLoadFileFormatsFromJsonContent()
        {
            var formats = FileFormatsLoader.LoadFromJsonFile("AudioCorrect.json");
            Assert.IsNotNull(formats);
        }

        [TestMethod]
        [DeploymentItem(@"AudioIncorrect.json")]
        public void CannotLoadFileFormatsFromJsonContent()
        {
            Assert.ThrowsException<InvalidDataException>(() => FileFormatsLoader.LoadFromJsonFile("AudioIncorrect.json"));
        }
    }
}
