using System;
using Newtonsoft.Json;
using System.Linq;

namespace FileFormatDetection.Core
{
    public class FormatSignature
    {
        //TODO: improve with readonly/immutable 

        public int Offset { get; set; }
        public string Hex { get; set; }
        [JsonIgnore]
        public byte[] Bytes => Hex?.Split('-').Select(x => Convert.ToByte(x, 16)).ToArray();
        [JsonIgnore]
        public int ToRead => Bytes?.Length ?? 0;
    }
}