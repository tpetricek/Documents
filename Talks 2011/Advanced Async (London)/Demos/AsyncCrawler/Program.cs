using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using AsyncAgents;

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

		// Use blocking collection implemented using F# agent (in F#)
		// that does not block thread while calling 'TakeAsync'.
		// Note that the method works nicely with C# 5 'await' keyword

    static BlockingQueueAgent<string> pending =
			new BlockingQueueAgent<string>();
    static ConcurrentDictionary<string, bool> visited =
      new ConcurrentDictionary<string, bool>();

    async static Task Crawler()
    {
      while (true)
      {
        string url = await pending.TakeAsync();
        if (!visited.ContainsKey(url))
        {
          visited.TryAdd(url, true);
          var doc = await DownloadDocument(url);
          foreach (var link in ExtractLinks(doc))
            await pending.AddAsync(link);
          Console.WriteLine("[{0}]\n{1}\n", url, GetTitle(doc));
        }
      }
    }

    // --------------------------------------------------------------

    static void Main(string[] args)
    {
      pending.AddAsync("http://www.guardian.co.uk");
      for (int i = 0; i < 100; i++)
        Crawler();
      Console.ReadLine();
    }
  }
}
