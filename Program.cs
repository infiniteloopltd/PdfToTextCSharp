using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace PDFExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            // Install-Package itext7 -Version 7.1.15
            using var webClient = new WebClient();
            const string url = "http://electoral.gov.vc/electoral/images/PDF/voters_list/second_quarter_2021/ckalpha.pdf";
            var data = webClient.DownloadData(url);
            Console.WriteLine("Downloaded");

            var ts = DateTime.Now;
            var doc = new FastPDFDocument(data);
            var pages = doc.Find("YORKE");


            var memoryStream = new MemoryStream(data);
            var pdfDocument = new PdfDocument(new PdfReader(memoryStream));
            var strategy = new LocationTextExtractionStrategy();
           
            var allText = "";
     
            // Works
            foreach (var pageNumber in pages.Select(p => p.PageNumber))
            {
               var page = pdfDocument.GetPage(pageNumber);
               string text = PdfTextExtractor.GetTextFromPage(page, strategy);
               string processed = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));
               allText += processed;
            }

            var elapsed = DateTime.Now - ts;
            Console.WriteLine("seconds" + +elapsed.TotalSeconds); // 7 seconds down to 1.1 seconds !
            pdfDocument.Close();
        }
    }
}
