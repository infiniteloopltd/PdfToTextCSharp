using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PDFExtract
{
    class FastPDFPage
    {
        public FastPDFPage(byte[] stream)
        {
            // Decompress Stream
            RawContent = Decompress(stream);
            // Extract content
            const string strContentRegex = @"\((?<Content>.*?)\)Tj";
            UnstructuredContent = Regex.Matches(RawContent, strContentRegex)
                .Select(m => m.Groups["Content"].Value)
                .ToList();
        }

        public string RawContent { get; set; }

        public int StartPointer { get; set; }

        public int EndPointer { get; set; }

        public int PageNumber { get; set; }

        public List<string> UnstructuredContent { get; set; }

        private static string Decompress(byte[] input)
        {
            var cutInput = new byte[input.Length - 2];
            Array.Copy(input, 2, cutInput, 0, cutInput.Length);
            var stream = new MemoryStream();
            using (var compressStream = new MemoryStream(cutInput))
            using (var deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                deflateStream.CopyTo(stream);
            return Encoding.Default.GetString(stream.ToArray());
        }
    }
}
