using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

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
		// To add cancellation support in C#, we need to pass
		// around 'CancellationToken' and explicitly check if it 
		// was cancelled using 'ThrowIfCancellationRequested'

    async static Task<HtmlDocument> DownloadDocument
			(string url, CancellationToken cancellationToken)
    {
      try
      {
        var wc = new WebClient();
        var html = await wc.DownloadStringTaskAsync(new Uri(url), cancellationToken);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
				cancellationToken.ThrowIfCancellationRequested();
        return doc;
      }
      catch 
      {
        return new HtmlDocument();
      }
    }

    // --------------------------------------------------------------

    static BlockingCollection<string> pending =
      new BlockingCollection<string>();
    static ConcurrentDictionary<string, bool> visited =
      new ConcurrentDictionary<string, bool>();

		// Propagate CancellationToken to the 'DownloadDocument'
		// method and also check for cancellation at various places

    async static Task Crawler(CancellationToken cancellationToken)
    {
      while (true)
      {
        string url = pending.Take();
				cancellationToken.ThrowIfCancellationRequested();
				if (!visited.ContainsKey(url))
        {
          visited.TryAdd(url, true);
					var doc = await DownloadDocument(url, cancellationToken);
					cancellationToken.ThrowIfCancellationRequested();
					foreach (var link in ExtractLinks(doc))
            pending.Add(link);
          Console.WriteLine("[{0}]\n{1}\n", url, GetTitle(doc));
        }
      }
    }

    // --------------------------------------------------------------

    static void Main(string[] args)
    {
			CancellationTokenSource src = new CancellationTokenSource();
      pending.Add("http://www.guardian.co.uk");
      for (int i = 0; i < 100; i++)
        Crawler(src.Token);

      Console.ReadLine();
			// Cancel the computation!
			src.Cancel();
			Console.WriteLine("Cancelled");
      Console.ReadLine();
    }
  }
}
