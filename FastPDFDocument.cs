using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDFExtract
{
    class FastPDFDocument
    {
        public FastPDFDocument(byte[] document)
        {
            // parse document into pages
            Pages = new List<FastPDFPage>();
            var intStartMarker = 0;
            var intEndMarker = 0;
            for (var pointer = 0; pointer < document.Length; pointer++)
            {
                if (ReadAheadForMarker(document.Skip(pointer),"stream"))
                {
                    intStartMarker = pointer + 7;
                }
                if (ReadAheadForMarker(document.Skip(pointer), "endstream"))
                {
                    intEndMarker = pointer;
                    var lengthOfStream = intEndMarker - intStartMarker;
                    var compressed = document.Skip(intStartMarker).Take(lengthOfStream).ToArray();
                    Pages.Add(new FastPDFPage(compressed) 
                        { 
                            EndPointer = intEndMarker, 
                            StartPointer = intStartMarker,
                            PageNumber = Pages.Count + 1
                        });
                }
            }
            // Prune empty pages.
            Pages.RemoveAll(p => p.UnstructuredContent.Count == 0);
        }

        private static bool ReadAheadForMarker(IEnumerable<byte> document, string marker)
        {
            var lenMarker = Encoding.UTF8.GetBytes(marker).Length;
            var readAhead = Encoding.UTF8.GetString(document.Take(lenMarker).ToArray());
            return readAhead == marker;
        }

        public List<FastPDFPage> Pages { get; set; }

        public List<FastPDFPage> Find(string text)
        {
            return Pages
                .Where(p => p.UnstructuredContent
                    .Any(c => string.Equals(c, text, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
