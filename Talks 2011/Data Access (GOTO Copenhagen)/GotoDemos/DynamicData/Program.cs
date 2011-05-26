using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Goto.Dynamic.Xml;
using System.Xml.Linq;

namespace DynamicData
{
  class Program
  {
    static void Main(string[] args)
    {
      // ------------------------------------------------------------------------
      // Listing of World Bank regions

      dynamic wb = new DynamicWorldBank();
      dynamic regions = wb.Region(new { PerPage = 100 });
      foreach (var reg in regions.Regions.Region)
      {
        Console.WriteLine("{0} ({1})", reg.Name.Value, reg.Code.Value);
      }
      Console.WriteLine();

      // ------------------------------------------------------------------------
      // Select countries in a specified region

      dynamic doc = wb.Countries(new {Region = "EMU", PerPage=100});
      foreach (var el in doc.Countries.Country)
      {
        Console.WriteLine("{0} ({1})", el.Name.Value, el.CapitalCity.Value);
      }
    }
  }
}
