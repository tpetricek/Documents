using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lists
{
  public class FunctionalList<T>
  {
    // Creates a new list that is empty
    public FunctionalList() { IsEmpty = true; }

    // Creates a new list containe value and a reference to tail
    public FunctionalList(T head, FunctionalList<T> tail) {
      IsEmpty = false; Head = head; Tail = tail;
    }

    // Is the list empty?
    public bool IsEmpty { get; private set; }
    // Properties valid for a non-empty list
    public T Head { get; private set; }
    public FunctionalList<T> Tail { get; private set; }
  }


  class Program
  {
    static int SumList(FunctionalList<int> numbers) {
      return 0;
    }

    static void Main(string[] args)
    {
      // Creates list containing 1, 2, 3, 4, 5
      var list = new FunctionalList<int>(1, new FunctionalList<int>(2,
         new FunctionalList<int>(3, new FunctionalList<int>(4,
         new FunctionalList<int>(5, new FunctionalList<int>())))));

      int sum = SumList(list); // Calculates sum and
      Console.WriteLine(sum);  // prints '15' as the result
    }

    #region Code for demos

    // if (numbers.IsEmpty) return 0;
    // else return numbers.Head + SumList(numbers.Tail);

    #endregion
  }
}
