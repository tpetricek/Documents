using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Drawing;
using System.Threading;
using System.Globalization;

namespace Reactive.WebServices
{
  public partial class store : System.Web.UI.Page
  {
    protected override void OnInit(EventArgs e)
    {
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

      var doc = XDocument.Load(Request.InputStream);
      var rc = new RectangleF
        (Single.Parse(doc.Root.Attribute("Left").Value),
         Single.Parse(doc.Root.Attribute("Top").Value),
         Single.Parse(doc.Root.Attribute("Right").Value),
         Single.Parse(doc.Root.Attribute("Bottom").Value));

      var clr = Color.FromArgb
        (Int32.Parse(doc.Root.Attribute("R").Value),
         Int32.Parse(doc.Root.Attribute("G").Value),
         Int32.Parse(doc.Root.Attribute("B").Value));

      var list = Application["rectangles"] as List<Tuple<long, RectangleF, Color>>;
      if (list == null)
        Application["rectangles"] = list = new List<Tuple<long, RectangleF, Color>>();

      list.Add(Tuple.Create(DateTime.Now.Ticks, rc, clr));
      base.OnInit(e);
    }
  }
}