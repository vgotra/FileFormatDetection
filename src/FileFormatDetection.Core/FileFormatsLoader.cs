using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using Newtonsoft.Json;
using System.Linq;

namespace FileFormatDetection.Core
{
    public static class FileFormatsLoader
    {
        public static List<FileFormat> LoadFromJsonFile(this string path)
        {
            return File.ReadAllText(path).LoadFromJsonContent();
        }

        public static List<FileFormat> LoadFromJsonContent(this string formatsJson)
        {
            var formats = JsonConvert.DeserializeObject<List<FileFormat>>(formatsJson);
            ValidateFileFormats(formats);
            return formats;
        }

        public static void ValidateFileFormats(this List<FileFormat> formats)
        {
            for (var i = 0; i < formats.Count; i++)
            {
                try
                {
                    formats[i].ValidateFileFormat();
                }
                catch (InvalidDataException ex)
                {
                    throw new InvalidDataException($"Wrong file format for element with index {i}. Details in inner exception.", ex);
                }
            }
        }

        public static void ValidateFileFormat(this FileFormat fileFormat)
        {
            if (fileFormat == null)
            {
                throw new ArgumentNullException(nameof(fileFormat));
            }

            if (string.IsNullOrWhiteSpace(fileFormat.Type))
            {
                throw new InvalidDataException($"{nameof(fileFormat.Type)} property should not be null or empty for file format.");
            }

            if (string.IsNullOrWhiteSpace(fileFormat.Name))
            {
                throw new InvalidDataException($"{nameof(fileFormat.Name)} property should not be null or empty for file format.");
            }

            if (fileFormat.Extensions == null || !fileFormat.Extensions.Any() || fileFormat.Extensions.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidDataException($"{nameof(fileFormat.Extensions)} property should not be null or empty for file format.");
            }

            if (fileFormat.FormatSignatures == null || !fileFormat.FormatSignatures.Any())
            {
                throw new InvalidDataException($"{nameof(fileFormat.FormatSignatures)} property should not be null or empty for file format.");
            }
        }

        public static void ValidateFileSignature(this FormatSignature fileSignature)
        {
            if (fileSignature == null)
            {
                throw new ArgumentNullException(nameof(fileSignature));
            }

            if (string.IsNullOrWhiteSpace(fileSignature.Hex))
            {
                throw new InvalidDataException($"{nameof(fileSignature.Hex)} property should not be null or empty for file signature.");
            }

            if (!fileSignature.Hex.All(x => char.IsLetterOrDigit(x) || x == '-'))
            {
                throw new InvalidDataException($"Only letter or digit or dash allowed in {nameof(fileSignature.Hex)}.");
            }

            if (fileSignature.Offset < 0)
            {
                throw new InvalidDataException($"{nameof(fileSignature.Offset)} property should not be less than 0.");
            }
        }
    }
}
