using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace TescoCheckout.Service
{
  // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
  // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
  public class TescoService : ITescoService
  {
    static Dictionary<string, Tesco.Session> sessions = new Dictionary<string,Tesco.Session>();

    static List<Product> specials = new List<Product>() 
      { new Product { Name = "The Economist", Price = 4.20, EANBarcode = "9770013061206", ImagePath = @"C:\Tomas\Materials\Public\Talks 2012\F# Applications (New York)\TescoCheckout\Data\economist.jpg" } };

    public Session Login(string email, string password, string developerKey, string applicationKey)
    {
      try
      {
        var client = new Tesco.SOAPServiceSoapClient();
        Tesco.Session session;
        client.Login(email, password, developerKey, applicationKey, out session);
        var key = Guid.NewGuid().ToString();
        sessions[key] = session;
        return new Session { Value = key };
      }
      catch
      {
        return new Session { Value = "N/A" };
      }
    }

    public Product[] ProductSearch(Session session, string search)
    {
      if (session.Value == "N/A")
      {
        return specials.Where(s => s.EANBarcode == search || s.Name.Contains(search)).ToArray();
      }
      else
      {
        var client = new Tesco.SOAPServiceSoapClient();
        int pageCount, prodCount;
        Tesco.Product[] products;
        client.ProductSearch(sessions[session.Value], search, false, 1, out products, out pageCount, out prodCount);
        return
          (specials
            .Where(s => s.EANBarcode == search || s.Name.Contains(search))
            .Concat(products.Select(p =>
              new Product
              {
                EANBarcode = p.EANBarcode,
                ImagePath = p.ImagePath,
                Name = p.Name,
                Price = p.Price
              }))).ToArray();
      }
    }
  }
}
