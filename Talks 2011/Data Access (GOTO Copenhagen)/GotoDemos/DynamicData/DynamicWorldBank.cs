using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using Goto.Dynamic.Xml;

namespace DynamicData
{
  class DynamicWorldBank : DynamicObject
  {
    /// <summary>
    /// Turn a member name into a method name expected by the World Bank.
    /// For example "RegionDetails" will become "region_details".
    /// </summary>
    private IEnumerable<char> NormalizeName(string name)
    {
      yield return Char.ToLowerInvariant(name[0]);
      foreach (char c in name.Skip(1))
      {
        if (Char.IsUpper(c))
        {
          yield return '_';
          yield return Char.ToLowerInvariant(c);
        }
        else yield return c;
      }
    }

    /// <summary>
    /// Called when the C# code using 'dynamic' uses "wb.Foo(...)". The method
    /// treats this as a call to the "foo" service provided by WorldBank. The
    /// method takes an anonymous type as an argument, so it is possible to 
    /// specify any additional parameters dynamically. 
    /// </summary>
    /// <example><code>
    /// dynamic wb = new DynamicWorldBank();
    /// dynamic regions = wb.Region(new { PerPage = 100 });
    /// </code></example>
    /// <returns>
    /// Returns an instance of 'DynamicXml' type that can be used for easy
    /// access to XML documents using C# 4 'dynamic' keyword.
    /// </returns>
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
      // Create URL for the request
      string url = "http://api.worldbank.org/" + new String(NormalizeName(binder.Name).ToArray());
      if (args.Length == 1) 
      {
        // If there is some anonymous type as argument, get its properties
        // and append them as arguments to the URL
        var separator = "?";
        foreach (var prop in args[0].GetType().GetProperties())
        {
          url = url + separator + 
            new String(NormalizeName(prop.Name).ToArray()) + "=" + 
            prop.GetValue(args[0], new object[] {}).ToString();
          separator = "&";
        }
      }
      // Perform HTTP request & return result as DynamicXml
      result = new DynamicXml(url, CamelCase: true, Namespace: "http://www.worldbank.org");
      return true;
    }
  }
}
