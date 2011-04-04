using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Xml.Linq;

namespace Reactive.WebServices
{
  public partial class get : System.Web.UI.Page
  {
    protected override void OnInit(EventArgs e)
    {
      var list = Application["rectangles"] as List<Tuple<long, RectangleF, Color>>;
      if (list == null) 
        Application["rectangles"] = list = new List<Tuple<long, RectangleF, Color>>();

      var stamp = Int64.Parse(Request.QueryString["timestamp"]);
      var elements = 
        from rc in list
        where rc.Item1 > stamp
        select 
          new XElement("Rectangle",
            new XAttribute("Stamp", rc.Item1),
            new XAttribute("Left", rc.Item2.Left),
            new XAttribute("Top", rc.Item2.Top),
            new XAttribute("Right", rc.Item2.Width),
            new XAttribute("Bottom", rc.Item2.Height),
            new XAttribute("R", rc.Item3.R),
            new XAttribute("G", rc.Item3.G),
            new XAttribute("B", rc.Item3.B));

      var doc = new XDocument(new XElement("Rectangles", elements));

      doc.Save(Response.OutputStream);
      Response.End();
    }
  }
}