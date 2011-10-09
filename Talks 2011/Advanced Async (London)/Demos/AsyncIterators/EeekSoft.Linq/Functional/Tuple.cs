using System;
using System.Collections.Generic;
using System.Text;

namespace EeekSoft.Functional
{
	public struct Tuple<T1, T2>
	{
		private T1 _first;
		private T2 _second;

		public T2 Second
		{
			get { return _second; }
			set { _second = value; }
		}
	
		public T1 First
		{
			get { return _first; }
			set { _first = value; }
		}

		public Tuple(T1 first, T2 second)
		{
			_first = first;
			_second = second;
		}
	}

	public static class Tuple
	{
		public static Tuple<T1, T2> New<T1, T2>(T1 v1, T2 v2)
		{
			return new Tuple<T1, T2>(v1, v2);
		}
	}
}
