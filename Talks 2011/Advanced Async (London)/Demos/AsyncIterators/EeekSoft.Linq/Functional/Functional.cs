using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace EeekSoft.Functional
{
	public static class EnumerableExts
	{
		public static S Fold<T, S>(this IEnumerable<T> list, Func<S, T, S> fun, S init)
		{
			foreach (T el in list)
				init = fun(init, el);
			return init;
		}

		public static IEnumerable<Tuple<T1, T2>> ZipWith<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second)
		{
			IEnumerator<T1> e1 = first.GetEnumerator();
			IEnumerator<T2> e2 = second.GetEnumerator();
			while (e1.MoveNext() && e2.MoveNext())
				yield return Tuple.New(e1.Current, e2.Current);
		}
	}

	public static class FuncExts
	{
		public static Func<A1, A2, A3, T> Curry<A0, A1, A2, A3, T>(this Func<A0, A1, A2, A3, T> func, A0 arg)
		{
			return ( a1, a2, a3 ) => func(arg, a1, a2, a3);
		}

		public static Func<A1, A2, T> Curry<A0, A1, A2, T>(this Func<A0, A1, A2, T> func, A0 arg)
		{
			return ( a1, a2 ) => func(arg, a1, a2);
		}

		public static Func<A1, T> Curry<A0, A1, T>(this Func<A0, A1, T> func, A0 arg)
		{
			return ( a1 ) => func(arg, a1);
		}
	}
}
