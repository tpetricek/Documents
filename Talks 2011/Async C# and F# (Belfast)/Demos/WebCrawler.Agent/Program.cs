using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

// Use 'CrawlerAgent' from F# library
using WebCrawler.Core;

namespace WebCrawler
{
  class Program
  {
    #region Extracting stuff from HtmlDocument

    static IEnumerable<string> ExtractLinks(HtmlDocument doc)
    {
      try
      {
        return
          (from a in doc.DocumentNode.SelectNodes("//a")
           where a.Attributes.Contains("href")
           let href = a.Attributes["href"].Value
           where href.StartsWith("http://")
           let endl = Math.Min(href.IndexOf('?'), href.IndexOf('#'))
           select endl > 0 ? href.Substring(0, endl) : href).ToArray();
      }
      catch
      {
        return Enumerable.Empty<string>();
      }
    }

    static string GetTitle(HtmlDocument doc)
    {
      try
      {
        var title = doc.DocumentNode.SelectSingleNode("//title");
        return title != null ? title.InnerText.Trim() : "Untitled";
      }
      catch
      {
        return "Untitled";
      }
    }

    #endregion

    // --------------------------------------------------------------
		// Asynchronous web crawler (using async/await)
		// --------------------------------------------------------------
		
    async static Task<HtmlDocument> DownloadDocument(string url)
    {
      try
      {
        var wc = new WebClient();
        var html = await wc.DownloadStringTaskAsync(new Uri(url));
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
      }
      catch 
      {
        return new HtmlDocument();
      }
    }

    // --------------------------------------------------------------
		// Crawler process that adds URLs to be crawled to 'pending'
		// and keeps a list of visited URLs in 'visited'

    static CrawlerAgent agent = new CrawlerAgent();

    async static Task Crawler()
    {
      while (true)
      {
        string url = await agent.GetAsync();
        var doc = await DownloadDocument(url);
        agent.Enqueue(url, ExtractLinks(doc).ToArray());
        Console.WriteLine("[{0}]\n{1}\n", url, GetTitle(doc));
      }
    }

    // --------------------------------------------------------------
		// Start 100 of web crawlers using only small number of threads

    static void Main(string[] args)
    {
      agent.Start("http://www.guardian.co.uk");
			for (int i = 0; i < 100; i++)
				Crawler();
      Console.ReadLine();
    }
  }
}
