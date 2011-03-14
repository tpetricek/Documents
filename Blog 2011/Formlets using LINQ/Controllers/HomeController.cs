using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Text;

namespace LinqFormlets.Controllers
{
  public class Booking
  {
    public string Name { get; set; }
    public DateTime Departure { get; set; }
    public DateTime Return { get; set; }
  }

  [HandleError]
  public class HomeController : Controller
  {
    /*
    static Formlet<DateTime> formlet =
      Formlet.PureFunc((int day, string month, int year) => new DateTime(year, Int32.Parse(month), day)).
        Apply(Formlet.DropDownRange(1, 31)).
        Apply(Formlet.DropDown(monthNames)).
        Apply(Formlet.DropDownRange(1960, 50));
    */

    static IEnumerable<Tuple<string, string>> monthNames = 
      from m in Enumerable.Range(1, 12)
      let name = new DateTime(2000, m, 1).ToString("MMMM")
      select Tuple.Create(m.ToString(), name);

    static Formlet<DateTime> dateSelectorUsingMethods =
      Formlet.DropDownRange(1, 31).Merge(
      Formlet.DropDown(monthNames).Select(Int32.Parse)).Merge(
      Formlet.DropDownRange(1960, 50)).Select(tup =>
        new DateTime(tup.Item1.Item1, tup.Item1.Item2, tup.Item2));

    static Formlet<DateTime> dateSelector =
      from day in Formlet.DropDownRange(1, 31)
      join month in Formlet.DropDown(monthNames) on 1 equals 1
      let monthNo = Int32.Parse(month)
      join year in Formlet.DropDownRange(1960, 50) on 1 equals 1
      select new DateTime(year, monthNo, day);

    static Formlet<Booking> formlet =
      from name in Formlet.Input()
      join o1 in Formlet.Tag("br") on 1 equals 1
      join departure in dateSelector on 1 equals 1
      join o2 in Formlet.Tag("br") on 1 equals 1
      join retrn in dateSelector on 1 equals 1
      select new Booking { Name = name, Departure = departure, Return = retrn };

    public ActionResult Index()
    {
      ViewData.Model = formlet;
      return View();
    }

    public ActionResult Register()
    {
      var env = new Environment
        (Request.Form.AllKeys.Select(k =>
          Tuple.Create(k, Request.Form[k])));

      var res = formlet.Evaluate(env);
      ViewData.Model = res;
      return View();
    }
  }
}
