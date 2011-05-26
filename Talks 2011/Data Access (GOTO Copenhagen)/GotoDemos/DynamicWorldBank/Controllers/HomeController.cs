using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Goto.Dynamic;

namespace DynamicWorldBank.Controllers
{
  // ------------------------------------------------------------------------
  // Types for storing information that are returned to the model

  public class Region
  {
    public string Name { get; set; }
    public string Code { get; set; }
  }

  public class Country
  {
    public string CapitalCity { get; set; }
    public string Name { get; set; }
  }

  public class HomeController : Controller
  {
    // ------------------------------------------------------------------------
    // Listing of World Bank regions

    public ActionResult Index()
    {
      // Request data for 'region' with 100 items per page
      dynamic wb = new WorldBank();
      dynamic xml = wb.Region(new { PerPage = 100 });

      // Get all XML nodes matching the /regions/region path
      IEnumerable<dynamic> regions = xml.Regions.Region;

      // For every region, get the value from /node and /code elements
      ViewData.Model = 
        from reg in regions
        select new Region { 
          Name = reg.Name.Value, 
          Code = reg.Code.Value 
        };
      return View();
    }

    // ------------------------------------------------------------------------
    // Select countries in a specified region

    public ActionResult Countries(string id)
    {
      // Request data for 'countries' with the specified region & 100 items per page
      dynamic wb = new WorldBank();
      dynamic xml = wb.Countries(new {Region = id, PerPage=100});

      // Get all XML nodes matching the /countries/country path
      IEnumerable<dynamic> countries = xml.Countries.Country;

      // For every country, get the value from /name and /capitalCity elements
      ViewData.Model =
        from country in countries
        select new Country {
          Name = country.Name.Value,
          CapitalCity = country.CapitalCity.Value
        };
      return View();
    }
  }
}
