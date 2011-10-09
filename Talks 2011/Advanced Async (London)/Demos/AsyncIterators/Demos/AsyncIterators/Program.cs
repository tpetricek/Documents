using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

using EeekSoft.Asynchronous;

namespace Demos.AsyncDemo
{
	class Program
	{
		// This example demonstrates that iterators are quite 
		// similar to async - in fact, it is possible to simulate
		// async methods using iterators (but the code looks
		// definitely a bit clumsy)

		// Asynchronous method that downloads the specified url and prints the HTML title
		static IEnumerable<IAsync> AsyncMethod(string url)
		{
			WebRequest req = HttpWebRequest.Create(url);
			Async<WebResponse> response = req.GetResponseAsync();
			yield return response;

			Stream resp = response.Result.GetResponseStream();
			Async<string> html = resp.ReadToEndAsync().ExecuteAsync<string>();
			yield return html;

			Regex reg = new Regex(@"<title[^>]*>(.*)</title[^>]*>");
			string title = reg.Match(html.Result).Groups[1].Value;
			Console.WriteLine("\n[{0}]\n{1}", url, title);
		}


		// Method which performs several HTTP requests asyncrhonously in parallel
		static IEnumerable<IAsync> DownloadAll()
		{
			var methods = Async.Parallel(
				AsyncMethod("http://www.microsoft.com"),
				AsyncMethod("http://www.google.com"),
				AsyncMethod("http://www.apple.com"),
				AsyncMethod("http://www.novell.com"));
			yield return methods;

			Console.WriteLine("\nCompleted all!");
		}


    static void Main(string[] args)
    {
      DownloadAll().ExecuteAndWait();
      Console.ReadLine();
    }
  }
}
