using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iterators
{
  class Program
  {
		// Iterators are similar to asynchronous methods 
		// they both can be suspended and later resumed to do
		// more work (in case of iterators, this means generating
		// the next element, in case of async, this means 
		// performing operation after callback is called by
		// the system)

    static IEnumerable<int> Numbers()
    {
      Console.WriteLine("Numbers: entering...");

      yield return 1;
      Console.WriteLine("Numbers: returned 1");

      yield return 2;
      Console.WriteLine("Numbers: returned 2");
    }

    static void Main(string[] args)
    {
      var en = Numbers().GetEnumerator();
      Console.WriteLine("Main: got enumerator");
      while (en.MoveNext())
      {
        Console.WriteLine("Main: got {0}", en.Current);
      }
    }
  }
}
