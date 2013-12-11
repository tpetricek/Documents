using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace TescoCheckout.Service
{
  [ServiceContract]
  public interface ITescoService
  {
    [OperationContract]
    Session Login(string email, string password, string developerKey, string applicationKey);

    [OperationContract]
    Product[] ProductSearch(Session session, string search);
  }

  [DataContract]
  public class Product
  {
    string name;
    string imagePath;
    double price;
    string barcode;

    [DataMember]
    public string Name { get { return name; } set { name = value; } }
    [DataMember]
    public string ImagePath { get { return imagePath; } set { imagePath = value; } }
    [DataMember]
    public double Price { get { return price; } set { price = value; } }
    [DataMember]
    public string EANBarcode { get { return barcode; } set { barcode = value; } }
  }

  [DataContract]
  public class Session
  {
    string value = "";

    [DataMember]
    public string Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }
}
