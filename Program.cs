﻿using System;
using System.Collections.Concurrent;
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

            

            var memoryStream = new MemoryStream(data);
            var pdfDocument = new PdfDocument(new PdfReader(memoryStream));
            var strategy = new LocationTextExtractionStrategy();
            var ts = DateTime.Now;
            var iPageList = Enumerable.Range(1, pdfDocument.GetNumberOfPages());
            var allText = "";
            var pageTextSync = new ConcurrentBag<string>();
            var pageTextParallel = new ConcurrentBag<string>();

            foreach (var pageNumber in iPageList)
           {
               var page = pdfDocument.GetPage(pageNumber);
               string text = PdfTextExtractor.GetTextFromPage(page, strategy);
               string processed = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));
               pageTextSync.Add(processed);
            }
            

            Parallel.ForEach(iPageList, (pageNumber) => 
                {
                    var memoryStreamTemp = new MemoryStream(data);
                    var pdfDocumentTemp = new PdfDocument(new PdfReader(memoryStreamTemp));
                    var strategyTemp = new LocationTextExtractionStrategy();
                    var page = pdfDocumentTemp.GetPage(pageNumber);
                    string text = PdfTextExtractor.GetTextFromPage(page, strategyTemp);
                    string processed = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));
                    Console.WriteLine(processed.Length);
                    pageTextParallel.Add(processed);
                }
             );
             var allTextSync = string.Join("", pageTextSync);
             var allTextParallel = string.Join("", pageTextParallel);
            // 		allText.Length	462709	int
            // 		allText.Length	67813669	int


            var elapsed = DateTime.Now - ts;
            Console.WriteLine("seconds" + +elapsed.TotalSeconds); // 7 seconds
            pdfDocument.Close();
        }
    }
}
