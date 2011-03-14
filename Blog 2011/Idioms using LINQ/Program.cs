using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ApplicativeJoin
{
  // The alternative implementation below implements 
  // functional lazy list and so it gives a more 
  // realistic and efficient implementation of matrix
  // transpose demo. 

  #region Alternative implementation 

  class ZipList<T>
  {
    public bool IsEmpty { get; private set; }
    public T Value { get; private set; }
    public Lazy<ZipList<T>> Tail { get; private set; }

    public ZipList() { IsEmpty = true; }
    public ZipList(T value, Lazy<ZipList<T>> tail) { 
      Value = value; Tail = tail;  
    }
  }

  static class ZipList
  {
    public static ZipList<T> Empty<T>() {
      return new ZipList<T>();
    }
    
    public static ZipList<T> Cons<T>(T value, Lazy<ZipList<T>> tail) {
      return new ZipList<T>(value, tail);
    }

    public static IEnumerable<T> AsEnumerable<T>(this ZipList<T> zip) {
      ZipList<T> current = zip;
      while (!current.IsEmpty)
      {
        yield return current.Value;
        current = current.Tail.Value;
      }
    }

    public static ZipList<T> ToZipList<T>(this IEnumerable<T> sequence) {
      var en = sequence.GetEnumerator();
      Func<ZipList<T>> next = null;
      next = () => en.MoveNext() ? ZipList.Cons(en.Current, new Lazy<ZipList<T>>(next)) : ZipList.Empty<T>();
      return next();
    }

    public static ZipList<int> Range(int from, int count) {
      return Enumerable.Range(from, count).ToZipList();
    }

    public static ZipList<T> Pure<T>(T v) {
      Func<ZipList<T>> next = null;
      next = () => ZipList.Cons(v, new Lazy<ZipList<T>>(next));
      return next();
    }

    public static ZipList<R> Apply<T, R>(this ZipList<Func<T, R>> f, ZipList<T> a) {
      return Enumerable.Zip(f.AsEnumerable(), a.AsEnumerable(), (func, arg) => func(arg)).ToZipList();
    }

    public static ZipList<R> Select<T, R>(this ZipList<T> source, Func<T, R> func) {
      return ZipList.Pure(func).Apply(source);
    }

    #region Boilerplate

    public static ZipList<Func<T, R>> PureFunc<T, R>(Func<T, R> v) {
      return ZipList.Pure(v);
    }

    public static ZipList<Func<T1, Func<T2, R>>> PureFunc<T1, T2, R>(Func<T1, T2, R> v) {
      return ZipList.Pure(v).Select<Func<T1, T2, R>, Func<T1, Func<T2, R>>>(f => a => b => f(a, b));
    }

    public static ZipList<Func<T1, Func<T2, Func<T3, R>>>> PureFunc<T1, T2, T3, R>(Func<T1, T2, T3, R> v) {
      return ZipList.Pure(v).Select<Func<T1, T2, T3, R>, Func<T1, Func<T2, Func<T3, R>>>>(f => a => b => c => f(a, b, c));
    }

    #endregion

    public static ZipList<TResult> Join<TOuter, TInner, TKey, TResult>
      (this ZipList<TOuter> outer, ZipList<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector)
    {
      return
        ZipList.PureFunc(resultSelector).
          Apply(outer).Apply(inner);
    }
  }
  
  #endregion

  // This sample implements ZipList idiom using the 
  // standard IEnumerable type. This may not be very 
  // efficient, but it is easy to understand.

  #region Enumerable-based ZipList

  static class ZipEnumerable
  {
    public static IEnumerable<T> Return<T>(T value) {
      while (true) yield return value;
    }

    public static IEnumerable<Tuple<T1, T2>> Merge<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second) {
      return Enumerable.Zip(first, second, Tuple.Create);
    }

    public static IEnumerable<R> Select<T, R>(this IEnumerable<T> source, Func<T, R> func) {
      return Enumerable.Select(source, func);
    }

    public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>
      (this IEnumerable<TOuter> outer, IEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector)
    {
      return outer.Merge(inner).Select(tup => resultSelector(tup.Item1, tup.Item2));
    }  
  }

  #endregion

  static class Program
  {
    // The transpose function implemented using the 
    // lazy functional list declared above

    #region Alternative implementation of Transpose

    static ZipList<ZipList<T>> Transpose<T>(this ZipList<ZipList<T>> matrix)
    {
      if (matrix.IsEmpty)
        return ZipList.Pure(ZipList.Empty<T>());
      else
        return
          ZipList.
            PureFunc((T v, ZipList<T> l) => ZipList.Cons(v, new Lazy<ZipList<T>>(() => l))).
            Apply(matrix.Value).
            Apply(Transpose(matrix.Tail.Value));
    }

    static ZipList<ZipList<T>> Transpose2<T>(this ZipList<ZipList<T>> matrix)
    {
      if (matrix.IsEmpty)
        return ZipList.Pure(ZipList.Empty<T>());
      else
        return
          from m in matrix.Value
          join r in Transpose(matrix.Tail.Value) on 1 equals 1
          select ZipList.Cons<T>(m, new Lazy<ZipList<T>>(() => r));
    }

    #endregion

    // Two versions of the Transpose operation based on 
    // ZipList idiom using IEnumerable<T>. One version is written
    // directly and the other one uses LINQ syntax with 'join'

    #region Transpose from the blog post

    static IEnumerable<IEnumerable<T>> Transpose2<T>(this IEnumerable<IEnumerable<T>> matrix)
    {
      if (matrix.Count() == 0)
        return ZipEnumerable.Return(Enumerable.Empty<T>());
      else
        return
          ZipEnumerable.
            Merge(matrix.First(), Transpose(matrix.Skip(1))).
            Select(tuple => 
              Enumerable.Concat(new[] { tuple.Item1 }, tuple.Item2));
    }

    static IEnumerable<IEnumerable<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> matrix)
    {
      return matrix.Count() == 0 ?
        ZipEnumerable.Return(Enumerable.Empty<T>()) :
        from xs in matrix.First()
        join xss in Transpose(matrix.Skip(1)) on 1 equals 1
        select Enumerable.Concat(new[] { xs }, xss);
    }

    #endregion

    static void Main(string[] args)
    {
      var prices = new[] { 
        new[] { 10.0, 4.0, 13.0, 20.0 },
        new[] { 12.0, 3.0, 11.0, 25.0 },
        new[] {  9.0, 1.0, 16.0, 24.0 },
      };

      // Calculating average stock prices using idioms (directly)

      var res1 = ZipEnumerable.Return(0.0);
      foreach (var day in prices)
        res1 = res1.Merge(day).Select(tup => tup.Item1 + tup.Item2);
      var avg1 = res1.Select(sum => sum / prices.Count());

      // Calculating average stock prices using idioms (LINQ)

      var res = ZipEnumerable.Return(0.0);
      foreach (var day in prices)
        res = from sum in res
              join price in day on 1 equals 1
              select sum + price;
      var avg = from sum in res select sum / prices.Count();
      
      foreach (var price in avg)
        Console.Write("{0}, ", price);


      // Matrix transposition example

      var matrix = new[] { 
        new[] { 1,   2,   3 },
        new[] { 10,  20,  30 },
        new[] { 100, 200, 300 } };

      var transposed = matrix.Transpose();
      foreach (var a in transposed) {
        foreach (var b in a)
          Console.Write("{0}, ", b);
        Console.WriteLine();
      }
    }

    // Similar demos as those from blog post, but based
    // on the functional lazy list implementation

    #region Alternative demos

    private static void ZipListDemos()
    {
      var nums1 = ZipList.Range(0, 10);
      var nums2 = ZipList.Range(0, 10);

      var q =
        from n in nums1
        join m in nums2 on 1 equals 1
        select n + m;

      foreach (var z in q.AsEnumerable())
        Console.Write("{0}, ", z);

      Console.WriteLine();
      Console.WriteLine();

      var matrix = new[] { 
        new[] { 1,   2,   3 },
        new[] { 10,  20,  30 },
        new[] { 100, 200, 300 } };

      var zipMatrix = matrix.Select(ZipList.ToZipList).ToZipList();

      var transposed = zipMatrix.Transpose2();

      foreach (var a in transposed.AsEnumerable())
      {
        foreach (var b in a.AsEnumerable())
          Console.Write("{0}, ", b);
        Console.WriteLine();
      }
    }

    #endregion
  }
}
