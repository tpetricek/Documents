using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tuples
{
  class Program
  {
    static Random rnd = new Random();

    static Tuple<double, double> RandomPrice(double max)
    {
      var p1 = Math.Round(rnd.NextDouble() * max, 1);
      var p2 = Math.Round(rnd.NextDouble() * max, 1);
      return Tuple.Create(p1, p2);
    }

    static Tuple<double, double> Normalize(double first, double second)
    {
      if (first < second) return Tuple.Create(first, second);
      else return Tuple.Create(second, first);
    }

    static void Main(string[] args)
    {
      var range = RandomPrice(100);
      var min = range.Item1;
      var max = range.Item2;

      // TODO: Call normalize to fix the range

      Console.WriteLine("From ${0} to ${1}", min, max);
    }

    #region Code for demos

    #region TODO: Call normalize to correct range

    // range = Normalize(min, max);
    // min = range.Item1;
    // max = range.Item2; 

    #endregion
    #region TODO: Use tuple as argument

    static Tuple<double, double> Normalize2(Tuple<double, double> price)
    {
      if (price.Item1 < price.Item2) return price;
      else return Tuple.Create(price.Item2, price.Item1);
    }
    
    // var range = Normalize(RandomPrice(100));

    #endregion

    #endregion
  }
}
